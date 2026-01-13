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
        public const string ClearDisplay = "\x1b[2J";

        // Mouse tracking - SGR extended mode (1006) for better coordinates
        public const string EnableMouseTracking = "\x1b[?1000h\x1b[?1006h";
        public const string DisableMouseTracking = "\x1b[?1000l\x1b[?1006l";
    }
}
