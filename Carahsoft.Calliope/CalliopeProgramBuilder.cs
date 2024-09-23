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

        public CalliopeProgramBuilder(TProgram program)
        {
            _program = program;
        }

        public CalliopeProgramBuilder<TProgram> Fullscreen()
        {
            _fullscreen = true;
            return this;
        }

        public ProgramRunner<TProgram> Build()
        {
            return new ProgramRunner<TProgram>(_program, new ProgramOptions
            {
                Fullscreen = _fullscreen
            });
        }

        public Task<TProgram> RunAsync()
        {
            var pr = Build();
            return pr.RunAsync();
        }
    }
}
