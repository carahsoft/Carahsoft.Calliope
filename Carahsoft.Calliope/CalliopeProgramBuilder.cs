using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class CalliopeProgramBuilder<TModel>
    {
        private readonly ICalliopeProgram<TModel> _program;

        private bool _fullscreen;

        public CalliopeProgramBuilder(ICalliopeProgram<TModel> program)
        {
            _program = program;
        }

        public CalliopeProgramBuilder<TModel> Fullscreen()
        {
            _fullscreen = true;
            return this;
        }

        //public CalliopeProgramBuilder<TModel> 

        public ProgramRunner<TModel> Build()
        {
            return new ProgramRunner<TModel>(_program, new ProgramOptions
            {
                Fullscreen = _fullscreen
            });
        }

        public Task<TModel> RunAsync()
        {
            var pr = Build();
            return pr.RunAsync();
        }
    }
}
