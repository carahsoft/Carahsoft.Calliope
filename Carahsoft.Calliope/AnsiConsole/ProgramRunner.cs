using Carahsoft.Calliope.Constants;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace Carahsoft.Calliope.AnsiConsole
{
    public class ProgramRunner<TProgram> where TProgram : ICalliopeProgram
    {
        private const double MIN_KEY_RATE = 1000 / 60;

        private readonly TProgram _program;
        private readonly ProgramOptions _opts;
        private readonly TimeSpan _framerate;
        private readonly PeriodicTimer _renderTimer;
        private readonly SemaphoreSlim _renderLock = new SemaphoreSlim(1, 1);
        private readonly CommandRunner _commands;

        private readonly Channel<CalliopeMsg> _messageChannel = Channel.CreateUnbounded<CalliopeMsg>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });

        private readonly Channel<CalliopeCmd> _commandChannel = Channel.CreateUnbounded<CalliopeCmd>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });

        private int _linesRendered = 0;
        private bool _updated = true;
        private bool _quitting = false;
        private string[]? _previousRender;
        private int _screenHeight = 0;
        private int _screenWidth = 0;
        private bool _flush;
        // An exception from the program's Update or View, rethrown by RunAsync
        // once the terminal is restored
        private Exception? _error;

        public ProgramRunner(TProgram program, ProgramOptions opts)
        {
            _program = program;
            _opts = opts;

            if (_opts.Framerate <= 0 || _opts.Framerate > 60)
                _opts.Framerate = 60;

            _framerate = TimeSpan.FromSeconds(1) / _opts.Framerate;
            _renderTimer = new PeriodicTimer(_framerate);
            // Fails if the program has stopped, in which case the message isn't needed
            _commands = new CommandRunner(msg => _messageChannel.Writer.TryWrite(msg));
        }

        /// <summary>
        /// Send a message to the program from outside of the program.
        /// This method is mainly used for integrating your Calliope program
        /// with an external library.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendAsync(CalliopeMsg msg)
        {
            await _messageChannel.Writer.WriteAsync(msg);
        }

        /// <summary>
        /// This executes the <see cref="TProgram"/>, taking over the
        /// console to render the application.
        /// </summary>
        /// <returns>
        /// Returns the final state of the program when the program exits cleanly.
        /// </returns>
        /// <remarks>
        /// If the program's Init, Update or View throws, the program stops,
        /// the terminal is restored, and the exception is rethrown.
        /// </remarks>
        public async Task<TProgram> RunAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsConsole.SetWindowsConsoleMode();
            }

            var ctrlCRestore = Console.TreatControlCAsInput;
            var encodingRestore = Console.OutputEncoding;
            Console.TreatControlCAsInput = true;
            Console.OutputEncoding = Encoding.UTF8;
            if (_opts.StandardOut == null)
                _opts.StandardOut = Console.Out;

            _opts.StandardOut.Write(AnsiConstants.HideCursor);

            if (_opts.Fullscreen)
            {
                _opts.StandardOut.Write(AnsiConstants.EnableAltScreenBuffer);
                _opts.StandardOut.Write(AnsiConstants.ClearDisplay);
                Console.SetCursorPosition(0, 0);
            }

            if (_opts.EnableMouse)
            {
                _opts.StandardOut.Write(AnsiConstants.EnableMouseTracking);
            }

            try
            {
                await RunLoopsAsync();
            }
            finally
            {
                _commands.StopAll();
                Console.TreatControlCAsInput = ctrlCRestore;
                Console.OutputEncoding = encodingRestore;
                if (_opts.EnableMouse)
                    _opts.StandardOut.Write(AnsiConstants.DisableMouseTracking);
                _opts.StandardOut.Write(AnsiConstants.ShowCursor);
                if (_opts.Fullscreen)
                    _opts.StandardOut.Write(AnsiConstants.DisableAltScreenBuffer);
            }

            if (_error != null)
                ExceptionDispatchInfo.Throw(_error);

            // Return the final state of the program to the caller
            return _program;
        }

        private async Task RunLoopsAsync()
        {
            _screenHeight = Console.BufferHeight;
            _screenWidth = Console.BufferWidth;
            await _messageChannel.Writer.WriteAsync(new WindowSizeChangeMsg
            {
                ScreenHeight = _screenHeight,
                ScreenWidth = _screenWidth,
            });

            var cmd = _program.Init();

            if (cmd != null)
                await _commandChannel.Writer.WriteAsync(cmd);

            var messageLoopTask = Task.Run(MessageLoop);
            var commandLoopTask = Task.Run(CommandLoop);

            // Render screen in background task while primary thread waits on user input
            var renderTask = Task.Run(async () =>
            {
                while (await _renderTimer.WaitForNextTickAsync())
                {
                    if (_quitting)
                    {
                        await messageLoopTask;
                        await commandLoopTask;
                        if (_error == null)
                            await RenderBuffer();
                        return;
                    }

                    if (Console.BufferHeight != _screenHeight || Console.BufferWidth != _screenWidth)
                    {
                        _flush = true;
                        _screenHeight = Console.BufferHeight;
                        _screenWidth = Console.BufferWidth;

                        // if screen shrunk and we painted more than the new
                        // height lines, ignore lines from the top
                        if (_screenHeight < _linesRendered)
                        {
                            _linesRendered = _screenHeight;
                            _previousRender = _previousRender![^_linesRendered..];
                        }

                        _messageChannel.Writer.TryWrite(new WindowSizeChangeMsg
                        {
                            ScreenHeight = _screenHeight,
                            ScreenWidth = _screenWidth
                        });
                    }

                    if (!_updated)
                        continue;

                    try
                    {
                        await RenderBuffer();
                    }
                    catch (Exception ex)
                    {
                        Fail(ex);
                        return;
                    }

                    _updated = false;
                }
            });

            // Start key capture
            await Task.Run(async () =>
            {
                while (true)
                {
                    var input = await GetInputWithTimeoutAsync();
                    if (input == null)
                        break;

                    // Skip ignored mouse events (clicks, etc.)
                    if (input is IgnoredMouseMsg)
                        continue;

                    _messageChannel.Writer.TryWrite(input);
                }
            });

            await renderTask;
        }

        /// <summary>
        /// Stops the program after its Update or View throws
        /// </summary>
        private void Fail(Exception ex)
        {
            _error ??= ex;
            _quitting = true;
            _commands.StopAll();
            _messageChannel.Writer.TryComplete();
            _commandChannel.Writer.TryComplete();
        }

        private async Task<CalliopeMsg?> GetInputWithTimeoutAsync()
        {
            while (!_quitting)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    // Check for mouse escape sequence: ESC [ <
                    if (_opts.EnableMouse && key.Key == ConsoleKey.Escape)
                    {
                        var mouseMsg = await TryParseMouseSequenceAsync();
                        if (mouseMsg != null)
                            return mouseMsg;

                        // If not a mouse sequence, return the ESC as a key press
                        return new KeyPressMsg
                        {
                            Key = key.Key,
                            KeyChar = key.KeyChar,
                            Modifiers = key.Modifiers
                        };
                    }

                    return new KeyPressMsg
                    {
                        Key = key.Key,
                        KeyChar = key.KeyChar,
                        Modifiers = key.Modifiers
                    };
                }
                // Check for user input at minimum 60 times per second
                var delay = (int)Math.Min(
                    _framerate.TotalMilliseconds,
                    MIN_KEY_RATE
                );
                await Task.Delay(delay);
            }
            return null;
        }

        // Sentinel message to indicate a mouse event was consumed but should be ignored
        private class IgnoredMouseMsg : CalliopeMsg { }
        private static readonly IgnoredMouseMsg _ignoredMouseMsg = new();

        private Task<CalliopeMsg?> TryParseMouseSequenceAsync()
        {
            // SGR mouse format: ESC [ < button ; x ; y M/m
            // We've already consumed ESC. Escape sequences arrive as a burst,
            // so if the next char isn't immediately available, it's just ESC.
            if (!Console.KeyAvailable)
                return Task.FromResult<CalliopeMsg?>(null); // Just an ESC key press

            var next = Console.ReadKey(true);
            if (next.KeyChar != '[')
            {
                // Not a CSI sequence
                return Task.FromResult<CalliopeMsg?>(null);
            }

            // At this point we're committed to consuming a CSI sequence
            // Read until we hit a letter (the terminator)
            var sb = new StringBuilder();
            var maxChars = 30;

            while (Console.KeyAvailable && sb.Length < maxChars)
            {
                var c = Console.ReadKey(true);

                // CSI sequences end with a letter
                if (char.IsLetter(c.KeyChar))
                {
                    // Check if this is SGR mouse format: < button ; x ; y M/m
                    var seq = sb.ToString();
                    if (seq.StartsWith('<') && (c.KeyChar == 'M' || c.KeyChar == 'm'))
                    {
                        var result = ParseSgrMouse(seq.Substring(1));
                        return Task.FromResult<CalliopeMsg?>(result);
                    }
                    // Some other CSI sequence - consumed and ignored
                    return Task.FromResult<CalliopeMsg?>(_ignoredMouseMsg);
                }

                sb.Append(c.KeyChar);
            }

            // Incomplete sequence - consumed what we could
            return Task.FromResult<CalliopeMsg?>(_ignoredMouseMsg);
        }

        private CalliopeMsg ParseSgrMouse(string data)
        {
            // Parse button;x;y
            var parts = data.Split(';');
            if (parts.Length != 3)
                return _ignoredMouseMsg;

            if (!int.TryParse(parts[0], out var button) ||
                !int.TryParse(parts[1], out var x) ||
                !int.TryParse(parts[2], out var y))
                return _ignoredMouseMsg;

            // Scroll wheel has bit 6 set (64). Bit 0 is direction (0=up, 1=down)
            // Bits 2-4 are modifiers (shift=4, meta=8, ctrl=16)
            if ((button & 64) != 0)
            {
                var direction = (button & 1) == 0 ? MouseWheelDirection.Up : MouseWheelDirection.Down;
                return new MouseWheelMsg { Direction = direction, X = x, Y = y };
            }

            // Click events are ignored - use Ctrl+G to toggle mouse for text selection
            return _ignoredMouseMsg;
        }

        private async Task MessageLoop()
        {
            while (await _messageChannel.Reader.WaitToReadAsync())
            {
                while (_messageChannel.Reader.TryRead(out var msg))
                {
                    if (msg is QuitMsg)
                    {
                        _quitting = true;
                    }

                    if (msg is MouseToggleMsg mtm)
                    {
                        _opts.EnableMouse = mtm.Enable;
                        _opts.StandardOut!.Write(mtm.Enable
                            ? AnsiConstants.EnableMouseTracking
                            : AnsiConstants.DisableMouseTracking);
                        continue;
                    }

                    if (msg is BatchMsg batch)
                    {
                        foreach (var batchCommand in batch.Commands)
                        {
                            _commandChannel.Writer.TryWrite(batchCommand);
                        }
                    }
                    else
                    {
                        CalliopeCmd? cmd;
                        try
                        {
                            cmd = await UpdateProgram(msg);
                        }
                        catch (Exception ex)
                        {
                            Fail(ex);
                            return;
                        }

                        if (cmd != null)
                            _commandChannel.Writer.TryWrite(cmd);
                    }

                    if (_quitting)
                    {
                        // tell the command loop to shutdown
                        _commandChannel.Writer.TryWrite(new ShutdownCmd());
                        return;
                    }
                }
            }
        }

        private async Task CommandLoop()
        {
            // Process commands as they are added to the channel
            while (await _commandChannel.Reader.WaitToReadAsync())
            {
                while (_commandChannel.Reader.TryRead(out var cmd))
                {
                    if (cmd is ShutdownCmd)
                    {
                        _commands.StopAll();
                        return;
                    }

                    _ = _commands.Start(cmd);
                }
            }
        }

        private async Task<CalliopeCmd?> UpdateProgram(CalliopeMsg msg)
        {
            await _renderLock.WaitAsync();
            try
            {
                var cmd = _program.Update(msg);

                _updated = true;

                return cmd;
            }
            finally
            {
                _renderLock.Release();
            }
        }

        private async Task RenderBuffer()
        {
            string?[] renderLines;
            await _renderLock.WaitAsync();
            try
            {
                var view = _program.View();
                using var reader = new StringReader(view);
                List<string> lines = [];
                var line = await reader.ReadLineAsync();
                while (line != null)
                {
                    lines.Add(line);
                    line = await reader.ReadLineAsync();
                }
                renderLines = [.. lines];
            }
            finally
            {
                _renderLock.Release();
            }

            if (renderLines.Length > _screenHeight)
            {
                renderLines = renderLines[^_screenHeight..];
            }

            var sb = new StringBuilder();

            bool[] skipLines = new bool[_linesRendered];
            if (_flush && _opts.Fullscreen)
            {
                sb.Append(AnsiConstants.ClearDisplay);
                Console.SetCursorPosition(0, 0);
            }
            else if (_linesRendered > 0)
            {
                for (int i = _linesRendered - 1; i >= 0; i--)
                {
                    if (_flush || i >= renderLines.Length || string.IsNullOrEmpty(renderLines[i]))
                    {
                        sb.Append(AnsiConstants.ClearLine);
                    }
                    else if (_previousRender![i] == renderLines[i])
                    {
                        skipLines[i] = true;
                    }

                    if (i != 0)
                    {
                        sb.Append(AnsiConstants.CursorUp);
                    }
                }
            }

            for (int i = 0; i < renderLines.Length; i++)
            {
                if (i != 0)
                    sb.AppendLine();
                else
                    sb.Append("\x1b[" + _screenWidth + "D");

                if (i >= _linesRendered || !skipLines[i])
                {
                    sb.Append(AnsiTextHelper.RenderLine(renderLines[i], _screenWidth));
                }
            }
            _linesRendered = renderLines.Length;

            _flush = false;
            _opts.StandardOut!.Write(sb.ToString());
            _previousRender = renderLines!;
        }

        /// <summary>
        /// Special internal cmd class that tells the command loop to shutdown
        /// </summary>
        private class ShutdownCmd : CalliopeCmd
        {
            public ShutdownCmd() : base((Func<CalliopeMsg>)(() => null!))
            {
            }
        }
    }
}
