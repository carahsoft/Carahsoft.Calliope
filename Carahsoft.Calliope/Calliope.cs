using Carahsoft.Calliope.Animations;
using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public static class Calliope
    {

        private static HackerWriter _hackerWriter;

        public static void Print(
            string bannerText,
            int width = 120,
            int height = 28,
            string font = "Roboto",
            int fontSize = 18,
            ConsoleColor fontColor = ConsoleColor.DarkRed,
            char drawChar = '\u2580',
            char spaceChar = ' ',
            byte drawThreshold = 255,
            bool antiAliasing = true)
        {
            Print(bannerText, new CalliopePrintOptions
            {
                AntiAliasing = antiAliasing,
                Width = width,
                DrawChar = drawChar,
                SpaceChar = spaceChar,
                Font = font,
                FontSize = fontSize,
                Height = height
            });
        }

        public static void Print(
            string bannerText,
            CalliopePrintOptions options)
        {
            var skConverter = new SkiaConverter(
                bannerText: bannerText,
                options: options
            );


            skConverter.Print();
        }

        public static async Task PrintAnimatedEffect(
            CalliopeAnimation animator)
        {
            await NewProgram(animator).RunAsync();
        }

        public static string PrintString(string bannerText, CalliopePrintOptions options)
        {
            var skConverter = new SkiaConverter(
                bannerText: bannerText,
                options: options
            );
            return skConverter.ToString();
        }

        public static CalliopeProgramBuilder<TProgram> NewProgram<TProgram>(TProgram program)
            where TProgram : ICalliopeProgram
        {
            return new CalliopeProgramBuilder<TProgram>(program);
        }

        public static void EnableHackerText()
        {
            if (_hackerWriter == null)
            {
                _hackerWriter = new HackerWriter(Console.Out);
                Console.SetOut(_hackerWriter);
            }
            _hackerWriter.Hackify = true;
        }

        public static void DisableHackerText()
        {
            if(_hackerWriter!=null)
                _hackerWriter.Hackify = false;
        }

        public static string ColorText(string text, RgbColor color, RgbColor? background = null)
        {
            return AnsiTextHelper.ColorText(text, color, background);
        }

        public static string ColorTextGradient(string text, RgbColor start, RgbColor end, RgbColor? background = null)
        {
            return AnsiTextHelper.ColorTextGradient(text, start, end, background);
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
            return AnsiTextHelper.TruncateLineToLength(line, length);
        }
    }
}
