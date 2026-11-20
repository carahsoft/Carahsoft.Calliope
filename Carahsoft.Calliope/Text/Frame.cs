namespace Carahsoft.Calliope.Text
{
    /// <summary>
    /// The characters and styles a <see cref="Frame"/> is drawn with. The
    /// defaults are rounded corners with no styles; see
    /// <see cref="Theme.Box"/> and <see cref="Theme.Dialog"/> for the styled ones.
    /// </summary>
    public sealed record FrameStyle
    {
        public char TopLeft { get; init; } = '╭';
        public char TopRight { get; init; } = '╮';
        public char BottomLeft { get; init; } = '╰';
        public char BottomRight { get; init; } = '╯';
        public char Horizontal { get; init; } = '─';
        public char Vertical { get; init; } = '│';
        /// <summary>
        /// Where the footer divider meets the left border
        /// </summary>
        public char LeftT { get; init; } = '├';
        /// <summary>
        /// Where the footer divider meets the right border
        /// </summary>
        public char RightT { get; init; } = '┤';

        public Style Border { get; init; } = Style.Default;
        public Style Title { get; init; } = Style.Default;
        public Style Footer { get; init; } = Style.Default;
    }

    /// <summary>
    /// Boxes, dialogs, dividers and bars, drawn as lines of text
    /// </summary>
    public static class Frame
    {
        /// <summary>
        /// A box of the given total width around the lines, with an optional
        /// title in the top border. Content is cut or padded to fit inside.
        /// </summary>
        public static IReadOnlyList<string> Box(string? title, int width, IEnumerable<string> content, FrameStyle? style = null)
        {
            style ??= Theme.Default.Box;
            var b = style.Border;
            var inner = Math.Max(1, width - 4);
            var h = style.Horizontal;

            string top;
            if (string.IsNullOrEmpty(title))
            {
                top = b.Apply(style.TopLeft + new string(h, Math.Max(0, width - 2)) + style.TopRight);
            }
            else
            {
                var t = Ansi.Truncate(title, inner - 2);
                top = b.Apply($"{style.TopLeft}{h} ") + style.Title.Apply(t) +
                    b.Apply(" " + new string(h, Math.Max(0, width - 5 - Ansi.VisibleLength(t))) + style.TopRight);
            }

            var lines = new List<string> { top };
            foreach (var line in content)
                lines.Add(b.Apply(style.Vertical + " ") + Ansi.PadRight(line, inner) + b.Apply(" " + style.Vertical));
            lines.Add(b.Apply(style.BottomLeft + new string(h, Math.Max(0, width - 2)) + style.BottomRight));
            return lines;
        }

        /// <summary>
        /// A fixed-width dialog box: the title centered in the top border, and
        /// the footer (if any) in a divider above the bottom border. It never
        /// resizes to fit its content, so wrap long text to
        /// <c>width - 4</c> (see <see cref="Ansi.Wrap"/>).
        /// </summary>
        public static IReadOnlyList<string> Dialog(string? title, string? footer, int width, IEnumerable<string> content, FrameStyle? style = null)
        {
            style ??= Theme.Default.Dialog;
            var b = style.Border;
            var inner = Math.Max(1, width - 4);
            var h = style.Horizontal;

            // ─── text ─── filling width - 2 columns between the corner characters
            string Labeled(string? text, Style textStyle)
            {
                text = Ansi.Truncate(text, Math.Max(0, inner - 2));
                var dashes = Math.Max(0, width - 2 - Ansi.VisibleLength(text) - 2);
                var left = dashes / 2;
                return b.Apply(new string(h, left)) + textStyle.Apply($" {text} ") + b.Apply(new string(h, dashes - left));
            }

            var lines = new List<string>
            {
                b.Apply(style.TopLeft.ToString()) + Labeled(title, style.Title) + b.Apply(style.TopRight.ToString()),
            };
            foreach (var line in content)
                lines.Add(b.Apply(style.Vertical + " ") + Ansi.PadRight(line, inner) + b.Apply(" " + style.Vertical));
            if (!string.IsNullOrEmpty(footer))
                lines.Add(b.Apply(style.LeftT.ToString()) + Labeled(footer, style.Footer) + b.Apply(style.RightT.ToString()));
            lines.Add(b.Apply(style.BottomLeft + new string(h, Math.Max(0, width - 2)) + style.BottomRight));
            return lines;
        }

        /// <summary>
        /// A horizontal divider, e.g. between sections inside a box
        /// </summary>
        public static string Rule(int width, Style? style = null, char line = '─') =>
            (style ?? Theme.Default.Dim).Apply(new string(line, Math.Max(0, width)));

        /// <summary>
        /// A full-width bar with text on the left and right, like a title or
        /// status bar. The text's own styles are replaced by the bar's.
        /// </summary>
        public static string Bar(Style style, int width, string? left, string? right)
        {
            right = Ansi.Truncate(right, Math.Max(0, width / 2));
            var leftWidth = Math.Max(0, width - Ansi.VisibleLength(right) - 1);
            return style.Apply(Ansi.PadRight(" " + Ansi.Strip(left), leftWidth) + " " + Ansi.Strip(right));
        }
    }
}
