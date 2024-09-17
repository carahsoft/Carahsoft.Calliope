using Carahsoft.Calliope.AnsiConsole;
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
        public TextInputState SearchState { get; init; }
        public string? Choice { get; init; }
    }

    public class SelectList : ICalliopeProgram<SelectListState>
    {
        private readonly SelectListProps _props;
        //private readonly Cursor _cursor;
        private readonly TextInput _searchInput;

        public SelectList(SelectListProps props)
        {
            _props = props;
            //_cursor = new Cursor();
            _searchInput = new TextInput();
        }

        public (SelectListState, CalliopeCmd?) Init()
        {
            var (searchState, searchCmd) = _searchInput.Init();
            return (new() { Index = 0, SearchState = searchState },
                CalliopeCmd.Combine(searchCmd, TextInput.StopBlinking()));
        }

        public (SelectListState, CalliopeCmd?) Update(SelectListState state, CalliopeMsg msg)
        {
            if (!state.Searching)
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
                        return (state with { Searching = true }, TextInput.StartBlinking());
                    }
                }
            }
            else
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.Key == ConsoleKey.Enter)
                    {
                        return (state with { Searching = false }, TextInput.StopBlinking());
                    }
                    if (kpm.Key == ConsoleKey.Escape)
                    {
                        return (state with
                        {
                            Searching = false,
                            SearchState = state.SearchState with
                            {
                                Enabled = false,
                                CursorIndex = 0,
                                Text = ""
                            }
                        }, TextInput.StopBlinking());
                    }

                }
            }

            var (updatedSearch, searchCmd) = _searchInput.Update(state.SearchState, msg);

            int index = state.Index;
            if (updatedSearch.Text != state.SearchState.Text)
                index = 0;

            return (state with
            {
                SearchState = updatedSearch,
                Index = index
            }, searchCmd);
        }

        public string View(SelectListState state)
        {
            var filteredItems = _props.Items;
            if (!string.IsNullOrEmpty(state.SearchState.Text))
            {
                filteredItems = _props.Items
                    .Where(x => x.Value.Contains(state.SearchState.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var height = filteredItems.Count > _props.MaxHeight ?
                _props.MaxHeight : filteredItems.Count;

            var startIndex = Math.Max(state.Index - (height / 2), 0);
            var endIndex = startIndex + height;
            if (endIndex > filteredItems.Count)
            {
                endIndex = filteredItems.Count;
                startIndex = filteredItems.Count - height;
            }

            var sb = new StringBuilder();

            sb.Append(">");
            if (!state.Searching)
            {
                if (string.IsNullOrEmpty(state.SearchState.Text))
                {
                    sb.AppendLine(AnsiTextHelper.ColorText("[Press / to search]",
                        new() { Blue = 100, Red = 100, Green = 100 }));
                }
                else
                {
                    sb.AppendLine(AnsiTextHelper.ColorText(state.SearchState.Text,
                        new() { Blue = 100, Red = 100, Green = 100 }));
                }
            }
            else
            {
                sb.AppendLine(_searchInput.View(state.SearchState));
                //sb.AppendLine(InsertCursorIntoSearchString(state));
            }


            for (int i = startIndex; i < endIndex; i++)
            {
                if (i == state.Index)
                {
                    sb.AppendLine(
                        AnsiTextHelper.ColorText(filteredItems[i].Value,
                        new() { Red = 45, Green = 156, Blue = 218 }));
                }
                else
                {
                    sb.AppendLine(filteredItems[i].Value);
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
