using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Constants
{
    public static class AnsiConstants
    {
        public const string EnableAltScreenBuffer = "\x1b[?1049h";
        public const string DisableAltScreenBuffer = "\x1b[?1049l";
        public const string CursorUp = "\x1b[A";
        public const string ClearLine = "\x1b[2K";
        public const string ClearRight = "\x1b[0K";
        public const string ShowCursor = "\x1b[?25h";
        public const string HideCursor = "\x1b[?25l";
    }
}
