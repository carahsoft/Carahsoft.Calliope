# Plan: Move kmc's UI components into Carahsoft.Calliope

## Context

Building `kmc` on Carahsoft.Calliope 0.2.0 meant writing a lot of UI that any Calliope app would want: a selectable table, forms with dropdowns, confirm prompts, floating windows, ANSI-aware layout helpers, a `:` command line, and an Elm-style adapter for F#. It also meant working around several Calliope bugs.

This plan moves that work into Calliope in three steps:
1. **Bug fixes:** fix the bugs in Calliope so apps don't need workarounds.
2. **C# additions:** add the general-purpose components and layout helpers to the core `Carahsoft.Calliope` package, usable from C# and F#.
3. **F# Elm package:** create `Carahsoft.Calliope.Elm`, with the F#-specific pieces (Elm program adapter, `Cmd`, test helpers), and move kmc onto the packages.

All the source to port is in this repo under `Carahsoft.KafkaMonitor.Cli/`, with tests in `Carahsoft.KafkaMonitor.Cli.Tests/`. Calliope today is one C# project (`Carahsoft.Calliope/`, net10.0, LangVersion 14), plus `Examples/`. It has no test project, and its CI (`AzurePipelines/Carahsoft.Calliope-CI.yaml`) still says .NET 8 and base version 0.1.

### Decision to confirm before step 2: how components keep their state

kmc's components are pure F#: an update function takes a key and the current state and returns new state, and the view is a function of that state. Calliope's components are mutable classes implementing `ICalliopeProgram`. Writing the new components as mutable C# classes would suit C# apps but break the pure approach for F#. Writing them twice would mean two implementations to keep in sync.

**Proposed:** each component is an immutable C# record plus static update and render functions, with a thin mutable `ICalliopeProgram` wrapper for C# apps:

```csharp
public sealed record TableState<TRow>(...);                      // immutable
public static class Table
{
    public static TableState<TRow> Update<TRow>(TableState<TRow> s, KeyPressMsg key);
    public static IReadOnlyList<string> Render<TRow>(TableState<TRow> s, int width, int height, bool focused);
}
public class TableView<TRow> : ICalliopeProgram { public TableState<TRow> State { get; set; } ... }  // wrapper
```

F# uses the records and functions directly, and the Elm package adds F#-friendly modules on top. There's only one implementation of each component's logic.

**Versions:** step 1 ships as 0.3.0 (no breaking changes), step 2 as 0.4.0, and step 3 as `Carahsoft.Calliope.Elm` 0.1.0, which depends on Calliope 0.4.

---

## Step 0: Groundwork (in the Calliope repo)

- **Test project:** add `Carahsoft.Calliope.Tests` (xUnit) to `Carahsoft.Calliope.sln`. Every fix and component below gets tests. Rendering tests compare plain text with ANSI codes stripped, and separately check the escape codes where styling matters, the way kmc's `UiTests.fs` does.
- **CI:** update `AzurePipelines/Carahsoft.Calliope-CI.yaml` to .NET 10 and base version 0.3. Run the tests. Decide how packages reach nuget.org (0.2.0 is there today, but the pipeline only publishes a build artifact).

## Step 1: Bug fixes (0.3.0)

Each fix has a test that fails before the change. Several of these are recorded in `docs/calliope-issues.md`.

