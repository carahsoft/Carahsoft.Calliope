﻿using Carahsoft.Calliope;

await Calliope.NewProgram(new CounterProgram()).RunAsync();

public class CounterProgram : ICalliopeProgram
{
    public int Count { get; private set; }

    public CalliopeCmd? Init()
    {
        return null;
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
            {
                // If the user pressed ctrl+c, returning the Quit command exits the program
                return CalliopeCmd.Quit;
            }

            if (kpm.Key == ConsoleKey.RightArrow)
            {
                // Right arrow means increment the count!
                // null command means we don't need any extra effects right now.
                Count++;
                return null;
            }
            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                Count--;
                return null;
            }
        }

        // Ignore any other messages we get
        return null;
    }

    public string View()
    {
        // The string returned from the View function is what is rendered on screen!
        return
            $"""
            Your count is currently {Count}!!
            →: Increment
            ←: Decrement
            ctrl+c: Quit
            """;
    }
}