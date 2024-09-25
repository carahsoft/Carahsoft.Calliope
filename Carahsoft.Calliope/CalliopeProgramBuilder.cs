using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class CalliopeProgramBuilder<TProgram>
        where TProgram : ICalliopeProgram
    {
        private readonly TProgram _program;

        private bool _fullscreen;
        private int _framerate;
        private TextWriter? _stdOut;

        public CalliopeProgramBuilder(TProgram program)
        {
            _program = program;
        }

        public CalliopeProgramBuilder<TProgram> Fullscreen()
        {
            _fullscreen = true;
            return this;
        }

        public CalliopeProgramBuilder<TProgram> SetOut(TextWriter? stdOut)
        {
            _stdOut = stdOut;
            return this;
        }

        public CalliopeProgramBuilder<TProgram> Framerate(int framerate)
        {
            _framerate = framerate;
            return this;
        }

        public ProgramRunner<TProgram> Build()
        {
            var opts = new ProgramOptions
            {
                Fullscreen = _fullscreen,
            };

            if (_framerate > 0)
                opts.Framerate = _framerate;
            if (_stdOut != null)
                opts.StandardOut = _stdOut;
            return new ProgramRunner<TProgram>(_program, opts);
        }

        public Task<TProgram> RunAsync()
        {
            var pr = Build();
            return pr.RunAsync();
        }
    }
}
