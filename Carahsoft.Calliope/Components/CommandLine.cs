using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// A command that can be run from a <see cref="CommandLine"/>
    /// </summary>
    /// <param name="Name">What's typed after the colon, e.g. "quit"</param>
    /// <param name="Description">Shown in suggestions and help</param>
    public sealed record CommandLineCommand(string Name, string Description)
    {
        /// <summary>
        /// Other names that run the command, e.g. "q"
        /// </summary>
        public IReadOnlyList<string> Aliases { get; init; } = [];
        /// <summary>
        /// Runs the command with the rest of the line (trimmed) as its
        /// arguments. Used by <see cref="CommandLineView"/>; with the state
        /// functions, check <see cref="CommandLineState.Invoked"/> instead.
        /// </summary>
        public Func<string, CalliopeCmd?>? Run { get; init; }

        public IEnumerable<string> Names => [Name, .. Aliases];
    }

    /// <summary>
    /// The commands, the prompt while it's open, and the result of the last
    /// key press
    /// </summary>
    public sealed record CommandLineState(IReadOnlyList<CommandLineCommand> Commands)
    {
        /// <summary>
        /// The text typed after the colon, or null while the prompt is closed
        /// </summary>
        public TextFieldState? Input { get; init; }
        public bool IsOpen => Input != null;
        /// <summary>
        /// The command run by the last key press (Enter), if any
        /// </summary>
        public CommandLineCommand? Invoked { get; init; }
        /// <summary>
        /// The arguments <see cref="Invoked"/> was run with
        /// </summary>
        public string InvokedArgs { get; init; } = "";
        /// <summary>
        /// A message from the last key press, e.g. for an unknown command
        /// </summary>
        public string? Message { get; init; }
    }

    /// <summary>
    /// A vim-style <c>:</c> prompt for running commands by name. : opens it,
    /// Enter runs the command (the first word; the rest are its arguments),
    /// Tab completes the name, and Esc, or Backspace on an empty prompt,
    /// closes it. An unknown command sets a message listing the commands.
    /// </summary>
    public static class CommandLine
    {
        public static CommandLineState Create(params IEnumerable<CommandLineCommand> commands) => new(commands.ToList());

        public static CommandLineState Open(CommandLineState state) =>
            state with { Input = TextFieldState.Empty, Invoked = null, InvokedArgs = "", Message = null };

        public static CommandLineState Close(CommandLineState state) => state with { Input = null };

        /// <summary>
        /// The command with the given name or alias, ignoring case
        /// </summary>
        public static CommandLineCommand? Find(CommandLineState state, string name) =>
            state.Commands.FirstOrDefault(c => c.Names.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)));

        /// <summary>
        /// Commands whose name or an alias starts with what's been typed, for
        /// suggestions and completion
        /// </summary>
        public static IReadOnlyList<CommandLineCommand> Matches(CommandLineState state)
        {
            var name = (state.Input?.Text ?? "").TrimStart().Split(' ', 2)[0];
            return state.Commands
                .Where(c => c.Names.Any(n => n.StartsWith(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Handles a key. While closed, only : is used (to open the prompt) and
        /// other keys return null so the caller can handle them. While open,
        /// the prompt takes every key.
        /// </summary>
        public static CommandLineState? Update(CommandLineState state, KeyPressMsg key)
        {
            state = state with { Invoked = null, InvokedArgs = "", Message = null };

            if (state.Input is not { } input)
                return key.KeyChar == ':' ? Open(state) : null;

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    return Run(Close(state), input.Text);
                case ConsoleKey.Escape:
                    return Close(state);
                case ConsoleKey.Tab:
                    return Matches(state) is [var first, ..] ? state with { Input = TextFieldState.Of(first.Name) } : state;
                // Backspace on an empty line closes it, like vim
                case ConsoleKey.Backspace when input.Text == "":
                    return Close(state);
            }

            return TextField.Update(input, key) is { } next ? state with { Input = next } : state;
        }

        /// <summary>
        /// The prompt: a colon and what's been typed. Empty while closed.
        /// </summary>
        public static string Render(CommandLineState state, int width, Theme? theme = null)
        {
            if (state.Input is not { } input)
                return "";
            return (theme ?? Theme.Default).Key.Apply(":") + TextField.Render(input, Math.Max(1, width - 2), true);
        }

        /// <summary>
        /// The commands matching what's been typed, with their descriptions,
        /// for a status line. Empty while closed.
        /// </summary>
        public static string RenderSuggestions(CommandLineState state, int width, Theme? theme = null)
        {
            if (!state.IsOpen)
                return "";

            theme ??= Theme.Default;
            var matches = Matches(state);
            if (matches.Count == 0)
                return Ansi.Truncate(theme.Warning.Apply("No matching command"), width);

            var text = string.Join("   ", matches.Select(c => theme.Key.Apply(":" + c.Name) + " " + theme.Dim.Apply(c.Description)));
            return Ansi.Truncate(text + theme.Dim.Apply("   (tab completes)"), width);
        }

        /// <summary>
        /// Every command with its names and description, e.g. for a help screen
        /// </summary>
        public static IReadOnlyList<string> RenderHelp(CommandLineState state, Theme? theme = null)
        {
            theme ??= Theme.Default;
            var names = state.Commands.Select(c => string.Join(" ", c.Names.Select(n => ":" + n))).ToList();
            var width = names.Count == 0 ? 0 : names.Max(n => n.Length);
            return state.Commands.Select((c, i) => theme.Key.Apply(Ansi.PadRight(names[i], width)) + "  " + c.Description).ToList();
        }

        private static CommandLineState Run(CommandLineState state, string text)
        {
            var parts = text.Trim().Split(' ', 2, StringSplitOptions.TrimEntries);
            if (parts[0] == "")
                return state;

            if (Find(state, parts[0]) is { } command)
                return state with { Invoked = command, InvokedArgs = parts.Length > 1 ? parts[1] : "" };

            var available = string.Join(", ", state.Commands.Select(c => ":" + c.Name));
            return state with { Message = $"Unknown command :{parts[0]}. Commands: {available}" };
        }
    }

    /// <summary>
    /// A <see cref="CommandLine"/> as a program, for C# apps that keep mutable
    /// components. Runs each command's <see cref="CommandLineCommand.Run"/>
    /// when it's entered, returning its command.
    /// </summary>
    public class CommandLineView : ICalliopeProgram
    {
        public CommandLineView(params IEnumerable<CommandLineCommand> commands)
        {
            State = CommandLine.Create(commands);
        }

        public CommandLineState State { get; set; }
        public bool IsOpen => State.IsOpen;
        public int Width { get; set; } = 80;
        public Theme Theme { get; set; } = Theme.Default;

        public CalliopeCmd? Init() => null;

        /// <summary>
        /// Handles a key, returning whether the command line used it. Keys
        /// it doesn't use (anything but : while closed) are for the caller.
        /// </summary>
        public bool HandleKey(KeyPressMsg key, out CalliopeCmd? cmd)
        {
            cmd = null;
            if (CommandLine.Update(State, key) is not { } next)
            {
                // Clear the last message once the user moves on
                State = State with { Message = null };
                return false;
            }

            State = next;
            if (next.Invoked is { } command)
                cmd = command.Run?.Invoke(next.InvokedArgs);
            return true;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg key && HandleKey(key, out var cmd))
                return cmd;
            return null;
        }

        /// <summary>
        /// The prompt while open, otherwise the last message (e.g. an unknown
        /// command), if any
        /// </summary>
        public string View() =>
            State.IsOpen ? CommandLine.Render(State, Width, Theme)
            : State.Message is { } message ? Theme.Warning.Apply(Ansi.Truncate(message, Width))
            : "";
    }
}
