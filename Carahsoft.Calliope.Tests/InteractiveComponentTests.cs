using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Table;
using Carahsoft.Calliope.Text;
using static Carahsoft.Calliope.Tests.Keys;

namespace Carahsoft.Calliope.Tests;

public class TextFieldTests
{
    private static TextFieldState Type(string text, TextFieldState field) =>
        text.Aggregate(field, (f, c) => TextField.Update(f, Char(c))!);

    [Fact]
    public void Edits_at_the_cursor()
    {
        var field = Type("helo", TextFieldState.Empty);
        field = TextField.Update(field, Left)!;
        field = Type("l", field);
        Assert.Equal("hello", field.Text);
        Assert.Equal(4, field.Cursor);

        field = TextField.Update(field, Backspace)!;
        Assert.Equal("helo", field.Text);
    }

    [Fact]
    public void Ignores_Enter_and_Escape()
    {
        Assert.Null(TextField.Update(TextFieldState.Empty, Enter));
        Assert.Null(TextField.Update(TextFieldState.Empty, Escape));
    }

    [Fact]
    public void Deletes_words_and_clears()
    {
        var field = TextFieldState.Of("one two three");
        field = TextField.Update(field, Key(ConsoleKey.Backspace, '\b', ConsoleModifiers.Control))!;
        Assert.Equal("one two ", field.Text);

        Assert.Equal(TextFieldState.Empty, TextField.Update(field, Key(ConsoleKey.U, '\x15', ConsoleModifiers.Control)));
    }

    [Fact]
    public void Never_quits_on_ctrl_c()
    {
        Assert.Null(TextField.Update(TextFieldState.Empty, Key(ConsoleKey.C, '\x03', ConsoleModifiers.Control)));
    }

    [Fact]
    public void Scrolls_to_keep_the_cursor_visible()
    {
        var field = TextFieldState.Of("abcdefghij");
        Assert.Equal("ghij ", Plain(TextField.Render(field, 5, true)));
        // Unfocused, it keeps the same scroll position rather than jumping
        Assert.Equal("ghij", TextField.Render(field, 5, false));
        Assert.Equal("abcd…", TextField.Render(field with { Cursor = 0 }, 5, false));
    }

    [Fact]
    public void Cursor_in_the_last_column_is_not_replaced_by_the_ellipsis()
    {
        var field = TextFieldState.Of("abcdef");
        Assert.Equal("abcd\x1b[7me" + Ansi.Reset, TextField.Render(field with { Cursor = 4 }, 5, true));
        // With room for it, the ellipsis still shows there's more text
        Assert.Equal("abc\x1b[7md" + Ansi.Reset + "…", TextField.Render(field with { Cursor = 3 }, 5, true));
    }

    [Fact]
    public void Draws_the_cursor_inverted()
    {
        var field = TextFieldState.Of("ab") with { Cursor = 1 };
        Assert.Equal("a\x1b[7mb" + Ansi.Reset, TextField.Render(field, 10, true));
    }

    [Fact]
    public void View_wraps_the_state()
    {
        var view = new TextFieldView { Width = 10 };
        view.Update(Char('h'));
        view.Update(Char('i'));
        Assert.Equal("hi", view.Text);
        Assert.Null(view.Update(Key(ConsoleKey.C, '\x03', ConsoleModifiers.Control)));
    }
}

public class DataGridTests
{
    private record Row(string Name, int Size);

    private static readonly List<Row> Rows = Enumerable.Range(1, 10).Select(i => new Row($"row{i}", 100 - i)).ToList();

    private static DataGridState<Row> Grid(IEnumerable<Row> rows)
    {
        var grid = DataGrid.Create(
            new DataGridColumn<Row>("NAME", 10, r => r.Name) { Grow = true },
            new DataGridColumn<Row>("SIZE", 5, r => r.Size.ToString()) { Align = ColumnAlignment.Right, SortKey = r => r.Size });
        return DataGrid.SetViewHeight(DataGrid.SetRows(grid, rows), 3);
    }

    private static DataGridState<Row> Press(char c, DataGridState<Row> grid) => DataGrid.Update(grid, Char(c))!;

