namespace Carahsoft.Calliope.Elm

open System
open System.Text.Json.Nodes
open Carahsoft.Calliope
open Carahsoft.Calliope.Components
open Carahsoft.Calliope.Forms.Json

// F# modules over Calliope's component state records. The logic is in Carahsoft.Calliope; these take
// Keys, return options rather than nulls and F# lists rather than IReadOnlyLists, and draw with the
// default theme. Each module's Model is the C# state record, so the two can be mixed. F# can't use C#'s
// `with` on those records, so each module has functions for the properties apps set.

/// A single line text input. It never quits the program and doesn't blink.
module TextField =
    type Model = TextFieldState

    let empty: Model = TextFieldState.Empty

    /// A field containing the text, with the cursor at the end
    let ofText (text: string) : Model = TextFieldState.Of text

    /// Handles editing keys. Returns None for keys it doesn't use (Enter, Esc, Tab, ↑/↓…).
    let update (key: Key) (m: Model) : Model option =
        Components.TextField.Update(m, key.ToMsg()) |> Option.ofObj

    /// The text in at most width columns, with the cursor as an inverted block when focused
    let view (width: int) (focused: bool) (m: Model) = Components.TextField.Render(m, width, focused)

/// A Yes/No prompt that defaults to No
module Confirm =
    type Model = ConfirmState

    type Outcome =
        | Pending of Model
        | Confirmed
        | Cancelled

    let create (title: string) (message: string list) : Model = Components.Confirm.Create(title, message)

    let update (key: Key) (m: Model) =
        let next = Components.Confirm.Update(m, key.ToMsg())

        match next.Result with
        | ConfirmResult.Confirmed -> Confirmed
        | ConfirmResult.Cancelled -> Cancelled
        | _ -> Pending next

    /// The message and buttons, to draw inside a popup
    let content (m: Model) = Components.Confirm.Content m |> List.ofSeq

    let footer = Components.Confirm.Footer

    /// The prompt as a centered popup over the background
    let view (width: int) (height: int) (background: string list) (m: Model) =
        Components.Confirm.Render(m, width, height, background) |> List.ofSeq

/// A floating list to choose an option from, opened with Space
module Dropdown =
    type Model = DropdownState

    let create (options: string list) (value: string option) : Model =
        Components.Dropdown.Create(options, Option.toObj value)

    let value (m: Model) = Option.ofObj m.Value

    let isOpen (m: Model) = m.IsOpen

    /// Opens the list on the current value
    let openList (m: Model) = Components.Dropdown.Open m

    /// Closes the list without changing the value
    let closeList (m: Model) = Components.Dropdown.Close m

    /// Options shown at once; longer lists scroll
    let withRows (rows: int) (m: Model) = RecordCopies.WithRows(m, rows)

    /// Handles a key. Returns None for keys it doesn't use, which happens only while it's closed.
    let update (key: Key) (m: Model) : Model option =
        Components.Dropdown.Update(m, key.ToMsg()) |> Option.ofObj

    /// The current value with a ▾
    let view (focused: bool) (m: Model) = Components.Dropdown.Render(m, focused)

    /// The open list's box, between 12 and maxWidth columns wide. Empty while closed.
    let list (maxWidth: int) (m: Model) = Components.Dropdown.RenderList(m, maxWidth) |> List.ofSeq

