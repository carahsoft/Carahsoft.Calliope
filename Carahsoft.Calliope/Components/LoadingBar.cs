using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class LoadingBarOptions
    {
        public RgbColor? Color { get; set; }
        public RgbColor? GradientStart { get; set; }
        public RgbColor? GradientEnd { get; set; }
    }

    public class LoadingBar : ICalliopeProgram
    {
        private const char UNLOADED = '\u2591';
        private const char DARK = '\u2593';
        private const char FULL = '\u2588';

        private readonly LoadingBarOptions _options;

        public int Percent { get; set; }

        public LoadingBar(LoadingBarOptions options)
        {
            if (options.Color == null)
                options.Color = new RgbColor { Blue = 255, Green = 255, Red = 255 };
            _options = options;
        }

        public CalliopeCmd? Init()
        {
            Percent = 0;
            return null;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            return null;
        }

        public string View()
        {
            var width = 50;
            var pct = (decimal)Percent / 100;
            var loadedCount = (int)Math.Ceiling(pct * width);
            var unloadedCount = width - loadedCount;
            var loadedString = new string(FULL, loadedCount);
            var unloadedString = new string(UNLOADED, unloadedCount);

            if (_options.GradientStart != null && _options.GradientEnd != null)
            {
                return AnsiTextHelper.ColorTextGradient(
                    loadedString + unloadedString,
                    _options.GradientStart.Value,
                    _options.GradientEnd.Value)
                    + $" {Percent}%";
            }

            return AnsiTextHelper.ColorText(
                loadedString + unloadedString + $" {Percent}%",
                _options.Color!.Value);
        }
    }
}
