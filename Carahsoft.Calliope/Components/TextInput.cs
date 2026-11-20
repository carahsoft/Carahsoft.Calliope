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

        private string _text = string.Empty;
        public Style? Style { get; set; }
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? string.Empty;

                // Clamp cursor to valid range
                if (CursorIndex > _text.Length)
                    CursorIndex = _text.Length;
                if (CursorIndex < 0)
                    CursorIndex = 0;
            }
        }

        /// <summary>
        /// When false, key presses are ignored (except Ctrl+C, see
        /// <see cref="QuitOnCtrlC"/>) and the text is drawn without a cursor.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Quit the program when Ctrl+C is pressed. Turn off when the input
        /// is part of a larger program that handles Ctrl+C itself.
        /// </summary>
        public bool QuitOnCtrlC { get; set; } = true;
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
                    return QuitOnCtrlC ? CalliopeCmd.Quit : null;

                if (!Enabled)
                    return null;

                // Ensure cursor is in valid range before slicing
                var safeIndex = Math.Clamp(CursorIndex, 0, Text.Length);
                var preCursor = Text[..safeIndex];
                var postCursor = Text[safeIndex..];

                if (kpm.Key == ConsoleKey.Backspace)
                {
                    if (safeIndex <= 0 || string.IsNullOrEmpty(Text))
                        return null; // Nothing to delete
                    Text = preCursor[..^1] + postCursor;
                    CursorIndex = safeIndex - 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.Delete)
                {
                    if (safeIndex >= Text.Length || string.IsNullOrEmpty(Text))
                        return null; // Nothing to delete
                    Text = preCursor + postCursor[1..];
                    // CursorIndex stays the same
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
                CursorIndex = safeIndex + 1;
                return null;
            }

            var cursorMsg = _cursor.Update(msg);
            return cursorMsg;
        }

        public string View()
        {
            var text = Enabled ? InsertCursor() : Text;
            return Style?.Apply(text) ?? text;
        }

        /// <summary>
        /// Starts every text input's cursor in the program blinking. Use
        /// <see cref="StartBlink"/> to start only this input's cursor.
        /// </summary>
        public static CalliopeCmd StartBlinking()
        {
            return Cursor.StartBlinking();
        }

        /// <summary>
        /// Stops every text input's cursor in the program blinking. Use
        /// <see cref="StopBlink"/> to stop only this input's cursor.
        /// </summary>
        public static CalliopeCmd StopBlinking()
        {
            return Cursor.StopBlinking();
        }

        /// <summary>
        /// Starts this input's cursor blinking
        /// </summary>
        public CalliopeCmd StartBlink() => _cursor.StartBlink();

        /// <summary>
        /// Stops this input's cursor blinking
        /// </summary>
        public CalliopeCmd StopBlink() => _cursor.StopBlink();

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