    [Fact]
    public void Cursor_stays_in_range_and_scrolls()
    {
        var grid = Enumerable.Range(0, 20).Aggregate(Grid(Rows), (g, _) => Press('j', g));
        Assert.Equal(9, grid.Cursor);
        Assert.Equal(7, grid.Offset);

        grid = Press('k', Press('g', grid));
        Assert.Equal(0, grid.Cursor);
        Assert.Equal(0, grid.Offset);
    }

    [Fact]
    public void Refreshing_keeps_the_cursor_at_the_same_position_even_if_the_order_changes()
    {
        var grid = Press('j', Press('j', Grid(Rows)));
        Assert.Equal("row3", DataGrid.Selected(grid)!.Name);

        // The third row is now row8, and the cursor stays on the third row
        grid = DataGrid.SetRows(grid, Enumerable.Reverse(Rows));
        Assert.Equal(2, grid.Cursor);
        Assert.Equal("row8", DataGrid.Selected(grid)!.Name);
    }

    [Fact]
    public void Refreshing_keeps_the_scroll_position()
    {
        // Scrolled so rows 6-8 show, with the cursor on row 7
        var grid = Press('k', Enumerable.Range(0, 6).Aggregate(Grid(Rows), (g, _) => Press('j', g)));
        var (offset, cursor) = (grid.Offset, grid.Cursor);

        grid = DataGrid.SetRows(grid, Rows.Select(r => r with { Size = r.Size + 1 }));
        Assert.Equal((offset, cursor), (grid.Offset, grid.Cursor));
    }

    [Fact]
    public void Refreshing_with_fewer_rows_clamps_the_cursor()
    {
        var grid = Press('G', Grid(Rows));
        Assert.Equal(9, grid.Cursor);

        grid = DataGrid.SetRows(grid, Rows.Take(3));
        Assert.Equal(2, grid.Cursor);
        Assert.Equal("row3", DataGrid.Selected(grid)!.Name);
    }

    [Fact]
    public void Filter_and_sort()
    {
        var filtered = DataGrid.SetFilter(Grid(Rows), "row1");
        Assert.Equal(["row1", "row10"], DataGrid.VisibleRows(filtered).Select(r => r.Name));

        // s sorts by the selected column, which starts on NAME
        var grid = Press('s', Grid(Rows));
        Assert.Equal(new DataGridSort(0, false), grid.Sort);
        Assert.Equal(["row1", "row10", "row2"], DataGrid.VisibleRows(grid).Take(3).Select(r => r.Name));

        // → moves to SIZE; s sorts it ascending, then again for descending
        DataGridState<Row> MoveRight(DataGridState<Row> g) => DataGrid.Update(g, Right)!;
        grid = Press('s', MoveRight(grid));
        Assert.Equal(new DataGridSort(1, false), grid.Sort);
        Assert.Equal("row10", DataGrid.VisibleRows(grid)[0].Name);

        grid = Press('s', grid);
        Assert.Equal(new DataGridSort(1, true), grid.Sort);
        Assert.Equal("row1", DataGrid.VisibleRows(grid)[0].Name);

        // The column can't move past the last one
        Assert.Equal(1, MoveRight(MoveRight(grid)).Column);
    }

    [Fact]
    public void Sorting_keeps_the_cursor_on_the_selected_row()
    {
        var grid = Press('j', Grid(Rows));
        Assert.Equal("row2", DataGrid.Selected(grid)!.Name);

        grid = Press('s', DataGrid.Update(grid, Right)!);
        Assert.Equal("row2", DataGrid.Selected(grid)!.Name);
    }

    [Fact]
    public void Selected_columns_title_is_highlighted_in_the_header()
    {
        var theme = Theme.Default;
        var lines = DataGrid.Render(DataGrid.Update(Grid(Rows), Right)!, 40, 3, true);
        var header = lines[0];
        Assert.Contains(theme.SelectedHeader.Apply(Ansi.PadLeft("SIZE", 5)), header);
        Assert.Contains(theme.Header.Apply(Ansi.PadRight("NAME", 34)), header);
        Assert.Equal(40, Ansi.VisibleLength(header));

        // The selected row is one plain highlight, with no per-column styling
        Assert.Equal(theme.Selected.Apply(Ansi.PadRight(Plain(lines[1]), 40)), lines[1]);
    }

