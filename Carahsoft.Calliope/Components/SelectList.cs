using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class SelectListProps
    {
        public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public int MaxHeight { get; set; } = 10;
    }

    public class SelectListItem
    {
        public string Value { get; set; }
        public string? FilterValue { get; set; }
    }

    public record SelectListState
    {
        public int Index { get; init; }
        public bool Searching { get; init; }
        public string? Choice { get; init; }
    }

    public class SelectList : ICalliopeProgram<SelectListState>
    {
        private readonly SelectListProps _props;

        public SelectList(SelectListProps props)
        {
            _props = props;
        }

        public (SelectListState, CalliopeCmd?) Init()
        {
            return (new() { Index = 0 }, null);
        }

        public (SelectListState, CalliopeCmd?) Update(SelectListState state, CalliopeMsg msg)
        {
            if (!state.Searching)
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.Key == ConsoleKey.DownArrow || kpm.Key == ConsoleKey.J)
                    {
                        var updatedIndex = state.Index + 1;
                        if (updatedIndex > _props.Items.Count - 1)
                            updatedIndex = 0;

                        return (state with { Index = updatedIndex }, null);
                    }

                    if (kpm.Key == ConsoleKey.UpArrow || kpm.Key == ConsoleKey.K)
                    {
                        var updatedIndex = state.Index - 1;
                        if (updatedIndex < 0)
                            updatedIndex = _props.Items.Count - 1;

                        return (state with { Index = updatedIndex }, null);
                    }

                    if (kpm.Key == ConsoleKey.Enter)
                    {
                        return (state with { Choice = _props.Items[state.Index].Value }, CalliopeCmd.Quit);
                    }

                    if (kpm.KeyChar == '/')
                    {
                        return (state with { Searching = true }, null);
                    }
                }
            }
            else
            {
                // Search mode
            }

            return (state, null);
        }

        public string View(SelectListState state)
        {
            var height = _props.Items.Count > _props.MaxHeight ?
                _props.MaxHeight : _props.Items.Count;

            var scrollable = _props.Items.Count > _props.MaxHeight;

            var startIndex = Math.Max(state.Index - (height / 2), 0);
            var endIndex = startIndex + height;
            if (endIndex > _props.Items.Count)
            {
                endIndex = _props.Items.Count;
                startIndex = _props.Items.Count - height;
            }

            var sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                if (i == state.Index)
                {
                    sb.AppendLine(
                        AnsiTextHelper.ColorText(_props.Items[i].Value,
                        new() { Red = 45, Green = 156, Blue = 218 }));
                }
                else
                {
                    sb.AppendLine(_props.Items[i].Value);
                }
            }

            sb.AppendLine();
            sb.AppendLine(AnsiTextHelper.ColorText(
                "[j/↓]-down [k/↑]-up [Enter]-select",
                new() { Blue = 100, Red = 100, Green = 100 }));

            return sb.ToString();
        }
    }
}
