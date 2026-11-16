module Carahsoft.Calliope.Elm.Tests.ComponentTests

open System
open System.Text.Json.Nodes
open Xunit
open Carahsoft.Calliope
// Opened before Elm on purpose: the F# modules must win over the C# classes with the same names
open Carahsoft.Calliope.Components
open Carahsoft.Calliope.Elm

let plain (s: string) = Text.Ansi.Strip s
let space = Key.Plain(ConsoleKey.Spacebar, ' ')

// Layout

[<Fact>]
let ``layout helpers take curried arguments and lists`` () =
    let red = "\x1b[31mhello world\x1b[0m"
    Assert.Equal("hello…", plain (Layout.truncate 6 red))
    Assert.Equal(15, Layout.visibleLength (Layout.padRight 15 red))
    Assert.Equal<string list>([ "ab  x"; "c   y"; "    z" ], Layout.hjoin 2 [ [ "ab"; "c" ]; [ "x"; "y"; "z" ] ])
    Assert.Equal<string list>([ "the quick"; "brown fox" ], Layout.wrap 10 "the quick brown fox")
    Assert.Equal<string list>([ "a"; "" ], Layout.fitHeight 2 [ "a" ])
    Assert.All(Layout.box "Title" 30 [ "content" ], fun l -> Assert.Equal(30, Layout.visibleLength l))

[<Fact>]
let ``popup reports where its content starts`` () =
    let background = List.replicate 20 (String('.', 60))
    let lines, (row, column) = Layout.popup 60 20 "Title" "footer" [ "hello" ] background
    Assert.Equal(20, lines.Length)
    Assert.Equal("hello", (plain lines[row]).Substring(column, 5))

[<Fact>]
let ``floatBelow opens upwards when there's no room below`` () =
    let lines = List.replicate 10 (String('.', 20))
    let above = Layout.floatBelow 10 8 5 [ "1"; "2"; "3" ] lines |> List.map plain
    Assert.Equal('1', above[5][5])

// TextField

[<Fact>]
let ``text field edits and returns None for keys it doesn't use`` () =
    let field = "helo" |> Seq.fold (fun f c -> TextField.update (Key.OfChar c) f |> Option.get) TextField.empty
    let field = TextField.update (Key.Plain ConsoleKey.LeftArrow) field |> Option.get
    let field = TextField.update (Key.OfChar 'l') field |> Option.get
    Assert.Equal("hello", field.Text)
    Assert.True((TextField.update (Key.Plain ConsoleKey.Enter) field).IsNone)
    Assert.Equal("hello", TextField.view 10 false field)

// Table

type Row = { Name: string; Size: int }

let rows = [ for i in 1..10 -> { Name = $"row{i}"; Size = 100 - i } ]

let table rows =
    Table.create
        [ Table.column<Row> "NAME" 10 _.Name |> Table.grow
          Table.column<Row> "SIZE" 5 (fun r -> string r.Size) |> Table.right |> Table.sortBy (fun r -> r.Size :> IComparable) ]
        _.Name
        _.Name
    |> Table.setRows rows
    |> Table.setViewHeight 3

let press (c: char) (m: Table.Model<Row>) = Table.update (Key.OfChar c) m |> Option.get

[<Fact>]
let ``table cursor stays in range and scrolls`` () =
    let t = [ 1..20 ] |> List.fold (fun t _ -> press 'j' t) (table rows)
    Assert.Equal(9, t.Cursor)
    Assert.Equal(7, t.Offset)
    Assert.Equal("row10", (Table.selected t |> Option.get).Name)

[<Fact>]
let ``table filters and sorts`` () =
    let t = table rows |> Table.setFilter "row1"
    Assert.Equal<string list>([ "row1"; "row10" ], Table.visibleRows t |> List.map _.Name)

    let right t = Table.update (Key.Plain ConsoleKey.RightArrow) t |> Option.get
    let t = press 's' (right (table rows))
    Assert.Equal(Some(1, false), Table.sort t)
    Assert.Equal("row10", (Table.visibleRows t |> List.head).Name)
    Assert.True((Table.update (Key.OfChar 'x') t).IsNone)

[<Fact>]
let ``table settings`` () =
    let t =
        table [ { Name = "bad"; Size = 0 } ]
        |> Table.withRowStyle (fun r -> if r.Size = 0 then Some Style.Red else None)
        |> Table.withSpanFrom (fun r -> if r.Size = 0 then Some(1, "failed") else None)

    let line = (Table.view 30 2 false t)[1]
    Assert.Equal(Style.Red.Apply("bad".PadRight(24) + " fail…"), line)

    let empty = table [] |> Table.withEmptyText "Nothing here"
    Assert.Equal("Nothing here", (Table.view 30 2 true empty |> List.item 1 |> plain).Trim())
    Assert.Equal("Loading…", (Table.view 30 2 true (Table.loadingUntil false empty) |> List.item 1 |> plain).Trim())
    Assert.Equal("0/0", Table.position empty)
    Assert.Equal(None, Table.selected empty)

// Confirm