| # | Bug | Where | Fix | kmc workaround to remove afterwards |
|---|---|---|---|---|
| 1 | `TruncateLineToLength` drops escape codes after the last visible character, even when the line already fits, so a full-width styled line loses its closing reset and its color bleeds into later lines. | `AnsiConsole/AnsiTextHelper.cs:85`, used by `ProgramRunner.cs:469` | Return the line unchanged when it fits. When cutting, append `ESC[0m` if a style is open. | `App.view` and `Setup.view` start every line with a reset; `Layout.truncate`/`cut` append a reset. Delete the "Calliope's truncation drops the reset" test in `UiTests.fs`, which exists to catch this fix. |
| 2 | Cutting a line that really is too long leaves the style open, so it bleeds the same way. | same | Same fix as #1. | same |
| 3 | `PopupDialog` overlays leak and lose styles: the background text left of the popup can leave a style open going into the border, and text to the right of the popup loses the styles that were active. | `Components/PopupDialog.cs:128` and the right-part slicing below it | Use the style-preserving cut from step 2 (`Ansi.Cut` and `Ansi.Skip`, below) on both sides. | kmc no longer uses `PopupDialog`; nothing to remove. |
| 4 | `TruncateLineToLength` probably drops the character before a tab when the tab crosses the limit (`return line[..(i - 1)]`). | `AnsiTextHelper.cs`, tab branch | Confirm with a test, then return `line[..i]`. | none |
| 5 | `TruncateLineToLength` silently returns multi-line input unchanged, with a `throw` after the `return` that can never run. | same | Either handle each line or throw as intended. Pick one and document it. | none |
| 6 | `ConsoleTable<T>` crashes on an empty list (`testList[0]`). | `Table/ConsoleTable.cs`, `getDataTable` | Build the columns from `typeof(T)`'s properties instead of the first row. | `printTable` in `Program.fs` checks for an empty list first. |
| 7 | `SelectList.View()` crashes on an empty list (`Items.Max`). | `Components/SelectList.cs` | Render an empty state. | none (kmc has its own dropdown) |
| 8 | Components quit the whole app on their own: `SelectList` on Enter and Ctrl+C, and `TextInput`/`ScrollView` on Ctrl+C. | `SelectList.cs`, `TextInput.cs`, `ScrollView.cs` | Add `QuitOnCtrlC` (default `true`, so nothing breaks) and `SelectList.QuitOnSelect` (default `true`). When they're off, Enter only sets `Choice`. | kmc's own `TextField` exists partly because of this. |
| 9 | `TextInput.Enabled` exists but is never checked. | `TextInput.cs` | Ignore keys when disabled, and draw without a cursor. | none |
| 10 | Cursor blink messages go to every cursor in the app. | `Components/Cursor.cs` | Give each cursor an ID and carry it in its blink messages. | none |
| 11 | `ScrollView` sets `Height` to the full screen on every resize, so it overflows under headers. | `ScrollView.cs` | Add `AutoHeight` (default `true`); when it's off, keep the height the parent set. | none |
| 12 | `Components.Table` ignores `HeaderStyle`/`RowStyle` and can't refresh after `Init`. | `Components/Table.cs` | Apply the styles and add `SetRows`. Or mark it obsolete in favor of the new table in step 2. | none |
| 13 | An exception in `View` calls `Environment.Exit(1)`, leaving the terminal in the alternate screen with the cursor hidden. | `AnsiConsole/ProgramRunner.cs:162` | Restore the terminal, then rethrow from `RunAsync` (or send an `ErrorMsg` and keep running, as an option). | `ElmProgram.View` catches exceptions; keep that, as it's still the friendlier behavior. |

Smaller cleanups to do alongside:
- `SelectListItem.FilterValue` and `ViewPort.Width` aren't used. Either use them or remove them, marking them obsolete first.
- `Inputs/MultiSelectListInput` is an empty stub. Remove it or build it (step 2 covers it with the dropdown).

## Step 2: C# additions (0.4.0)

New namespaces in `Carahsoft.Calliope`, ported from kmc. Each item lists its kmc source. All of them follow the state design above unless noted.

### 2a. Text and layout helpers (static, no state)

Namespace `Carahsoft.Calliope.Text`, from `Ui/Layout.fs`:
- **`Ansi`:**
  - `VisibleLength`.
  - `Truncate`: adds `…`.
  - `Cut`: no ellipsis, closes any open style.
  - `Skip(n)`: the style-preserving `dropVisible`. It re-applies SGR codes still active at the cut, and handles `ESC[m`, `ESC[0m` and `ESC[0;1m`.
  - `PadRight`/`PadLeft`/`Center`.
  - `Wrap`: word wrap on plain text.
  - These and the step 1 fixes should share one ANSI scanner.
