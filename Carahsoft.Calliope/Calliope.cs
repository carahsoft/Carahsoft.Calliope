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

        public static async Task PrintAnimatedEffect(string bannerText, CalliopeOptions options, CalliopeAnimation animation)
        {

            if (animation == CalliopeAnimation.Twinkle)
            {
                await NewProgram(new TwinkleProgram(bannerText, options)).RunAsync();
            }
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
            private readonly Random rand = new Random();

            public TwinkleProgram(string bannerText, CalliopeOptions options)
            {
                _bannerText = bannerText;
                _options = options;
                _options.Effect = CalliopeEffect.GalacticCrush;
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
              
                StringBuilder sb = new StringBuilder();

                foreach(char c in skConverter.ToString())
                {
                    if(rand.Next(100) < 10 && c!= ' ')
                    {
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255))));
                    }
                    else
                    {
                        if(rand.Next(2)== 1)
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new(0, (byte)(100 + rand.Next(155)), 0)));

                        else
                            sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)(100 + rand.Next(155)), 0, 0)));
                    }
                }

                return sb.ToString();
            }

            private CalliopeCmd Twinkle()
            {
                return CalliopeCmd.Make(async () =>
                {
                    await Task.Delay(333);
                    return new TwinkleMsg();
                });
            }
        }
    }
}
