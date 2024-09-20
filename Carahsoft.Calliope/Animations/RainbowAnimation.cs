using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    public record RainbowAnimationState
    {
        public int Frame { get; init; }
    }
    public class RainbowAnimation : CalliopeAnimation<RainbowAnimationState>
    {
        private readonly List<List<RainbowChar>> _renderComponents;

        public RainbowAnimation(string renderText, CalliopeOptions options)
            : base(renderText, options)
        {
            var renderLines = Calliope.PrintString(renderText, options).Replace("\r\n", "\n").Split('\n');

            _renderComponents = renderLines.Select(line => line.Select((x, i) => new RainbowChar(x, i * 6)).ToList()).ToList();
        }

        public override (RainbowAnimationState, CalliopeCmd?) Init()
        {
            return (new(), Tick());
        }

        public override (RainbowAnimationState, CalliopeCmd?) Update(RainbowAnimationState state, CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return (state, CalliopeCmd.Quit);
                }
            }
            if (msg is TickMsg)
            {
                if (state.Frame > 100000)
                    return (state, CalliopeCmd.Quit);

                return (state with { Frame = state.Frame + 2 }, Tick());
            }

            return (state, null);
        }

        public override string View(RainbowAnimationState state)
        {
            var sb = new StringBuilder();
            foreach (var line in _renderComponents)
            {
                foreach (var c in line)
                {
                    sb.Append(c.View(state.Frame));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private CalliopeCmd Tick()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1) / 60);
                return new TickMsg();
            });
        }

        private class RainbowChar
        {
            private readonly char _displayChar;
            private readonly int _startIndex;

            public RainbowChar(char displayChar, int startIndex)
            {
                _displayChar = displayChar;
                _startIndex = startIndex;
            }

            public string View(int frame)
            {
                if (_displayChar == ' ')
                {
                    return " ";
                }

                var i = (frame + _startIndex) % 600;
                var p = (decimal)(i % 100) / 100;
                var g = (byte)(p * 255);
                var ig = (byte)(255 - g);
                RgbPixel c = i switch
                {
                    int x when x < 100 => new(255, g, 0),
                    int x when x < 200 => new(ig, 255, 0),
                    int x when x < 300 => new(0, 255, g),
                    int x when x < 400 => new(0, ig, 255),
                    int x when x < 500 => new(g, 0, 255),
                    int x when x < 600 => new(255, 0, ig),
                };
                return AnsiTextHelper.ColorText(_displayChar.ToString(), c);
            }
        }

        private class TickMsg : CalliopeMsg { }
    }
}
