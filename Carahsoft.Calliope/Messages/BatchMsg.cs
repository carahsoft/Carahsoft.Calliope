using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class BatchMsg : CalliopeMsg
    {
        private readonly List<CalliopeCmd> _commands;

        public BatchMsg(params CalliopeCmd?[] commands)
        {
            _commands = commands.Where(x => x != null).ToList()!;
        }

        public List<CalliopeCmd> Commands => _commands;
    }
}
