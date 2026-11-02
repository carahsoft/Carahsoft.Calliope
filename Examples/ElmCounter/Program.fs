// A counter written as a pure Elm-style program. It counts up every second, ←/→ change it, space
// pauses the timer, r asks before resetting, and ctrl+c quits.

open System
open Carahsoft.Calliope
open Carahsoft.Calliope.Elm

type Model =
    { Count: int
      Paused: bool
      Confirm: Confirm.Model option
      Width: int
      Height: int }

type Msg =
    | KeyPressed of Key
    | Resized of width: int * height: int
    | Tick

let tick = Cmd.every (TimeSpan.FromSeconds 1.0) Tick |> Cmd.withKey "tick"

let init () =
    { Count = 0; Paused = false; Confirm = None; Width = 80; Height = 24 }, tick

let update msg model =
    match msg with
    | KeyPressed k when k.Ctrl && k.Key = ConsoleKey.C -> model, Cmd.quit
    | KeyPressed k ->
        match model.Confirm with
        | Some confirm ->
            match Confirm.update k confirm with
            | Confirm.Pending confirm -> { model with Confirm = Some confirm }, Cmd.none
            | Confirm.Confirmed -> { model with Confirm = None; Count = 0 }, Cmd.none
            | Confirm.Cancelled -> { model with Confirm = None }, Cmd.none
        | None ->
            match k.Key, k.Char with
            | ConsoleKey.RightArrow, _ -> { model with Count = model.Count + 1 }, Cmd.none
            | ConsoleKey.LeftArrow, _ -> { model with Count = model.Count - 1 }, Cmd.none
            | _, 'r' -> { model with Confirm = Some(Confirm.create "Reset" [ $"Reset the count of {model.Count}?" ]) }, Cmd.none
            // Cancelling the keyed timer stops it; starting it again replaces any that's running
            | _, ' ' when model.Paused -> { model with Paused = false }, tick
            | _, ' ' -> { model with Paused = true }, Cmd.cancel "tick"
            | _ -> model, Cmd.none
    | Resized(width, height) -> { model with Width = width; Height = height }, Cmd.none
    | Tick -> { model with Count = model.Count + 1 }, Cmd.none

let view model =
    let hints = "←/→ change   space " + (if model.Paused then "resume" else "pause") + "   r reset   ctrl+c quit"

    let lines =
        Layout.box "Counter" (min 50 model.Width) [ $"Count: {model.Count}"; ""; Theme.Default.Dim.Apply hints ]

    let lines =
        match model.Confirm with
        | Some confirm -> Confirm.view model.Width model.Height lines confirm
        | None -> lines

    String.Join("\n", lines)

let ofCalliope (msg: CalliopeMsg) =
    match msg with
    | :? KeyPressMsg as k -> Some(KeyPressed(Key.Of k))
    | :? WindowSizeChangeMsg as w -> Some(Resized(w.ScreenWidth, w.ScreenHeight))
    | _ -> None

Calliope.NewProgram(ElmProgram(init, update, view, ofCalliope)).Fullscreen().RunAsync().Wait()
