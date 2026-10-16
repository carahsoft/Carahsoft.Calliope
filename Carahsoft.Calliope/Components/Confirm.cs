using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    public enum ConfirmResult
    {
        /// <summary>
        /// Not answered yet
        /// </summary>
        Pending,
        Confirmed,
        Cancelled,
    }

    /// <summary>
    /// A Yes/No question. <see cref="Yes"/> is the highlighted button and
    /// <see cref="Result"/> is the answer once given.
    /// </summary>
    public sealed record ConfirmState(string Title, IReadOnlyList<string> Message)
    {
        public bool Yes { get; init; }
        public ConfirmResult Result { get; init; } = ConfirmResult.Pending;
    }

    /// <summary>
    /// A Yes/No prompt, usually drawn as a popup. It defaults to No so a stray
    /// Enter doesn't do anything destructive. Keys: y/n answer directly,
    /// ←/→/Tab/h/l switch buttons, Enter picks the highlighted one, Esc cancels.
    /// </summary>
    public static class Confirm
    {
        public const string Footer = "y/n · ←/→ · enter";

        public static ConfirmState Create(string title, params IEnumerable<string> message) =>
            new(title, message.ToList());

        public static ConfirmState Update(ConfirmState state, KeyPressMsg key)
        {
            switch (char.ToLowerInvariant(key.KeyChar))
            {
                case 'y':
                    return state with { Result = ConfirmResult.Confirmed };
                case 'n':
                    return state with { Result = ConfirmResult.Cancelled };
                case 'h':
                case 'l':
                    return state with { Yes = !state.Yes };
            }

            return key.Key switch
            {
                ConsoleKey.Escape => state with { Result = ConfirmResult.Cancelled },
                ConsoleKey.Enter => state with { Result = state.Yes ? ConfirmResult.Confirmed : ConfirmResult.Cancelled },
                ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.Tab => state with { Yes = !state.Yes },
                _ => state,
            };
        }

        /// <summary>
        /// The message and the buttons, to draw inside a dialog
        /// </summary>
        public static IReadOnlyList<string> Content(ConfirmState state, Theme? theme = null)
        {
            theme ??= Theme.Default;
            string Button(bool selected, string text) =>
                (selected ? theme.Selected : theme.Dim).Apply($" {text} ");

            return [.. state.Message, "", "      " + Button(state.Yes, "Yes") + "   " + Button(!state.Yes, "No")];
        }

        /// <summary>
        /// The prompt as a centered popup over the background
        /// </summary>
        public static IReadOnlyList<string> Render(ConfirmState state, int width, int height, IReadOnlyList<string> background, Theme? theme = null) =>
            Window.Popup(width, height, state.Title, Footer, Content(state, theme), background, (theme ?? Theme.Default).Dialog).Lines;
    }

    /// <summary>
    /// A <see cref="Confirm"/> prompt as a program, for C# apps that keep
    /// mutable components. On its own it draws the dialog box; use
    /// <see cref="RenderOverlay"/> to draw it over other content.
    /// </summary>
    public class ConfirmView : ICalliopeProgram
    {
        public ConfirmView(string title, params IEnumerable<string> message)
        {
            State = Confirm.Create(title, message);
        }

        public ConfirmState State { get; set; }
        public ConfirmResult Result => State.Result;
        public Theme Theme { get; set; } = Theme.Default;
        /// <summary>
        /// Quit the program once the prompt is answered, e.g. when it's run on
        /// its own with <see cref="Calliope.NewProgram"/>
        /// </summary>
        public bool QuitWhenAnswered { get; set; }

        public CalliopeCmd? Init() => null;

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is not KeyPressMsg key)
                return null;

            State = Confirm.Update(State, key);
            return QuitWhenAnswered && State.Result != ConfirmResult.Pending ? CalliopeCmd.Quit : null;
        }

        public string View()
        {
            var content = Confirm.Content(State, Theme);
            var width = Math.Max(content.Max(Ansi.VisibleLength), Math.Max(State.Title.Length, Confirm.Footer.Length) + 4) + 4;
            return string.Join('\n', Frame.Dialog(State.Title, Confirm.Footer, width, content, Theme.Dialog));
        }

        public string RenderOverlay(string background, int width, int height) =>
            string.Join('\n', Confirm.Render(State, width, height, Block.Lines(background), Theme));
    }
}