    [Fact]
    public void Render_fills_the_height_and_width()
    {
        var lines = DataGrid.Render(Grid(Rows), 40, 5, true);
        Assert.Equal(5, lines.Count);
        Assert.All(lines, l => Assert.True(Ansi.VisibleLength(l) <= 40));
        Assert.StartsWith("NAME", Plain(lines[0]));
    }

    [Fact]
    public void Render_clips_columns_wider_than_the_grid()
    {
        var grid = DataGrid.SetRows(DataGrid.Create(
            new DataGridColumn<Row>("NAME", 10, r => r.Name),
            new DataGridColumn<Row>("SIZE", 10, r => r.Size.ToString())), Rows);

        Assert.All(DataGrid.Render(grid, 12, 4, true), l => Assert.True(Ansi.VisibleLength(l) <= 12, $"{Ansi.VisibleLength(l)} columns: {Plain(l)}"));
        Assert.All(DataGrid.Render(grid, 12, 4, false), l => Assert.True(Ansi.VisibleLength(l) <= 12));
    }

    [Fact]
    public void View_keeps_the_cursor_on_screen_at_its_default_height()
    {
        var view = new DataGridView<Row>(new DataGridColumn<Row>("NAME", 10, r => r.Name)) { AutoSize = false };
        view.SetRows(Rows);
        for (int i = 0; i < 9; i++)
            view.Update(Down);

        Assert.Equal("row10", view.Selected!.Name);
        Assert.Contains("row10", Block.Lines(view.View()).Select(Plain).Select(l => l.Trim()));
    }

    [Fact]
    public void Empty_grid_shows_a_message()
    {
        Assert.Equal("No data", Plain(DataGrid.Render(Grid([]), 40, 3, true)[1]).Trim());
        Assert.Equal("Loading…", Plain(DataGrid.Render(Grid([]) with { Loading = true }, 40, 3, true)[1]).Trim());
        Assert.Equal("No rows match \"zzz\"", Plain(DataGrid.Render(DataGrid.SetFilter(Grid(Rows), "zzz"), 40, 3, true)[1]).Trim());
    }

    [Fact]
    public void Rows_can_be_styled_and_spanned()
    {
        var grid = Grid(Rows.Take(2)) with
        {
            RowStyle = r => r.Name == "row2" ? Style.Red : null,
            SpanFrom = r => r.Name == "row2" ? new DataGridSpan(1, "failed") : null,
        };

        // Unfocused, so the cursor row isn't highlighted. NAME grows to 14 columns, leaving 5 for the span.
        var lines = DataGrid.Render(grid, 20, 3, false);
        Assert.Equal("row1".PadRight(14) + "    99", lines[1]);
        Assert.Equal(Style.Red.Apply("row2".PadRight(14) + " fail…"), lines[2]);
    }

    [Fact]
    public void Position_label()
    {
        Assert.Equal("2/10", DataGrid.Position(Press('j', Grid(Rows))));
        Assert.Equal("0/0", DataGrid.Position(Grid([])));
    }

    [Fact]
    public void View_sizes_itself_to_the_screen()
    {
        var view = new DataGridView<Row>(new DataGridColumn<Row>("NAME", 10, r => r.Name));
        view.SetRows(Rows);
        view.Update(new WindowSizeChangeMsg { ScreenWidth = 30, ScreenHeight = 4 });
        view.Update(Char('G'));

        var lines = Block.Lines(view.View());
        Assert.Equal(4, lines.Count);
        Assert.Equal("row10", Plain(lines[3]).Trim());
        Assert.Equal("row10", view.Selected!.Name);
    }
}

public class ConfirmTests
{
    [Fact]
    public void Defaults_to_no()
    {
        var confirm = Confirm.Create("t", "sure?");
        Assert.Equal(ConfirmResult.Cancelled, Confirm.Update(confirm, Enter).Result);
        Assert.Equal(ConfirmResult.Confirmed, Confirm.Update(confirm, Char('y')).Result);
        Assert.Equal(ConfirmResult.Cancelled, Confirm.Update(confirm, Escape).Result);
    }

    [Fact]
    public void Arrows_switch_the_button()
    {
        var confirm = Confirm.Update(Confirm.Create("t", "sure?"), Left);
        Assert.True(confirm.Yes);
        Assert.Equal(ConfirmResult.Pending, confirm.Result);
        Assert.Equal(ConfirmResult.Confirmed, Confirm.Update(confirm, Enter).Result);
    }

