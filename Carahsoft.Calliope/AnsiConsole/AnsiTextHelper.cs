using System.Text;
using Carahsoft.Calliope.Text;

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
                        ? Ansi.Cut(lines[i][..^1], length) + "\r"
                        : Ansi.Cut(lines[i], length);
                }
                return string.Join('\n', lines);
            }

            return Ansi.Cut(line, length);
        }
    }
}
