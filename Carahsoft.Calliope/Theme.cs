using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope
{
    /// <summary>
    /// The styles the built-in components and frames are drawn with. Use
    /// <c>with</c> to change some of them, e.g.
    /// <c>Theme.Default with { Accent = RgbColors.Red }</c>.
    /// </summary>
    public sealed record Theme
    {
        private static readonly RgbColor Grey = new(130, 130, 130);

        public static Theme Default { get; } = new();

        /// <summary>
        /// The main color, used for dialog borders and highlights
        /// </summary>
        public RgbColor Accent { get; init; } = RgbColors.CarahBlue;
        /// <summary>
        /// Secondary text: hints, descriptions, borders of boxes
        /// </summary>
        public Style Dim { get; init; } = Style.FromColor(Grey);
        /// <summary>
        /// The selected row, option or button
        /// </summary>
        public Style Selected { get; init; } = Style.Default.WithForegroundColor(RgbColors.White).WithBackgroundColor(RgbColors.CarahBlue);
        /// <summary>
        /// Table column titles
        /// </summary>
        public Style Header { get; init; } = Style.Bold.WithForegroundColor(Grey);
        /// <summary>
        /// The selected column's title in a table header
        /// </summary>
        public Style SelectedHeader { get; init; } = Style.Bold.WithForegroundColor(new RgbColor(120, 175, 255));
        /// <summary>
        /// Keys in hints, and the focused field's label
        /// </summary>
        public Style Key { get; init; } = Style.Bold.WithForegroundColor(RgbColors.White);
        /// <summary>
        /// Titles of boxes
        /// </summary>
        public Style Label { get; init; } = Style.Bold;
        public Style Error { get; init; } = Style.FromColor(new RgbColor(235, 90, 90));
        public Style Warning { get; init; } = Style.FromColor(new RgbColor(230, 190, 60));

        /// <summary>
        /// The frame of <see cref="Frame.Box"/>: dim rounded borders with a bold title
        /// </summary>
        public FrameStyle Box => new() { Border = Dim, Title = Label, Footer = Dim };

        /// <summary>
        /// The frame of <see cref="Frame.Dialog"/> and popups: rounded borders
        /// and a bold title in the accent color
        /// </summary>
        public FrameStyle Dialog => new()
        {
            Border = Style.FromColor(Accent),
            Title = Style.Bold.WithForegroundColor(Accent),
            Footer = Dim,
        };
    }
}
