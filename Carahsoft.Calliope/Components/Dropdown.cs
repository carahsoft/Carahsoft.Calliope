using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// A choice between options, and its list when open
    /// </summary>
    /// <param name="Options">The options to choose from</param>
    /// <param name="Selected">The index of the chosen option</param>
    public sealed record DropdownState(IReadOnlyList<string> Options, int Selected = 0)
    {
        public bool IsOpen { get; init; }
        /// <summary>
        /// The option the cursor is on while the list is open
        /// </summary>
        public int Highlight { get; init; }
        /// <summary>
        /// The first option shown, when there are more than fit
        /// </summary>
        public int Offset { get; init; }
        /// <summary>
        /// Options shown at once; longer lists scroll
        /// </summary>
        public int Rows { get; init; } = 8;

        /// <summary>
        /// The chosen option, or null if there are no options
        /// </summary>
        public string? Value => Selected >= 0 && Selected < Options.Count ? Options[Selected] : null;
    }

    /// <summary>
    /// A floating list to choose an option from. While closed, Space opens it
    /// on the current value and ←/→ step through the options. While open, it
    /// takes every key: ↑/↓ (or k/j), PgUp/PgDn and Home/End move, Enter or
    /// Space picks, and Esc or Tab close it without changing the value.
    /// Draw the open list over the screen with <see cref="Window.FloatBelow(int, int, int, IReadOnlyList{string}, IReadOnlyList{string})"/>
    /// so it doesn't push anything down.
    /// </summary>
    public static class Dropdown
    {
        public static DropdownState Create(IEnumerable<string> options, string? value = null)
        {
            var list = options.ToList();
            return new DropdownState(list, Math.Max(0, value == null ? 0 : list.IndexOf(value)));
        }

        /// <summary>
        /// Opens the list on the current value
        /// </summary>
        public static DropdownState Open(DropdownState state) =>
            state.Options.Count == 0 ? state : state with
            {
                IsOpen = true,
                Highlight = state.Selected,
                Offset = Math.Max(0, state.Selected - state.Rows + 1),
            };

        /// <summary>
        /// Closes the list without changing the value
        /// </summary>
        public static DropdownState Close(DropdownState state) => state with { IsOpen = false };

        /// <summary>
        /// Handles a key. Returns null for keys it doesn't use, which happens
        /// only while the list is closed (e.g. Enter, Esc or Tab), so the caller
        /// can handle them.
        /// </summary>
        public static DropdownState? Update(DropdownState state, KeyPressMsg key)
        {
            var count = state.Options.Count;

            if (!state.IsOpen)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        return Open(state);
                    case ConsoleKey.LeftArrow when count > 0:
                        return state with { Selected = (state.Selected - 1 + count) % count };
                    case ConsoleKey.RightArrow when count > 0:
                        return state with { Selected = (state.Selected + 1) % count };
                    default:
                        return null;
                }
            }

            DropdownState MoveTo(int index)
            {
                index = Math.Clamp(index, 0, Math.Max(0, count - 1));
                // Keep the highlighted option in view
                var offset = index < state.Offset ? index
                    : index >= state.Offset + state.Rows ? index - state.Rows + 1
                    : state.Offset;
                return state with { Highlight = index, Offset = offset };
            }

            // j/k match on the character since terminals don't reliably report Shift
            if (key.KeyChar == 'k')
                return MoveTo(state.Highlight - 1);
            if (key.KeyChar == 'j')
                return MoveTo(state.Highlight + 1);

            return key.Key switch
            {
                ConsoleKey.Escape or ConsoleKey.Tab => Close(state),
                ConsoleKey.Enter or ConsoleKey.Spacebar => state with { IsOpen = false, Selected = state.Highlight },
                ConsoleKey.UpArrow => MoveTo(state.Highlight - 1),
                ConsoleKey.DownArrow => MoveTo(state.Highlight + 1),
                ConsoleKey.PageUp => MoveTo(state.Highlight - state.Rows),
                ConsoleKey.PageDown => MoveTo(state.Highlight + state.Rows),
                ConsoleKey.Home => MoveTo(0),
                ConsoleKey.End => MoveTo(count - 1),
                _ => state,
            };
        }

        /// <summary>
        /// The current value with a ▾ marking it as a dropdown
        /// </summary>
        public static string Render(DropdownState state, bool focused, Theme? theme = null)
        {
            var text = $"{state.Value} ▾";
            return focused ? (theme ?? Theme.Default).Selected.Apply(text) : text;
        }

        /// <summary>
        /// The open list's box: the visible options with the highlighted one
        /// marked, and "↑ n"/"↓ n" in the borders when there are more than fit.
        /// It's as wide as the longest option, between 12 and
        /// <paramref name="maxWidth"/> columns. Empty while the list is closed.
        /// </summary>
        public static IReadOnlyList<string> RenderList(DropdownState state, int maxWidth, Theme? theme = null)
        {
            if (!state.IsOpen)
                return [];

            theme ??= Theme.Default;
            var options = state.Options;
            var widest = options.Count == 0 ? 0 : options.Max(Ansi.VisibleLength);
            var width = Math.Clamp(widest + 4, 12, Math.Max(12, maxWidth));
            var border = Style.FromColor(theme.Accent);
            var inner = Math.Max(1, width - 4);
            var visible = options.Skip(state.Offset).Take(state.Rows).ToList();
            var below = options.Count - state.Offset - visible.Count;

            string Edge(char left, char right, string more)
            {
                var label = more == "" ? "" : $"─ {more} ";
                return border.Apply(left + label + new string('─', Math.Max(0, width - 2 - label.Length)) + right);
            }

            var lines = new List<string> { Edge('╭', '╮', state.Offset > 0 ? $"↑ {state.Offset}" : "") };
            for (int i = 0; i < visible.Count; i++)
            {
                var text = Ansi.PadRight(visible[i], inner);
                if (state.Offset + i == state.Highlight)
                    text = theme.Selected.Apply(text);
                lines.Add(border.Apply("│ ") + text + border.Apply(" │"));
            }
            lines.Add(Edge('╰', '╯', below > 0 ? $"↓ {below}" : ""));
            return lines;
        }
    }

    /// <summary>
    /// A <see cref="Dropdown"/> as a program, for C# apps that keep mutable
    /// components. Its view is the value, with the list below it while open.
    /// </summary>
    public class DropdownView : ICalliopeProgram
    {
        public DropdownView(IEnumerable<string> options, string? value = null)
        {
            State = Dropdown.Create(options, value);
        }

        public DropdownState State { get; set; }
        public string? Value => State.Value;
        public bool Focused { get; set; } = true;
        public int MaxWidth { get; set; } = 40;
        public Theme Theme { get; set; } = Theme.Default;

        public CalliopeCmd? Init() => null;

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (Focused && msg is KeyPressMsg key && Dropdown.Update(State, key) is { } next)
                State = next;
            return null;
        }

        public string View() =>
            string.Join('\n', [Dropdown.Render(State, Focused, Theme), .. Dropdown.RenderList(State, MaxWidth, Theme)]);
    }
}
