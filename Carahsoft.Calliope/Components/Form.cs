using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    public enum FormFieldKind
    {
        Text,
        /// <summary>
        /// A whole number, submitted as a <see cref="long"/>
        /// </summary>
        Integer,
        /// <summary>
        /// Submitted as a <see cref="decimal"/>
        /// </summary>
        Number,
        /// <summary>
        /// Typed like 2026-10-13, submitted as a <see cref="DateOnly"/>
        /// </summary>
        Date,
        /// <summary>
        /// A checkbox toggled with Space or ←/→, submitted as a <see cref="bool"/>
        /// </summary>
        Boolean,
        /// <summary>
        /// One of a list of options, picked with a dropdown. Submitted as the
        /// option's text.
        /// </summary>
        Choice,
        /// <summary>
        /// Typed as JSON, submitted as a <see cref="JsonNode"/>
        /// </summary>
        RawJson,
    }

    /// <summary>
    /// A field of a <see cref="Form"/>. Create one with <see cref="Text"/>,
    /// <see cref="Integer"/>, <see cref="Choice"/> and so on.
    /// </summary>
    public sealed record FormField(string Name, FormFieldKind Kind)
    {
        /// <summary>
        /// Shown next to the input. Defaults to <see cref="Name"/>.
        /// </summary>
        public string? Label { get; init; }
        /// <summary>
        /// Shown below the form while the field has focus
        /// </summary>
        public string? Description { get; init; }
        /// <summary>
        /// Submitting fails while a required field is empty
        /// </summary>
        public bool Required { get; init; }
        /// <summary>
        /// The text of text, number, date and JSON fields
        /// </summary>
        public TextFieldState Input { get; init; } = TextFieldState.Empty;
        /// <summary>
        /// The value of a boolean field
        /// </summary>
        public bool Toggle { get; init; }
        /// <summary>
        /// The options and value of a choice field
        /// </summary>
        public DropdownState Choices { get; init; } = new([]);
        /// <summary>
        /// Set by a failed submit; cleared when the field is edited
        /// </summary>
        public string? Error { get; init; }
        /// <summary>
        /// Hidden fields aren't shown, can't be focused, and are left out of
        /// the submitted values
        /// </summary>
        public bool Hidden { get; init; }

        public static FormField Text(string name, string? value = null) =>
            new(name, FormFieldKind.Text) { Input = TextFieldState.Of(value) };
        public static FormField Integer(string name, long? value = null) =>
            new(name, FormFieldKind.Integer) { Input = TextFieldState.Of(value?.ToString(CultureInfo.InvariantCulture)) };
        public static FormField Number(string name, decimal? value = null) =>
            new(name, FormFieldKind.Number) { Input = TextFieldState.Of(value?.ToString(CultureInfo.InvariantCulture)) };
        public static FormField Date(string name, DateOnly? value = null) =>
            new(name, FormFieldKind.Date) { Input = TextFieldState.Of(value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)) };
        public static FormField Boolean(string name, bool value = false) =>
            new(name, FormFieldKind.Boolean) { Toggle = value };
        public static FormField Choice(string name, IEnumerable<string> options, string? value = null) =>
            new(name, FormFieldKind.Choice) { Choices = Dropdown.Create(options, value) };
        public static FormField RawJson(string name, string? json = null) =>
            new(name, FormFieldKind.RawJson) { Input = TextFieldState.Of(json) };

        internal string DisplayLabel => Label ?? Name;
    }

    public enum FormStatus
    {
        Editing,
        /// <summary>
        /// Enter was pressed and every field is valid; see <see cref="FormState.Values"/>
        /// </summary>
        Submitted,
        /// <summary>
        /// Esc was pressed
        /// </summary>
        Cancelled,
    }

    /// <summary>
    /// The fields of a <see cref="Form"/>, which one has focus, and whether
    /// it's been submitted
    /// </summary>
    public sealed record FormState(IReadOnlyList<FormField> Fields)
    {
        /// <summary>
        /// The index of the focused field
        /// </summary>
        public int Focus { get; init; }
        public FormStatus Status { get; init; } = FormStatus.Editing;
        /// <summary>
        /// Each field's value by name once submitted. Hidden fields, and empty
        /// optional ones, are left out.
        /// </summary>
        public IReadOnlyDictionary<string, object?>? Values { get; init; }

        public FormField? FocusedField => Focus >= 0 && Focus < Fields.Count ? Fields[Focus] : null;
    }

    /// <summary>
    /// A form of labelled fields. Tab or ↓ moves to the next field and
    /// Shift+Tab or ↑ to the previous one, skipping hidden fields. Enter
    /// validates and submits, and Esc cancels. Choice fields open a dropdown
    /// with Space, which is drawn separately (see <see cref="RenderDropdown"/>)
    /// so it floats over the screen.
    /// </summary>
    public static class Form
    {
        public const string Footer = "tab next · shift+tab prev · space choose · enter submit · esc cancel";

        public static FormState Create(params IEnumerable<FormField> fields) => FocusVisible(new FormState(fields.ToList()));

        public static bool IsEmpty(FormState state) => state.Fields.Count == 0;

        /// <summary>
        /// True while a choice field's dropdown list is open
        /// </summary>
        public static bool DropdownOpen(FormState state) =>
            state.FocusedField is { Kind: FormFieldKind.Choice, Choices.IsOpen: true };

        /// <summary>
        /// The selected value of a choice field
        /// </summary>
        public static string? ChoiceValue(FormState state, string name) =>
            state.Fields.FirstOrDefault(f => f.Name == name && f.Kind == FormFieldKind.Choice)?.Choices.Value;

        /// <summary>
        /// Shows or hides a field, e.g. one that only applies to some choices of another
        /// </summary>
        public static FormState SetHidden(FormState state, string name, bool hidden) =>
            FocusVisible(state with { Fields = state.Fields.Select(f => f.Name == name ? f with { Hidden = hidden } : f).ToList() });

        public static FormState Update(FormState state, KeyPressMsg key)
        {
            state = state with { Status = FormStatus.Editing, Values = null };
            var field = state.FocusedField;

            // An open dropdown gets every key, so Esc closes it rather than the form
            if (field is { Kind: FormFieldKind.Choice, Choices.IsOpen: true })
            {
                var choices = Dropdown.Update(field.Choices, key) ?? field.Choices;
                var picked = choices.Selected != field.Choices.Selected;
                state = SetFocused(state, f => f with { Choices = choices, Error = picked ? null : f.Error });

                // Tab closed the list; now move on as Tab normally does
                if (key.Key != ConsoleKey.Tab)
                    return state;
                field = state.FocusedField;
            }

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return state with { Status = FormStatus.Cancelled };
                case ConsoleKey.Enter:
                    return Submit(state);
                case ConsoleKey.Tab when key.Shift():
                case ConsoleKey.UpArrow:
                    return MoveFocus(state, -1);
                case ConsoleKey.Tab:
                case ConsoleKey.DownArrow:
                    return MoveFocus(state, 1);
            }

            if (field == null)
                return state;

            switch (field.Kind)
            {
                case FormFieldKind.Boolean:
                    return key.Key is ConsoleKey.Spacebar or ConsoleKey.LeftArrow or ConsoleKey.RightArrow
                        ? SetFocused(state, f => f with { Toggle = !f.Toggle, Error = null })
                        : state;
                case FormFieldKind.Choice:
                    return Dropdown.Update(field.Choices, key) is { } choices
                        ? SetFocused(state, f => f with { Choices = choices, Error = null })
                        : state;
                default:
                    return TextField.Update(field.Input, key) is { } input
                        ? SetFocused(state, f => f with { Input = input, Error = null })
                        : state;
            }
        }

        /// <summary>
        /// Validates every visible field. If they're all valid, the form is
        /// submitted with its <see cref="FormState.Values"/>; otherwise each
        /// invalid field gets an error and the first one gets focus.
        /// </summary>
        public static FormState Submit(FormState state)
        {
            var results = state.Fields.Select(f => f.Hidden ? (true, null, null) : Validate(f)).ToList();
            var fields = state.Fields.Select((f, i) => f with { Error = results[i].Error }).ToList();

            var firstError = results.FindIndex(r => !r.Ok);
            if (firstError >= 0)
                return state with { Fields = fields, Focus = firstError, Status = FormStatus.Editing, Values = null };

            var values = new Dictionary<string, object?>();
            for (int i = 0; i < fields.Count; i++)
            {
                if (results[i].Value is { } value)
                    values[fields[i].Name] = value;
            }

            return state with { Fields = fields, Status = FormStatus.Submitted, Values = values };
        }

        /// <summary>
        /// The form's lines: a label and input for each visible field, and
        /// its error below it if it has one. An open dropdown isn't included;
        /// see <see cref="RenderDropdown"/>.
        /// </summary>
        public static IReadOnlyList<string> Render(FormState state, int width, Theme? theme = null) =>
            FieldRows(state, width, theme ?? Theme.Default).SelectMany(r => r.Lines).ToList();

        /// <summary>
        /// The open dropdown as a window to draw over the screen, so it doesn't
        /// push anything down: its lines, and the line and column (relative
        /// to <see cref="Render"/>'s lines) of the value it belongs to. Use
        /// <see cref="Window.FloatBelow(int, FloatingWindow, IReadOnlyList{string}, int, int)"/>
        /// to place it. Null when no dropdown is open.
        /// </summary>
        public static FloatingWindow? RenderDropdown(FormState state, int width, Theme? theme = null)
        {
            if (!DropdownOpen(state))
                return null;

            theme ??= Theme.Default;
            var row = FieldRows(state, width, theme).TakeWhile(r => r.Index != state.Focus).Sum(r => r.Lines.Count);
            var list = Dropdown.RenderList(state.FocusedField!.Choices, InputWidth(state, width), theme);
            // Values start after the label, two spaces, and the ▏ marker
            return new FloatingWindow(row, LabelWidth(state) + 3, list);
        }

        /// <summary>
        /// The focused field's description, dim and wrapped to
        /// <paramref name="width"/>, with a blank line above it. Empty if it
        /// has none.
        /// </summary>
        public static IReadOnlyList<string> Description(FormState state, int width, Theme? theme = null)
        {
            if (state.FocusedField?.Description is not { } text)
                return [];
            theme ??= Theme.Default;
            return ["", .. Ansi.Wrap(text, width).Select(theme.Dim.Apply)];
        }

        private static (bool Ok, object? Value, string? Error) Validate(FormField f)
        {
            var text = f.Input.Text.Trim();
            (bool, object?, string?) Ok(object? value) => (true, value, null);
            (bool, object?, string?) Fail(string error) => (false, null, error);

            switch (f.Kind)
            {
                case FormFieldKind.Boolean:
                    return Ok(f.Toggle);
                case FormFieldKind.Choice:
                    return f.Choices.Value == null && f.Required ? Fail("Required") : Ok(f.Choices.Value);
            }

            if (text == "")
                return f.Required ? Fail("Required") : Ok(null);

            switch (f.Kind)
            {
                case FormFieldKind.Integer:
                    return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
                        ? Ok(n) : Fail("Must be a whole number");
                case FormFieldKind.Number:
                    return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                        ? Ok(d) : Fail("Must be a number");
                case FormFieldKind.Date:
                    return DateOnly.TryParse(text, CultureInfo.InvariantCulture, out var date)
                        ? Ok(date) : Fail("Must be a date like 2026-10-13");
                case FormFieldKind.RawJson:
                    try
                    {
                        return Ok(JsonNode.Parse(text));
                    }
                    catch (JsonException e)
                    {
                        return Fail($"Invalid JSON: {e.Message}");
                    }
                default:
                    return Ok(f.Input.Text);
            }
        }

        private static FormState SetFocused(FormState state, Func<FormField, FormField> change) =>
            state with { Fields = state.Fields.Select((f, i) => i == state.Focus ? change(f) : f).ToList() };

        /// <summary>
        /// Moves focus by <paramref name="step"/> fields, skipping hidden ones
        /// </summary>
        private static FormState MoveFocus(FormState state, int step)
        {
            var count = state.Fields.Count;
            for (int n = 1; n <= count; n++)
            {
                var i = ((state.Focus + step * n) % count + count) % count;
                if (!state.Fields[i].Hidden)
                    return state with { Focus = i };
            }
            return state;
        }

        /// <summary>
        /// Moves focus off a hidden field, to the next visible one
        /// </summary>
        private static FormState FocusVisible(FormState state) =>
            state.FocusedField is { Hidden: true } ? MoveFocus(state, 1) : state;

        private static int LabelWidth(FormState state) =>
            state.Fields.Count == 0 ? 0 : state.Fields.Max(f => f.DisplayLabel.Length + 2);

        /// <summary>
        /// Fixed so the form doesn't grow while typing
        /// </summary>
        private static int InputWidth(FormState state, int width) => Math.Clamp(width - LabelWidth(state) - 4, 10, 40);

        private static string Hint(FormFieldKind kind) => kind switch
        {
            FormFieldKind.Integer => "integer",
            FormFieldKind.Number => "number",
            FormFieldKind.Date => "yyyy-MM-dd",
            FormFieldKind.RawJson => "JSON",
            _ => "",
        };

        /// <summary>
        /// Each visible field's index and lines
        /// </summary>
        private static List<(int Index, List<string> Lines)> FieldRows(FormState state, int width, Theme theme)
        {
            var labelWidth = LabelWidth(state);
            var inputWidth = InputWidth(state, width);
            var rows = new List<(int, List<string>)>();

            for (int i = 0; i < state.Fields.Count; i++)
            {
                var f = state.Fields[i];
                if (f.Hidden)
                    continue;

                var focused = i == state.Focus;
                var label = Ansi.PadRight(f.DisplayLabel + (f.Required ? "*" : ""), labelWidth);
                label = focused ? theme.Key.Apply(label) : theme.Dim.Apply(label);

                string input;
                switch (f.Kind)
                {
                    case FormFieldKind.Boolean:
                        var box = f.Toggle ? "[x]" : "[ ]";
                        input = focused ? theme.Selected.Apply(box) : box;
                        break;
                    case FormFieldKind.Choice:
                        input = Dropdown.Render(f.Choices, focused, theme);
                        break;
                    default:
                        input = f.Input.Text == "" && !focused && Hint(f.Kind) != ""
                            ? theme.Dim.Apply(Hint(f.Kind))
                            : TextField.Render(f.Input, inputWidth, focused);
                        break;
                }

                var marker = (focused ? theme.Key : theme.Dim).Apply("▏");
                var lines = new List<string> { label + "  " + marker + Ansi.PadRight(input, inputWidth) };
                if (f.Error != null)
                    lines.Add(new string(' ', labelWidth + 2) + theme.Error.Apply(f.Error));

                rows.Add((i, lines));
            }

            return rows;
        }
    }

    /// <summary>
    /// A <see cref="Form"/> as a program, for C# apps that keep mutable
    /// components. Its view is the fields, the open dropdown floating below
    /// its field, and the focused field's description.
    /// </summary>
    public class FormView : ICalliopeProgram
    {
        public FormView(params IEnumerable<FormField> fields)
        {
            State = Form.Create(fields);
        }

        public FormState State { get; set; }
        public FormStatus Status => State.Status;
        public IReadOnlyDictionary<string, object?>? Values => State.Values;
        public int Width { get; set; } = 60;
        public Theme Theme { get; set; } = Theme.Default;
        /// <summary>
        /// Quit the program once the form is submitted or cancelled, e.g. when
        /// it's run on its own with <see cref="Calliope.NewProgram"/>
        /// </summary>
        public bool QuitWhenDone { get; set; }

        public CalliopeCmd? Init() => null;

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is not KeyPressMsg key)
                return null;

            State = Form.Update(State, key);
            return QuitWhenDone && State.Status != FormStatus.Editing ? CalliopeCmd.Quit : null;
        }

        public string View()
        {
            IReadOnlyList<string> lines = [.. Form.Render(State, Width, Theme), .. Form.Description(State, Width, Theme)];
            if (Form.RenderDropdown(State, Width, Theme) is { } dropdown)
                lines = Window.OverlayAt(dropdown.Row + 1, dropdown.Column, dropdown.Lines, lines);
            return string.Join('\n', lines);
        }
    }
}
