using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class CalliopeProgram<TModel>
    {
        private readonly ICalliopeProgram<TModel> _program;

        private bool _fullscreen;

        public CalliopeProgram(ICalliopeProgram<TModel> program)
        {
            _program = program;
        }

        public CalliopeProgram<TModel> Fullscreen()
        {
            _fullscreen = true;
            return this;
        }

        public Task RunAsync()
        {
            var pr = new ProgramRunner<TModel>(_program, new ProgramOptions
            {
                Fullscreen = _fullscreen
            });

            return pr.RunAsync();
        }
    }
}