/// A selectable, sortable, filterable table (Calliope's DataGrid)
module Table =
    type Model<'row> = DataGridState<'row>
    type Column<'row> = DataGridColumn<'row>

    /// A column with a minimum width. Sorting by it sorts by the cell's text, without styles.
    let column<'row> (header: string) (width: int) (cell: 'row -> string) : Column<'row> =
        DataGridColumn<'row>(header, width, Func<'row, string> cell)

    /// Takes a share of the width left over after every column's minimum
    let grow (c: Column<'row>) = RecordCopies.WithGrow(c, true)

    let right (c: Column<'row>) = RecordCopies.WithAlign(c, Table.ColumnAlignment.Right)

    let center (c: Column<'row>) = RecordCopies.WithAlign(c, Table.ColumnAlignment.Center)

    let sortBy (key: 'row -> IComparable) (c: Column<'row>) = RecordCopies.WithSortKey(c, Func<'row, IComparable> key)

    /// s doesn't sort by this column
    let unsortable (c: Column<'row>) = RecordCopies.WithSortable(c, false)

    /// A table with no rows yet
    /// <param name="rowKey">Identifies a row, so the cursor can stay on it when the filter or sort changes</param>
    /// <param name="filterText">The text the filter searches</param>
    let create (columns: Column<'row> list) (rowKey: 'row -> string) (filterText: 'row -> string) : Model<'row> =
        let grid = DataGrid.Create<'row>(columns)
        let grid = RecordCopies.WithRowKey(grid, Func<'row, string> rowKey)
        RecordCopies.WithFilterText(grid, Func<'row, string> filterText)

    /// A style for a whole row, e.g. red for errors
    let withRowStyle (style: 'row -> Style option) (m: Model<'row>) =
        RecordCopies.WithRowStyle(m, Func<'row, Style>(style >> Option.toObj))

    /// Replaces a row's cells from a column index onwards with one piece of text, e.g. an error
    let withSpanFrom (span: 'row -> (int * string) option) (m: Model<'row>) =
        RecordCopies.WithSpanFrom(m, Func<'row, DataGridSpan>(span >> Option.map DataGridSpan >> Option.toObj))

    /// Shown instead of rows when there are none
    let withEmptyText (text: string) (m: Model<'row>) = RecordCopies.WithEmptyText(m, text)

    /// Shows "Loading…" instead of the empty text until loaded
    let loadingUntil (loaded: bool) (m: Model<'row>) = RecordCopies.WithLoading(m, not loaded)

    /// The rows in their default order
    let rows (m: Model<'row>) = List.ofSeq m.Rows

    /// The rows after filtering and sorting, in display order
    let visibleRows (m: Model<'row>) = DataGrid.VisibleRows m |> List.ofSeq

    let selected (m: Model<'row>) = visibleRows m |> List.tryItem m.Cursor

    /// The column sorted by and whether it's descending, or None for the default order
    let sort (m: Model<'row>) =
        if m.Sort.HasValue then Some(m.Sort.Value.Column, m.Sort.Value.Descending) else None

    /// Replaces the rows, e.g. on a refresh. The cursor and scroll position stay where they were.
    let setRows (rows: 'row list) (m: Model<'row>) = DataGrid.SetRows(m, rows)

    /// Filters the rows, keeping the cursor on the selected row if it still matches
    let setFilter (filter: string) (m: Model<'row>) = DataGrid.SetFilter(m, filter)

    /// Rows that fit on screen, used to scroll
    let setViewHeight (height: int) (m: Model<'row>) = DataGrid.SetViewHeight(m, height)

    /// Handles navigation and sort keys. Returns None for keys the table doesn't use.
    let update (key: Key) (m: Model<'row>) : Model<'row> option =
        DataGrid.Update(m, key.ToMsg()) |> Option.ofObj

    /// The header and visible rows in exactly height lines
    let view (width: int) (height: int) (focused: bool) (m: Model<'row>) =
        DataGrid.Render(m, width, height, focused) |> List.ofSeq

    /// "3/12" position text for status bars
    let position (m: Model<'row>) = DataGrid.Position m

/// Fields of a Form
module Field =
    type Model = FormField

    let text (name: string) = FormField.Text name
    let integer (name: string) = FormField.Integer name
    let number (name: string) = FormField.Number name
    let date (name: string) = FormField.Date name
    let boolean (name: string) (value: bool) = FormField.Boolean(name, value)
    let choice (name: string) (options: string list) (value: string option) = FormField.Choice(name, options, Option.toObj value)
    let rawJson (name: string) = FormField.RawJson name

    /// Gives a text, number, date or JSON field a starting value
    let withText (text: string) (f: Model) = RecordCopies.WithInput(f, TextFieldState.Of text)

    let required (f: Model) = RecordCopies.WithRequired(f, true)
    /// Shown below the form while the field has focus
    let describe (description: string) (f: Model) = RecordCopies.WithDescription(f, description)
    /// Shown instead of the name
    let label (label: string) (f: Model) = RecordCopies.WithLabel(f, label)
    let hidden (f: Model) = RecordCopies.WithHidden(f, true)

/// A form of labelled fields, built from fields or a JSON Schema
module Form =
    type Model = FormState

    type Outcome =
        | Editing of Model
        /// Every field is valid; see values and json
        | Submitted of Model
        | Cancelled

    let create (fields: Field.Model list) : Model = Components.Form.Create(fields)

    /// A form with a field for each of the schema's properties, as exported by JsonSchemaExporter
    let ofSchema (schema: JsonNode) : Model = JsonSchemaForm.Create(schema)

    let fields (m: Model) = List.ofSeq m.Fields

    let isEmpty (m: Model) = Components.Form.IsEmpty m

    /// True while a choice field's dropdown list is open
    let dropdownOpen (m: Model) = Components.Form.DropdownOpen m

    /// The selected value of a choice field
    let choiceValue (name: string) (m: Model) = Components.Form.ChoiceValue(m, name) |> Option.ofObj

    /// Shows or hides a field, e.g. one that only applies to some choices of another
    let setHidden (name: string) (hidden: bool) (m: Model) = Components.Form.SetHidden(m, name, hidden)

    let update (key: Key) (m: Model) =
        let next = Components.Form.Update(m, key.ToMsg())

        match next.Status with
        | FormStatus.Submitted -> Submitted next
        | FormStatus.Cancelled -> Cancelled
        | _ -> Editing next

    /// Validates every field: Ok with the values set, or Error with the errors set on the fields
    let submit (m: Model) : Result<Model, Model> =
        let next = Components.Form.Submit m
        if next.Status = FormStatus.Submitted then Ok next else Error next

    /// The submitted values by field name: string, int64, decimal, DateOnly, bool or JsonNode.
    /// Hidden fields and empty optional ones are left out.
    let values (m: Model) : Map<string, obj> =
        match m.Values with
        | null -> Map.empty
        | values -> values |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq

    /// The submitted values as a JSON object
    let json (m: Model) = JsonSchemaForm.ToJson(m).ToJsonString()

    /// The form's lines. An open dropdown isn't included; see dropdown.
    let view (width: int) (m: Model) = Components.Form.Render(m, width) |> List.ofSeq

    /// The open dropdown as a window to draw over the screen: the line its field is on and the
    /// column values start at (both relative to view's lines), and its lines. Use
    /// Layout.floatBelow to place it.
    let dropdown (width: int) (m: Model) =
        Components.Form.RenderDropdown(m, width)
        |> Option.ofObj
        |> Option.map (fun d -> d.Row, d.Column, List.ofSeq d.Lines)

    /// The focused field's description, wrapped, with a blank line above it
    let description (width: int) (m: Model) = Components.Form.Description(m, width) |> List.ofSeq

    let footer = Components.Form.Footer

/// A vim-style : prompt for running commands by name
module CommandLine =
    type Model = CommandLineState
    type Command = CommandLineCommand

    let command (name: string) (description: string) (aliases: string list) : Command =
        RecordCopies.WithAliases(CommandLineCommand(name, description), Array.ofList aliases)

    let create (commands: Command list) : Model = Components.CommandLine.Create(commands)

    let isOpen (m: Model) = m.IsOpen
    let openPrompt (m: Model) = Components.CommandLine.Open m
    let close (m: Model) = Components.CommandLine.Close m

    /// The text typed after the colon, while open
    let text (m: Model) = m.Input |> Option.ofObj |> Option.map _.Text

    /// While closed, only : is used (to open it) and other keys return None. While open, it takes
    /// every key.
    let update (key: Key) (m: Model) : Model option =
        Components.CommandLine.Update(m, key.ToMsg()) |> Option.ofObj

    /// The command run by the last key press (Enter), and its arguments
    let invoked (m: Model) =
        m.Invoked |> Option.ofObj |> Option.map (fun c -> c.Name, m.InvokedArgs)

    /// A message from the last key press, e.g. for an unknown command
    let message (m: Model) = Option.ofObj m.Message

    /// Commands whose name starts with what's been typed
    let matches (m: Model) = Components.CommandLine.Matches m |> List.ofSeq

    /// The prompt: a colon and what's been typed. Empty while closed.
    let view (width: int) (m: Model) = Components.CommandLine.Render(m, width)

    /// The matching commands with their descriptions, for a status line
    let suggestions (width: int) (m: Model) = Components.CommandLine.RenderSuggestions(m, width)

    /// Every command with its names and description
    let help (m: Model) = Components.CommandLine.RenderHelp m |> List.ofSeq
