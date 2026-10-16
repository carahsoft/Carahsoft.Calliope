using Carahsoft.Calliope.Text;
using static Carahsoft.Calliope.Tests.Keys;

namespace Carahsoft.Calliope.Tests;

public class TextTests
{
    private const string Red = "\x1b[31mhello world\x1b[0m";

    // Ansi

    [Fact]
    public void Truncate_ignores_escape_codes_and_adds_an_ellipsis()
    {
        Assert.Equal(11, Ansi.VisibleLength(Red));
        Assert.Equal(Red, Ansi.Truncate(Red, 11));
        Assert.Equal("hello…", Plain(Ansi.Truncate(Red, 6)));
        Assert.Equal(6, Ansi.VisibleLength(Ansi.Truncate(Red, 6)));
    }

    [Fact]
    public void Truncate_closes_the_style_it_cuts_before_the_ellipsis()
    {
        Assert.Equal("\x1b[31mhello" + Ansi.Reset + "…", Ansi.Truncate(Red, 6));
    }

    [Fact]
    public void Cut_has_no_ellipsis()
    {
        Assert.Equal("\x1b[31mhel" + Ansi.Reset, Ansi.Cut(Red, 3));
        Assert.Equal(Red, Ansi.Cut(Red, 11));
    }

    [Fact]
    public void VisibleLength_counts_tabs_to_the_next_stop()
    {
        Assert.Equal(10, Ansi.VisibleLength("ab\tcd"));
    }

    [Fact]
    public void Padding_gives_exactly_the_width()
    {
        Assert.Equal(15, Ansi.VisibleLength(Ansi.PadRight(Red, 15)));
        Assert.Equal(5, Ansi.VisibleLength(Ansi.PadRight(Red, 5)));
        Assert.Equal("  ab", Ansi.PadLeft("ab", 4));
        Assert.Equal(" ab  ", Ansi.Center("ab", 5));
    }

    [Fact]
    public void Wrap_breaks_on_spaces()
    {
        Assert.Equal(["the quick", "brown fox"], Ansi.Wrap("the quick brown fox", 10));
        Assert.Equal(["abcde", "fgh"], Ansi.Wrap("abcdefgh", 5));
        Assert.Equal(["a", "b"], Ansi.Wrap("a\nb", 5));
    }

    [Fact]
    public void Skip_doesnt_replay_styles_reset_before_the_cut()
    {
        var line = Theme.Default.Error.Apply("red") + "plain text here";
        Assert.Equal(Ansi.Reset + "in text here", Ansi.Skip(line, 6));
    }

    [Fact]
    public void Skip_replays_combined_and_stacked_styles()
    {
        var bold = "\x1b[1m";
        var blue = "\x1b[38;2;0;0;255m";
        Assert.Equal(Ansi.Reset + bold + blue + "ef" + Ansi.Reset, Ansi.Skip(bold + "ab" + blue + "cdef" + Ansi.Reset, 4));

        // A reset combined with a new style (ESC[0;1m) drops what came before but keeps the new part
        Assert.Equal(Ansi.Reset + "\x1b[0;1m" + "ef", Ansi.Skip(blue + "ab" + "\x1b[0;1m" + "cdef", 4));
    }

    // Block

    [Fact]
    public void FitHeight_returns_exactly_height_lines()
    {
        Assert.Equal(["a", "b", "", ""], Block.FitHeight(["a", "b"], 4));
        Assert.Equal(["a"], Block.FitHeight(["a", "b"], 1));
        Assert.Empty(Block.FitHeight(["a"], 0));
    }

    [Fact]
    public void JoinHorizontal_places_blocks_side_by_side()
    {
        Assert.Equal(["ab  x", "c   y", "    z"], Block.JoinHorizontal(2, ["ab", "c"], ["x", "y", "z"]));
    }

    [Fact]
    public void JoinVertical_stacks_blocks()
    {
        Assert.Equal(["a", "b", "c"], Block.JoinVertical(["a"], ["b", "c"]));
    }

    [Fact]
    public void Lines_splits_on_either_line_ending()
    {
        Assert.Equal(["a", "b", "c"], Block.Lines("a\r\nb\nc"));
    }

    // Frame

