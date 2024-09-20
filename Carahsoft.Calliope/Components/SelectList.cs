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

    public record SelectListState
    {
        public int Index { get; init; }
        public string? Choice { get; init; }
        public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public int MaxHeight { get; set; } = 10;
    }

    public class SelectList : ICalliopeProgram<SelectListState>
    {
        public (SelectListState, CalliopeCmd?) Init()
        {
            return (new(), null);
        }

        public (SelectListState, CalliopeCmd?) Update(SelectListState state, CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return (state with { Choice = null }, CalliopeCmd.Quit);
                }

                if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                {
                    var updatedIndex = state.Index + 1;
                    if (updatedIndex > state.Items.Count - 1)
                        updatedIndex = 0;

                    return (state with { Index = updatedIndex }, null);
                }

                if (kpm.Key == ConsoleKey.UpArrow || kpm.Key == ConsoleKey.K)
                {
                    var updatedIndex = state.Index - 1;
                    if (updatedIndex < 0)
                        updatedIndex = state.Items.Count - 1;

                    return (state with { Index = updatedIndex }, null);
                }

                if (kpm.Key == ConsoleKey.Enter)
                {
                    return (state with { Choice = state.Items[state.Index].Value }, CalliopeCmd.Quit);
                }
            }

            return (state, null);
        }

        public string View(SelectListState state)
        {
            var height = state.Items.Count > state.MaxHeight ?
                state.MaxHeight : state.Items.Count;

            var startIndex = Math.Max(state.Index - (height / 2), 0);
            var endIndex = startIndex + height;
            if (endIndex > state.Items.Count)
            {
                endIndex = state.Items.Count;
                startIndex = state.Items.Count - height;
            }

            var sb = new StringBuilder();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i == state.Index)
                {
                    sb.AppendLine(
                        AnsiTextHelper.ColorText(state.Items[i].Value,
                        new() { Red = 45, Green = 156, Blue = 218 }));
                }
                else
                {
                    sb.AppendLine(state.Items[i].Value);
                }
            }

            return sb.ToString();
        }
    }
}
