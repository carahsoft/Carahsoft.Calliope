using System.Data;
using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Table;

namespace Carahsoft.Calliope.Tests;

public class ComponentTests
{
    private const string Reset = "\x1b[0m";

    private static KeyPressMsg Key(ConsoleKey key, char keyChar = '\0', ConsoleModifiers modifiers = 0) =>
        new() { Key = key, KeyChar = keyChar, Modifiers = modifiers };

    private static KeyPressMsg Char(char c) =>
        Key(Enum.TryParse<ConsoleKey>(c.ToString(), true, out var k) ? k : ConsoleKey.Oem1, c);

    private static readonly KeyPressMsg CtrlC = Key(ConsoleKey.C, '\x03', ConsoleModifiers.Control);
    private static readonly KeyPressMsg Enter = Key(ConsoleKey.Enter, '\r');

    private static List<SelectListItem> Items(params string[] values) =>
        values.Select(v => new SelectListItem { Value = v }).ToList();

    public record Row(string Name, int? Count);

    // ConsoleTable

    [Fact]
    public void ConsoleTable_of_an_empty_list_renders_the_headers()
    {
        var table = new ConsoleTable<Row>(new List<Row>()).ToString();

        Assert.Contains("Name", table);
        Assert.Contains("Count", table);
    }

    [Fact]
    public void ConsoleTable_supports_nullable_properties()
    {
        var table = new ConsoleTable<Row>([new Row("a", 1), new Row("b", null)]).ToString();

        var rows = table.Split('\n');
        Assert.StartsWith("| a ", rows[3]);
        Assert.Contains("1", rows[3]);
        Assert.StartsWith("| b ", rows[4]);
    }

    // SelectList

    [Fact]
    public void SelectList_with_no_items_renders_and_ignores_keys()
    {
        var list = new SelectList();

        Assert.Equal("(no options)", list.View());
        list.Update(Key(ConsoleKey.UpArrow));
        Assert.Null(list.Update(Enter));
        Assert.Null(list.Choice);
    }

    [Fact]
    public void SelectList_quits_on_select_and_ctrl_c_by_default()
    {
        var list = new SelectList { Items = Items("a", "b") };

        Assert.Same(CalliopeCmd.Quit, list.Update(CtrlC));
        Assert.Same(CalliopeCmd.Quit, list.Update(Enter));
        Assert.Equal("a", list.Choice);
    }

    [Fact]
    public void SelectList_can_select_without_quitting()
    {
        var list = new SelectList { Items = Items("a", "b"), QuitOnSelect = false, QuitOnCtrlC = false };

        Assert.Null(list.Update(CtrlC));
        list.Update(Key(ConsoleKey.DownArrow));
        Assert.Null(list.Update(Enter));
        Assert.Equal("b", list.Choice);
    }

    // FilterableSelectList

    [Fact]
    public void FilterableSelectList_matches_FilterValue()
    {
        var list = new FilterableSelectList(
        [
            new SelectListItem { Value = "Apple", FilterValue = "fruit red" },
            new SelectListItem { Value = "Carrot", FilterValue = "vegetable orange" },
        ]);
        list.Init();

        list.Update(Char('/'));
        foreach (var c in "orange")
            list.Update(Char(c));

        var view = AnsiTextHelper.StripAnsi(list.View());
        Assert.Contains("Carrot", view);
        Assert.DoesNotContain("Apple", view);
    }

    [Fact]
    public void FilterableSelectList_can_search_again_after_clearing()
    {
        var list = new FilterableSelectList(Items("a", "b"));
        list.Init();

        list.Update(Char('/'));
        list.Update(Key(ConsoleKey.Escape, '\x1b'));
        list.Update(Char('/'));
        list.Update(Char('b'));

        Assert.Equal("b", list.SearchInput.Text);
    }

    // TextInput

    [Fact]
    public void TextInput_quits_on_ctrl_c_by_default()
    {
        Assert.Same(CalliopeCmd.Quit, new TextInput().Update(CtrlC));
    }

    [Fact]
    public void TextInput_can_ignore_ctrl_c()
    {
        var input = new TextInput { QuitOnCtrlC = false };

        Assert.Null(input.Update(CtrlC));
        Assert.Equal("", input.Text);
    }

    [Fact]
    public void TextInput_is_enabled_by_default()
    {
        var input = new TextInput();

        input.Update(Char('a'));

        Assert.Equal("a", input.Text);
    }

    [Fact]
    public void Disabled_TextInput_ignores_keys_and_draws_no_cursor()
    {
        var input = new TextInput { Text = "abc", Enabled = false };

        input.Update(Char('x'));
        input.Update(Key(ConsoleKey.Backspace, '\b'));

        Assert.Equal("abc", input.Text);
        Assert.Equal("abc", input.View());
    }

    // ScrollView

