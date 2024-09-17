# Carahsoft.Calliope

```
                                    ███     ███
           ███████                 ████    ████
         ██████████                ████    ████     ███
        ███████████                ████    ████    ████
       ██████  ████                ████    ████     ███
      █████     ███                ████    ████
      ████                         ████    ████
     ████                          ████    ████                          ██
     ████               ███████    ████    ████     ██       ██████     ████ ██████        ███████
    ████              ██████████   ████    ████    ████    █████████    ████████████     ██████████
    ███              ███████████   ████    ████    ████   ███████████   ████████████    ████████████
   ████             ██████  ████   ████    ████    ████   █████  ████   ██████  █████   █████   ████
   ████             ████    ████   ████    ████    ████  ████     ███   ████     ████  ████    ████
   ███              ███     ████   ████    ████    ████  ████     ████  ████      ███  ███   ██████
   ███             ████     ████   ████    ████    ████  ███      ████   ███      ███  ██████████
   ███             ████     ████   ████    ████    ████  ███      ████   ███      ███  ████████
   ████        ███ ████      ███   ████    ████    ████  ███      ███    ███      ███  █████
   ████      █████ ████      ███   ████    ████    ████  ████     ███    ███     ████  ████       ██
    █████  ███████  ████   █████   ████    ████    ████  █████  █████    ████   █████  ██████   ████
    █████████████   █████████████  ████    ████    ████   ██████████     ███████████    ████████████
     ██████████      ████████████  ████    ████    ████    ████████      ██████████      ███████████
       ███████        ███████ ███  ███     ███     ███      ██████       █████████        ████████
                                                                         ███
                                                                         ███
                                                                         ███
                                                                         ███
                                                                         ███
                                                                         ███
                                                                         ███
                                                                         ███
```

A C# library for quickly writing awesome, responsive console apps.

## What does it do?

- Render text in any font into ASCII art, with optional different effects and animations
- MVU framework for rendering responsive, easy to write interactive console apps
- Easy ANSI color support for rendering 24 bit color (on supported terminals)

## Calliope MVU

The MVU framework was inspired by [Elm](https://elm-lang.org/) and [BubbleTea](https://github.com/charmbracelet/bubbletea). Calliope Programs implement the simple `Init`, `Update`, and `View` functions to render elements to the screen. Because the `View` function returns a `string`, programs can be easily composed together, or even used outside of the Calliope framework entirely. One of the primary advantages of this approach compared to traditional console libraries is that you can combine multiple interactive components together and seamlessly render them all at the same time. For example, the [TODO LINK HERE](LINK) example project demonstrates combining the text input element with the multi-select element for a filterable multi-select element.

Here is a simple C# application that tracks a count and prints the current count to the console. You can increase the count by pressing the right arrow key, and decrease the count by pressing the left arrow key. ctrl+c exits the program.

```csharp
using Carahsoft.Calliope;

// This runs our program and returns the state of the program when exited.
// We don't care about the last state value, so we can just ignore the returned state here.
await Calliope.NewProgram(new CounterProgram()).RunAsync();

// This defines our state model.
public record CounterState
{
    public int Count { get; init; }
}

// This defines our program class
public class CounterProgram : ICalliopeProgram<CounterState>
{
    // Init returns the initial state of our program, and is called when the program starts up.
    // Note that this also returns an optional CalliopeCmd, which we will get into later. For now,
    // we return null because we don't need to execute any commands.
    public (CounterState, CalliopeCmd?) Init()
    {
        return (new(), null);
    }

    // Update is called whenever a message is sent to the application. Here, we look for
    // KeyPressMsg messages, which are sent whenever the user presses any key in the terminal
    // window.
    public (CounterState, CalliopeCmd?) Update(CounterState state, CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            // If the user pressed ctrl+c, returning the Quit command exits the program
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                return (state, CalliopeCmd.Quit);

            // Right arrow means increment the count!
            if (kpm.Key == ConsoleKey.RightArrow)
                return (state with { Count = state.Count + 1 }, null);
            // Left arrow means decrement the count!
            if (kpm.Key == ConsoleKey.LeftArrow)
                return (state with { Count = state.Count - 1 }, null);
        }

        // Ignore any other messages we get
        return (state, null);
    }

    // The string returned from the View function is what is rendered on screen!
    public string View(CounterState state)
    {
        return
            $"""
            Your count is currently {state.Count}!!
            →: Increment
            ←: Decrement
            ctrl+c: Quit
            """;
    }
}
```

## Roadmap

### For SkiaSharp + single executable:
If you are using the text-to-ascii functions and want to produce a single packaged executable file, you will need to add the following line to your .*proj file:

```xml
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

Without this line, `dotnet publish` will produce the `libSkiaSharp.dll` file alongside your exe file, since it is a native library.