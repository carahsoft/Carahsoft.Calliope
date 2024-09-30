using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
using Carahsoft.Calliope.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEffectExample
{
    public class TextEffectPicker : ICalliopeProgram
    {
        private CalliopeAnimation _rainbow;
        private CalliopeAnimation _twinkle;
        private CalliopeAnimation _blizzard;

        private int _selectedIndex;

        public TextEffectPicker()
        {
            var rainbowOptions = new CalliopePrintOptions
            {
                Font = "Trebuchet MS",
                Width = 100,
                Height = 20,
                DrawChar = '\u2588'
            };
            var twinkleOptions = new CalliopePrintOptions
            {
                Font = "Trebuchet MS",
                Width = 100,
                Height = 20,
            };
            var blizzardOptions = new CalliopePrintOptions
            {
                Font = "Trebuchet MS",
                Width = 100,
                Height = 20,
            };
            _rainbow = new RainbowAnimation("carahsoft", rainbowOptions);
            _twinkle = new TwinkleAnimation("carahsoft", twinkleOptions);
            _blizzard = new BlizzardAnimation("carahsoft", blizzardOptions);
        }

        public CalliopeCmd? Init()
        {
            return CalliopeCmd.Combine(
                _rainbow.Init(),
                _twinkle.Init(),
                _blizzard.Init()
            );
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return CalliopeCmd.Quit;
                }
                if (kpm.Key == ConsoleKey.D1)
                {
                    _selectedIndex = 0;
                    return null;
                }
                if (kpm.Key == ConsoleKey.D2)
                {
                    _selectedIndex = 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.D3)
                {
                    _selectedIndex = 2;
                    return null;
                }
            }

            return CalliopeCmd.Combine(
                _rainbow.Update(msg),
                _twinkle.Update(msg),
                _blizzard.Update(msg)
            );
        }

        public string View()
        {
            var sb = new StringBuilder();
            var rendered = _selectedIndex switch
            {
                0 => _rainbow.View(),
                1 => _twinkle.View(),
                _ => _blizzard.View()
            };
            sb.AppendLine(rendered);
            sb.AppendLine(
                Calliope.ColorText("Press 1-3 to cycle between the different effects! ctrl+c to quit.",
                RgbColors.Grey));
            sb.Append(GetDisplayFor("Rainbow", 1));
            sb.Append(GetDisplayFor("Twinkle", 2));
            sb.Append(GetDisplayFor("Blizzard", 3));
            return sb.ToString();
        }

        private string GetDisplayFor(string name, int key)
        {
            if (_selectedIndex == key - 1)
            {
                return Calliope.ColorText($"[ {key} - {name} ]   ", RgbColors.CarahBlue);
            }
            else
            {
                return Calliope.ColorText($"[ {key} - {name} ]   ", RgbColors.Grey);
            }
        }
    }
}