- **`Block`:** `FitHeight`, `JoinHorizontal` (`hjoin`), `JoinVertical`, `Lines`.
- **`Frame`:**
  - `Box`: title in the top border.
  - `Dialog`: fixed width, title, and footer divider.
  - `Rule`: horizontal divider.
  - `Bar`: left and right text across the full width.
  - Border characters and colors come from a small style record, defaulting to today's look.
- **`Window`:**
  - `OverlayAt(row, col, over, under)`: draws over lines, keeping the text on both sides with their styles.
  - `Popup`: centers a dialog and returns its lines and where its content starts.
  - `FloatBelow`: anchors a window below a line, or above it if there's no room.
  - This is what makes dropdowns and nested popups possible.

`PopupDialog` becomes a thin wrapper over `Frame.Dialog` + `Window.Popup`, so it gets the fixed styles too.

### 2b. Commands

From `Elm.fs`, `Screens/Common.fs` and the `Cmd.delay` tick pattern:
- `CalliopeCmd.Delay(TimeSpan, msg)`, and `CalliopeCmd.Every(TimeSpan, Func<CalliopeMsg>)` for repeating ticks.
- **Cancellation:** pass a `CancellationToken` to command functions and cancel it when the program quits. Optionally support keyed commands, where starting one cancels the previous with the same key; that replaces kmc's stale-request IDs.

### 2c. Interactive components

Namespace `Carahsoft.Calliope.Components`, each as a state record, update/render functions, and a wrapper:

| Component | kmc source | Notes |
|---|---|---|
| `TextField` | `Ui/TextField.fs` | Single-line input: word delete, Ctrl+U, scrolls to keep the cursor visible, inverted cursor, no blink messages. Never quits. Could become the core of `TextInput` once step 1's #8–#10 are in. |
| `Confirm` | `Ui/Confirm.fs` | Yes/No, defaults to No; y/n, ←/→, Tab, Enter, Esc. Renders as dialog content. |
| `Dropdown` | inside `Ui/Form.fs` (`Dropdown`, `updateDropdown`, `dropdownBox`) | Floating list: opens on Space, ↑/↓ and j/k, PgUp/PgDn, Home/End. Shows 8 rows at a time with ↑ n/↓ n counts. Esc closes only the list. Reports where to float it (`Form.dropdown`). Also usable on its own as a select. |
| `DataTable<TRow>` | `Ui/Table.fs` | Columns with min width, grow, alignment, sort key; cursor and scroll; selected column (←/→) with the header highlighted; `s` toggles the sort. Filtering, per-row style, `SpanFrom` rows, a loading empty state and a "3/12" position label. A refresh keeps the cursor index. Name it something other than `Table` to avoid clashing with the existing one. |
| `Form` | `Ui/Form.fs` | Fields: text, integer, number, date, boolean, choice (with the dropdown), and raw JSON. Required checks and per-field errors. Tab, Shift+Tab, ↑/↓ focus, skipping hidden fields. The focused field's description renders below, wrapped. `SetHidden` and `ChoiceValue` support dependent fields. Fixed input width so it doesn't grow while typing. |
| `CommandLine` | `App.fs` (`metaCommands`, `metaCommandMatches`, `commandLineKey`) | `:` prompt: a registry of commands (names, aliases, description, `Action<string args>`), prefix suggestions, Tab completion, and "unknown command" listing the commands. Backspace on an empty prompt closes it. |

### 2d. Forms from JSON Schema

Namespace `Carahsoft.Calliope.Forms.Json`, from `Form.ofSchema` and `Form.submit`. It builds a `Form` from a JSON Schema, as produced by `System.Text.Json.Schema.JsonSchemaExporter`:
- enums become choices; `["string","null"]` types are nullable;
- `["string","integer"]` (`Int64`) becomes an integer field;
- `format: date` becomes a date field;
- it reads `default`, `description` and `required`.

It submits a JSON object. System.Text.Json is part of .NET, so it adds no dependency, but it's a separate namespace because it's an extra feature. The kmc tests in `FormTests.fs` hold the schemas to copy into tests.

### 2e. Examples and docs

- Add `Examples/DashboardExample`: a table with refresh, a confirm prompt, a form with a dropdown, a `:` command line and a floating window. It's a small version of kmc and the place to try these by hand.
- Update the README's component list. Mark Focus/Blur done on its roadmap, since the form handles focus.

