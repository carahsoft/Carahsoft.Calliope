using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class TextInput : ICalliopeProgram
    {
        private readonly Cursor _cursor = new();
        public string Text { get; set; } = "";
        public bool Enabled { get; set; }
        public int CursorIndex { get; set; }

        public CalliopeCmd? Init()
        {
            var cursorCmd = _cursor.Init();
            return cursorCmd;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                    return CalliopeCmd.Quit;

                var preCursor = Text[..CursorIndex];
                var postCursor = Text[CursorIndex..];

                if (kpm.Key == ConsoleKey.Backspace)
                {
                    Text = string.IsNullOrEmpty(Text) ?  Text : preCursor[..^1] + postCursor;
                    CursorIndex = CursorIndex - 1 < 0 ? 0 : CursorIndex - 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.RightArrow)
                {
                    CursorIndex = CursorIndex + 1 > Text.Length ?
                        Text.Length : CursorIndex + 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.LeftArrow)
                {
                    CursorIndex = CursorIndex - 1 < 0 ? 0 : CursorIndex - 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.Home)
                {
                    CursorIndex = 0;
                    return null;
                }
                if (kpm.Key == ConsoleKey.End)
                {
                    CursorIndex = Text.Length;
                    return null;
                }
                if (char.IsControl(kpm.KeyChar))
                {
                    // do nothing
                    return null;
                }

                var updatedText = preCursor + kpm.KeyChar + postCursor;
                _cursor.CursorChar = kpm.KeyChar;

                Text = updatedText;
                CursorIndex = CursorIndex + 1;
                return null;
            }

            var cursorMsg = _cursor.Update(msg);
            return cursorMsg;
        }

        public string View()
        {
            return InsertCursor();
        }

        public static CalliopeCmd StartBlinking()
        {
            return Cursor.StartBlinking();
        }

        public static CalliopeCmd StopBlinking()
        {
            return Cursor.StopBlinking();
        }

        private string InsertCursor()
        {
            if (string.IsNullOrEmpty(Text))
            {
                _cursor.CursorChar = ' ';
                return _cursor.View();
            }

            var sb = new StringBuilder();
            for (int i = 0; i < Text.Length; i++)
            {
                if (i == CursorIndex)
                {
                    _cursor.CursorChar = Text[i];
                    sb.Append(_cursor.View());
                }
                else
                {
                    sb.Append(Text[i]);
                }
            }

            if (CursorIndex == Text.Length)
            {
                _cursor.CursorChar = ' ';
                sb.Append(_cursor.View());
            }

            return sb.ToString();
        }
    }
}
