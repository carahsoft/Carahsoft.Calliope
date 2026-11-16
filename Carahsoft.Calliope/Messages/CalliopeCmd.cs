using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    /// <summary>
    /// CalliopeCmd is an alias for Func&lt;Task&lt;<see cref="CalliopeMsg"/>&gt;&gt;
    /// A CalliopeCmd is an async function that takes no parameters and returns a
    /// <see cref="CalliopeMsg"/> object.
    /// </summary>
    /// <remarks>
    /// Commands made with a <see cref="CancellationToken"/> parameter are
    /// canceled when the program quits. A command with a <see cref="Key"/>
    /// (see <see cref="WithKey"/>) also cancels the previous command with the
    /// same key when it starts, and can be canceled with <see cref="Cancel"/>.
    /// The message of a canceled command is never delivered.
    /// </remarks>
    public class CalliopeCmd
    {
        // Sends any number of messages until it completes or is canceled
        private readonly Func<CancellationToken, Action<CalliopeMsg>, Task> _run;

        public CalliopeCmd(Func<CalliopeMsg> cmd)
            : this(_ => Task.FromResult(cmd()))
        {
        }

        public CalliopeCmd(Func<Task<CalliopeMsg>> cmd)
            : this(_ => cmd())
        {
        }

        public CalliopeCmd(Func<CancellationToken, Task<CalliopeMsg>> cmd)
        {
            _run = async (token, send) => send(await cmd(token));
        }

        private CalliopeCmd(Func<CancellationToken, Action<CalliopeMsg>, Task> run, string? key)
        {
            _run = run;
            Key = key;
        }

        /// <summary>
        /// Starting a command with a key cancels the running command with the
        /// same key, e.g. so a new search replaces the one in progress
        /// </summary>
        public string? Key { get; }

        /// <summary>
        /// Runs the command without cancellation, returning its first message
        /// </summary>
        public Func<Task<CalliopeMsg>> CommandFunc => FirstMessageAsync;

        /// <summary>
        /// Runs the command, passing each message it produces to
        /// <paramref name="send"/> until it completes or the token is canceled
        /// </summary>
        public Task RunAsync(CancellationToken cancellationToken, Action<CalliopeMsg> send) =>
            _run(cancellationToken, send);

        /// <summary>
        /// The same command with a key: starting it cancels any running
        /// command with the same key
        /// </summary>
        public CalliopeCmd WithKey(string key) => new(_run, key);

        private async Task<CalliopeMsg> FirstMessageAsync()
        {
            using var cts = new CancellationTokenSource();
            var first = new TaskCompletionSource<CalliopeMsg>(TaskCreationOptions.RunContinuationsAsynchronously);
            var run = _run(cts.Token, msg =>
            {
                if (first.TrySetResult(msg))
                    cts.Cancel();
            });

            try
            {
                await run;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }

            first.TrySetException(new InvalidOperationException("The command completed without a message"));
            return await first.Task;
        }

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
        public static CalliopeCmd Make(Func<CancellationToken, Task<CalliopeMsg>> cmd) => new CalliopeCmd(cmd);
        public static CalliopeCmd Make(Func<CalliopeMsg> cmd) => new CalliopeCmd(cmd);
        public static CalliopeCmd Make(CalliopeMsg msg) => new CalliopeCmd(() => msg);

        public static readonly CalliopeCmd Quit = Make(() => new QuitMsg());

        public static CalliopeCmd Combine(params CalliopeCmd?[] cmds)
        {
            return Make(() => new BatchMsg(cmds));
        }

        /// <summary>
        /// Sends the message after a delay. Canceled if the program quits first.
        /// </summary>
        public static CalliopeCmd Delay(TimeSpan delay, CalliopeMsg msg) =>
            Make(async token =>
            {
                await Task.Delay(delay, token);
                return msg;
            });

        /// <summary>
        /// Sends a message every <paramref name="interval"/> until the program
        /// quits. Give it a key with <see cref="WithKey"/> to stop it with
        /// <see cref="Cancel"/>, or to restart it.
        /// </summary>
        public static CalliopeCmd Every(TimeSpan interval, Func<CalliopeMsg> msg) =>
            Stream(async (token, send) =>
            {
                using var timer = new PeriodicTimer(interval);
                while (await timer.WaitForNextTickAsync(token))
                    send(msg());
            });

        /// <summary>
        /// A command that sends any number of messages, e.g. lines from a log
        /// stream, by calling the function it's given, until it completes or is
        /// canceled (when the program quits, or by its key)
        /// </summary>
        public static CalliopeCmd Stream(Func<CancellationToken, Action<CalliopeMsg>, Task> run) => new(run, null);

        /// <summary>
        /// Cancels the running command with the given key, if there is one
        /// </summary>
        public static CalliopeCmd Cancel(string key) => new CancelCmd(key);

        internal sealed class CancelCmd(string key) : CalliopeCmd((_, _) => Task.CompletedTask, null)
        {
            public string TargetKey { get; } = key;
        }
    }
}
