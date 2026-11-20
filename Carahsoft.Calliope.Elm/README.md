# Carahsoft.Calliope.Elm

Elm-style F# programs for [Carahsoft.Calliope](https://github.com/carahsoft/Carahsoft.Calliope). Your program is a pure `init`, `update` and `view`: `update` returns a new model and the effects to run, and `ElmProgram` runs it as a Calliope program.

```fsharp
open System
open Carahsoft.Calliope
open Carahsoft.Calliope.Elm

type Msg =
    | KeyPressed of Key
    | Tick

let init () = 0, Cmd.every (TimeSpan.FromSeconds 1.0) Tick

let update msg count =
    match msg with
    | KeyPressed k when k.Ctrl && k.Key = ConsoleKey.C -> count, Cmd.quit
    | KeyPressed k when k.Key = ConsoleKey.RightArrow -> count + 1, Cmd.none
    | KeyPressed k when k.Key = ConsoleKey.LeftArrow -> count - 1, Cmd.none
    | KeyPressed _ -> count, Cmd.none
    | Tick -> count + 1, Cmd.none

let view count = $"Count: {count}\n←/→ to change it, ctrl+c to quit"

let ofCalliope (msg: CalliopeMsg) =
    match msg with
    | :? KeyPressMsg as k -> Some(KeyPressed(Key.Of k))
    | _ -> None

Calliope.NewProgram(ElmProgram(init, update, view, ofCalliope)).RunAsync().Wait()
```

`ElmProgram` builds the model before Calliope calls `Init`, since a resize can arrive first, and draws an error instead of stopping if `view` throws.

## Commands

`Cmd<'msg>` is a list of effects: `Cmd.none`, `Cmd.batch`, `Cmd.quit`, `Cmd.ofMsg`, `Cmd.ofTask`, `Cmd.ofCancelableTask`, `Cmd.stream`, `Cmd.delay`, `Cmd.every` and `Cmd.map`.

Effects are canceled when the program quits. `Cmd.withKey "search"` gives an effect a key: starting another with the same key cancels the first, so the response to an old request is never delivered. `Cmd.cancel "search"` cancels it outright, e.g. to stop a `Cmd.every` timer.

## Keys

`Key` is a key press with `Key`, `Char`, `Ctrl`, `Shift` and `Alt`. Make one with `Key.Of` (from Calliope's `KeyPressMsg`), `Key.Plain` or `Key.OfChar`. Match letters on `Char`, since terminals don't reliably report Shift. `Key.Normalize` turns Ctrl+N/Ctrl+P into ↓/↑, if you want them.

## Components and layout

These modules wrap Calliope's components with curried functions, `Key`s, options and F# lists:

- `TextField`, `Confirm`, `Dropdown`, `Table` (Calliope's `DataGrid`), `Field` and `Form`, and `CommandLine`. Each `Model` is Calliope's immutable state record, and `update` returns `None` for keys the component doesn't use. F# can't use `with` on those records, so set their properties with the module functions, like `Table.withEmptyText` or `Field.required`.
- `Form.ofSchema` builds a form from a JSON Schema, and `Form.json` gives the submitted values as JSON.
- `Layout` measures, cuts, pads and wraps styled text, and draws boxes, dialogs, popups and floating windows.

## Testing

`Carahsoft.Calliope.Elm.Testing` runs update functions without a terminal:

- `Testing.runEffects cmd` runs each effect once and returns the messages it sent within 100ms, so slow effects like ticks are skipped.
- `Testing.settle update (model, cmd)` feeds those messages back into `update` until nothing is left, running each effect only once, and returns the final model and whether it quit.
- `Testing.quits cmd` checks for `Cmd.quit`.
