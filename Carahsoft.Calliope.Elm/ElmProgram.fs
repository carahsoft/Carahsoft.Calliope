namespace Carahsoft.Calliope.Elm

open System
open System.Threading.Tasks
open Carahsoft.Calliope

/// Carries app messages through Calliope
type internal AppMsg<'msg>(msg: 'msg) =
    inherit CalliopeMsg()
    member _.Msg = msg

module internal Effects =
    /// Calliope's command for the effects, or null if there are none
    let toCalliope (cmd: Cmd<'msg>) : CalliopeCmd =
        let cmds =
            cmd
            |> List.map (function
                | Effect.Run(key, run) ->
                    let calliope =
                        CalliopeCmd.Stream(fun token send -> run token (fun msg -> send.Invoke(AppMsg msg)))

                    match key with
                    | Some key -> calliope.WithKey key
                    | None -> calliope
                | Effect.Cancel key -> CalliopeCmd.Cancel key
                | Effect.Quit -> CalliopeCmd.Quit)
            |> Array.ofList

        match cmds with
        | [||] -> null
        | [| single |] -> single
        | many -> CalliopeCmd.Combine many

/// Runs a pure init/update/view program as a Calliope program
/// <param name="init">The first model and the effects to start with</param>
/// <param name="update">Handles a message, returning the new model and the effects to run</param>
/// <param name="view">Draws the model</param>
/// <param name="ofCalliope">Converts Calliope's own messages (keys, resizes, errors) to app messages; None ignores them</param>
/// <param name="renderError">What to draw if view throws. Defaults to the error and how to quit.</param>
type ElmProgram<'model, 'msg>
    (
        init: unit -> 'model * Cmd<'msg>,
        update: 'msg -> 'model -> 'model * Cmd<'msg>,
        view: 'model -> string,
        ofCalliope: CalliopeMsg -> 'msg option,
        ?renderError: exn -> string
    ) =
    // Calliope can send a resize before Init, so build the model up front
    let initialModel, initialCmd = init ()
    let mutable model = initialModel

    let renderError =
        defaultArg renderError (fun e -> $"Failed to render: {e.Message}\n\nPress Ctrl+C to quit")

    /// The current model
    member _.Model = model

    interface ICalliopeProgram with
        member _.Init() = Effects.toCalliope initialCmd

        member _.Update(msg) =
            let appMsg =
                match msg with
                | :? AppMsg<'msg> as a -> Some a.Msg
                | other -> ofCalliope other

            match appMsg with
            | Some m ->
                let next, cmd = update m model
                model <- next
                Effects.toCalliope cmd
            | None -> null

        member _.View() =
            // An exception here would stop the program; showing it is friendlier
            try
                view model
            with e ->
                renderError e