    [Fact]
    public void ScrollView_quits_on_ctrl_c_by_default()
    {
        Assert.Same(CalliopeCmd.Quit, new ScrollView().Update(CtrlC));
        Assert.Null(new ScrollView { QuitOnCtrlC = false }.Update(CtrlC));
    }

    [Fact]
    public void ScrollView_takes_the_screen_height_by_default()
    {
        var view = new ScrollView { Height = 5 };

        view.Update(new WindowSizeChangeMsg { ScreenHeight = 40, ScreenWidth = 100 });

        Assert.Equal(40, view.Height);
        Assert.Equal(100, view.Width);
    }

    [Fact]
    public void ScrollView_without_AutoHeight_keeps_its_height()
    {
        var view = new ScrollView { Height = 5, AutoHeight = false };

        view.Update(new WindowSizeChangeMsg { ScreenHeight = 40, ScreenWidth = 100 });

        Assert.Equal(5, view.Height);
        Assert.Equal(100, view.Width);
    }

    // Components.Table

    private static DataTable Data(params string[] names)
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        foreach (var n in names)
            dt.Rows.Add(n);
        return dt;
    }

    [Fact]
    public void Table_applies_header_and_row_styles()
    {
        var table = new Components.Table(Data("alice", "bob"))
        {
            HeaderStyle = Style.Bold,
            RowStyle = Style.Red,
        };
        table.Init();

        var lines = table.ScrollView.RenderView.Split('\n');

        Assert.Equal("+-------+", lines[0]);
        Assert.Equal(Style.Bold.Apply("| Name  |"), lines[1]);
        Assert.Equal("+-------+", lines[2]);
        Assert.Equal(Style.Red.Apply("| alice |"), lines[3]);
        Assert.Equal(Style.Red.Apply("| bob   |"), lines[4]);
        Assert.Equal("+-------+", lines[5]);
    }

    [Fact]
    public void Table_SetRows_refreshes_the_view()
    {
        var table = new Components.Table(Data("alice"));
        table.Init();
        table.Update(new WindowSizeChangeMsg { ScreenHeight = 20, ScreenWidth = 80 });

        table.SetRows(Data("carol", "dave"));

        var view = table.View();
        Assert.DoesNotContain("alice", view);
        Assert.Contains("carol", view);
        Assert.Contains("dave", view);
    }

    // Cursor

    [Fact]
    public async Task Cursor_ignores_another_cursors_blinks()
    {
        var a = new Cursor();
        var b = new Cursor();
        var blink = await a.Init()!.CommandFunc();

        Assert.Null(b.Update(blink));
        Assert.True(b.CursorVisible);

        Assert.NotNull(a.Update(blink));
        Assert.False(a.CursorVisible);
    }

    [Fact]
    public async Task Restarting_a_cursor_does_not_double_its_blink_rate()
    {
        var cursor = new Cursor();
        var firstBlink = cursor.Init()!.CommandFunc();
        var restart = cursor.Update(await Cursor.StartBlinking().CommandFunc());

        // The blink scheduled before the restart is stale
        Assert.Null(cursor.Update(await firstBlink));
        Assert.True(cursor.CursorVisible);

        Assert.NotNull(cursor.Update(await restart!.CommandFunc()));
        Assert.False(cursor.CursorVisible);
    }

    [Fact]
    public async Task Cursor_instance_commands_only_reach_that_cursor()
    {
        var a = new Cursor();
        var b = new Cursor();
        a.Init();
        b.Init();

        var stop = await a.StopBlink().CommandFunc();
        a.Update(stop);
        b.Update(stop);

        Assert.False(a.Blinking);
        Assert.True(b.Blinking);
    }

    // PopupDialog

    private class Text(string text) : ICalliopeProgram
    {
        public CalliopeCmd? Init() => null;
        public CalliopeCmd? Update(CalliopeMsg msg) => null;
        public string View() => text;
    }

    [Fact]
    public void PopupDialog_keeps_the_background_style_on_both_sides()
    {
        const string blue = "\x1b[48;2;49;85;164m";
        var bg = Style.Default.WithBackgroundColor(RgbColors.CarahBlue);
        var background = string.Join('\n', Enumerable.Repeat(bg.Apply(new string('x', 40)), 7));
        var popup = new PopupDialog<Text>(new Text("hi"), "T");

        var lines = popup.RenderOverlay(background, 40, 7).Split('\n');

        Assert.Equal(7, lines.Length);
        foreach (var line in lines)
            Assert.Equal(40, AnsiTextHelper.StripAnsi(line).Length);

        // The 9-wide, 3-line popup (it has no footer) starts at column 15 of row 2. The text left of it
        // closes the blue before the border starts, and the text right of it opens the blue again.
        const string border = "\x1b[38;2;80;80;80m";
        var row = lines[2];
        Assert.StartsWith(blue + new string('x', 15) + Reset + border + "╭", row);
        Assert.EndsWith("╮" + Reset + Reset + blue + new string('x', 16) + Reset, row);
    }
}
