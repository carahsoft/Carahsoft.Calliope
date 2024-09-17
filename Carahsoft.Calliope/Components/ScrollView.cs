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
            var lines = state.View.Split('\n');
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return (state, CalliopeCmd.Quit);
                }
                if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                {
                    if (state.Height > lines.Length)
                        return (state with { Index = 0 }, null);

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
                if (kpm.Key == ConsoleKey.End || (kpm.Key == ConsoleKey.G && kpm.Modifiers == ConsoleModifiers.Shift))
                {
                    if (state.Height > lines.Length)
                        return (state with { Index = 0 }, null);

                    var updatedIndex = lines.Length - state.Height - 1;
                    return (state with { Index = updatedIndex }, null);
                }
                if (kpm.Key == ConsoleKey.Home || kpm.Key == ConsoleKey.G)
                {
                    return (state with { Index = 0 }, null);
                }
                if (kpm.Key == ConsoleKey.PageDown || (kpm.Key == ConsoleKey.F && kpm.Modifiers == ConsoleModifiers.Control))
                {
                    var updatedIndex = state.Index + state.Height;
                    if (updatedIndex > lines.Length - state.Height - 1)
                        updatedIndex = lines.Length - state.Height - 1;
                    return (state with { Index = updatedIndex }, null);
                }
                if (kpm.Key == ConsoleKey.PageUp || (kpm.Key == ConsoleKey.B && kpm.Modifiers == ConsoleModifiers.Control))
                {
                    var updatedIndex = state.Index - state.Height;
                    if (updatedIndex < 0)
                        updatedIndex = 0;
                    return (state with { Index = updatedIndex }, null);
                }
            }
            if (msg is WindowSizeChangeMsg wmsg)
            {
                var updatedIndex = state.Index;
                if (wmsg.ScreenHeight > state.Height && state.Index > lines.Length - wmsg.ScreenHeight && wmsg.ScreenHeight <= lines.Length)
                {
                    updatedIndex = lines.Length - wmsg.ScreenHeight;
                }
                return (state with { Index = updatedIndex, Height = wmsg.ScreenHeight, Width = wmsg.ScreenWidth }, null);
            }

            return (state, null);
        }

        public string View(ScrollViewState state)
        {
            var lines = state.View.Replace("\r\n", "\n").Split("\n");

            var startIndex = state.Index;
            var endIndex = Math.Min(startIndex + state.Height, lines.Length);

            var sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                sb.AppendLine(lines[i]);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
