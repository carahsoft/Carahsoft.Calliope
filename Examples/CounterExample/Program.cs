using Carahsoft.Calliope;

await Calliope.NewProgram(new CounterProgram()).RunAsync();

public record CounterState
{
    public int Count { get; init; }
}

public class CounterProgram : ICalliopeProgram<CounterState>
{
    public (CounterState, CalliopeCmd?) Init()
    {
        return (new(), null);
    }

    public (CounterState, CalliopeCmd?) Update(CounterState state, CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
            {
                // If the user pressed ctrl+c, returning the Quit command exits the program
                return (state, CalliopeCmd.Quit);
            }

            if (kpm.Key == ConsoleKey.RightArrow)
            {
                // Right arrow means increment the count!
                // null command means we don't need any extra effects right now.
                return (state with { Count = state.Count + 1 }, null);
            }
            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                return (state with { Count = state.Count - 1 }, null);
            }
        }

        // Ignore any other messages we get
        return (state, null);
    }

    public string View(CounterState state)
    {
        // The string returned from the View function is what is rendered on screen!
        return
            $"""
            Your count is currently {state.Count}!!
            →: Increment
            ←: Decrement
            ctrl+c: Quit
            """;
    }
}