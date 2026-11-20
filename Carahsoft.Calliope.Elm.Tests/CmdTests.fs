module Carahsoft.Calliope.Elm.Tests.CmdTests

open System
open System.Threading.Tasks
open Xunit
open Carahsoft.Calliope
open Carahsoft.Calliope.Elm

// Keys

[<Fact>]
let ``keys convert to Calliope's messages and back`` () =
    let key = { Key.Plain(ConsoleKey.C, '\003') with Ctrl = true; Shift = true }
    let msg = key.ToMsg()
    Assert.Equal(ConsoleModifiers.Control ||| ConsoleModifiers.Shift, msg.Modifiers)
    Assert.Equal(key, Key.Of msg)

[<Fact>]
let ``OfChar sets the key and Shift for capitals`` () =
    Assert.Equal(ConsoleKey.G, (Key.OfChar 'G').Key)
    Assert.True (Key.OfChar 'G').Shift
    Assert.Equal(ConsoleKey.D5, (Key.OfChar '5').Key)
    Assert.Equal(ConsoleKey.Spacebar, (Key.OfChar ' ').Key)

[<Fact>]
let ``Normalize turns ctrl+n and ctrl+p into arrows`` () =
    Assert.Equal(ConsoleKey.DownArrow, (Key.Normalize { Key.Plain ConsoleKey.N with Ctrl = true }).Key)
    Assert.Equal(ConsoleKey.UpArrow, (Key.Normalize(Key.Plain(ConsoleKey.NoName, '\x10'))).Key)
    Assert.Equal(Key.OfChar 'n', Key.Normalize(Key.OfChar 'n'))

// Cmd and Testing

type Msg =
    | Got of int
    | Text of string

[<Fact>]
let ``runEffects runs tasks and skips slow ones`` () =
    let cmd =
        Cmd.batch
            [ Cmd.ofMsg (Got 1)
              Cmd.ofTask (fun () -> Task.FromResult 2) Got
              Cmd.delay (TimeSpan.FromHours 1.0) (Got 3)
              Cmd.every (TimeSpan.FromHours 1.0) (Got 4)
              Cmd.quit ]

    Assert.Equal<Msg list>([ Got 1; Got 2 ], Testing.runEffects cmd)
    Assert.True(Testing.quits cmd)
    Assert.False(Testing.quits (Cmd.ofMsg (Got 1)))

[<Fact>]
let ``streams can send many messages`` () =
    let cmd =
        Cmd.stream (fun _ send ->
            for i in 1..3 do
                send (Got i)

            Task.CompletedTask)

    Assert.Equal<Msg list>([ Got 1; Got 2; Got 3 ], Testing.runEffects cmd)

[<Fact>]
let ``map converts the messages`` () =
    let cmd = Cmd.ofMsg 5 |> Cmd.map (fun n -> Text(string n))
    Assert.Equal<Msg list>([ Text "5" ], Testing.runEffects cmd)

[<Fact>]
let ``withKey keys every effect`` () =
    let cmd = Cmd.batch [ Cmd.ofMsg 1; Cmd.quit ] |> Cmd.withKey "k"

    match cmd with
    | [ Effect.Run(Some "k", _); Effect.Quit ] -> ()
    | other -> failwith $"unexpected {other}"

[<Fact>]
let ``effects that throw fail the test with the exception`` () =
    let cmd = Cmd.ofTask (fun () -> Task.FromException<int>(InvalidOperationException "boom")) Got
    let e = Assert.Throws<InvalidOperationException>(fun () -> Testing.runEffects cmd |> ignore)
    Assert.Equal("boom", e.Message)

[<Fact>]
let ``settle runs each effect once`` () =
    // An effect that consumes what it reads, like a log batch: running it twice would lose lines
    let mutable reads = 0

    let read () =
        reads <- reads + 1
        Cmd.ofMsg (Got reads)

    let update msg (seen: int list) =
        match msg with
        | Got n when n < 3 -> n :: seen, read ()
        | Got n -> n :: seen, Cmd.quit
        | Text _ -> seen, Cmd.none

    let seen, quit = Testing.settle update ([], read ())
    Assert.Equal<int list>([ 3; 2; 1 ], seen)
    Assert.Equal(3, reads)
    Assert.True quit

[<Fact>]
let ``settle stops a runaway loop`` () =
    let update (_: Msg) (n: int) = n + 1, Cmd.ofMsg (Got 0)
    Assert.Throws<Exception>(fun () -> Testing.settle update (0, Cmd.ofMsg (Got 0)) |> ignore) |> ignore

// ElmProgram

let private program () =
    let init () = [], Cmd.ofMsg (Text "started")

    let update msg (log: string list) =
        match msg with
        | Text "quit" -> log, Cmd.quit
        | Text t -> t :: log, Cmd.none
        | Got n -> string n :: log, Cmd.none

    let ofCalliope (msg: CalliopeMsg) =
        match msg with
        | :? KeyPressMsg as k -> Some(Text(string k.KeyChar))
        | _ -> None

    ElmProgram(init, update, (fun log -> String.Join(",", List.rev log)), ofCalliope)

[<Fact>]
let ``the model exists before Init`` () =
    let p = program ()
    let calliope = p :> ICalliopeProgram
    // A resize can arrive before Init; it's ignored rather than failing
    Assert.Null(calliope.Update(WindowSizeChangeMsg(ScreenWidth = 80, ScreenHeight = 24)))
    Assert.Equal("", calliope.View())

[<Fact>]
let ``init's effects send app messages through Calliope`` () =
    task {
        let p = program ()
        let calliope = p :> ICalliopeProgram
        let! msg = calliope.Init().CommandFunc.Invoke()
        Assert.Null(calliope.Update msg)
        calliope.Update(KeyPressMsg(KeyChar = 'x')) |> ignore
        Assert.Equal("started,x", calliope.View())
    }

[<Fact>]
let ``quit becomes Calliope's quit`` () =
    task {
        let p = program ()
        let cmd = (p :> ICalliopeProgram).Update(KeyPressMsg(KeyChar = 'q'))
        Assert.Null cmd
        let! _ = (p :> ICalliopeProgram).Init().CommandFunc.Invoke()
        let cmd = Effects.toCalliope (Cmd.quit: Cmd<Msg>)
        Assert.Same(CalliopeCmd.Quit, cmd)
    }

[<Fact>]
let ``keys and cancels reach Calliope's commands`` () =
    let keyed = Effects.toCalliope (Cmd.ofMsg (Got 1) |> Cmd.withKey "load")
    Assert.Equal("load", keyed.Key)
    Assert.Null(Effects.toCalliope Cmd.none)
    Assert.NotNull(Effects.toCalliope (Cmd.cancel "load": Cmd<Msg>))

[<Fact>]
let ``a view that throws shows the error`` () =
    let p = ElmProgram((fun () -> 0, Cmd.none), (fun (_: Msg) m -> m, Cmd.none), (fun _ -> failwith "broken"), (fun _ -> None))
    Assert.StartsWith("Failed to render: broken", (p :> ICalliopeProgram).View())

    let custom =
        ElmProgram((fun () -> 0, Cmd.none), (fun (_: Msg) m -> m, Cmd.none), (fun _ -> failwith "broken"), (fun _ -> None), renderError = fun e -> "oops: " + e.Message)

    Assert.Equal("oops: broken", (custom :> ICalliopeProgram).View())
