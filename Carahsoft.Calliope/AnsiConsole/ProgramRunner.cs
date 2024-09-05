using Carahsoft.Calliope.Constants;
using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    internal class ProgramRunner<TModel>
    {
        private readonly ICalliopeProgram<TModel> _program;
        private readonly ProgramOptions _opts;
        private readonly PeriodicTimer _timer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private TModel _state;
        private int _linesRendered = 0;
        private bool _quitting = false;

        public ProgramRunner(ICalliopeProgram<TModel> program, ProgramOptions opts)
        {
            _program = program;
            _opts = opts;
            _timer = new PeriodicTimer(TimeSpan.FromSeconds((double)1 / _opts.Framerate));
        }

        public async Task RunAsync()
        {
            var ctrlCRestore = Console.TreatControlCAsInput;
            Console.TreatControlCAsInput = true;

            // TODO: handle init cmd
            (_state, _) = _program.Init();

            // Render screen in background task while primary thread waits on user input
            var renderTask = Task.Run(async () =>
            {
                while (await _timer.WaitForNextTickAsync())
                {
                    if (_quitting)
                        break;

                    await RenderBuffer();
                }
            });

            while (true)
            {
                var key = Console.ReadKey(true);
                var (newState, cmd) = _program.Update(_state, new KeyPressMsg
                {
                    Key = key.Key,
                    KeyChar = key.KeyChar,
                    Modifiers = key.Modifiers
                });

                if (cmd is QuitMsg msg)
                {
                    _quitting = true;
                    break;
                }
                _state = newState;
            }

            Console.TreatControlCAsInput = ctrlCRestore;
        }

        public async Task RenderBuffer()
        {
            // TODO: Check if we need a render and skip render if not
            var renderLines = _program.View(_state).Split('\n');
            var sb = new StringBuilder();

            if (_linesRendered > 0)
            {
                for (int i = _linesRendered - 1; i >= 0; i--)
                {
                    sb.Append(AnsiConstants.CursorUp);
                    sb.Append(AnsiConstants.ClearLine);
                }
            }

            foreach (var line in renderLines)
            {
                // TODO: length overflow check
                sb.AppendLine(line);
            }
            _linesRendered = renderLines.Length;

            Console.Write(sb.ToString());
        }
    }
}
