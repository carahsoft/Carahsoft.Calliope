using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// A popup dialog that overlays content on top of a background view.
    /// Wraps an inner ICalliopeProgram and renders it inside a bordered dialog box.
    /// </summary>
    public class PopupDialog<T> where T : ICalliopeProgram
    {
        private readonly T _innerProgram;

        public string Title { get; set; } = "";
        public string Footer { get; set; } = "";
        public RgbColor BorderColor { get; set; } = new RgbColor(80, 80, 80);
        public RgbColor TitleColor { get; set; } = new RgbColor(49, 85, 164);
        public RgbColor FooterColor { get; set; } = new RgbColor(100, 100, 100);

        /// <summary>
        /// The inner program being displayed in the popup
        /// </summary>
        public T Inner => _innerProgram;

        public PopupDialog(T innerProgram)
        {
            _innerProgram = innerProgram;
        }

        public PopupDialog(T innerProgram, string title, string footer = "")
        {
            _innerProgram = innerProgram;
            Title = title;
            Footer = footer;
        }

        /// <summary>
        /// Initialize the inner program
        /// </summary>
        public CalliopeCmd? Init() => _innerProgram.Init();

        /// <summary>
        /// Forward messages to the inner program
        /// </summary>
        public CalliopeCmd? Update(CalliopeMsg msg) => _innerProgram.Update(msg);

        /// <summary>
        /// Get the inner program's view (without popup border)
        /// </summary>
        public string View() => _innerProgram.View();

        /// <summary>
        /// Render the popup dialog overlaid on top of the background view
        /// </summary>
        /// <param name="background">The background view to overlay on</param>
        /// <param name="screenWidth">Screen width for centering</param>
        /// <param name="screenHeight">Screen height for centering</param>
        /// <returns>The combined view with popup overlaid</returns>
        public string RenderOverlay(string background, int screenWidth, int screenHeight)
        {
            var content = _innerProgram.View();
            return OverlayPopup(background, content, screenWidth, screenHeight);
        }

        private string OverlayPopup(string background, string content, int screenWidth, int screenHeight)
        {
            var bgLines = Block.Lines(background);
            if (screenHeight <= 0) screenHeight = bgLines.Count;
            if (screenWidth <= 0) screenWidth = 80;

            var style = new FrameStyle
            {
                Border = Style.FromColor(BorderColor),
                Title = Style.FromColor(TitleColor),
                Footer = Style.FromColor(FooterColor),
            };

            var popup = Window.Popup(screenWidth, screenHeight, Title, Footer, Block.Lines(content), bgLines, style);
            return string.Join('\n', popup.Lines);
        }
    }
}