    [Fact]
    public void Content_has_the_message_and_buttons()
    {
        var lines = Plain(Confirm.Content(Confirm.Create("t", "Delete it?")));
        Assert.Equal(["Delete it?", "", "       Yes     No "], lines);
    }

    [Fact]
    public void Renders_as_a_popup()
    {
        var background = Enumerable.Repeat(new string('.', 40), 10).ToList();
        var lines = Confirm.Render(Confirm.Create("Delete", "Delete it?"), 40, 10, background);
        Assert.Equal(10, lines.Count);
        Assert.Contains(lines, l => Plain(l).Contains("Delete it?"));
        Assert.Contains(lines, l => Plain(l).Contains(Confirm.Footer));
    }

    [Fact]
    public void View_can_quit_when_answered()
    {
        var view = new ConfirmView("t", "sure?") { QuitWhenAnswered = true };
        Assert.Null(view.Update(Left));
        Assert.Same(CalliopeCmd.Quit, view.Update(Enter));
        Assert.Equal(ConfirmResult.Confirmed, view.Result);
    }
}

public class DropdownTests
{
    private static DropdownState Options(params string[] options) => Dropdown.Create(options);

    [Fact]
    public void Space_opens_on_the_current_value_and_enter_picks()
    {
        var dropdown = Dropdown.Update(Dropdown.Create(["dev", "test", "prod"], "test"), Space)!;
        Assert.True(dropdown.IsOpen);
        Assert.Equal(1, dropdown.Highlight);

        dropdown = Dropdown.Update(Dropdown.Update(dropdown, Down)!, Enter)!;
        Assert.False(dropdown.IsOpen);
        Assert.Equal("prod", dropdown.Value);
    }

    [Fact]
    public void Escape_and_tab_close_without_changing_it()
    {
        var open = Dropdown.Update(Dropdown.Update(Options("dev", "test"), Space)!, Down)!;
        Assert.Equal("dev", Dropdown.Update(open, Escape)!.Value);
        Assert.False(Dropdown.Update(open, Tab)!.IsOpen);
    }

    [Fact]
    public void Closed_it_steps_with_arrows_and_leaves_other_keys()
    {
        var dropdown = Options("a", "b", "c");
        Assert.Equal("c", Dropdown.Update(dropdown, Left)!.Value);
        Assert.Equal("b", Dropdown.Update(dropdown, Right)!.Value);
        Assert.Null(Dropdown.Update(dropdown, Enter));
        Assert.Null(Dropdown.Update(dropdown, Escape));
        Assert.Null(Dropdown.Update(dropdown, Down));
    }

    [Fact]
    public void J_and_k_move_even_when_options_start_with_those_letters()
    {
        var open = Dropdown.Update(Options("k8s1", "k8s2", "kube"), Space)!;
        var down = Dropdown.Update(Dropdown.Update(open, Char('j'))!, Char('j'))!;
        Assert.Equal(2, down.Highlight);
        var up = Dropdown.Update(down, Char('k'))!;
        Assert.Equal(1, up.Highlight);
        // Stops at the top rather than wrapping
        Assert.Equal(0, Dropdown.Update(Dropdown.Update(up, Char('k'))!, Char('k'))!.Highlight);
    }

    [Fact]
    public void Long_lists_scroll()
    {
        var options = Enumerable.Range(1, 12).Select(i => $"option{i:D2}").ToArray();
        var open = Dropdown.Update(Dropdown.Update(Options(options), Space)!, End)!;
        Assert.Equal(11, open.Highlight);

        var lines = Plain(Dropdown.RenderList(open, 40));
        Assert.Equal(open.Rows + 2, lines.Count);
        Assert.Contains(lines, l => l.Contains("↑ 4"));
        Assert.Contains(lines, l => l.Contains("option12"));
        Assert.DoesNotContain(lines, l => l.Contains("option01"));

        lines = Plain(Dropdown.RenderList(Dropdown.Update(open, Home)!, 40));
        Assert.Contains(lines, l => l.Contains("↓ 4"));
        Assert.Contains(lines, l => l.Contains("option01"));
        Assert.DoesNotContain(lines, l => l.Contains("option12"));
    }

