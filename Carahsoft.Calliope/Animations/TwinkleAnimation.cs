using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    public class TwinkleAnimation : CalliopeAnimation
    {
        private readonly Random rand = new Random();

        private Guid? LastTwinkle { get; set; }
        private string Render { get; set; }


        public TwinkleAnimation(string bannerText, CalliopePrintOptions options)
            : base(bannerText, options)
        {
            // always display with the GalacticCrush effect
            Options.Effect = CalliopeEffect.GalacticCrush;
        }

        public override CalliopeCmd? Init()
        {
            Render = GenerateText();
            return Twinkle();
        }

        public override CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return CalliopeCmd.Quit;
                }
            }

            if (msg is TwinkleMsg)
            {
                Render = GenerateText();
                return Twinkle();
            }

            return null;
        }

        public override string View()
        {
            return Render;
        }

        private string GenerateText()
        {
            var rendered = Calliope.PrintString(RenderText, Options);

            StringBuilder sb = new StringBuilder();

            foreach (char c in rendered.Replace("\r\n", "\n"))
            {
                if (rand.Next(100) < 10 && c != ' ')
                {
                    sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255))));
                }
                else if (c != ' ')
                {
                    if (rand.Next(2) == 1)
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new(0, (byte)(100 + rand.Next(155)), 0)));

                    else
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)(100 + rand.Next(155)), 0, 0)));
                }
                else
                {
                    sb.Append(c);
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

        private class TwinkleMsg : CalliopeMsg { }
    }
}
