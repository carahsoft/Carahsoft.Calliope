using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class SelectListItem
    {
        public string Value { get; set; }
        public string? FilterValue { get; set; }
    }

    public class SelectList : ICalliopeProgram
    {
        public int Index { get; set; }
        public string? Choice { get; set; }
        public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public int MaxHeight { get; set; } = 10;

        public CalliopeCmd? Init()
        {
            return null;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    Choice = null;
                    return CalliopeCmd.Quit;
                }

                if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                {
                    var updatedIndex = Index + 1;
                    if (updatedIndex > Items.Count - 1)
                        updatedIndex = 0;

                    Index = updatedIndex;
                    return null;
                }

                if (kpm.Key == ConsoleKey.UpArrow || kpm.Key == ConsoleKey.K)
                {
                    var updatedIndex = Index - 1;
                    if (updatedIndex < 0)
                        updatedIndex = Items.Count - 1;

                    Index = updatedIndex;
                    return null;
                }

                if (kpm.Key == ConsoleKey.Enter)
                {
                    Choice = Items[Index].Value;
                    return CalliopeCmd.Quit;
                }
            }

            return null;
        }

        public string View()
        {
            var height = Items.Count > MaxHeight ?
                MaxHeight : Items.Count;

            var startIndex = Math.Max(Index - (height / 2), 0);
            var endIndex = startIndex + height;
            if (endIndex > Items.Count)
            {
                endIndex = Items.Count;
                startIndex = Items.Count - height;
            }

            var sb = new StringBuilder();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i == Index)
                {
                    sb.AppendLine(
                        AnsiTextHelper.ColorText(Items[i].Value,
                        new() { Red = 45, Green = 156, Blue = 218 }));
                }
                else
                {
                    sb.AppendLine(Items[i].Value);
                }
            }

            return sb.ToString();
        }
    }
}
