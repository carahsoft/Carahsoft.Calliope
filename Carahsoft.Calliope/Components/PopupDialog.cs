using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            // Get content lines and find max width
            var contentLines = content.Split('\n');
            var maxContentWidth = contentLines.Max(l => AnsiTextHelper.StripAnsi(l).Length);
            var boxWidth = Math.Max(Math.Max(maxContentWidth, Title.Length + 4), Footer.Length + 4) + 4;

            // Get background lines
            var bgLines = background.Split('\n').ToList();
            if (screenHeight <= 0) screenHeight = bgLines.Count;
            if (screenWidth <= 0) screenWidth = 80;

            // Calculate popup position (centered)
            var popupHeight = contentLines.Length + 4; // content + title + footer + borders
            var leftPad = Math.Max((screenWidth - boxWidth) / 2, 0);
            var topPad = Math.Max((screenHeight - popupHeight) / 2, 0);

            // Build popup lines (without left padding - we'll handle that in overlay)
            var popupLines = new List<string>();

            // Top border with title
            var titlePad = (boxWidth - Title.Length - 4) / 2;
            popupLines.Add(
                AnsiTextHelper.ColorText("╭" + new string('─', titlePad), BorderColor) +
                AnsiTextHelper.ColorText($" {Title} ", TitleColor) +
                AnsiTextHelper.ColorText(new string('─', boxWidth - titlePad - Title.Length - 4) + "╮", BorderColor));

            // Content lines
            foreach (var line in contentLines)
            {
                var strippedLen = AnsiTextHelper.StripAnsi(line).Length;
                var rightPadLen = boxWidth - strippedLen - 4;
                popupLines.Add(
                    AnsiTextHelper.ColorText("│ ", BorderColor) +
                    line +
                    new string(' ', Math.Max(rightPadLen, 0)) +
                    AnsiTextHelper.ColorText(" │", BorderColor));
            }

            // Footer line
            var footerPad = (boxWidth - Footer.Length - 4) / 2;
            popupLines.Add(
                AnsiTextHelper.ColorText("├" + new string('─', footerPad), BorderColor) +
                AnsiTextHelper.ColorText($" {Footer} ", FooterColor) +
                AnsiTextHelper.ColorText(new string('─', boxWidth - footerPad - Footer.Length - 4) + "┤", BorderColor));

            // Bottom border
            popupLines.Add(AnsiTextHelper.ColorText("╰" + new string('─', boxWidth - 2) + "╯", BorderColor));

            // Overlay popup lines onto background, preserving text on left and right
            var popupEndCol = leftPad + boxWidth;
            for (int i = 0; i < popupLines.Count && (topPad + i) < bgLines.Count; i++)
            {
                var bgLine = bgLines[topPad + i];
                var bgStripped = AnsiTextHelper.StripAnsi(bgLine);

                // Get the part of background before the popup (truncate to leftPad chars)
                string leftPart;
                if (leftPad > 0 && bgStripped.Length > 0)
                {
                    leftPart = AnsiTextHelper.TruncateLineToLength(bgLine, leftPad);
                    // Pad if background was shorter than leftPad
                    var leftPartLen = AnsiTextHelper.StripAnsi(leftPart).Length;
                    if (leftPartLen < leftPad)
                        leftPart += new string(' ', leftPad - leftPartLen);
                }
                else
                {
                    leftPart = new string(' ', leftPad);
                }

                // Get the part of background after the popup
                var rightPart = "";
                if (bgStripped.Length > popupEndCol)
                {
                    // Skip to the position after popup ends (accounting for ANSI codes)
                    var visibleCount = 0;
                    var rightStartIdx = 0;
                    var inEscape = false;
                    for (int j = 0; j < bgLine.Length; j++)
                    {
                        var c = bgLine[j];
                        if (inEscape)
                        {
                            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                                inEscape = false;
                        }
                        else if (c == '\x1b')
                        {
                            inEscape = true;
                        }
                        else
                        {
                            visibleCount++;
                            if (visibleCount == popupEndCol)
                            {
                                rightStartIdx = j + 1;
                                break;
                            }
                        }
                    }
                    if (rightStartIdx > 0 && rightStartIdx < bgLine.Length)
                        rightPart = bgLine.Substring(rightStartIdx);
                }

                bgLines[topPad + i] = leftPart + popupLines[i] + rightPart;
            }

            return string.Join('\n', bgLines);
        }
    }
}
