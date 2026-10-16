using System.Text;

namespace Carahsoft.Calliope.AnsiConsole
{
    public static class AnsiTextHelper
    {
        public static string ColorText(string text, RgbColor color, RgbColor? background = null)
        {
            var sb = new StringBuilder();
            sb.Append("\x1b[");
            sb.Append($"38;2;{(int)color.Red};{(int)color.Green};{(int)color.Blue}");
            
            if (background != null)
            {
                sb.Append(';');
                sb.Append($"48;2;{(int)background.Value.Red};{(int)background.Value.Green};{(int)background.Value.Blue}");
            }

            sb.Append("m");
            sb.Append(text);
            sb.Append("\x1b[0m");
            return sb.ToString(); 
        }

        public static string ColorTextGradient(string text, RgbColor start, RgbColor end, RgbColor? background = null)
        {
            var sb = new StringBuilder();
            var steps = text.Length;

            decimal stepR = (decimal)(end.Red - start.Red) / steps;
            decimal stepG = (decimal)(end.Green - start.Green) / steps;
            decimal stepB = (decimal)(end.Blue - start.Blue) / steps;

            foreach (var step in Enumerable.Range(0, steps))
            {
                var color = new RgbColor
                {
                    Red = (byte)((step * stepR) + start.Red),
                    Green = (byte)((step * stepG) + start.Green),
                    Blue = (byte)((step * stepB) + start.Blue),
                };
                sb.Append(ColorText(text[step].ToString(), color, background));
            }

            return sb.ToString(); 
        }

        /// <summary>
        /// Strips ANSI escape sequences from a string, returning only the visible text.
        /// </summary>
        public static string StripAnsi(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var sb = new StringBuilder();
            bool escape = false;
            foreach (var c in text)
            {
                if (escape && ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    escape = false;
                    continue;
                }
                if (escape)
                    continue;
                if (c == '\x1b')
                {
                    escape = true;
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Truncates the given line to the given length, ignoring
        /// unprinted ANSI escape sequences in the line. A line that
        /// already fits is returned unchanged. A line that is cut ends
        /// with a reset if a style was still active where it was cut,
        /// so the style doesn't carry into whatever is drawn next.
        /// Multi-line strings have each line truncated separately.
        /// </summary>
        /// <remarks>
        /// Currently does not support full-width characters
        /// </remarks>
        public static string TruncateLineToLength(string? line, int length)
        {
            if (string.IsNullOrEmpty(line))
                return "";

            if (line.Contains('\n'))
            {
                var lines = line.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].EndsWith('\r')
                        ? TruncateSingleLine(lines[i][..^1], length) + "\r"
                        : TruncateSingleLine(lines[i], length);
                }
                return string.Join('\n', lines);
            }

            return TruncateSingleLine(line, length);
        }

        private static string TruncateSingleLine(string line, int length)
        {
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

                var width = CharWidth(c, count);
                if (count + width > length)
                {
                    var cut = line[..i];
                    return styled ? cut + Reset : cut;
                }
                count += width;
            }
            return line;
        }

        /// <summary>
        /// Drops the first <paramref name="count"/> visible characters of a line.
        /// What's left starts with a reset followed by the styles still active
        /// where it was cut, so it keeps its color when drawn after other text.
        /// Returns an empty string if no visible characters are left.
        /// </summary>
        internal static string SkipVisible(string line, int count)
        {
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
            if (StripAnsi(rest).Length == 0)
                return "";

            // A tab that crossed the cut leaves part of its width to fill
            var fill = new string(' ', Math.Max(visible - count, 0));
            return Reset + string.Concat(active) + fill + rest;
        }

        private const string Reset = "\x1b[0m";

        /// <summary>
        /// The columns taken by a character at the given column: tabs move
        /// to the next tab stop every 8 columns, and other control characters
        /// take none.
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
