using System.Collections.Concurrent;
using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Tests;

public class CommandTests
{
    private class Msg(string name) : CalliopeMsg
    {
        public string Name { get; } = name;
    }

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Delay_sends_the_message_after_the_delay()
    {
        var msg = new Msg("a");
        var started = DateTime.UtcNow;
        Assert.Same(msg, await CalliopeCmd.Delay(TimeSpan.FromMilliseconds(50), msg).CommandFunc());
        Assert.True(DateTime.UtcNow - started >= TimeSpan.FromMilliseconds(45));
    }

    [Fact]
    public async Task Every_sends_repeatedly_until_canceled()
    {
        var count = 0;
        using var cts = new CancellationTokenSource();
        var run = CalliopeCmd.Every(TimeSpan.FromMilliseconds(10), () => new Msg("tick"))
            .RunAsync(cts.Token, _ =>
            {
                if (Interlocked.Increment(ref count) == 3)
                    cts.Cancel();
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run.WaitAsync(Timeout));
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Existing_command_types_still_work()
    {
        var msg = new Msg("a");
        Assert.Same(msg, await CalliopeCmd.Make(msg).CommandFunc());
        Assert.Same(msg, await CalliopeCmd.Make(async () => { await Task.Yield(); return (CalliopeMsg)msg; }).CommandFunc());
        Assert.Same(msg, await new CalliopeCmd(() => msg).CommandFunc());
    }

    [Fact]
    public async Task Runner_delivers_messages_and_errors()
    {
        var delivered = new ConcurrentQueue<CalliopeMsg>();
        var runner = new CommandRunner(delivered.Enqueue);

        await runner.Start(CalliopeCmd.Make(new Msg("a")));
        await runner.Start(CalliopeCmd.Make((Func<CalliopeMsg>)(() => throw new InvalidOperationException("boom"))));

        Assert.Equal("a", Assert.IsType<Msg>(delivered.ToArray()[0]).Name);
        Assert.Equal("boom", Assert.IsType<ErrorMsg>(delivered.ToArray()[1]).Exception.Message);
    }

    [Fact]
    public async Task Stopping_cancels_running_commands_and_drops_their_messages()
    {
        var delivered = new ConcurrentQueue<CalliopeMsg>();
        var runner = new CommandRunner(delivered.Enqueue);
        var canceled = false;

        var running = runner.Start(CalliopeCmd.Make(async token =>
        {
            try
            {
                await Task.Delay(Timeout, token);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                throw;
            }
            return new Msg("late");
        }));

        runner.StopAll();
        await running.WaitAsync(Timeout);

        Assert.True(canceled);
        Assert.Empty(delivered);

        // Commands started after stopping don't run
        await runner.Start(CalliopeCmd.Make(new Msg("after")));
        Assert.Empty(delivered);
    }

    [Fact]
    public async Task Starting_a_keyed_command_cancels_the_previous_one()
    {
        var delivered = new ConcurrentQueue<CalliopeMsg>();
        var runner = new CommandRunner(delivered.Enqueue);
        var firstCanRespond = new TaskCompletionSource();

        // A slow request that ignores cancellation, like an API call that responds anyway
        var first = runner.Start(CalliopeCmd.Make(async () =>
        {
            await firstCanRespond.Task;
            return (CalliopeMsg)new Msg("stale");
        }).WithKey("search"));

        var second = runner.Start(CalliopeCmd.Make(new Msg("fresh")).WithKey("search"));
        firstCanRespond.SetResult();
        await Task.WhenAll(first, second).WaitAsync(Timeout);

        Assert.Equal(["fresh"], delivered.Select(m => ((Msg)m).Name));
    }

    [Fact]
    public async Task Different_keys_dont_cancel_each_other()
    {
        var delivered = new ConcurrentQueue<CalliopeMsg>();
        var runner = new CommandRunner(delivered.Enqueue);

        await Task.WhenAll(
            runner.Start(CalliopeCmd.Delay(TimeSpan.FromMilliseconds(20), new Msg("a")).WithKey("a")),
            runner.Start(CalliopeCmd.Delay(TimeSpan.FromMilliseconds(20), new Msg("b")).WithKey("b"))).WaitAsync(Timeout);

        Assert.Equal(["a", "b"], delivered.Select(m => ((Msg)m).Name).Order());
    }

    [Fact]
    public async Task Cancel_stops_a_keyed_command()
    {
        var delivered = new ConcurrentQueue<CalliopeMsg>();
        var runner = new CommandRunner(delivered.Enqueue);

        var ticking = runner.Start(CalliopeCmd.Every(TimeSpan.FromMilliseconds(10), () => new Msg("tick")).WithKey("ticks"));
        await Task.Delay(100);
        await runner.Start(CalliopeCmd.Cancel("ticks"));
        await ticking.WaitAsync(Timeout);

        var count = delivered.Count;
        Assert.True(count > 0);
        await Task.Delay(50);
        Assert.Equal(count, delivered.Count);
    }
}
