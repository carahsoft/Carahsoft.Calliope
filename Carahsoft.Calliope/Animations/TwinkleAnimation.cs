using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    public record TwinkleState
    {
        public Guid? LastTwinkle { get; init; }
        public required string Render { get; init; }
    }

    public class TwinkleMsg : CalliopeMsg
    {
        public required Guid TwinkleId { get; init; }
    }

    public class TwinkleAnimation : CalliopeAnimation<TwinkleState>
    {
        private readonly Random rand = new Random();

        public TwinkleAnimation(string bannerText, CalliopeOptions options)
            : base(bannerText, options)
        {
            // always display with the GalacticCrush effect
            Options.Effect = CalliopeEffect.GalacticCrush;
        }

        public override (TwinkleState, CalliopeCmd?) Init()
        {
            return (new() { Render = GenerateText() }, Twinkle());
        }

        public override (TwinkleState, CalliopeCmd?) Update(TwinkleState state, CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return (state, CalliopeCmd.Quit);
                }
            }

            if (msg is TwinkleMsg tm)
            {
                return (state with { Render = GenerateText() }, Twinkle());
            }

            return (state, null);
        }

        public override string View(TwinkleState state)
        {
            return state.Render;
        }

        private string GenerateText()
        {
            var rendered = Calliope.PrintString(RenderText, Options);

            StringBuilder sb = new StringBuilder();

            foreach (char c in rendered)
            {
                if (rand.Next(100) < 10 && c != ' ')
                {
                    sb.Append(AnsiTextHelper.ColorText(c.ToString(), new((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255))));
                }
                else
                {
                    if (rand.Next(2) == 1)
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
                return new TwinkleMsg
                {
                    TwinkleId = Guid.NewGuid()
                };
            });
        }
    }
}
