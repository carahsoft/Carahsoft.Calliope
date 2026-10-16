using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class ScrollView : ICalliopeProgram
    {
        public string RenderView { get; set; } = "";
        public int Index { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// Set <see cref="Height"/> to the screen height whenever the window
        /// is resized. Turn off when the view shares the screen with other
        /// content and the parent sets its height.
        /// </summary>
        public bool AutoHeight { get; set; } = true;
        /// <summary>
        /// Quit the program when Ctrl+C is pressed. Turn off when the view
        /// is part of a larger program that handles Ctrl+C itself.
        /// </summary>
        public bool QuitOnCtrlC { get; set; } = true;

        public CalliopeCmd? Init()
        {
            return null;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            var lines = RenderView.Split('\n');
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return QuitOnCtrlC ? CalliopeCmd.Quit : null;
                }
                if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                {
                    if (Height > lines.Length)
                    {
                        Index = 0;
                        return null;
                    }

                    Index = Index + 1;
                    if (Index > lines.Length - Height)
                        Index = lines.Length - Height;

                    return null;
                }
                if (kpm.Key == ConsoleKey.UpArrow || kpm.Key == ConsoleKey.K)
                {
                    Index = Index - 1;
                    if (Index < 0)
                        Index = 0;

                    return null;
                }
                if (kpm.Key == ConsoleKey.End || (kpm.Key == ConsoleKey.G && kpm.Modifiers == ConsoleModifiers.Shift))
                {
                    if (Height > lines.Length)
                    {
                        Index = 0;
                        return null;
                    }

                    Index = lines.Length - Height - 1;
                    return null;
                }
                if (kpm.Key == ConsoleKey.Home || kpm.Key == ConsoleKey.G)
                {
                    Index = 0;
                    return null;
                }
                if (kpm.Key == ConsoleKey.PageDown || (kpm.Key == ConsoleKey.F && kpm.Modifiers == ConsoleModifiers.Control))
                {
                    Index = Index + Height;
                    if (Index > lines.Length - Height - 1)
                        Index = lines.Length - Height - 1;

                    return null;
                }
                if (kpm.Key == ConsoleKey.PageUp || (kpm.Key == ConsoleKey.B && kpm.Modifiers == ConsoleModifiers.Control))
                {
                    Index = Index - Height;
                    if (Index < 0)
                        Index = 0;
                    return null;
                }
            }
            if (msg is WindowSizeChangeMsg wmsg)
            {
                Width = wmsg.ScreenWidth;
                if (!AutoHeight)
                    return null;

                if (wmsg.ScreenHeight > Height && Index > lines.Length - wmsg.ScreenHeight
                    && wmsg.ScreenHeight <= lines.Length)
                {
                    Index = lines.Length - wmsg.ScreenHeight;
                }
                Height = wmsg.ScreenHeight;
                return null;
            }

            return null;
        }

        public string View()
        {
            var lines = RenderView.Replace("\r\n", "\n").Split("\n");

            var startIndex = Index;
            var endIndex = Math.Min(startIndex + Height, lines.Length);

            var sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                sb.AppendLine(lines[i]);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
