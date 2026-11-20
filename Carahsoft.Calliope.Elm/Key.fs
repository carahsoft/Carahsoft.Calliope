namespace Carahsoft.Calliope.Elm

open System
open Carahsoft.Calliope

/// A key press with the modifiers split out. Match letters on Char rather than Key and Shift, since
/// terminals don't reliably report Shift.
type Key =
    { Key: ConsoleKey
      Char: char
      Ctrl: bool
      Shift: bool
      Alt: bool }

    static member Of(k: KeyPressMsg) =
        { Key = k.Key
          Char = k.KeyChar
          Ctrl = k.Modifiers.HasFlag ConsoleModifiers.Control
          Shift = k.Modifiers.HasFlag ConsoleModifiers.Shift
          Alt = k.Modifiers.HasFlag ConsoleModifiers.Alt }

    /// A key with no modifiers, for tests and key matching
    static member Plain(key: ConsoleKey, ?c: char) =
        { Key = key
          Char = defaultArg c '\000'
          Ctrl = false
          Shift = false
          Alt = false }

    /// A typed character, with Shift for capitals
    static member OfChar(c: char) =
        let key =
            if Char.IsLetter c then enum<ConsoleKey> (int (Char.ToUpperInvariant c))
            elif Char.IsDigit c then enum<ConsoleKey> (int c)
            elif c = ' ' then ConsoleKey.Spacebar
            else ConsoleKey.NoName

        { Key = key
          Char = c
          Ctrl = false
          Shift = Char.IsUpper c
          Alt = false }

    /// Ctrl+N/Ctrl+P become ↓/↑, so they work wherever the arrows do, including forms where j/k are
    /// typed as text. Terminals send them as control characters, which don't always come with the
    /// Control modifier. Opt in by normalizing keys in your update.
    static member Normalize(key: Key) =
        match key.Key, key.Char with
        | ConsoleKey.N, _ when key.Ctrl -> Key.Plain ConsoleKey.DownArrow
        | _, '\x0e' -> Key.Plain ConsoleKey.DownArrow
        | ConsoleKey.P, _ when key.Ctrl -> Key.Plain ConsoleKey.UpArrow
        | _, '\x10' -> Key.Plain ConsoleKey.UpArrow
        | _ -> key

    /// Calliope's message for the key, to pass to its components
    member k.ToMsg() =
        let modifiers =
            (if k.Ctrl then ConsoleModifiers.Control else enum 0)
            ||| (if k.Shift then ConsoleModifiers.Shift else enum 0)
            ||| (if k.Alt then ConsoleModifiers.Alt else enum 0)

        KeyPressMsg(Key = k.Key, KeyChar = k.Char, Modifiers = modifiers)
