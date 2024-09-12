using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record TextInputState
    {
        public string Text { get; init; } = "";
        public bool Enabled { get; init; }
        public int CursorIndex { get; init; }
        public CursorState CursorState { get; init; }
    }

    public class TextInput : ICalliopeProgram<TextInputState>
    {
        private readonly Cursor _cursor = new();

        public (TextInputState, CalliopeCmd?) Init()
        {
            var (cursorState, cursorCmd) = _cursor.Init();
            return (new() { CursorState = cursorState }, cursorCmd);
        }

        public (TextInputState, CalliopeCmd?) Update(TextInputState state, CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                    return (state, CalliopeCmd.Quit);

                var preCursor = state.Text[..state.CursorIndex];
                var postCursor = state.Text[state.CursorIndex..];

                if (kpm.Key == ConsoleKey.Backspace)
                {
                    return (state with
                    {
                        Text =
                            string.IsNullOrEmpty(state.Text) ?
                            state.Text :
                            preCursor[..^1] + postCursor,
                        CursorIndex = state.CursorIndex - 1 < 0 ? 0 : state.CursorIndex - 1
                    }, null);
                }
                if (kpm.Key == ConsoleKey.RightArrow)
                {
                    return (state with
                    {
                        CursorIndex = state.CursorIndex + 1 > state.Text.Length ?
                            state.Text.Length : state.CursorIndex + 1
                    }, null);
                }
                if (kpm.Key == ConsoleKey.LeftArrow)
                {
                    return (state with
                    {
                        CursorIndex = state.CursorIndex - 1 < 0 ?
                            0 : state.CursorIndex - 1
                    }, null);
                }
                if (kpm.Key == ConsoleKey.Home)
                {
                    return (state with
                    {
                        CursorIndex = 0
                    }, null);
                }
                if (kpm.Key == ConsoleKey.End)
                {
                    return (state with
                    {
                        CursorIndex = state.Text.Length
                    }, null);
                }

                var updatedText = preCursor + kpm.KeyChar + postCursor;

                return (state with
                {
                    Text = updatedText,
                    CursorIndex = state.CursorIndex + 1
                }, null);
            }

            var (cursorState, cursorMsg) = _cursor.Update(state.CursorState, msg);

            return (state with { CursorState = cursorState }, cursorMsg);
        }

        public string View(TextInputState state)
        {
            return InsertCursor(state);
        }

        public static CalliopeCmd StartBlinking()
        {
            return Cursor.StartBlinking();
        }

        public static CalliopeCmd StopBlinking()
        {
            return Cursor.StopBlinking();
        }

        private string InsertCursor(TextInputState state)
        {
            if (string.IsNullOrEmpty(state.Text))
                return _cursor.View(state.CursorState with { CursorChar = ' ' });

            var sb = new StringBuilder();
            for (int i = 0; i < state.Text.Length; i++)
            {
                if (i == state.CursorIndex)
                    sb.Append(_cursor.View(
                        state.CursorState with { CursorChar = state.Text[i] }
                    ));
                else
                    sb.Append(state.Text[i]);
            }

            if (state.CursorIndex == state.Text.Length)
                sb.Append(_cursor.View(state.CursorState with { CursorChar = ' ' }));

            return sb.ToString();
        }
    }
}
