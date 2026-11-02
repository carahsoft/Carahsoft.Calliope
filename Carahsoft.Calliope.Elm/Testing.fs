/// Helpers for testing update functions without a terminal
module Carahsoft.Calliope.Elm.Testing

open System
open System.Threading
open System.Threading.Tasks

/// True if the command quits the program
let quits (cmd: Cmd<'msg>) =
    cmd |> List.exists (function
        | Effect.Quit -> true
        | _ -> false)

/// Runs each effect in the command once, returning the messages they sent within the timeout. Slow
/// effects, like a refresh tick, are cancelled once the timeout passes. An effect that throws fails
/// with its exception.
let runEffectsWithin (timeout: TimeSpan) (cmd: Cmd<'msg>) : 'msg list =
    [ for effect in cmd do
          match effect with
          | Effect.Run(_, run) ->
              use cts = new CancellationTokenSource()
              let messages = ResizeArray()
              let send msg = lock messages (fun () -> messages.Add msg)
              let task = try run cts.Token send with e -> Task.FromException e

              try
                  task.Wait timeout |> ignore
              with _ ->
                  ()

              cts.Cancel()

              if task.IsFaulted then
                  raise (task.Exception.GetBaseException())

              yield! lock messages (fun () -> List.ofSeq messages)
          | Effect.Cancel _
          | Effect.Quit -> () ]

/// Runs each effect in the command once, returning the messages they sent within 100ms
let runEffects (cmd: Cmd<'msg>) = runEffectsWithin (TimeSpan.FromMilliseconds 100.0) cmd

/// Feeds each effect's messages back into update until there are no more. Each effect runs only
/// once, since some (like reading the next batch of a stream) consume what they read. Returns the
/// final model and whether any command quit.
let settle (update: 'msg -> 'model -> 'model * Cmd<'msg>) (model: 'model, cmd: Cmd<'msg>) : 'model * bool =
    let rec go rounds (model, cmd) =
        if rounds > 1000 then
            failwith "settle: effects kept producing messages after 1000 rounds, e.g. a fast repeating timer"

        let quit = quits cmd

        match runEffects cmd with
        | [] -> model, quit
        | messages ->
            let model, cmds =
                messages
                |> List.fold
                    (fun (m, cmds) msg ->
                        let m, c = update msg m
                        m, c :: cmds)
                    (model, [])

            let model, quitLater = go (rounds + 1) (model, Cmd.batch (List.rev cmds))
            model, quit || quitLater

    go 0 (model, cmd)
