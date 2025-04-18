﻿using Carahsoft.Calliope.Constants;
using System.ComponentModel.DataAnnotations;
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

        public ProgramRunner(TProgram program, ProgramOptions opts)
        {
            _program = program;
            _opts = opts;

            if (_opts.Framerate <= 0 || _opts.Framerate > 60)
                _opts.Framerate = 60;

            _framerate = TimeSpan.FromSeconds(1) / _opts.Framerate;
            _renderTimer = new PeriodicTimer(_framerate);
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

                        await _messageChannel.Writer.WriteAsync(new WindowSizeChangeMsg
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
                        await _messageChannel.Writer.WriteAsync(new ErrorMsg(ex));
                        // TODO: do we need to panic here?
                        Console.Error.WriteLine(ex);
                        Environment.Exit(1);
                    }

                    _updated = false;
                }
            });

            // Start key capture
            await Task.Run(async () =>
            {
                while (true)
                {
                    var key = await GetKeyWithTimeoutAsync();
                    if (key == null)
                        break;

                    await _messageChannel.Writer.WriteAsync(new KeyPressMsg
                    {
                        Key = key.Value.Key,
                        KeyChar = key.Value.KeyChar,
                        Modifiers = key.Value.Modifiers
                    });
                }
            });

            await renderTask;

            Console.TreatControlCAsInput = ctrlCRestore;
            Console.OutputEncoding = encodingRestore;
            _opts.StandardOut.Write(AnsiConstants.ShowCursor);
            if (_opts.Fullscreen)
                _opts.StandardOut.Write(AnsiConstants.DisableAltScreenBuffer);

            // Return the final state of the program to the caller
            return _program;
        }

        private async Task<ConsoleKeyInfo?> GetKeyWithTimeoutAsync()
        {
            while (!_quitting)
            {
                if (Console.KeyAvailable)
                {
                    return Console.ReadKey(true);
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

                    if (msg is BatchMsg batch)
                    {
                        foreach (var batchCommand in batch.Commands)
                        {
                            await _commandChannel.Writer.WriteAsync(batchCommand);
                        }
                    }
                    else
                    {
                        var cmd = await UpdateProgram(msg);

                        if (cmd != null)
                            await _commandChannel.Writer.WriteAsync(cmd);
                    }

                    if (_quitting)
                    {
                        // tell the command loop to shutdown
                        await _commandChannel.Writer.WriteAsync(new ShutdownCmd());
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
                        return;

                    _ = Task.Run(async () =>
                    {
                        CalliopeMsg msg;
                        try
                        {
                            msg = await cmd.CommandFunc();
                        }
                        catch (Exception ex)
                        {
                            msg = new ErrorMsg(ex);
                        }
                        await _messageChannel.Writer.WriteAsync(msg);
                    });
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
                    var renderLine = AnsiTextHelper.TruncateLineToLength(renderLines[i], _screenWidth);
                    sb.Append(renderLine + AnsiConstants.ClearRight);
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
