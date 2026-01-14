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
        public required string Value { get; set; }
        public string? FilterValue { get; set; }
    }

    public class SelectList : ICalliopeProgram
    {
        public int Index { get; set; }
        public string? Choice { get; set; }
        public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public int MaxHeight { get; set; } = 10;
        public Style? SelectedStyle { get; set; }
        public Style? ItemStyle { get; set; }

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

                if (kpm.Key == ConsoleKey.PageDown)
                {
                    Index = Math.Min(Index + MaxHeight, Items.Count - 1);
                    return null;
                }

                if (kpm.Key == ConsoleKey.PageUp)
                {
                    Index = Math.Max(Index - MaxHeight, 0);
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
                startIndex = Math.Max(Items.Count - height, 0);
            }

            // Calculate max width from ALL items for consistent dialog width (strip ANSI codes for visible length)
            var maxWidth = Items.Max(item => AnsiTextHelper.StripAnsi(item.Value).Length);
            var lineWidth = maxWidth + 2; // prefix "> " (2 chars) + content

            var sb = new StringBuilder();
            var dimColor = new RgbColor { Red = 100, Green = 100, Blue = 100 };

            // Always show top line for consistent height (arrow with count or empty)
            if (startIndex > 0)
                sb.AppendLine(AnsiTextHelper.ColorText($"  ↑ {startIndex}".PadRight(lineWidth), dimColor));
            else
                sb.AppendLine(new string(' ', lineWidth));

            for (int i = startIndex; i < endIndex; i++)
            {
                var prefix = i == Index ? "> " : "  ";
                var visibleLen = AnsiTextHelper.StripAnsi(Items[i].Value).Length;
                var padding = Math.Max(maxWidth - visibleLen, 0);
                var text = prefix + Items[i].Value + new string(' ', padding);
                if (i == Index)
                {
                    var selectedText = SelectedStyle?.Apply(text) ??
                        AnsiTextHelper.ColorText(text, new() { Red = 45, Green = 156, Blue = 218 });
                    sb.AppendLine(selectedText);
                }
                else
                {
                    var itemText = ItemStyle?.Apply(text) ?? text;
                    sb.AppendLine(itemText);
                }
            }

            // Always show bottom line for consistent height (arrow with count or empty) - no trailing newline
            if (endIndex < Items.Count)
                sb.Append(AnsiTextHelper.ColorText($"  ↓ {Items.Count - endIndex}".PadRight(lineWidth), dimColor));
            else
                sb.Append(new string(' ', lineWidth));

            return sb.ToString();
        }
    }
}
