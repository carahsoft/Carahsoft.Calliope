using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record FilterableSelectListState
    {
        /// <summary>
        /// True if key inputs should be passed to the search TextInput, false
        /// if not.
        /// </summary>
        public bool Searching { get; init; }
        /// <summary>
        /// Represents all possible items, while SelectListState.Items represents
        /// the currently displayed items after the filter is applied.
        /// </summary>
        public List<SelectListItem> AllItems { get; init; } = new();
        /// <summary>
        /// State for the <see cref="SelectList"/> holding the currently
        /// visible options.
        /// </summary>
        public SelectListState SelectListState { get; init; } = new();
        /// <summary>
        /// State for the <see cref="TextInput"/> holding the current
        /// search value.
        /// </summary>
        public TextInputState SearchState { get; init; } = new();
    }

    public class FilterableSelectList : ICalliopeProgram<FilterableSelectListState>
    {
        private readonly SelectList _selectList = new();
        private readonly TextInput _searchInput = new();
        private readonly List<SelectListItem> _allOptions;

        public FilterableSelectList()
        {
            _allOptions = new();
        }

        public FilterableSelectList(List<SelectListItem> options)
        {
            _allOptions = options;
        }

        public (FilterableSelectListState, CalliopeCmd?) Init()
        {
            var (searchState, searchCmd) = _searchInput.Init();
            // ignore initial state, we want to render our own select list
            var (_, selectCmd) = _selectList.Init();
            return (new()
            {
                AllItems = _allOptions,
                SelectListState = new() { Items = _allOptions },
                SearchState = searchState
            },
            CalliopeCmd.Combine(searchCmd, selectCmd, TextInput.StopBlinking()));
        }

        public (FilterableSelectListState, CalliopeCmd?) Update(FilterableSelectListState state, CalliopeMsg msg)
        {
            if (!state.Searching)
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.KeyChar == '/')
                    {
                        return (state with { Searching = true }, TextInput.StartBlinking());
                    }
                    else
                    {
                        var (selectState, selectCmd) = _selectList.Update(state.SelectListState, msg);
                        return (state with { SelectListState = selectState }, selectCmd);
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

            return (state with
            {
                SearchState = updatedSearch,
                SelectListState = state.SelectListState with { Index = 0 }
            }, searchCmd);
        }

        public string View(FilterableSelectListState state)
        {
            var filteredItems = state.AllItems;
            if (!string.IsNullOrEmpty(state.SearchState.Text))
            {
                filteredItems = state.AllItems
                    .Where(x => x.Value.Contains(state.SearchState.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
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

            sb.Append(_selectList.View(state.SelectListState with { Items = filteredItems }));


            sb.AppendLine();
            RgbPixel helpColor = new() { Blue = 100, Red = 100, Green = 100 };
            var help = state.Searching switch
            {
                true => AnsiTextHelper.ColorText("[Esc]-clear [Enter]-search", helpColor),
                false => AnsiTextHelper.ColorText("[j/↓]-down [k/↑]-up [Enter]-select", helpColor)
            };
            sb.AppendLine(help);

            return sb.ToString();
        }
    }
}