    [Fact]
    public void Closed_list_renders_nothing()
    {
        Assert.Empty(Dropdown.RenderList(Options("a"), 40));
        Assert.Equal("a ▾", Dropdown.Render(Options("a"), false));
    }

    [Fact]
    public void View_shows_the_list_below_the_value()
    {
        var view = new DropdownView(["dev", "prod"]);
        view.Update(Space);
        var lines = Plain(Block.Lines(view.View()));
        Assert.Equal(["dev ▾", "╭──────────╮", "│ dev      │", "│ prod     │", "╰──────────╯"], lines);
    }
}

public class FormTests
{
    private static FormState Type(string text, FormState form) => text.Aggregate(form, (f, c) => Form.Update(f, Char(c)));

    private static FormState ChoiceForm(params string[] options) =>
        Form.Create(FormField.Choice("env", options), FormField.Text("note"));

    [Fact]
    public void Required_fields_must_be_filled_in()
    {
        var form = Form.Create(FormField.Text("name") with { Required = true });

        var submitted = Form.Update(form, Enter);
        Assert.Equal(FormStatus.Editing, submitted.Status);
        Assert.Equal("Required", submitted.Fields[0].Error);

        submitted = Form.Update(Type("bob", submitted), Enter);
        Assert.Equal(FormStatus.Submitted, submitted.Status);
        Assert.Equal("bob", submitted.Values!["name"]);
        Assert.Null(submitted.Fields[0].Error);
    }

    [Fact]
    public void Values_are_typed()
    {
        var form = Form.Create(
            FormField.Integer("count", 42),
            FormField.Number("price", 1.5m),
            FormField.Date("day", new DateOnly(2026, 10, 13)),
            FormField.Boolean("all", true),
            FormField.Choice("env", ["dev", "prod"], "prod"),
            FormField.RawJson("extra", """{"a":1}"""),
            FormField.Text("optional"));

        var values = Form.Submit(form).Values!;
        Assert.Equal(42L, values["count"]);
        Assert.Equal(1.5m, values["price"]);
        Assert.Equal(new DateOnly(2026, 10, 13), values["day"]);
        Assert.Equal(true, values["all"]);
        Assert.Equal("prod", values["env"]);
        Assert.Equal("""{"a":1}""", ((System.Text.Json.Nodes.JsonNode)values["extra"]!).ToJsonString());
        // Empty optional fields are left out
        Assert.False(values.ContainsKey("optional"));
    }

    [Fact]
    public void Invalid_values_get_errors_and_focus()
    {
        var form = Form.Create(FormField.Text("a"), FormField.Integer("n", null), FormField.Date("d"));
        form = Form.Update(form, Down);
        form = Type("x", form);
        form = Form.Update(Type("not a date", Form.Update(form, Down)), Enter);

        Assert.Equal(1, form.Focus);
        Assert.Equal("Must be a whole number", form.Fields[1].Error);
        Assert.Equal("Must be a date like 2026-10-13", form.Fields[2].Error);
    }

    [Fact]
    public void Escape_cancels()
    {
        Assert.Equal(FormStatus.Cancelled, Form.Update(Form.Create(FormField.Text("a")), Escape).Status);
    }

    [Fact]
    public void Booleans_toggle_with_space()
    {
        var form = Form.Update(Form.Create(FormField.Boolean("all")), Space);
        Assert.True(form.Fields[0].Toggle);
    }

    [Fact]
    public void Tab_and_arrows_move_focus_skipping_hidden_fields()
    {
        var form = Form.Create(FormField.Text("a"), FormField.Text("b") with { Hidden = true }, FormField.Text("c"));
        Assert.Equal(2, Form.Update(form, Tab).Focus);
        Assert.Equal(2, Form.Update(form, Up).Focus);
        Assert.Equal(0, Form.Update(Form.Update(form, Down), ShiftTab).Focus);
    }

    [Fact]
    public void Hidden_fields_can_depend_on_a_choice()
    {
        var form = Form.Create(FormField.Choice("cluster", ["kube", "url"]), FormField.Text("operator") with { Required = true });
        FormState Sync(FormState f) => Form.SetHidden(f, "operator", Form.ChoiceValue(f, "cluster") != "url");

        form = Sync(form);
        Assert.True(form.Fields[1].Hidden);
        // Hidden fields are left out, even when required
        Assert.Equal(FormStatus.Submitted, Form.Submit(form).Status);
        Assert.Equal(["cluster"], Form.Submit(form).Values!.Keys);

        form = Sync(Form.Update(form, Right));
        Assert.Equal("url", Form.ChoiceValue(form, "cluster"));
        Assert.False(form.Fields[1].Hidden);
        Assert.Single(Form.Render(form, 60).Skip(1));
    }

