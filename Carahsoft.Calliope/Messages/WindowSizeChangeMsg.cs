using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Messages
{
    public class WindowSizeChangeMsg : CalliopeMsg
    {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
    }
}
