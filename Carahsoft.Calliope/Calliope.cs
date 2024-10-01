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

        [Obsolete("Use Print(string, CalliopePrintOptions) instead")]
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

        /// <summary>
        /// Print the ASCII text directly to StandardOut
        /// </summary>
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

        /// <summary>
        /// Run the CalliopeAnimation program
        /// </summary>
        public static async Task PrintAnimatedEffect(
            CalliopeAnimation animator)
        {
            await NewProgram(animator).RunAsync();
        }

        /// <summary>
        /// Print the ASCII text to a string
        /// </summary>
        public static string PrintString(string bannerText, CalliopePrintOptions options)
        {
            var skConverter = new SkiaConverter(
                bannerText: bannerText,
                options: options
            );
            return skConverter.ToString();
        }

        /// <summary>
        /// Creates a new CalliopeProgramBuilder which is used to set options
        /// on the program before building and executing the ProgramRunner
        /// </summary>
        public static CalliopeProgramBuilder<TProgram> NewProgram<TProgram>(TProgram program)
            where TProgram : ICalliopeProgram
        {
            return new CalliopeProgramBuilder<TProgram>(program);
        }

        /// <summary>
        /// Enable awesomeness (slow console printing for l33t h4x0r skills).
        /// Not to be used with ICalliopePrograms
        /// </summary>
        public static void EnableHackerText()
        {
            if (_hackerWriter == null)
            {
                _hackerWriter = new HackerWriter(Console.Out);
                Console.SetOut(_hackerWriter);
            }
            _hackerWriter.Hackify = true;
        }

        /// <summary>
        /// Disables awesomeness
        /// </summary>
        public static void DisableHackerText()
        {
            if(_hackerWriter!=null)
                _hackerWriter.Hackify = false;
        }

        /// <summary>
        /// Wraps the text in ANSI color codes for printing to console.
        /// </summary>
        public static string ColorText(string text, RgbColor foreground, RgbColor? background = null)
        {
            return AnsiTextHelper.ColorText(text, foreground, background);
        }

        /// <summary>
        /// Wraps the text in ANSI color codes for printing to console.
        /// Foreground color fades between start and end, with optional background color.
        /// </summary>
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
