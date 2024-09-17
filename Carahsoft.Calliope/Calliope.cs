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

        public static async Task PrintEffect(string bannerText, CalliopeOptions options)
        {
            var skConverter = new SkiaConverter(
                bannerText: bannerText,
                options: options
            );

            if (options.Effect == CalliopeEffect.Twinkle)
            {
                await NewProgram(new TwinkleProgram(bannerText, options)).RunAsync();
            }

            skConverter.Print();
        }

        public static string PrintString(string bannerText, CalliopeOptions options)
        {
            var skConverter = new SkiaConverter(
                bannerText: bannerText,
                options: options
            );
            return skConverter.ToString();
        }

        public static CalliopeProgramBuilder<TModel> NewProgram<TModel>(ICalliopeProgram<TModel> program)
        {
            return new CalliopeProgramBuilder<TModel>(program);
        }

        public static (TModel, CalliopeCmd?) Return<TModel>(TModel model, CalliopeCmd? cmd = null)
        {
            return (model, cmd);
        }

        private record TwinkleState
        {
            public int LastRender { get; init; }
        }

        private class TwinkleMsg : CalliopeMsg { }

        private class TwinkleProgram : ICalliopeProgram<TwinkleState>
        {
            private readonly string _bannerText;
            private readonly CalliopeOptions _options;

            public TwinkleProgram(string bannerText, CalliopeOptions options)
            {
                _bannerText = bannerText;
                _options = options;
            }

            public (TwinkleState, CalliopeCmd?) Init()
            {
                return (new(), Twinkle());
            }

            public (TwinkleState, CalliopeCmd?) Update(TwinkleState state, CalliopeMsg msg)
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                    {
                        return (state, CalliopeCmd.Quit);
                    }
                }

                if (msg is TwinkleMsg)
                {
                    return (state, Twinkle());
                }
                return (state, Twinkle());
            }

            public string View(TwinkleState state)
            {
                var skConverter = new SkiaConverter(_bannerText, _options);
                return skConverter.ToString();
            }

            private CalliopeCmd Twinkle()
            {
                return CalliopeCmd.Make(async () =>
                {
                    await Task.Delay(1000);
                    return new TwinkleMsg();
                });
            }
        }
    }
}
