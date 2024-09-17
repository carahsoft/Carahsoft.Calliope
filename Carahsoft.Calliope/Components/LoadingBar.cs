using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record LoadingBarState
    {
        public required int Percent { get; init; }
    }

    public class SetPercentMsg : CalliopeMsg
    {
        public int Percent { get; set; }
    }

    public class LoadingBarOptions
    {
        public RgbPixel? Color { get; set; }
        public RgbPixel? GradientStart { get; set; }
        public RgbPixel? GradientEnd { get; set; }
    }

    public class LoadingBar : ICalliopeProgram<LoadingBarState>
    {
        private const char UNLOADED = '\u2591';
        private const char DARK = '\u2593';
        private const char FULL = '\u2588';

        private readonly LoadingBarOptions _options;

        public LoadingBar(LoadingBarOptions options)
        {
            if (options.Color == null)
                options.Color = new RgbPixel { Blue = 255, Green = 255, Red = 255 };
            _options = options;
        }

        public (LoadingBarState, CalliopeCmd?) Init()
        {
            return Calliope.Return(new LoadingBarState { Percent = 0 });
        }

        public (LoadingBarState, CalliopeCmd?) Update(LoadingBarState state, CalliopeMsg msg)
        {
            if (msg is SetPercentMsg spm)
            {
                return Calliope.Return(state with { Percent = spm.Percent });
            }
            // Do nothing
            return Calliope.Return(state);
        }

        public string View(LoadingBarState state)
        {
            var width = 50;
            var pct = (decimal)state.Percent / 100;
            var loadedCount = (int)Math.Ceiling(pct * width);
            var unloadedCount = width - loadedCount;
            var loadedString = new string(FULL, loadedCount);
            var unloadedString = new string(UNLOADED, unloadedCount);

            if (_options.GradientStart != null && _options.GradientEnd != null)
            {
                return AnsiTextHelper.GradientLine(
                    loadedString + unloadedString,
                    _options.GradientStart.Value,
                    _options.GradientEnd.Value)
                    + $" {state.Percent}%";
            }

            return AnsiTextHelper.ColorText(
                loadedString + unloadedString + $" {state.Percent}%",
                _options.Color!.Value);
        }
    }
}
