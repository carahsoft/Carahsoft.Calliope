using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Tests;

public class AnsiTextHelperTests
{
    private const string Reset = "\x1b[0m";
    private static readonly Style Blue = Style.Default.WithBackgroundColor(RgbColors.CarahBlue);

    [Fact]
    public void Full_width_styled_line_keeps_its_reset()
    {
        var line = Blue.Apply(new string('x', 10));

        Assert.Equal(line, AnsiTextHelper.TruncateLineToLength(line, 10));
    }

    [Fact]
    public void Line_that_fits_is_unchanged()
    {
        var line = "ab" + Blue.Apply("cd") + "\x1b[1m";

        Assert.Equal(line, AnsiTextHelper.TruncateLineToLength(line, 20));
    }

    [Fact]
    public void Cutting_inside_a_style_closes_it()
    {
        var line = Blue.Apply(new string('x', 10)) + "after";

        var cut = AnsiTextHelper.TruncateLineToLength(line, 4);

        Assert.Equal("xxxx", AnsiTextHelper.StripAnsi(cut));
        Assert.EndsWith(Reset, cut);
    }

    [Fact]
    public void Cutting_after_a_closed_style_adds_no_reset()
    {
        var line = Blue.Apply("ab") + "cdefgh";

        var cut = AnsiTextHelper.TruncateLineToLength(line, 4);

        Assert.Equal(Blue.Apply("ab") + "cd", cut);
    }

    [Fact]
    public void Cutting_plain_text_adds_no_codes()
    {
        Assert.Equal("abc", AnsiTextHelper.TruncateLineToLength("abcdef", 3));
    }

    [Fact]
    public void Tab_that_crosses_the_limit_keeps_the_character_before_it()
    {
        // "abc" then a tab to column 8; cut at 5
        Assert.Equal("abc", AnsiTextHelper.TruncateLineToLength("abc\tdef", 5));
    }

    [Fact]
    public void Tab_that_reaches_the_limit_exactly_fits()
    {
        Assert.Equal("abc\t", AnsiTextHelper.TruncateLineToLength("abc\tdef", 8));
    }

    [Fact]
    public void Tab_stops_ignore_escape_codes()
    {
        // The escape code is 4 characters long but takes no columns, so the tab still ends at column 8
        Assert.Equal("\x1b[1mab\t" + Reset, AnsiTextHelper.TruncateLineToLength("\x1b[1mab\tcd", 8));
    }

    [Fact]
    public void Multi_line_input_truncates_each_line()
    {
        Assert.Equal("abc\nde\r\nfgh", AnsiTextHelper.TruncateLineToLength("abcdef\nde\r\nfghij", 3));
    }

    [Fact]
    public void Zero_length_returns_empty()
    {
        Assert.Equal("", AnsiTextHelper.TruncateLineToLength("abc", 0));
    }

    [Fact]
    public void SkipVisible_reapplies_active_styles()
    {
        var bold = "\x1b[1m";
        var line = "ab" + bold + "cdef" + Reset + "gh";

        var rest = Text.Ansi.Skip(line, 3);

        Assert.Equal("defgh", AnsiTextHelper.StripAnsi(rest));
        Assert.Equal(Reset + bold + "def" + Reset + "gh", rest);
    }

    [Fact]
    public void SkipVisible_forgets_styles_that_were_reset()
    {
        var line = "\x1b[1mab\x1b[mcd\x1b[0;4mef";

        Assert.Equal(Reset + "d\x1b[0;4mef", Text.Ansi.Skip(line, 3));
        Assert.Equal(Reset + "\x1b[0;4mf", Text.Ansi.Skip(line, 5));
    }

    [Fact]
    public void SkipVisible_past_the_end_is_empty()
    {
        Assert.Equal("", Text.Ansi.Skip(Blue.Apply("abc"), 3));
        Assert.Equal("", Text.Ansi.Skip("abc", 10));
    }
}