    [Fact]
    public void Hiding_the_focused_field_moves_focus()
    {
        var form = Form.Update(Form.Create(FormField.Text("a"), FormField.Text("b")), Down);
        Assert.Equal(0, Form.SetHidden(form, "b", true).Focus);
    }

    [Fact]
    public void Space_opens_a_dropdown_on_the_current_value_and_enter_picks()
    {
        var opened = Form.Update(ChoiceForm("dev", "test", "prod"), Space);
        Assert.True(Form.DropdownOpen(opened));
        Assert.Equal(0, opened.Fields[0].Choices.Highlight);

        var chosen = Form.Update(Form.Update(Form.Update(opened, Down), Down), Enter);
        Assert.False(Form.DropdownOpen(chosen));
        // Enter picked from the list rather than submitting the form
        Assert.Equal(FormStatus.Editing, chosen.Status);
        Assert.Equal("prod", Form.ChoiceValue(chosen, "env"));
    }

    [Fact]
    public void Escape_closes_the_dropdown_without_changing_it_or_cancelling_the_form()
    {
        var closed = Form.Update(Form.Update(Form.Update(ChoiceForm("dev", "test", "prod"), Space), Down), Escape);
        Assert.False(Form.DropdownOpen(closed));
        Assert.Equal(FormStatus.Editing, closed.Status);
        Assert.Equal("dev", Form.ChoiceValue(closed, "env"));
    }

    [Fact]
    public void Tab_closes_the_dropdown_and_moves_on()
    {
        var moved = Form.Update(Form.Update(Form.Update(ChoiceForm("dev", "prod"), Space), Down), Tab);
        Assert.False(moved.Fields[0].Choices.IsOpen);
        Assert.Equal(1, moved.Focus);
        Assert.Equal("dev", Form.ChoiceValue(moved, "env"));
    }

    [Fact]
    public void An_open_dropdown_is_a_separate_window_anchored_under_its_value()
    {
        var form = ChoiceForm("dev", "prod");
        var closed = Plain(Form.Render(form, 60));
        Assert.Equal(2, closed.Count);
        Assert.Contains("dev ▾", closed[0]);
        Assert.Null(Form.RenderDropdown(form, 60));

        var opened = Form.Update(form, Space);
        // Opening it doesn't change the form's own lines, so nothing around it moves
        Assert.Equal(closed, Plain(Form.Render(opened, 60)));

        var dropdown = Form.RenderDropdown(opened, 60)!;
        Assert.Equal(0, dropdown.Row);
        Assert.Equal(closed[0].IndexOf("dev"), dropdown.Column);
        var box = Plain(dropdown.Lines);
        Assert.Equal(4, box.Count);
        Assert.StartsWith("╭", box[0]);
        Assert.Contains("dev", box[1]);
        Assert.Contains("prod", box[2]);
        Assert.StartsWith("╰", box[3]);
    }

    [Fact]
    public void Dropdown_row_counts_error_lines_above_it()
    {
        var form = Form.Create(FormField.Text("a") with { Required = true }, FormField.Choice("env", ["dev", "prod"]));
        form = Form.Update(Form.Update(Form.Update(form, Enter), Down), Space);
        Assert.Equal(2, Form.RenderDropdown(form, 60)!.Row);
    }

    [Fact]
    public void Description_of_the_focused_field_is_wrapped()
    {
        var form = Form.Create(FormField.Text("a") with { Description = "the quick brown fox" }, FormField.Text("b"));
        Assert.Equal(["", "the quick", "brown fox"], Plain(Form.Description(form, 10)));
        Assert.Empty(Form.Description(Form.Update(form, Tab), 10));
    }

    [Fact]
    public void Input_width_is_fixed_while_typing()
    {
        var form = Form.Create(FormField.Text("name"));
        var empty = Ansi.VisibleLength(Form.Render(form, 60)[0]);
        Assert.Equal(empty, Ansi.VisibleLength(Form.Render(Type("some text", form), 60)[0]));
    }

