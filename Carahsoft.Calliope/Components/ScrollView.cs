using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record ScrollViewState
    {
        public string View { get; init; } = "";
        public int Index { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
    }

    public class ScrollView : ICalliopeProgram<ScrollViewState>
    {
        public (ScrollViewState, CalliopeCmd?) Init()
        {
            return (new(), null);
        }

        public (ScrollViewState, CalliopeCmd?) Update(ScrollViewState state, CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                var lines = state.View.Split('\n');
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return (state, CalliopeCmd.Quit);
                }
                if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                {
                    var updatedIndex = state.Index + 1;
                    if (updatedIndex > lines.Length - state.Height)
                        updatedIndex = lines.Length - state.Height;

                    return (state with { Index = updatedIndex }, null);
                }
                if (kpm.Key == ConsoleKey.UpArrow || kpm.Key == ConsoleKey.K)
                {
                    var updatedIndex = state.Index - 1;
                    if (updatedIndex < 0)
                        updatedIndex = 0;

                    return (state with { Index = updatedIndex }, null);
                }
            }
            if (msg is WindowSizeChangeMsg wmsg)
            {
                return (state with { Height = wmsg.ScreenHeight, Width = wmsg.ScreenWidth }, null);
            }

            return (state, null);
        }

        public string View(ScrollViewState state)
        {
            var lines = state.View.Split("\r\n");

            var startIndex = state.Index;
            var endIndex = startIndex + state.Height;

            var sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                sb.AppendLine(lines[i]);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
