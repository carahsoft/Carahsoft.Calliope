namespace Carahsoft.Calliope.AnsiConsole
{
    /// <summary>
    /// Runs a program's commands in the background, delivering their messages
    /// and handling cancellation: every command is cancelled by
    /// <see cref="StopAll"/>, and starting a keyed command cancels the running
    /// one with the same key.
    /// </summary>
    internal sealed class CommandRunner
    {
        private readonly Action<CalliopeMsg> _deliver;
        private readonly CancellationTokenSource _stopping = new();
        private readonly Dictionary<string, CancellationTokenSource> _keyed = new();

        public CommandRunner(Action<CalliopeMsg> deliver)
        {
            _deliver = deliver;
        }

        /// <summary>
        /// Starts the command, returning a task that completes when it does
        /// </summary>
        public Task Start(CalliopeCmd cmd)
        {
            if (cmd is CalliopeCmd.CancelCmd cancel)
            {
                Cancel(cancel.TargetKey);
                return Task.CompletedTask;
            }

            if (_stopping.IsCancellationRequested)
                return Task.CompletedTask;

            CancellationTokenSource? keyed = null;
            var token = _stopping.Token;
            if (cmd.Key != null)
            {
                keyed = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                token = keyed.Token;
                lock (_keyed)
                {
                    if (_keyed.TryGetValue(cmd.Key, out var previous))
                        previous.Cancel();
                    _keyed[cmd.Key] = keyed;
                }
            }

            return Task.Run(async () =>
            {
                void Send(CalliopeMsg msg)
                {
                    if (msg != null && !token.IsCancellationRequested)
                        _deliver(msg);
                }

                try
                {
                    await cmd.RunAsync(token, Send);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    Send(new ErrorMsg(ex));
                }
                finally
                {
                    if (keyed != null)
                    {
                        lock (_keyed)
                        {
                            if (_keyed.TryGetValue(cmd.Key!, out var current) && current == keyed)
                                _keyed.Remove(cmd.Key!);
                        }
                        keyed.Dispose();
                    }
                }
            });
        }

        /// <summary>
        /// Cancels the running command with the given key
        /// </summary>
        public void Cancel(string key)
        {
            lock (_keyed)
            {
                if (_keyed.Remove(key, out var running))
                    running.Cancel();
            }
        }

        /// <summary>
        /// Cancels every running command, and ignores any started afterwards
        /// </summary>
        public void StopAll() => _stopping.Cancel();
    }
}
