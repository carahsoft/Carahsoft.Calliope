using Carahsoft.Calliope.AnsiConsole;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class Cursor : ICalliopeProgram
    {
        private static int _lastId;

        // Incremented whenever a new blink loop starts, so blinks from an
        // older loop are ignored rather than running alongside it
        private int _tag;

        /// <summary>
        /// Identifies this cursor's messages, so that other cursors in the
        /// program ignore them
        /// </summary>
        public int Id { get; } = Interlocked.Increment(ref _lastId);
        /// <summary>
        /// The character under the cursor
        /// </summary>
        public char CursorChar { get; set; } = ' ';
        public bool CursorVisible { get; set; } = true;
        public bool Blinking { get; set; } = true;
        public CalliopeCmd? Init()
        {
            _tag++;
            return Blink();
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is BlinkMsg blink)
            {
                if (Blinking && blink.Id == Id && blink.Tag == _tag)
                {
                    CursorVisible = !CursorVisible;
                    return Blink();
                }
                else
                {
                    return null;
                }
            }
            if (msg is StartBlinkMsg start && (start.Id == null || start.Id == Id))
            {
                Blinking = true;
                CursorVisible = true;
                _tag++;
                return Blink();
            }
            if (msg is StopBlinkMsg stop && (stop.Id == null || stop.Id == Id))
            {
                Blinking = false;
                _tag++;
                return null;
            }

            return null;
        }

        public string View()
        {
            if (CursorVisible)
            {
                return AnsiTextHelper.ColorText(CursorChar.ToString(),
                    RgbColors.Black,
                    RgbColors.White);
            }
            else
            {
                return CursorChar.ToString();
            }
        }

        /// <summary>
        /// Starts every cursor in the program blinking. Use
        /// <see cref="StartBlink"/> to start only this cursor.
        /// </summary>
        public static CalliopeCmd StartBlinking()
        {
            return CalliopeCmd.Make(() => new StartBlinkMsg(null));
        }

        /// <summary>
        /// Stops every cursor in the program blinking. Use
        /// <see cref="StopBlink"/> to stop only this cursor.
        /// </summary>
        public static CalliopeCmd StopBlinking()
        {
            return CalliopeCmd.Make(() => new StopBlinkMsg(null));
        }

        /// <summary>
        /// Starts this cursor blinking
        /// </summary>
        public CalliopeCmd StartBlink()
        {
            var id = Id;
            return CalliopeCmd.Make(() => new StartBlinkMsg(id));
        }

        /// <summary>
        /// Stops this cursor blinking
        /// </summary>
        public CalliopeCmd StopBlink()
        {
            var id = Id;
            return CalliopeCmd.Make(() => new StopBlinkMsg(id));
        }

        private CalliopeCmd Blink()
        {
            var id = Id;
            var tag = _tag;
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(500);
                return new BlinkMsg(id, tag);
            });
        }
        private class BlinkMsg(int id, int tag) : CalliopeMsg
        {
            public int Id { get; } = id;
            public int Tag { get; } = tag;
        }
        // A null Id means every cursor
        private class StartBlinkMsg(int? id) : CalliopeMsg
        {
            public int? Id { get; } = id;
        }
        private class StopBlinkMsg(int? id) : CalliopeMsg
        {
            public int? Id { get; } = id;
        }

    }
}