    [Fact]
    public void Box_lines_are_the_full_width()
    {
        var lines = Frame.Box("Title", 30, ["content", Red]);
        Assert.Equal(4, lines.Count);
        Assert.All(lines, l => Assert.Equal(30, Ansi.VisibleLength(l)));
        Assert.Equal("╭─ Title ────────────────────╮", Plain(lines[0]));
        Assert.Equal("│ content                    │", Plain(lines[1]));
    }

    [Fact]
    public void Dialog_has_a_centered_title_and_a_footer_divider()
    {
        var lines = Plain(Frame.Dialog("T", "f", 11, ["hi"]));
        Assert.Equal(["╭─── T ───╮", "│ hi      │", "├─── f ───┤", "╰─────────╯"], lines);
    }

    [Fact]
    public void Dialog_without_a_footer_has_no_divider()
    {
        Assert.Equal(3, Frame.Dialog("T", "", 11, ["hi"]).Count);
    }

    [Fact]
    public void Frame_characters_can_be_changed()
    {
        var square = new FrameStyle { TopLeft = '┌', TopRight = '┐', BottomLeft = '└', BottomRight = '┘' };
        var lines = Frame.Box(null, 4, [], square);
        Assert.Equal(["┌──┐", "└──┘"], lines);
    }

    [Fact]
    public void Bar_has_text_on_both_sides()
    {
        Assert.Equal(" left       right", Plain(Frame.Bar(Style.Bold, 17, "left", "right")));
    }

    // Window

    [Fact]
    public void Popup_keeps_the_background_size()
    {
        var background = Enumerable.Repeat(new string('.', 60), 20).ToList();
        var lines = Window.Popup(60, 20, "Title", "footer", ["hello"], background).Lines;
        Assert.Equal(20, lines.Count);
        Assert.Contains(lines, l => Plain(l).Contains("hello"));
        Assert.All(lines, l => Assert.Equal(60, Ansi.VisibleLength(l)));
    }

    [Fact]
    public void Popup_reports_where_its_content_starts()
    {
        var background = Enumerable.Repeat(new string('.', 60), 20).ToList();
        var popup = Window.Popup(60, 20, "Title", "footer", ["hello"], background);
        Assert.Equal("hello", Plain(popup.Lines[popup.ContentRow]).Substring(popup.ContentColumn, 5));
    }

    [Fact]
    public void OverlayAt_draws_over_the_middle_of_lines_keeping_both_sides()
    {
        var lines = Window.OverlayAt(1, 3, ["XX", "YY", "ZZ"], ["aaaaaaaaaa", Red + "bbbbbbb", "cc"]);
        // Row 1 is "hello world" (red) then "bbbbbbb"; XX replaces "lo", the rest stays
        Assert.Equal(["aaaaaaaaaa", "helXX worldbbbbbbb", "cc YY", "   ZZ"], Plain(lines));
    }

    [Fact]
    public void Text_right_of_a_window_keeps_its_color()
    {
        // A blue border running under a window, like a dialog's bottom edge under a dropdown
        var border = Style.FromColor(RgbColors.CarahBlue);
        var line = border.Apply("╰" + new string('─', 20) + "╯");
        var borderColor = line[..(line.IndexOf('m') + 1)];

        var result = Window.OverlayAt(0, 5, ["XXX"], [line])[0];
        Assert.Equal("╰────XXX─────────────╯", Plain(result));

        // After the window: a reset (clearing anything the window set), then the border's color again
        var right = result[(result.IndexOf("XXX") + 3)..];
        Assert.StartsWith(Ansi.Reset + borderColor + "─", right);
    }

    [Fact]
    public void FloatBelow_opens_upwards_when_theres_no_room_below()
    {
        var lines = Enumerable.Repeat(new string('.', 20), 10).ToList();
        string[] window = ["1", "2", "3"];

        var below = Plain(Window.FloatBelow(10, 2, 5, window, lines));
        Assert.Equal('1', below[3][5]);

        var above = Plain(Window.FloatBelow(10, 8, 5, window, lines));
        Assert.Equal('1', above[5][5]);
        Assert.Equal('3', above[7][5]);
        Assert.Equal(10, above.Count);
    }
}
