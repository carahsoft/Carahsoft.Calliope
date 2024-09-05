using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carahsoft.Calliope.Messages;

namespace Carahsoft.Calliope
{
    public interface ICalliopeProgram<TModel>
    {
        public (TModel, CalliopeMsg?) Update(TModel state, CalliopeMsg msg);
        public (TModel, CalliopeMsg?) Init();
        public string View(TModel state);
    }
}
