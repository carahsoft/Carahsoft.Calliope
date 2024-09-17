using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    public static class AnsiTextHelper
    {
        public static string ColorText(string text, RgbPixel color, RgbPixel? background = null)
        {
            var sb = new StringBuilder();
            sb.Append("\x1b[");
            sb.Append($"38;2;{(int)color.Red};{(int)color.Green};{(int)color.Blue}");
            
            if (background != null)
            {
                sb.Append(';');
                sb.Append($"48;2;{(int)background.Value.Red};{(int)background.Value.Green};{(int)background.Value.Blue}");
            }

            sb.Append("m");
            sb.Append(text);
            sb.Append("\x1b[0m");
            return sb.ToString(); 
        }

        public static string GradientLine(string text, RgbPixel start, RgbPixel end)
        {
            var sb = new StringBuilder();
            var steps = text.Length;

            decimal stepR = (decimal)(end.Red - start.Red) / steps;
            decimal stepG = (decimal)(end.Green - start.Green) / steps;
            decimal stepB = (decimal)(end.Blue - start.Blue) / steps;

            foreach (var step in Enumerable.Range(0, steps))
            {
                var color = new RgbPixel
                {
                    Red = (byte)((step * stepR) + start.Red),
                    Green = (byte)((step * stepG) + start.Green),
                    Blue = (byte)((step * stepB) + start.Blue),
                };
                sb.Append(ColorText(text[step].ToString(), color));
            }

            return sb.ToString(); 
        }
    }
}
