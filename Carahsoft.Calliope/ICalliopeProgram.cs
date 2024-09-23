using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public interface ICalliopeProgram
    {
        public CalliopeCmd? Update(CalliopeMsg msg);
        public CalliopeCmd? Init();
        public string View();
    }
}
