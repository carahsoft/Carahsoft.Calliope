using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public interface ICalliopeProgram<TModel>
    {
        public (TModel, CalliopeCmd?) Update(TModel state, CalliopeMsg msg);
        public (TModel, CalliopeCmd?) Init();
        public string View(TModel state);
    }
}
