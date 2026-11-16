using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// The text and cursor position of a <see cref="TextField"/>
    /// </summary>
    public sealed record TextFieldState(string Text, int Cursor)
    {
        public static TextFieldState Empty { get; } = new("", 0);

        /// <summary>
        /// A field containing the text, with the cursor at the end
        /// </summary>
        public static TextFieldState Of(string? text) => new(text ?? "", (text ?? "").Length);
    }

    /// <summary>
    /// A single line text input. Unlike <see cref="TextInput"/> it never quits
    /// the program and doesn't need cursor blink messages. Supports word
    /// delete (Ctrl+Backspace or Alt+Backspace) and Ctrl+U to clear.
    /// </summary>
    public static class TextField
    {
        /// <summary>
        /// Handles editing keys. Returns null for keys it doesn't use (Enter,
        /// Esc, Tab, ↑/↓…), so the caller can handle them.
        /// </summary>
        public static TextFieldState? Update(TextFieldState state, KeyPressMsg key)
        {
            var cursor = Math.Clamp(state.Cursor, 0, state.Text.Length);
            var before = state.Text[..cursor];
            var after = state.Text[cursor..];

            switch (key.Key)
            {
                case ConsoleKey.Backspace when key.Ctrl() || key.Alt():
                    // Delete the previous word
                    var start = before.TrimEnd().LastIndexOf(' ') + 1;
                    return new(before[..start] + after, start);
                case ConsoleKey.Backspace:
                    return cursor > 0 ? new(before[..^1] + after, cursor - 1) : state;
                case ConsoleKey.Delete:
                    return after.Length > 0 ? state with { Text = before + after[1..] } : state;
                case ConsoleKey.LeftArrow:
                    return state with { Cursor = Math.Max(0, cursor - 1) };
                case ConsoleKey.RightArrow:
                    return state with { Cursor = Math.Min(state.Text.Length, cursor + 1) };
                case ConsoleKey.Home:
                    return state with { Cursor = 0 };
                case ConsoleKey.End:
                    return state with { Cursor = state.Text.Length };
                case ConsoleKey.U when key.Ctrl():
                    return TextFieldState.Empty;
            }

            if (key.Ctrl() || key.Alt())
                return null;

            if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                return new(before + key.KeyChar + after, cursor + 1);

            return null;
        }

        /// <summary>
        /// Renders the text in at most <paramref name="width"/> columns,
        /// scrolled so the cursor stays visible. When focused, the cursor is
        /// drawn as an inverted block.
        /// </summary>
        public static string Render(TextFieldState state, int width, bool focused)
        {
            width = Math.Max(1, width);
            var cursor = Math.Clamp(state.Cursor, 0, state.Text.Length);
            var offset = Math.Max(0, cursor - width + 1);
            var visible = state.Text[Math.Min(offset, state.Text.Length)..];

            if (!focused)
                return Ansi.Truncate(visible, width);

            var cursorChar = cursor < state.Text.Length ? state.Text[cursor].ToString() : " ";
            var at = cursor - offset;
            var left = visible[..Math.Min(at, visible.Length)];
            var right = at + 1 <= visible.Length ? visible[(at + 1)..] : "";
            var line = left + "\x1b[7m" + cursorChar + Ansi.Reset + right;
            // The ellipsis takes the last column, so when the cursor is there, cut instead
            return at < width - 1 ? Ansi.Truncate(line, width) : Ansi.Cut(line, width);
        }
    }

    /// <summary>
    /// A <see cref="TextField"/> as a program, for C# apps that keep mutable
    /// components
    /// </summary>
    public class TextFieldView : ICalliopeProgram
    {
        public TextFieldState State { get; set; } = TextFieldState.Empty;
        public string Text => State.Text;
        public int Width { get; set; } = 40;
        public bool Focused { get; set; } = true;

        public CalliopeCmd? Init() => null;

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (Focused && msg is KeyPressMsg key && TextField.Update(State, key) is { } next)
                State = next;
            return null;
        }

        public string View() => TextField.Render(State, Width, Focused);
    }
}
