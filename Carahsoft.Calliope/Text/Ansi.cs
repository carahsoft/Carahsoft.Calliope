using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Text
{
    /// <summary>
    /// Measures, cuts and pads single lines of text that contain ANSI escape
    /// codes. Widths count visible characters only: escape codes take no
    /// columns, tabs move to the next tab stop every 8 columns, and other
    /// control characters take none.
    /// </summary>
    /// <remarks>
    /// Full-width characters aren't handled yet; each character is one column.
    /// </remarks>
    public static class Ansi
    {
        /// <summary>
        /// Clears all text styles
        /// </summary>
        public const string Reset = "\x1b[0m";

        /// <summary>
        /// The text with escape codes removed
        /// </summary>
        public static string Strip(string? text) => AnsiTextHelper.StripAnsi(text);

        /// <summary>
        /// The number of columns the line takes on screen
        /// </summary>
        public static int VisibleLength(string? line)
        {
            if (string.IsNullOrEmpty(line))
                return 0;

            var count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\x1b')
                {
                    i = EscapeEnd(line, i) - 1;
                    continue;
                }
                count += CharWidth(line[i], count);
            }
            return count;
        }

        /// <summary>
        /// Cuts the line to at most <paramref name="width"/> columns, ending with
        /// … when it's cut. A line that fits is returned unchanged.
        /// </summary>
        public static string Truncate(string? line, int width)
        {
            if (width <= 0 || string.IsNullOrEmpty(line))
                return "";
            if (VisibleLength(line) <= width)
                return line;
            return Cut(line, width - 1) + "…";
        }

        /// <summary>
        /// Cuts the line to at most <paramref name="width"/> columns with no
        /// ellipsis, e.g. to draw something next to it. A line that fits is
        /// returned unchanged. A line that is cut ends with a reset if a style
        /// was still active where it was cut, so the style doesn't carry into
        /// what is drawn next.
        /// </summary>
        public static string Cut(string? line, int width)
        {
            if (string.IsNullOrEmpty(line))
                return "";

            var styled = false;
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '\x1b')
                {
                    var end = EscapeEnd(line, i);
                    if (IsSgr(line, i, end))
                        styled = HasStyle(line, i, end) || (styled && !IsReset(line, i, end));
                    i = end - 1;
                    continue;
                }

                var charWidth = CharWidth(c, count);
                if (count + charWidth > width)
                {
                    var cut = line[..i];
                    return styled ? cut + Reset : cut;
                }
                count += charWidth;
            }
            return line;
        }

        /// <summary>
        /// Drops the first <paramref name="count"/> columns of a line. What's
        /// left starts with a reset followed by the styles still active where it
        /// was cut, so it keeps its color when drawn after other text (e.g. to
        /// the right of a window drawn over the line). Returns an empty string
        /// if nothing visible is left.
        /// </summary>
        /// <remarks>
        /// Handles resets written as <c>ESC[m</c>, <c>ESC[0m</c>, and combined
        /// with a new style, like <c>ESC[0;1m</c>.
        /// </remarks>
        public static string Skip(string? line, int count)
        {
            if (string.IsNullOrEmpty(line))
                return "";

            var visible = 0;
            var index = 0;
            // Style (SGR) codes seen since the last reset; replaying them restores the style at the cut
            var active = new List<string>();

            while (index < line.Length && visible < count)
            {
                if (line[index] == '\x1b')
                {
                    var end = EscapeEnd(line, index);
                    if (IsSgr(line, index, end))
                    {
                        if (IsReset(line, index, end))
                            active.Clear();
                        if (HasStyle(line, index, end))
                            active.Add(line[index..end]);
                    }
                    index = end;
                    continue;
                }

                visible += CharWidth(line[index], visible);
                index++;
            }

            var rest = line[index..];
            if (VisibleLength(rest) == 0)
                return "";

            // A tab that crossed the cut leaves part of its width to fill
            var fill = new string(' ', Math.Max(visible - count, 0));
            return Reset + string.Concat(active) + fill + rest;
        }

        /// <summary>
        /// Truncates or pads the line with spaces on the right to exactly
        /// <paramref name="width"/> columns
        /// </summary>
        public static string PadRight(string? line, int width)
        {
            var t = Truncate(line, width);
            return t + new string(' ', Math.Max(0, width - VisibleLength(t)));
        }

        /// <summary>
        /// Truncates or pads the line with spaces on the left to exactly
        /// <paramref name="width"/> columns
        /// </summary>
        public static string PadLeft(string? line, int width)
        {
            var t = Truncate(line, width);
            return new string(' ', Math.Max(0, width - VisibleLength(t))) + t;
        }

        /// <summary>
        /// Truncates or pads the line on both sides to exactly
        /// <paramref name="width"/> columns, centering it
        /// </summary>
        public static string Center(string? line, int width)
        {
            var t = Truncate(line, width);
            var space = Math.Max(0, width - VisibleLength(t));
            return new string(' ', space / 2) + t + new string(' ', space - space / 2);
        }

        /// <summary>
        /// Splits plain text into lines of at most <paramref name="width"/>
        /// characters, breaking on spaces where possible. Line breaks in the
        /// text are kept.
        /// </summary>
        public static IReadOnlyList<string> Wrap(string? text, int width)
        {
            width = Math.Max(1, width);
            var result = new List<string>();

            foreach (var line in Block.Lines(text))
            {
                var rest = line;
                while (rest.Length > width)
                {
                    var breakAt = rest.LastIndexOf(' ', width);
                    if (breakAt <= 0)
                        breakAt = width;

                    result.Add(rest[..breakAt]);
                    rest = rest[breakAt..].TrimStart();
                }
                result.Add(rest);
            }

            return result;
        }

        /// <summary>
        /// The columns taken by a character at the given column
        /// </summary>
        private static int CharWidth(char c, int column)
        {
            if (c == '\t')
                return 8 - column % 8;
            // TODO: handle full width chars
            return char.IsControl(c) ? 0 : 1;
        }

        /// <summary>
        /// The index just past the escape sequence starting at
        /// <paramref name="start"/>, which ends at the first letter.
        /// </summary>
        private static int EscapeEnd(string s, int start)
        {
            var i = start + 1;
            while (i < s.Length && !((s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= 'a' && s[i] <= 'z')))
                i++;
            return Math.Min(i + 1, s.Length);
        }

        /// <summary>
        /// Whether the escape sequence is a style code, ESC [ parameters m
        /// </summary>
        private static bool IsSgr(string s, int start, int end) =>
            end - start >= 3 && s[start + 1] == '[' && s[end - 1] == 'm';

        private static ReadOnlySpan<char> SgrParameters(string s, int start, int end) =>
            s.AsSpan(start + 2, end - start - 3);

        /// <summary>
        /// Whether the style code starts with a reset: ESC[m, ESC[0m, or ESC[0;...m
        /// </summary>
        private static bool IsReset(string s, int start, int end)
        {
            var p = SgrParameters(s, start, end);
            return p.IsEmpty || p[0] == ';' || (p[0] == '0' && (p.Length == 1 || p[1] == ';'));
        }

        /// <summary>
        /// Whether the style code sets anything other than a reset, e.g. ESC[1m or ESC[0;1m
        /// </summary>
        private static bool HasStyle(string s, int start, int end)
        {
            foreach (var code in SgrParameters(s, start, end).ToString().Split(';'))
            {
                if (code != "" && code != "0")
                    return true;
            }
            return false;
        }
    }
}
