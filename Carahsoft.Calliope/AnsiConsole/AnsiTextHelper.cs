using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    internal static class AnsiTextHelper
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
        /// Truncates the given line to the given length, ignoring
        /// unprinted ANSI escape sequences in the line. Line must be
        /// a single line string without newlines.
        /// </summary>
        /// <remarks>
        /// Currently does not support full-width characters
        /// </remarks>
        public static string TruncateLineToLength(string? line, int length)
        {
            if (string.IsNullOrEmpty(line))
                return "";

            if (line.Contains('\r') || line.Contains('\n'))
            {
                throw new ArgumentException("Multi-line string passed to LineDisplayLength", nameof(line));
            }

            bool escape = false;
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (count >= length)
                {
                    return line[..i];
                }

                var c = line[i];
                // Cancel escape if we are at the end of the sequence
                if (escape && ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    escape = false;
                    continue;
                }

                if (escape)
                {
                    continue;
                }

                if (c == '\x1b')
                {
                    escape = true;
                    continue;
                }
                if (c == '\t')
                {
                    // tab moves to the next tabstop every 8 chars
                    var tabstop = i % 8;
                    count += 8 - tabstop;
                    if (count >= length)
                        return line[..(i - 1)];

                    continue;
                }
                // TODO: handle full width chars

                // This should catch most unprintable characters
                if (!char.IsControl(c))
                    count++;
            }
            return line;
        }
    }
}
