using Carahsoft.Calliope.AnsiConsole;
using System.Text;

namespace Carahsoft.Calliope.Animations
{
    public class RainbowAnimation : CalliopeAnimation
    {
        private readonly List<List<RainbowChar>> _renderComponents;

        public int Frame { get; private set; }

        public RainbowAnimation(string renderText, CalliopePrintOptions options)
            : base(renderText, options)
        {
            var renderLines = Calliope.PrintString(renderText, options).Replace("\r\n", "\n").Split('\n');

            _renderComponents = renderLines.Select(line => line.Select((x, i) => new RainbowChar(x, i * 6)).ToList()).ToList();
        }

        public override CalliopeCmd? Init()
        {
            return Tick();
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
            if (msg is TickMsg)
            {
                Frame = Frame + 2;
                return Tick();
            }

            return null;
        }

        public override string View()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _renderComponents.Count; i++)
            {
                foreach (var c in _renderComponents[i])
                {
                    sb.Append(c.View(Frame));
                }

                if (i != _renderComponents.Count - 1)
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
                RgbColor c = i switch
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
