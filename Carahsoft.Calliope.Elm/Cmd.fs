namespace Carahsoft.Calliope.Elm

open System
open System.Threading
open System.Threading.Tasks

/// Something for the program to do after an update
[<RequireQualifiedAccess>]
type Effect<'msg> =
    /// Runs in the background, sending any number of messages, until it finishes or is cancelled.
    /// Starting one with a key cancels the running one with the same key.
    | Run of key: string option * run: (CancellationToken -> ('msg -> unit) -> Task)
    /// Cancels the running effect with the key
    | Cancel of key: string
    | Quit

/// Effects to run after an update. Each message they send is fed back into update.
type Cmd<'msg> = Effect<'msg> list

module Cmd =
    let none: Cmd<'msg> = []

    let batch (cmds: Cmd<'msg> list) : Cmd<'msg> = List.concat cmds

    let quit: Cmd<'msg> = [ Effect.Quit ]

    let ofMsg (msg: 'msg) : Cmd<'msg> =
        [ Effect.Run(None, fun _ send -> send msg; Task.CompletedTask) ]

    /// Runs a task, turning its result into a message. If it throws, the program gets Calliope's
    /// ErrorMsg instead.
    let ofTask (f: unit -> Task<'a>) (toMsg: 'a -> 'msg) : Cmd<'msg> =
        [ Effect.Run(None, fun _ send ->
              task {
                  let! result = f ()
                  send (toMsg result)
              }) ]

    /// Runs a task that can be cancelled: when the program quits, or by its key (see withKey)
    let ofCancellableTask (f: CancellationToken -> Task<'a>) (toMsg: 'a -> 'msg) : Cmd<'msg> =
        [ Effect.Run(None, fun token send ->
              task {
                  let! result = f token
                  send (toMsg result)
              }) ]

    /// Runs a function that sends any number of messages, e.g. lines of a log stream, until it
    /// finishes or the token is cancelled
    let stream (f: CancellationToken -> ('msg -> unit) -> Task) : Cmd<'msg> = [ Effect.Run(None, f) ]

    /// Sends the message after a delay
    let delay (ts: TimeSpan) (msg: 'msg) : Cmd<'msg> =
        [ Effect.Run(None, fun token send ->
              task {
                  do! Task.Delay(ts, token)
                  send msg
              }) ]

    /// Sends the message every interval until the program quits. Give it a key to stop it with
    /// cancel, or to restart it.
    let every (interval: TimeSpan) (msg: 'msg) : Cmd<'msg> =
        [ Effect.Run(None, fun token send ->
              task {
                  use timer = new PeriodicTimer(interval)

                  while! timer.WaitForNextTickAsync token do
                      send msg
              }) ]

    /// Gives the command's effects a key: starting it cancels the running effect with the same key,
    /// so e.g. a new request replaces one in progress and its old response is never delivered. Use
    /// it on a single effect; effects in a batch with the same key cancel each other.
    let withKey (key: string) (cmd: Cmd<'msg>) : Cmd<'msg> =
        cmd
        |> List.map (function
            | Effect.Run(_, run) -> Effect.Run(Some key, run)
            | other -> other)

    /// Cancels the running effect with the key, if there is one
    let cancel (key: string) : Cmd<'msg> = [ Effect.Cancel key ]

    let map (f: 'a -> 'b) (cmd: Cmd<'a>) : Cmd<'b> =
        cmd
        |> List.map (function
            | Effect.Run(key, run) -> Effect.Run(key, fun token send -> run token (f >> send))
            | Effect.Cancel key -> Effect.Cancel key
            | Effect.Quit -> Effect.Quit)
