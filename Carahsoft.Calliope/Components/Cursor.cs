using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Messages;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record CursorState
    {
        /// <summary>
        /// The character under the cursor
        /// </summary>
        public char CursorChar { get; init; }
        public bool CursorVisible { get; init; }
        public bool Blinking { get; init; }
    }

    public class BlinkMsg : CalliopeMsg { }
    public class StartBlinkMsg : CalliopeMsg { }
    public class StopBlinkMsg : CalliopeMsg { }

    public class Cursor : ICalliopeProgram<CursorState>
    {
        public (CursorState, CalliopeCmd?) Init()
        {
            return (new() { CursorVisible = true, Blinking = true }, Blink());
        }

        public (CursorState, CalliopeCmd?) Update(CursorState state, CalliopeMsg msg)
        {
            if (msg is BlinkMsg)
            {
                if (state.Blinking)
                    return (state with { CursorVisible = !state.CursorVisible }, Blink());
                else
                    return (state, null);
            }
            if (msg is StartBlinkMsg)
            {
                return (state with { Blinking = true }, Blink());
            }
            if (msg is StopBlinkMsg)
            {
                return (state with { Blinking = false }, null);
            }
            
            return (state, null);
        }

        public string View(CursorState state)
        {
            if (state.CursorVisible)
            {
                return AnsiTextHelper.ColorText(state.CursorChar.ToString(),
                    //new() { Red = 45, Green = 156, Blue = 218 },
                    new() { },
                    new() { Red = 255, Green = 255, Blue = 255 });
            }
            else
            {
                return state.CursorChar.ToString();
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
