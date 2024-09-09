using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    public class ProgramOptions
    {
        public int ConsoleWidth { get; set; }
        public int ConsoleHeight { get; set; }
        public int Framerate { get; set; } = 60;
        public bool Fullscreen { get; set; }
    }
}
