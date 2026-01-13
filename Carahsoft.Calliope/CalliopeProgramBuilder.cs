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
        private bool _enableMouse;
        private int _framerate;
        private TextWriter? _stdOut;

        public CalliopeProgramBuilder(TProgram program)
        {
            _program = program;
        }

        /// <summary>
        /// Enables the alt screen buffer and renders a full screen console application
        /// </summary>
        /// <returns></returns>
        public CalliopeProgramBuilder<TProgram> Fullscreen()
        {
            _fullscreen = true;
            return this;
        }

        /// <summary>
        /// If your application needs to write to StandardOut, you can redirect StandardOut
        /// to a custom TextWriter and pass StandardOut to this function to allow Calliope
        /// to still take over the console screen.
        /// </summary>
        /// <param name="stdOut"></param>
        /// <returns></returns>
        public CalliopeProgramBuilder<TProgram> SetOut(TextWriter? stdOut)
        {
            _stdOut = stdOut;
            return this;
        }

        /// <summary>
        /// Sets the framerate, in hz, of the application. Default is 60hz.
        /// </summary>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public CalliopeProgramBuilder<TProgram> Framerate(int framerate)
        {
            _framerate = framerate;
            return this;
        }

        /// <summary>
        /// Enables mouse tracking for mouse wheel scroll events
        /// </summary>
        /// <returns></returns>
        public CalliopeProgramBuilder<TProgram> EnableMouse()
        {
            _enableMouse = true;
            return this;
        }

        /// <summary>
        /// Build the <see cref="ProgramRunner{TProgram}"/>. The <see cref="ProgramRunner{TProgram}"/>
        /// reference can be used to send messages to the executing program using the
        /// <see cref="ProgramRunner{TProgram}.SendAsync(CalliopeMsg)"/> method.
        /// </summary>
        /// <returns></returns>
        public ProgramRunner<TProgram> Build()
        {
            var opts = new ProgramOptions
            {
                Fullscreen = _fullscreen,
                EnableMouse = _enableMouse,
            };

            if (_framerate > 0)
                opts.Framerate = _framerate;
            if (_stdOut != null)
                opts.StandardOut = _stdOut;
            return new ProgramRunner<TProgram>(_program, opts);
        }

        /// <inheritdoc cref="ProgramRunner{TProgram}.RunAsync"/>
        public Task<TProgram> RunAsync()
        {
            var pr = Build();
            return pr.RunAsync();
        }
    }
}
