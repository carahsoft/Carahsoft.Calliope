using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Messages
{
    /// <summary>
    /// CalliopeCmd is an alias for Func&lt;Task&lt;<see cref="CalliopeMsg"/>&gt;&gt;
    /// A CalliopeCmd is an async function that takes no parameters and returns a
    /// <see cref="CalliopeMsg"/> object.
    /// </summary>
    public class CalliopeCmd
    {
        private readonly Func<Task<CalliopeMsg>> _cmd;

        public CalliopeCmd(Func<CalliopeMsg> cmd)
        {
            _cmd = SyncToAsync(cmd);
        }

        public CalliopeCmd(Func<Task<CalliopeMsg>> cmd)
        {
            _cmd = cmd;
        }

        private Func<Task<CalliopeMsg>> SyncToAsync(Func<CalliopeMsg> cmd)
        {
            return () => Task.FromResult(cmd());
        }

        public Func<Task<CalliopeMsg>> CommandFunc => _cmd;

        public static implicit operator CalliopeCmd(Func<CalliopeMsg> cmd)
        {
            return new CalliopeCmd(cmd);
        }

        public static implicit operator CalliopeCmd(Func<Task<CalliopeMsg>> cmd)
        {
            return new CalliopeCmd(cmd);
        }

        public static implicit operator Func<Task<CalliopeMsg>>(CalliopeCmd cmd)
        {
            return cmd.CommandFunc;
        }

        public static CalliopeCmd Make(Func<Task<CalliopeMsg>> cmd) => new CalliopeCmd(cmd);
        public static CalliopeCmd Make(Func<CalliopeMsg> cmd) => new CalliopeCmd(cmd);

        public static CalliopeCmd Quit => Make(() => new QuitMsg());

        public static CalliopeCmd Combine(params CalliopeCmd[] cmds)
        {
            return Make(() => new BatchMsg(cmds));
        }
    }
}
