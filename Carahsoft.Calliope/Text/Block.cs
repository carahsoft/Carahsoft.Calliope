namespace Carahsoft.Calliope.Text
{
    /// <summary>
    /// Arranges blocks of lines, e.g. to place panels side by side
    /// </summary>
    public static class Block
    {
        /// <summary>
        /// Splits multi-line text into lines
        /// </summary>
        public static IReadOnlyList<string> Lines(string? text) =>
            (text ?? "").Replace("\r\n", "\n").Split('\n');

        /// <summary>
        /// Exactly <paramref name="height"/> lines: extra lines are dropped and
        /// missing ones are blank
        /// </summary>
        public static IReadOnlyList<string> FitHeight(IEnumerable<string> lines, int height)
        {
            height = Math.Max(0, height);
            var result = lines.Take(height).ToList();
            while (result.Count < height)
                result.Add("");
            return result;
        }

        /// <summary>
        /// Places blocks side by side with <paramref name="gap"/> spaces between
        /// them. Each block is padded to its widest line and to the height of
        /// the tallest block.
        /// </summary>
        public static IReadOnlyList<string> JoinHorizontal(int gap, params IEnumerable<IReadOnlyList<string>> blocks)
        {
            var list = blocks.ToList();
            var height = list.Count == 0 ? 0 : list.Max(b => b.Count);

            var padded = list
                .Select(block =>
                {
                    var width = block.Count == 0 ? 0 : block.Max(Ansi.VisibleLength);
                    return FitHeight(block, height).Select(l => Ansi.PadRight(l, width)).ToList();
                })
                .ToList();

            var separator = new string(' ', Math.Max(0, gap));
            return Enumerable.Range(0, height)
                .Select(i => string.Join(separator, padded.Select(b => b[i])))
                .ToList();
        }

        /// <summary>
        /// Places blocks one above the other
        /// </summary>
        public static IReadOnlyList<string> JoinVertical(params IEnumerable<IReadOnlyList<string>> blocks) =>
            blocks.SelectMany(b => b).ToList();
    }
}
