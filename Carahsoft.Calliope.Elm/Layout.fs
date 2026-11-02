namespace Carahsoft.Calliope.Elm

open Carahsoft.Calliope
open Carahsoft.Calliope.Text

/// Lines of styled text, measured, cut and arranged ignoring escape codes. These are Calliope's
/// Ansi, Block, Frame and Window helpers with F# lists and curried arguments; the *With versions
/// take a FrameStyle instead of the default theme's.
module Layout =
    /// Clears all text styles
    let reset = Ansi.Reset

    let visibleLength (s: string) = Ansi.VisibleLength s

    /// Cuts a line to at most width columns, ending with … when cut
    let truncate (width: int) (s: string) = Ansi.Truncate(s, width)

    /// Cuts a line to at most width columns with no ellipsis, closing any style it cuts off
    let cut (width: int) (s: string) = Ansi.Cut(s, width)

    /// Drops the first n columns of a line, keeping the styles still active there
    let skip (n: int) (s: string) = Ansi.Skip(s, n)

    /// Truncates or pads a line to exactly width columns
    let padRight (width: int) (s: string) = Ansi.PadRight(s, width)

    let padLeft (width: int) (s: string) = Ansi.PadLeft(s, width)

    let center (width: int) (s: string) = Ansi.Center(s, width)

    /// Splits plain text into lines of at most width characters, breaking on spaces where possible
    let wrap (width: int) (text: string) = Ansi.Wrap(text, width) |> List.ofSeq

    /// Splits multi-line text into lines
    let lines (s: string) = Block.Lines s |> List.ofSeq

    /// Exactly height lines: extra lines are dropped and missing ones are blank
    let fitHeight (height: int) (lines: string list) = Block.FitHeight(lines, height) |> List.ofSeq

    /// Places blocks side by side with a gap. Each block is padded to its widest line.
    let hjoin (gap: int) (blocks: string list list) =
        Block.JoinHorizontal(gap, blocks |> List.map (fun b -> b :> System.Collections.Generic.IReadOnlyList<string>))
        |> List.ofSeq

    /// A box of the given total width around the lines, with an optional title in the top border
    let boxWith (style: FrameStyle) (title: string) (width: int) (content: string list) =
        Frame.Box(title, width, content, style) |> List.ofSeq

    let box (title: string) (width: int) (content: string list) = boxWith Theme.Default.Box title width content

    /// A fixed-width dialog box with the title in the top border and the footer, if any, in a
    /// divider above the bottom border. Wrap long text to width - 4.
    let dialogWith (style: FrameStyle) (title: string) (footer: string) (width: int) (content: string list) =
        Frame.Dialog(title, footer, width, content, style) |> List.ofSeq

    let dialog (title: string) (footer: string) (width: int) (content: string list) =
        dialogWith Theme.Default.Dialog title footer width content

    /// A dim horizontal line, for dividing sections inside a box
    let rule (width: int) = Frame.Rule width

    /// A full-width bar with text on the left and right
    let bar (style: Style) (width: int) (left: string) (right: string) = Frame.Bar(style, width, left, right)

    /// Draws lines over others like a window, from the given row and column, keeping what's to
    /// the left and right of it. Adds lines if needed.
    let overlayAt (row: int) (column: int) (over: string list) (lines: string list) =
        Window.OverlayAt(row, column, over, lines) |> List.ofSeq

    /// Draws a centered dialog over the background, sized to its content (up to the screen).
    /// Returns exactly height lines, and the row and column its content starts at, so other
    /// windows can be placed relative to it.
    let popupWith (style: FrameStyle) (width: int) (height: int) (title: string) (footer: string) (content: string list) (background: string list) =
        let popup = Window.Popup(width, height, title, footer, content, background, style)
        List.ofSeq popup.Lines, (popup.ContentRow, popup.ContentColumn)

    let popup width height title footer content background =
        popupWith Theme.Default.Dialog width height title footer content background

    /// Draws a centered popup over the background lines
    let overlay width height title footer content background =
        popup width height title footer content background |> fst

    /// Draws a small window (e.g. a dropdown list) below the line at row, or above it if it wouldn't
    /// fit on screen below
    let floatBelow (height: int) (row: int) (column: int) (window: string list) (lines: string list) =
        Window.FloatBelow(height, row, column, window, lines) |> List.ofSeq