[<Fact>]
let ``confirm defaults to no`` () =
    let c = Confirm.create "t" [ "sure?" ]

    match Confirm.update (Key.Plain ConsoleKey.Enter) c with
    | Confirm.Canceled -> ()
    | other -> failwith $"expected Canceled, got {other}"

    match Confirm.update (Key.Plain ConsoleKey.LeftArrow) c with
    | Confirm.Pending c ->
        Assert.True c.Yes

        match Confirm.update (Key.Plain ConsoleKey.Enter) c with
        | Confirm.Confirmed -> ()
        | other -> failwith $"expected Confirmed, got {other}"
    | other -> failwith $"expected Pending, got {other}"

    Assert.Equal(3, (Confirm.content c).Length)

// Dropdown

[<Fact>]
let ``dropdown opens, moves and picks`` () =
    let d = Dropdown.create [ "dev"; "test"; "prod" ] (Some "test")
    Assert.Equal(Some "test", Dropdown.value d)
    Assert.True((Dropdown.update (Key.Plain ConsoleKey.Enter) d).IsNone)

    let d = d |> Dropdown.update space |> Option.get
    Assert.True(Dropdown.isOpen d)
    Assert.Equal(5, (Dropdown.list 40 d).Length)

    let d = d |> Dropdown.update (Key.OfChar 'j') |> Option.bind (Dropdown.update (Key.Plain ConsoleKey.Enter)) |> Option.get
    Assert.Equal(Some "prod", Dropdown.value d)
    Assert.Equal("prod ▾", Dropdown.view false d)

// Form

let editing (outcome: Form.Outcome) =
    match outcome with
    | Form.Editing f -> f
    | other -> failwith $"expected Editing, got {other}"

let typeInto (text: string) (form: Form.Model) =
    text |> Seq.fold (fun f c -> Form.update (Key.OfChar c) f |> editing) form

[<Fact>]
let ``form from a schema submits JSON`` () =
    let form = Form.ofSchema (JsonNode.Parse """{"type":["object","null"],"properties":{"monitorName":{"type":"string"}},"required":["monitorName"]}""")

    match Form.update (Key.Plain ConsoleKey.Enter) form with
    | Form.Editing f -> Assert.Equal("Required", (Form.fields f).Head.Error)
    | other -> failwith $"expected a validation error, got {other}"

    match Form.update (Key.Plain ConsoleKey.Enter) (typeInto "TeamsCdrProcessor" form) with
    | Form.Submitted f -> Assert.Equal("""{"monitorName":"TeamsCdrProcessor"}""", Form.json f)
    | other -> failwith $"expected Submitted, got {other}"

    match Form.update (Key.Plain ConsoleKey.Escape) form with
    | Form.Canceled -> ()
    | other -> failwith $"expected Canceled, got {other}"

[<Fact>]
let ``form built from fields`` () =
    let form =
        Form.create
            [ Field.text "name" |> Field.required |> Field.describe "Who to greet"
              Field.integer "count" |> Field.withText "3"
              Field.choice "cluster" [ "kube"; "url" ] None
              Field.text "operator" |> Field.label "Operator URL" ]

    let sync form = Form.setHidden "operator" (Form.choiceValue "cluster" form <> Some "url") form
    let form = sync form
    Assert.Equal<string list>([ ""; "Who to greet" ], Form.description 60 form |> List.map plain)
    Assert.Equal(3, (Form.view 60 form).Length)

    match Form.submit (typeInto "bob" form) with
    | Ok f ->
        let values = Form.values f
        Assert.Equal(box "bob", values["name"])
        Assert.Equal(box 3L, values["count"])
        Assert.False(values.ContainsKey "operator")
    | Error _ -> failwith "expected the form to submit"

    match Form.submit form with
    | Error f -> Assert.Equal("Required", (Form.fields f).Head.Error)
    | Ok _ -> failwith "expected a validation error"

[<Fact>]
let ``form dropdown floats under its value`` () =
    let form = Form.create [ Field.choice "env" [ "dev"; "prod" ] None; Field.text "note" ]
    Assert.True((Form.dropdown 60 form).IsNone)

    let opened = Form.update space form |> editing
    Assert.True(Form.dropdownOpen opened)

    match Form.dropdown 60 opened with
    | Some(row, column, box) ->
        Assert.Equal(0, row)
        Assert.Equal((Form.view 60 opened |> List.head |> plain).IndexOf "dev", column)
        Assert.Equal(4, box.Length)
    | None -> failwith "expected an open dropdown"

// CommandLine

[<Fact>]
let ``command line runs commands by name or alias`` () =
    let commands = CommandLine.create [ CommandLine.command "quit" "quit the app" [ "q" ]; CommandLine.command "help" "help" [] ]
    Assert.True((CommandLine.update (Key.OfChar 'x') commands).IsNone)

    let typed = ":q now" |> Seq.fold (fun m c -> CommandLine.update (Key.OfChar c) m |> Option.get) commands
    Assert.Equal(Some "q now", CommandLine.text typed)
    Assert.Equal<string list>([ "quit" ], CommandLine.matches typed |> List.map _.Name)

    let ran = CommandLine.update (Key.Plain ConsoleKey.Enter) typed |> Option.get
    Assert.False(CommandLine.isOpen ran)
    Assert.Equal(Some("quit", "now"), CommandLine.invoked ran)

    let unknown =
        ":nope" |> Seq.fold (fun m c -> CommandLine.update (Key.OfChar c) m |> Option.get) commands
        |> CommandLine.update (Key.Plain ConsoleKey.Enter)
        |> Option.get

    Assert.Equal(Some "Unknown command :nope. Commands: :quit, :help", CommandLine.message unknown)
    Assert.Equal(2, (CommandLine.help commands).Length)
