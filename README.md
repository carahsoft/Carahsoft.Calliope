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

// This defines our program class. In the Elm world, our program contains the Init, Update,
// and View functions, and also is our state model.
public class CounterProgram : ICalliopeProgram
{
    // This is the state we are tracking
    public int Count { get; set; }

    // Init is called when the program starts up. This returns an optional CalliopeCmd object
    // which we will get into later. For now, we return null because we don't need to execute
    // any commands.
    public CalliopeCmd? Init()
    {
        return null;
    }

    // Update is called whenever a message is sent to the application. Here, we look for
    // KeyPressMsg messages, which are sent whenever the user presses any key in the terminal
    // window.
    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            // If the user pressed ctrl+c, returning the Quit command exits the program
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                return CalliopeCmd.Quit;

            // Right arrow means increment the count!
            if (kpm.Key == ConsoleKey.RightArrow)
            {
                Count = Count + 1;
                return null;
            }

            // Left arrow means decrement the count!
            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                Count = Count - 1;
                return null;
            }
        }

        // Ignore any other messages we get
        return null;
    }

    // The string returned from the View function is what is rendered on screen!
    // View is always called after the Update method completes.
    public string View()
    {
        return
            $"""
            Your count is currently {Count}!!
            →: Increment
            ←: Decrement
            ctrl+c: Quit
            """;
    }
}
```

### Commands

In the above example, we returned the `CalliopeCmd.Quit` command when the user presses control + c. The `Quit` command is a special command that instructs the Calliope framework to cleanly exit and return the final state of the program as the return value of the `RunAsync` method. However, in practice commands are used for much more than just quitting your application.

A `CalliopeCmd` is just an async function that returns a `CalliopeMsg`, i.e. a `Func<Task<CalliopeMsg>>`. The `CalliopeMsg` that is returned from the command is passed to the `Update` function upon completion of the command. It represents any I/O operations or long running tasks that your program might need to perform. This might include loading data from an API, interacting with the filesystem, or just initiating a delayed effect with `Task.Delay`. All I/O and long running operations should be performed within a `CalliopeCmd` to ensure the program remains responsive for the user.

Here is the above example, modified to increment the state automatically once every second:

```csharp
using Carahsoft.Calliope;

await Calliope.NewProgram(new CounterProgram()).RunAsync();

public class TickUpMsg : CalliopeMsg { }

public class CounterProgram : ICalliopeProgram
{
    public int Count { get; set; }

    public CalliopeCmd? Init()
    {
        // Before we were returning null here because we didn't need
        // to do anything on startup. Now, we want to start our count
        // ticking up every second, so we return a CalliopeCmd that
        // waits 1 second and then sends a TickUpMsg to our Update
        // function
        return Tick();
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                return CalliopeCmd.Quit;

            if (kpm.Key == ConsoleKey.RightArrow)
            {
                Count = Count + 1;
                return null;
            }

            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                Count = Count - 1;
                return null;
            }
        }

        // If we received our TickUpMsg, increment the count and
        // start the 1 second countdown again.
        if (msg is TickUpMsg)
        {
            Count = Count + 1;
            return Tick();
        }

        return null;
    }

    public string View()
    {
        return
            $"""
            Your count is currently {Count}!!
            →: Increment
            ←: Decrement
            ctrl+c: Quit
            """;
    }

    // Tick returns a CalliopeCmd that waits for 1 second before
    // returning a TickUpMsg. The TickUpMsg will then be passed to
    // the Update function.
    private CalliopeCmd Tick()
    {
        return CalliopeCmd.Make(async () =>
        {
            await Task.Delay(1000);
            return new TickUpMsg();
        });
    }
}
```


## Calliope ASCII Rendering

Calliope can render text to ASCII art using the `Calliope.Print` and `Calliope.PrintString` methods. The `CalliopePrintOptions` class has all of the available options

## Roadmap

- [ ] Do things
- [x] Do other things

### For SkiaSharp + single executable:
If you are using the text-to-ascii functions and want to produce a single packaged executable file, you will need to add the following line to your .*proj file:

```xml
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

Without this line, `dotnet publish` will produce the `libSkiaSharp.dll` file alongside your exe file, since it is a native library.