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
            Print(bannerText, new CalliopeOptions
            {
                AntiAliasing = antiAliasing,
                Width = width,
                DrawChar = drawChar,
                SpaceChar = spaceChar,
                DrawThreshold = drawThreshold,
                Font = font,
                FontSize = fontSize,
                FontStartColor = fontColor,
                Height = height
            });
        }

        public static void Print(
            string bannerText,
            CalliopeOptions options)
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

        public static string PrintString(string bannerText, CalliopeOptions options)
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

        public static void EnableHAckerText()
        {
            if(_hackerWriter!=null)
                _hackerWriter.Hackify = false;
        }
    }
}
