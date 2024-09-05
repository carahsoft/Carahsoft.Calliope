using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Messages
{
    public class KeyPressMsg : CalliopeMsg
    {
        public ConsoleKey Key { get; set; }
        public char KeyChar { get; set; }
        public ConsoleModifiers Modifiers { get; set; }
    }
}
