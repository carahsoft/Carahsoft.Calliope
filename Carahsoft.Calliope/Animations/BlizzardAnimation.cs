using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{

    public class BlizzardMsg : CalliopeMsg
    {
        public required Guid BlizzardId { get; init; }
    }
    public class BlizzardAnimation : CalliopeAnimation
    {

        private readonly Random rand = new Random();
        private bool[,] SnowMap ;

        private string Render { get; set; }


        public BlizzardAnimation(string bannerText, CalliopeOptions options): base(bannerText, options)
        {
            SnowMap = new bool[Options.Width, Options.Height];

            Options.Effect = CalliopeEffect.None;
        }

        public override CalliopeCmd? Init()
        {
            Render = GenerateText();
            return Snow();
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

            if (msg is BlizzardMsg tm)
            {
                Render = GenerateText();
                return Snow();
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

            int i = 0;
            int j = 0;

            Tick();

            foreach (char c in rendered)
            {

                if (c == '\n')
                {
                    i = 0;
                    j++;
                    sb.Append(c);
                    continue;
                }
                else if (c == ' ')
                {
                    sb.Append(' ');
                }
                else
                {
                    if (SnowMap[i, j] == true)
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)255, (byte)255, (byte)255)));
                    else
                    {
                        sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)0, (byte)0, (byte)255)));
                    }
                }
                i++;
            }



            return sb.ToString();
        }

        private void Tick()
        {
            for (int i = 0; i < Options.Width; i++)
                SnowMap[i, Options.Height-1] = false;  //clear last row of drops

            for(int i = 0; i < Options.Width; i++)
            {
                for(int j = Options.Height -1; j > 0; j--)
                {
                    if (SnowMap[i,j-1] == true)
                    {
                        SnowMap[i, j - 1] = false;  //cascade drops down
                        SnowMap[i, j] = true;
                    }
                }
            }

            for(int i=0;i< rand.Next(50); i++)
            {
                SnowMap[rand.Next(Options.Width), 0] = true;  //add a random number of new drops at random places
            }
        }

      


        private CalliopeCmd Snow()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(200);
                return new BlizzardMsg
                {
                    BlizzardId = Guid.NewGuid()
                };
            });
        }
    }
}