### Optional, separate from 0.4

The ASCII-art and font features need SkiaSharp and its native assets, which every app pulls in today. They could move to `Carahsoft.Calliope.Figlet`. It's a breaking change (`Calliope.Print*` would move), so it belongs in 1.0.

## Step 3: F# Elm package (`Carahsoft.Calliope.Elm` 0.1.0)

A new F# project in the Calliope solution, `Carahsoft.Calliope.Elm/`, packaged separately and depending on Calliope 0.4. From `Elm.fs` unless noted:

| Piece | What it does |
|---|---|
| `ElmProgram<'model,'msg>` | Runs pure `init`/`update`/`view` as an `ICalliopeProgram`. It builds the model before `Init`, because a resize can arrive first, and turns exceptions in `view` into an error screen. |
| `Cmd<'msg>` module | `none`, `batch`, `quit`, `ofMsg`, `ofTask`, `delay`, `every`, `map`. Built on the 2b commands, so cancellation comes through. |
| `AppMsg<'msg>` | Carries app messages through `CalliopeMsg`. Internal unless there's a reason to expose it. |
| `Key` | Record with `Key`, `Char`, `Ctrl`/`Shift`/`Alt`, plus `Key.Of`, `Key.Plain` and `Key.OfChar`. `Key.Normalize` (Ctrl+N/Ctrl+P → ↓/↑) is an opt-in helper, since it's an app's choice of keys. Letter matching should compare `Char`, because terminals don't reliably report Shift. |
| F# component modules | `Table`, `TextField`, `Confirm`, `Dropdown`, `Form` and `CommandLine` modules over the 2c state records, with curried functions, `Key` instead of `KeyPressMsg`, and options instead of nulls. These are thin; the logic stays in C#. |
| `Testing` module | From kmc's tests: `runEffects` (runs each effect once, skipping slow ones like ticks) and `settle` (feeds messages back until nothing is left), so F# apps can test their update functions without a terminal. Note that `settle` must run each effect only once, which was a bug kmc's tests had. |
| Request guard | `nextRequestId` from `Screens/Common.fs`, unless 2b's keyed cancellation makes it unnecessary. |
| Screen stack (optional, 0.2) | From `App.fs` and `Screens/Common.fs`: a navigation stack with open/back intents, routing messages to the right screen, one popup per screen and a floating window anchored inside it. Leave it for later, until a second app shows what shape it should have. |

Also add an `Examples/ElmCounter` F# example, and a short README for the package.

### Then: move kmc onto the packages

In this repo, once 0.4 and Elm 0.1 are published:
1. Change `Carahsoft.KafkaMonitor.Cli.fsproj` to use `Carahsoft.Calliope` 0.4 and `Carahsoft.Calliope.Elm` 0.1.
2. Replace `Elm.fs`, `Ui/Layout.fs` (except `age`/`ageOfString`), `Ui/TextField.fs`, `Ui/Table.fs`, `Ui/Confirm.fs` and `Ui/Form.fs` with the package versions. Keep `Ui/Theme.fs` and the screens.
3. Remove the step 1 workarounds listed in the table, including the per-line reset in `App.view`/`Setup.view`.
4. Swap the test helpers for `Carahsoft.Calliope.Elm.Testing`.

## Verification

- **Calliope tests** (step 0's project):
  - every step 1 bug has a test that fails before the fix;
  - every 2a helper and 2c component has tests carried over from kmc's `UiTests.fs`, `FormTests.fs` and `SetupTests.fs`;
  - the F# modules have their own tests in an F# test project.
- **Rendering checks:** for the bleed fixes, check the escape codes directly, as kmc's tests do: the reset survives truncation, and styles are re-applied after an overlay.
- **Examples:** `dotnet run` each example in a real terminal and in a pseudo-terminal script, as kmc was tested.
- **kmc as the integration test:** after the migration, kmc's 114 tests should still pass with only the expected edits (imports, and the removed workaround test). Running `just kmc-local` should look and behave the same.