    [Fact]
    public void View_floats_the_open_dropdown_over_the_fields_below()
    {
        var view = new FormView(FormField.Choice("env", ["dev", "prod"]), FormField.Text("note"), FormField.Text("other"));
        var closed = Block.Lines(view.View()).Count;
        view.Update(Space);

        var lines = Plain(Block.Lines(view.View()));
        Assert.Contains("╭", lines[1]);
        Assert.Contains("prod", lines[3]);
        // The list covers the fields below rather than pushing them down
        Assert.Equal(Math.Max(closed, 5), lines.Count);
    }

    [Fact]
    public void View_can_quit_when_done()
    {
        var view = new FormView(FormField.Text("a")) { QuitWhenDone = true };
        Assert.Same(CalliopeCmd.Quit, view.Update(Enter));
        Assert.Equal(FormStatus.Submitted, view.Status);
    }
}

public class CommandLineTests
{
    private static readonly CommandLineState Commands = CommandLine.Create(
        new CommandLineCommand("quit", "quit the app") { Aliases = ["q"] },
        new CommandLineCommand("help", "show help"));

    private static CommandLineState Type(string text, CommandLineState state) =>
        text.Aggregate(state, (s, c) => CommandLine.Update(s, Char(c))!);

    [Fact]
    public void Colon_opens_it_and_other_keys_are_left_alone()
    {
        Assert.Null(CommandLine.Update(Commands, Char('x')));
        Assert.True(CommandLine.Update(Commands, Char(':'))!.IsOpen);
    }

    [Fact]
    public void Enter_runs_the_command_with_its_arguments()
    {
        var state = CommandLine.Update(Type(":q now please", Commands), Enter)!;
        Assert.False(state.IsOpen);
        Assert.Equal("quit", state.Invoked!.Name);
        Assert.Equal("now please", state.InvokedArgs);

        // Cleared by the next key
        Assert.Null(CommandLine.Update(state, Char(':'))!.Invoked);
    }

    [Fact]
    public void Unknown_commands_list_the_commands()
    {
        var state = CommandLine.Update(Type(":nope", Commands), Enter)!;
        Assert.Null(state.Invoked);
        Assert.Equal("Unknown command :nope. Commands: :quit, :help", state.Message);
    }

    [Fact]
    public void Tab_completes_and_suggestions_match_aliases()
    {
        var state = Type(":h", Commands);
        Assert.Equal(["help"], CommandLine.Matches(state).Select(c => c.Name));
        Assert.Equal("help", CommandLine.Update(state, Tab)!.Input!.Text);
        Assert.Contains(":help show help", Plain(CommandLine.RenderSuggestions(state, 80)));

        Assert.Equal("No matching command", Plain(CommandLine.RenderSuggestions(Type(":z", Commands), 80)));
    }

    [Fact]
    public void Backspace_on_an_empty_prompt_closes_it()
    {
        var state = Type(":a", Commands);
        state = CommandLine.Update(state, Backspace)!;
        Assert.True(state.IsOpen);
        Assert.False(CommandLine.Update(state, Backspace)!.IsOpen);
    }

    [Fact]
    public void Prompt_shows_the_colon_and_text()
    {
        Assert.Equal(":qu ", Plain(CommandLine.Render(Type(":qu", Commands), 20)));
        Assert.Equal("", CommandLine.Render(Commands, 20));
    }

    [Fact]
    public void Help_lists_every_command()
    {
        Assert.Equal([":quit :q  quit the app", ":help     show help"], Plain(CommandLine.RenderHelp(Commands)));
    }

    [Fact]
    public void View_runs_the_commands()
    {
        string? ran = null;
        var view = new CommandLineView(
            new CommandLineCommand("quit", "quit") { Run = _ => CalliopeCmd.Quit },
            new CommandLineCommand("echo", "echo") { Run = args => { ran = args; return null; } });

        Assert.False(view.HandleKey(Char('x'), out _));
        foreach (var c in ":echo hi")
            view.Update(Char(c));
        Assert.Null(view.Update(Enter));
        Assert.Equal("hi", ran);

        foreach (var c in ":quit")
            view.Update(Char(c));
        Assert.Same(CalliopeCmd.Quit, view.Update(Enter));
    }
}
