![Calliope Logo](Images/calliope_banner.png)

# Carahsoft.Calliope

A C# library for quickly writing awesome, responsive console apps.

## What does it do?

- MVU framework for rendering responsive, easy to write interactive console apps
- Render text in any font into ASCII art, with optional different effects and animations
- Easy ANSI color support for rendering 24 bit color (on supported terminals)


## Calliope MVU

The MVU framework was inspired by [Elm](https://elm-lang.org/) and [BubbleTea](https://github.com/charmbracelet/bubbletea). Calliope Programs implement the simple `Init`, `Update`, and `View` functions to render elements to the screen. Because the `View` function returns a `string`, programs can be easily composed together, or even used outside of the Calliope framework entirely. One of the primary advantages of this approach compared to traditional console libraries is that you can combine multiple interactive components together and seamlessly render them all at the same time. For example, the [FilterableSelectList](./Carahsoft.Calliope/Components/FilterableSelectList.cs) component demonstrates combining the text input element with the multi-select element for a filterable select list element.

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

`CalliopeCmd` also has helpers for timers like this one:

- `CalliopeCmd.Delay(TimeSpan, msg)` sends a message after a delay.
- `CalliopeCmd.Every(TimeSpan, () => msg)` sends a message repeatedly until the program quits.

Commands can take a `CancellationToken` (`CalliopeCmd.Make(async token => ...)`), which is canceled when the program quits. Give a command a key with `.WithKey("search")` and starting another command with the same key cancels the first, so a slow response to an old request is never delivered. `CalliopeCmd.Cancel("search")` cancels it outright, e.g. to stop an `Every` timer.

## Components

Calliope comes with components you can combine into larger apps. The interactive ones in `Carahsoft.Calliope.Components` come in two layers:

- **State records and functions.** Each component is an immutable record (e.g. `FormState`) with static functions to update and render it (`Form.Update(state, key)`, `Form.Render(state, width)`). Update functions return the new state, or null for keys the component doesn't use, so your program can handle them instead. Nothing quits the program or blinks on its own, so these compose freely, and they're easy to use from F#.
- **Views.** Each also has a mutable `ICalliopeProgram` wrapper (e.g. `FormView`) holding its state, for apps written in the usual C# style or to run a component on its own.

| Component | Records and functions | View | |
|---|---|---|---|
| Text field | `TextFieldState`, `TextField` | `TextFieldView` | Single-line input with word delete, Ctrl+U and scrolling |
| Confirm | `ConfirmState`, `Confirm` | `ConfirmView` | Yes/No prompt that defaults to No |
| Dropdown | `DropdownState`, `Dropdown` | `DropdownView` | A floating list of options, opened with Space |
| Data grid | `DataGridState<TRow>`, `DataGrid` | `DataGridView<TRow>` | Selectable, sortable, filterable table that keeps its place on refresh |
| Form | `FormState`, `FormField`, `Form` | `FormView` | Text, number, date, boolean, choice and JSON fields with validation |
| Command line | `CommandLineState`, `CommandLine` | `CommandLineView` | A vim-style `:` prompt with suggestions and Tab completion |

`Carahsoft.Calliope.Forms.Json.JsonSchemaForm` builds a form from a JSON Schema, like one from `JsonSchemaExporter`, and turns the submitted values into a JSON object.

The older components (`TextInput`, `SelectList`, `FilterableSelectList`, `ScrollView`, `Table`, `PopupDialog`, `LoadingBar`, `LoadingSpinner`) are mutable programs. `SelectList`, `TextInput` and `ScrollView` quit the program on Ctrl+C unless you turn off `QuitOnCtrlC`.

### Layout

`Carahsoft.Calliope.Text` has helpers for drawing with lines of styled text:

- `Ansi` measures, cuts, pads and wraps lines, ignoring escape codes, and never leaves a style running into the next line.
- `Block` places blocks of lines side by side or above each other.
- `Frame` draws boxes, dialogs, dividers and bars. Their characters and colors come from a `FrameStyle`.
- `Window` draws windows over other lines (popups, dropdowns), keeping the text and colors on both sides.

Colors come from `Theme.Default`, which you can change with `with`. [DashboardExample](./Examples/DashboardExample/Program.cs) puts them all together: a table that refreshes itself, a confirm prompt, a form in a popup with a dropdown, a `:` command line and a help window.

### F#

[Carahsoft.Calliope.Elm](./Carahsoft.Calliope.Elm/README.md) runs pure Elm-style F# programs (`init`, `update` and `view` with `Cmd`s), wraps the components in F# modules, and has helpers for testing update functions without a terminal. See [ElmCounter](./Examples/ElmCounter/Program.fs).


## Calliope ASCII Rendering

Calliope can render text to ASCII art using the `Calliope.Print` and `Calliope.PrintString` methods. The `CalliopePrintOptions` class has all of the available options for customizing the output string. All core rendering & ANSI helper functions are available on the `Calliope` static class.

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


## Roadmap

- [ ] Multi font support for different operating systems
- [ ] Enable custom ASCII fonts (e.g. FIGlet via [Figgle](https://github.com/drewnoakes/figgle))
- [x] Focused element support (Focus/Blur) for more complex apps
- [ ] Mouse support
- [ ] Full width character support (using [wcwidth](https://github.com/spectreconsole/wcwidth)?)
- [x] Implement `Style` class to easily style many components together
- [ ] Detect older terminals without TrueColor support and use color approximation to render colors in less depth
- [ ] Net standard 2.0 version
- [ ] Source generator for creating ASCII art during compile time (requires net standard 2.0 version)
- [ ] Automatically calculate height/width of ascii text render based on font size and string length

## For SkiaSharp + single executable:
If you are using the text-to-ascii functions and want to produce a single packaged executable file, you will need to add the following line to your .*proj file:

```xml
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

Without this line, `dotnet publish` will produce the `libSkiaSharp.dll` file alongside your exe file, since it is a native library.