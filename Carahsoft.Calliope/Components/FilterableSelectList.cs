using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class FilterableSelectList : ICalliopeProgram
    {
        private readonly SelectList _selectList = new();
        private readonly TextInput _searchInput = new();

        public TextInput SearchInput { get { return _searchInput; } }
        public SelectList SelectList { get { return _selectList; } }

        /// <summary>
        /// True if key inputs should be passed to the search TextInput, false
        /// if not.
        /// </summary>
        public bool Searching { get; set; }
        /// <summary>
        /// Represents all possible items, while SelectListState.Items represents
        /// the currently displayed items after the filter is applied.
        /// </summary>
        public List<SelectListItem> AllItems { get; set; } = new();

        public FilterableSelectList()
        {
            AllItems = new();
        }

        public FilterableSelectList(List<SelectListItem> options)
        {
            AllItems = options;
        }

        public CalliopeCmd? Init()
        {
            var searchCmd = _searchInput.Init();
            var selectCmd = _selectList.Init();
            _selectList.Items = AllItems;
            return CalliopeCmd.Combine(searchCmd, selectCmd, TextInput.StopBlinking());
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (!Searching)
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.KeyChar == '/')
                    {
                        Searching = true;
                        return TextInput.StartBlinking();
                    }
                    else
                    {
                        var selectCmd = _selectList.Update(msg);
                        return selectCmd;
                    }
                }
            }
            else
            {
                if (msg is KeyPressMsg kpm)
                {
                    if (kpm.Key == ConsoleKey.Enter)
                    {
                        Searching = false;
                        return TextInput.StopBlinking();
                    }
                    if (kpm.Key == ConsoleKey.Escape)
                    {
                        Searching = false;
                        _searchInput.Enabled = false;
                        _searchInput.CursorIndex = 0;
                        _searchInput.Text = "";
                        return TextInput.StopBlinking();
                    }
                }
            }

            var searchCmd = _searchInput.Update(msg);

            _selectList.Index = 0;
            return searchCmd;
        }

        public string View()
        {
            var filteredItems = AllItems;
            if (!string.IsNullOrEmpty(_searchInput.Text))
            {
                filteredItems = AllItems
                    .Where(x => x.Value.Contains(_searchInput.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var sb = new StringBuilder();

            sb.Append(">");
            if (!Searching)
            {
                if (string.IsNullOrEmpty(_searchInput.Text))
                {
                    sb.AppendLine(AnsiTextHelper.ColorText("[Press / to search]",
                        new() { Blue = 100, Red = 100, Green = 100 }));
                }
                else
                {
                    sb.AppendLine(AnsiTextHelper.ColorText(_searchInput.Text,
                        new() { Blue = 100, Red = 100, Green = 100 }));
                }
            }
            else
            {
                sb.AppendLine(_searchInput.View());
                //sb.AppendLine(InsertCursorIntoSearchString(state));
            }

            _selectList.Items = filteredItems;
            sb.AppendLine(_selectList.View());

            RgbPixel helpColor = new() { Blue = 100, Red = 100, Green = 100 };
            var help = Searching switch
            {
                true => AnsiTextHelper.ColorText("[Esc]-clear [Enter]-search", helpColor),
                false => AnsiTextHelper.ColorText("[j/↓]-down [k/↑]-up [Enter]-select", helpColor)
            };
            sb.AppendLine(help);

            return sb.ToString();
        }
    }
}
