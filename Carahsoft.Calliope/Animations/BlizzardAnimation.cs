using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{

    public class BlizzardMsg : CalliopeMsg { }

    public class BlizzardAnimation : CalliopeAnimation
    {

        private readonly Random rand = new Random();
        private int[,] SnowMap ;

        private string Render { get; set; }


        public BlizzardAnimation(string bannerText, CalliopePrintOptions options): base(bannerText, options)
        {
            SnowMap = new int[Options.Width, Options.Height];

            Options.Effect = CalliopeEffect.None;

            TractorPositions = Enumerable.Repeat(-1, Options.Height).ToArray();
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

            // sb.Append("🚜");

            foreach (char c in rendered)
            {

                if (c == '\n')
                {
                    i = 0;
                    j++;
                    sb.Append(c);
                    continue;
                }
                else if (i == TractorPositions[j])
                {
                    sb.Append("🚜");
                }
                else if (c == ' ')
                {
                    sb.Append(' ');
                }
                else
                {
                    if (SnowMap[i, j] == 1)
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

        private bool snowmode = true;

        private int[] TractorPositions;

        int tractor = 120;
        private void Tick()
        {


            if (snowmode && SnowMap.Cast<int>().Min() > 0)
            {
                snowmode = false;

                for (int x = 0; x < TractorPositions.Length; x++)
                    TractorPositions[x] = 120 + rand.Next(Options.Height);

            }
            else if (!snowmode && SnowMap.Cast<int>().Max() == 0)
            {
                snowmode = true;
                TractorPositions = Enumerable.Repeat(-1, Options.Height).ToArray();
            }

            if (snowmode)
            {
                for (int i = 0; i < Options.Width; i++)
                {
                    for (int j = Options.Height - 1; j > 0; j--)
                    {
                        if (SnowMap[i, j - 1] == 1 && SnowMap[i, j] != 1)
                        {
                            SnowMap[i, j - 1] = 0;  //cascade flakes down
                            SnowMap[i, j] = 1;
                        }
                    }
                }

                for (int i = 0; i < rand.Next(100); i++)
                {
                    SnowMap[rand.Next(Options.Width), 0] = 1;  //add a random number of new flakes at random places
                }
            }

            else
            {
                for (int x = 0; x < TractorPositions.Length; x++)
                {
                    TractorPositions[x]--;

                    if(TractorPositions[x] < Options.Width && TractorPositions[x] >= 0)
                        SnowMap[TractorPositions[x], x] = 0;
                    
                }
            }

        }

      


        private CalliopeCmd Snow()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(200);
                return new BlizzardMsg();
            });
        }
    }
}
