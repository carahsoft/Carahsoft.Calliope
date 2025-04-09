using Carahsoft.Calliope;
using System.Drawing;
using System.Text;

await Calliope.NewProgram(new FractalTyping.FractalTyping()).RunAsync();

namespace FractalTyping
{
    public class FractalTyping : ICalliopeProgram
    {
        #region Local Variables
        private int offset = 0;
        private int delay = 200;
        private int smoothing = 0;
        private string userInput = "";
        private string bigText = "";
        private string[] palette = [];

        #endregion

        #region Inherited
        CalliopeCmd? ICalliopeProgram.Init()
        {
            return OffsetAnimation();
        }

        CalliopeCmd? ICalliopeProgram.Update(CalliopeMsg msg)
        {
            if (msg is OffsetMsg)
            {
                offset = offset < 32767 ? offset + 1 : 0; // avoid crash on int overflow
                return OffsetAnimation();
            }

            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.Escape || kpm.Modifiers == ConsoleModifiers.Control && kpm.Key == ConsoleKey.C)
                    return CalliopeCmd.Quit;
                else if (kpm.Key == ConsoleKey.LeftArrow)
                    smoothing = smoothing > 0 ? smoothing - 1 : smoothing;
                else if (kpm.Key == ConsoleKey.RightArrow)
                    smoothing = smoothing < 4 ? smoothing + 1 : smoothing;
                else if (kpm.Key == ConsoleKey.UpArrow)
                    delay = delay > 25 ? delay - 25 : delay;
                else if (kpm.Key == ConsoleKey.DownArrow)
                    delay = delay < 1000 ? delay + 25 : delay;
                else if (kpm.Key == ConsoleKey.Backspace)
                    userInput = userInput.Length > 0 ? userInput.Substring(0, userInput.Length - 1) : userInput;
                else if (kpm.Key != ConsoleKey.Tab && kpm.Key != ConsoleKey.Enter && kpm.Modifiers != ConsoleModifiers.Control)
                    userInput += kpm.KeyChar;

                bigText = Calliope.PrintString(userInput, new CalliopePrintOptions() { Height = 23, Width = 180, });
                palette = stringToRainbowStringArray(userInput, smoothing);
            }
            return null;
        }

        string ICalliopeProgram.View()
        {
            string helpBanner = "Type to create fractal text, ↑/↓ to change speed, ←/→ to adjust color smoothing, ^C or ESC to quit\n";
            if (palette == null || palette.Length == 0)
                return helpBanner;

            string fractalText = repaintAsciiArt(bigText, palette, offset);
            return helpBanner + fractalText;
        }

        #endregion

        #region Animation Helper
        private class OffsetMsg : CalliopeMsg { }

        private CalliopeCmd OffsetAnimation()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(delay);
                return new OffsetMsg();
            });
        }

        #endregion

        #region String and Color Operations
        /// <summary>
        /// Replaces the individual characters of a block of ASCII Art with characters from a palette.
        /// Each string in the pallete should render as one character wide.
        /// </summary>
        /// <param name="bigText"></param>
        /// <param name="palette"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string repaintAsciiArt(string bigText, string[] palette, int offset = 0)
        {
            StringBuilder fractalText = new StringBuilder();
            int paletteIndex = 0;
            foreach (char c in bigText)
            {
                if (c != ' ' && c != '\n' && c != '\r')
                    fractalText.Append(palette[(paletteIndex + offset) % palette.Length]);
                else
                    fractalText.Append(c);

                paletteIndex = paletteIndex != palette.Length ? paletteIndex + 1 : 0;
            }
            return fractalText.ToString();
        }

        /// <summary>
        /// Returns a list with each character of the string rainbowfied using ANSI escape codes.
        /// Using large smoothing or separtor values with cause issues.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="smoothing"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] stringToRainbowStringArray(string str, int smoothing = 0, string separator = "")
        {
            string letters = str.Replace(" ", "");
            if (letters.Length <= 1) 
                return [letters];

            for (int i = 0; i < smoothing; i++)
                letters += letters;

            if (separator.Length > 0)
                for (int i = letters.Length - 1; i >= 0; i--)
                    letters = letters.Insert(i, separator);

            Color[] palette = generateNeonColorPalette(letters.Length);
            string[] rainbowStringArray = new string[letters.Length];
            for (int i = 0; i < letters.Length; i++)
                rainbowStringArray[i] = colorizeString(letters[i].ToString(), palette[i]);
            
            return rainbowStringArray;
        }

        /// <summary>
        /// Returns a list of equidistant neon Colors.
        /// </summary>
        /// <param name="numColors"></param>
        /// <returns></returns>
        public static Color[] generateNeonColorPalette(int numColors)
        {
            Color start = Color.FromArgb(255, 0, 0);
            Color[] palette = new Color[numColors];
            palette[0] = start;
            int stepSize = (255 * 6) / numColors;

            for (int i = 1; i < numColors; i++)
                palette[i] = rotateNeonHue(palette[i-1], stepSize);

            return palette;
        }

        /// <summary>
        /// Returns a neon Color with its hue rotated. (Neon meaning one RGB value is 0 and another is 255)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Color rotateNeonHue(Color c, int rotation)
        {
            int[] colorBuilder = { c.R, c.G, c.B };
            int[] rgbDeltas = { 0, 0, 0, 0, 0, 0 }; // +g, -r, +b, -g, +r, -b
            int deltaHead = 0;
            int rotationRounder = 0;

            if (c.R == 255 && c.G < 255 && c.B == 0) // +g
            {
                deltaHead = 0;
                rotationRounder = c.G;
                colorBuilder[1] = 0;
            }
            else if (c.R > 0 && c.G == 255 && c.B == 0) // -r
            {
                deltaHead = 1;
                rotationRounder = 255 - c.R;
                colorBuilder[0] = 255;
            }
            else if (c.R == 0 && c.G == 255 && c.B < 255) // +b
            {
                deltaHead = 2;
                rotationRounder = c.B;
                colorBuilder[2] = 0;
            }
            else if (c.R == 0 && c.G > 0 && c.B == 255) // -g
            {
                deltaHead = 3;
                rotationRounder = 255 - c.G;
                colorBuilder[1] = 255;
            }
            else if (c.R < 255 && c.G == 0 && c.B == 255) // +r
            {
                deltaHead = 4;
                rotationRounder = c.R;
                colorBuilder[0] = 0;
            }
            else if (c.R == 255 && c.G == 0 && c.B > 0) // -b
            {
                deltaHead = 5;
                rotationRounder = 255 - c.B;
                colorBuilder[2] = 255;
            }

            int totalRotation = (rotation % (255 *6)) + rotationRounder;
            while (totalRotation > 255)
            {
                rgbDeltas[deltaHead] += 255;
                totalRotation -= 255;
                deltaHead = (deltaHead < 6) ? deltaHead + 1 : 0;
            }
            rgbDeltas[deltaHead] += totalRotation;

            colorBuilder[0] += rgbDeltas[4] - rgbDeltas[1];
            colorBuilder[1] += rgbDeltas[0] - rgbDeltas[3];
            colorBuilder[2] += rgbDeltas[2] - rgbDeltas[5];

            return Color.FromArgb(colorBuilder[0], colorBuilder[1], colorBuilder[2]);
        }

        /// <summary>
        /// Returns a string colorized using ANSI escape codes.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string colorizeString(string str, Color c)
        {
            var sb = new StringBuilder();
            sb.Append($"\x1b[38;2;{(int)c.R};{(int)c.G};{(int)c.B}m{str}\x1b[0m");
            return sb.ToString();
        }

        #endregion
    }
}
