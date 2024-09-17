using Carahsoft.Calliope;
using Carahsoft.Calliope.AnsiConsole;

namespace TextEffectExample
{
    internal class RainbowChar
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
}
