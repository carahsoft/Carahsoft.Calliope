using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Tests;

/// <summary>
/// Key presses for tests
/// </summary>
internal static class Keys
{
    public static KeyPressMsg Key(ConsoleKey key, char keyChar = '\0', ConsoleModifiers modifiers = 0) =>
        new() { Key = key, KeyChar = keyChar, Modifiers = modifiers };

    /// <summary>
    /// A typed character, with Shift for capitals
    /// </summary>
    public static KeyPressMsg Char(char c)
    {
        var key = char.IsLetter(c) ? (ConsoleKey)char.ToUpperInvariant(c)
            : char.IsDigit(c) ? (ConsoleKey)c
            : c == ' ' ? ConsoleKey.Spacebar
            : ConsoleKey.NoName;
        return Key(key, c, char.IsUpper(c) ? ConsoleModifiers.Shift : 0);
    }

    public static readonly KeyPressMsg Enter = Key(ConsoleKey.Enter, '\r');
    public static readonly KeyPressMsg Escape = Key(ConsoleKey.Escape, '\x1b');
    public static readonly KeyPressMsg Tab = Key(ConsoleKey.Tab, '\t');
    public static readonly KeyPressMsg ShiftTab = Key(ConsoleKey.Tab, '\t', ConsoleModifiers.Shift);
    public static readonly KeyPressMsg Space = Key(ConsoleKey.Spacebar, ' ');
    public static readonly KeyPressMsg Up = Key(ConsoleKey.UpArrow);
    public static readonly KeyPressMsg Down = Key(ConsoleKey.DownArrow);
    public static readonly KeyPressMsg Left = Key(ConsoleKey.LeftArrow);
    public static readonly KeyPressMsg Right = Key(ConsoleKey.RightArrow);
    public static readonly KeyPressMsg Home = Key(ConsoleKey.Home);
    public static readonly KeyPressMsg End = Key(ConsoleKey.End);
    public static readonly KeyPressMsg Backspace = Key(ConsoleKey.Backspace, '\b');

    public static string Plain(string s) => Ansi.Strip(s);

    public static List<string> Plain(IEnumerable<string> lines) => lines.Select(Ansi.Strip).ToList();
}
