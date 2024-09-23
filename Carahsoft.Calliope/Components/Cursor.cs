using Carahsoft.Calliope.AnsiConsole;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class BlinkMsg : CalliopeMsg { }
    public class StartBlinkMsg : CalliopeMsg { }
    public class StopBlinkMsg : CalliopeMsg { }

    public class Cursor : ICalliopeProgram
    {
        /// <summary>
        /// The character under the cursor
        /// </summary>
        public char CursorChar { get; set; } = ' ';
        public bool CursorVisible { get; set; } = true;
        public bool Blinking { get; set; } = true;
        public CalliopeCmd? Init()
        {
            return Blink();
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is BlinkMsg)
            {
                if (Blinking)
                {
                    CursorVisible = !CursorVisible;
                    return Blink();
                }
                else
                {
                    return null;
                }
            }
            if (msg is StartBlinkMsg)
            {
                Blinking = true;
                return Blink();
            }
            if (msg is StopBlinkMsg)
            {
                Blinking = false;
                return null;
            }
            
            return null;
        }

        public string View()
        {
            if (CursorVisible)
            {
                return AnsiTextHelper.ColorText(CursorChar.ToString(),
                    //new() { Red = 45, Green = 156, Blue = 218 },
                    new() { },
                    new() { Red = 255, Green = 255, Blue = 255 });
            }
            else
            {
                return CursorChar.ToString();
            }
        }

        public static CalliopeCmd StartBlinking()
        {
            return CalliopeCmd.Make(() => new StartBlinkMsg());
        }

        public static CalliopeCmd StopBlinking()
        {
            return CalliopeCmd.Make(() => new StopBlinkMsg());
        }

        private CalliopeCmd Blink()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(500);
                return new BlinkMsg();
            });
        }
    }
}
