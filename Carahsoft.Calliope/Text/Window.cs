namespace Carahsoft.Calliope.Text
{
    /// <summary>
    /// A popup drawn over other lines, and where its content starts, so other
    /// windows can be placed relative to it
    /// </summary>
    /// <param name="Lines">The background with the popup drawn over it</param>
    /// <param name="ContentRow">The line the popup's first content line is on</param>
    /// <param name="ContentColumn">The column the popup's content starts at</param>
    public sealed record PopupLayout(IReadOnlyList<string> Lines, int ContentRow, int ContentColumn);

    /// <summary>
    /// A window to draw over other lines, anchored to a line and column of
    /// them, e.g. a dropdown list below its field
    /// </summary>
    public sealed record FloatingWindow(int Row, int Column, IReadOnlyList<string> Lines);

    /// <summary>
    /// Draws windows over other lines, keeping the text on both sides of them
    /// and its styles
    /// </summary>
    public static class Window
    {
        /// <summary>
        /// Draws lines over others: from the given row and column, each line of
        /// <paramref name="over"/> replaces that many columns of the line beneath,
        /// keeping what's to its left and right. Adds lines if needed.
        /// </summary>
        public static IReadOnlyList<string> OverlayAt(int row, int column, IReadOnlyList<string> over, IReadOnlyList<string> under)
        {
            row = Math.Max(0, row);
            column = Math.Max(0, column);
            var height = Math.Max(under.Count, row + over.Count);
            var result = new List<string>(height);

            for (int i = 0; i < height; i++)
            {
                var line = i < under.Count ? under[i] : "";
                if (i >= row && i - row < over.Count)
                {
                    var window = over[i - row];
                    var left = Ansi.Cut(line, column);
                    left += new string(' ', Math.Max(0, column - Ansi.VisibleLength(left)));
                    line = left + window + Ansi.Skip(line, column + Ansi.VisibleLength(window));
                }
                result.Add(line);
            }

            return result;
        }

        /// <summary>
        /// Draws a centered dialog (see <see cref="Frame.Dialog"/>) over the
        /// background, sized to its content up to the screen size. The result
        /// is exactly <paramref name="height"/> lines.
        /// </summary>
        public static PopupLayout Popup(int width, int height, string? title, string? footer,
            IReadOnlyList<string> content, IReadOnlyList<string> background, FrameStyle? style = null)
        {
            if (content.Count == 0)
                content = [""];
            // Room for the borders and the footer divider
            content = content.Take(Math.Max(1, height - 4)).ToList();

            var widest = content.Max(Ansi.VisibleLength);
            var labels = Math.Max(Ansi.VisibleLength(title), Ansi.VisibleLength(footer)) + 4;
            var boxWidth = Math.Min(Math.Max(12, width - 2), Math.Max(widest, labels) + 4);
            var box = Frame.Dialog(title, footer, boxWidth, content, style);

            var top = Math.Max(0, (height - box.Count) / 2);
            var left = Math.Max(0, (width - boxWidth) / 2);
            var lines = OverlayAt(top, left, box, Block.FitHeight(background, height)).Take(height).ToList();
            return new PopupLayout(lines, top + 1, left + 2);
        }

        /// <summary>
        /// Draws a small window (e.g. a dropdown list) below the line at
        /// <paramref name="row"/>, or above it if it wouldn't fit on screen
        /// below. The result is at most <paramref name="height"/> lines.
        /// </summary>
        public static IReadOnlyList<string> FloatBelow(int height, int row, int column, IReadOnlyList<string> window, IReadOnlyList<string> lines)
        {
            var below = row + 1;
            var top = below + window.Count <= height ? below : Math.Max(0, row - window.Count);
            return OverlayAt(top, column, window, lines).Take(height).ToList();
        }

        /// <summary>
        /// Draws a floating window below its anchor line, offset by the given
        /// row and column (e.g. where a popup's content starts)
        /// </summary>
        public static IReadOnlyList<string> FloatBelow(int height, FloatingWindow window, IReadOnlyList<string> lines, int rowOffset = 0, int columnOffset = 0) =>
            FloatBelow(height, window.Row + rowOffset, window.Column + columnOffset, window.Lines, lines);
    }
}
