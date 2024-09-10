using Carahsoft.Calliope.Constants;
using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    public class ProgramRunner<TModel>
    {
        private readonly ICalliopeProgram<TModel> _program;
        private readonly ProgramOptions _opts;
        private readonly TimeSpan _framerate;
        private readonly PeriodicTimer _renderTimer;

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

        private TModel _state;
        private int _linesRendered = 0;
        private bool _updated = true;
        private bool _quitting = false;
        private string[] _previousRender;

        public ProgramRunner(ICalliopeProgram<TModel> program, ProgramOptions opts)
        {
            _program = program;
            _opts = opts;
            _framerate = TimeSpan.FromSeconds((double)1 / _opts.Framerate);
            _renderTimer = new PeriodicTimer(_framerate);
        }

        public async Task SendAsync(CalliopeMsg msg)
        {
            await _messageChannel.Writer.WriteAsync(msg);
        }

        public async Task<TModel> RunAsync()
        {
            var ctrlCRestore = Console.TreatControlCAsInput;
            Console.TreatControlCAsInput = true;
            Console.Write(AnsiConstants.HideCursor);

            var (state, cmd) = _program.Init();
            _state = state;

            if (cmd != null)
                await _commandChannel.Writer.WriteAsync(cmd);

            // Render screen in background task while primary thread waits on user input
            var renderTask = Task.Run(async () =>
            {
                while (await _renderTimer.WaitForNextTickAsync())
                {
                    if (_quitting)
                        break;

                    if (!_updated)
                        continue;

                    try
                    {
                        await RenderBuffer();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                        Environment.Exit(1);
                    }
                    _updated = false;
                }
            });

            var messageLoopTask = Task.Run(MessageLoop);
            var commandLoopTask = Task.Run(CommandLoop);

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

                Console.TreatControlCAsInput = ctrlCRestore;
                Console.Write(AnsiConstants.ShowCursor);
            });

            // Return the final state of the program to the caller
            return _state;
        }

        private async Task<ConsoleKeyInfo?> GetKeyWithTimeoutAsync()
        {
            while (!_quitting)
            {
                if (Console.KeyAvailable)
                {
                    return Console.ReadKey(true);
                }
                // Delay 1 frame before checking again
                await Task.Delay(_framerate);
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

                    var cmd = UpdateProgram(msg);
                    if (cmd != null)
                        await _commandChannel.Writer.WriteAsync(cmd);
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
                    var msg = await cmd.CommandFunc();
                    await _messageChannel.Writer.WriteAsync(msg);
                }
            }
        }

        private CalliopeCmd? UpdateProgram(CalliopeMsg msg)
        {
            // TODO: async wait
            lock (this)
            {
                var (newState, cmd) = _program.Update(_state, msg);
                _state = newState;

                _updated = true;

                return cmd;
            }
        }

        private async Task RenderBuffer()
        {
            // TODO: Check if we need a render and skip render if not
            var renderLines = _program.View(_state).Split('\n');
            var sb = new StringBuilder();

            bool[] skipLines = new bool[_linesRendered];
            if (_linesRendered > 0)
            {
                //Console.SetCursorPosition(0, 0);
                for (int i = _linesRendered - 1; i >= 0; i--)
                {
                    sb.Append(AnsiConstants.CursorUp);

                    if (_previousRender[i] != renderLines[i])
                    {
                        sb.Append(AnsiConstants.ClearLine);
                    }
                    else
                    {
                        skipLines[i] = true;
                    }
                }
            }

            for (int i = 0; i < renderLines.Length; i++)
            {
                // TODO: length overflow check
                if (i < _linesRendered && skipLines[i])
                    sb.AppendLine();
                else
                    sb.AppendLine(renderLines[i]);
            }
            _linesRendered = renderLines.Length;

            Console.Write(sb.ToString());
            _previousRender = renderLines;
        }
    }
}
