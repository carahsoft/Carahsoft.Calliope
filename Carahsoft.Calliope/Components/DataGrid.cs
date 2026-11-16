using Carahsoft.Calliope.Table;
using Carahsoft.Calliope.Text;

namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// A column of a <see cref="DataGrid"/>
    /// </summary>
    /// <param name="Header">The column's title</param>
    /// <param name="Width">Minimum width. Columns with <see cref="Grow"/> share any leftover space.</param>
    /// <param name="Cell">The text of the column's cell for a row, which may be styled</param>
    public sealed record DataGridColumn<TRow>(string Header, int Width, Func<TRow, string> Cell)
    {
        /// <summary>
        /// Take a share of the width left over after every column's minimum
        /// </summary>
        public bool Grow { get; init; }
        public ColumnAlignment Align { get; init; } = ColumnAlignment.Left;
        /// <summary>
        /// Whether s sorts by this column
        /// </summary>
        public bool Sortable { get; init; } = true;
        /// <summary>
        /// What to sort by. Defaults to the cell's text without styles.
        /// </summary>
        public Func<TRow, IComparable>? SortKey { get; init; }

        internal IComparable SortValue(TRow row) => SortKey?.Invoke(row) ?? Ansi.Strip(Cell(row));
    }

    /// <summary>
    /// Text that replaces a row's cells from a column onwards, e.g. an error message
    /// </summary>
    public sealed record DataGridSpan(int FromColumn, string Text);

    /// <summary>
    /// A sort by a column, see <see cref="DataGridState{TRow}.Sort"/>
    /// </summary>
    public readonly record struct DataGridSort(int Column, bool Descending);

    /// <summary>
    /// The rows, columns, filter, sort and cursor of a <see cref="DataGrid"/>
    /// </summary>
    public sealed record DataGridState<TRow>(IReadOnlyList<DataGridColumn<TRow>> Columns)
    {
        /// <summary>
        /// Rows in their default order
        /// </summary>
        public IReadOnlyList<TRow> Rows { get; init; } = [];
        /// <summary>
        /// Identifies a row, so the cursor can stay on it when the filter or
        /// sort changes. Defaults to the first cell's text.
        /// </summary>
        public Func<TRow, string>? RowKey { get; init; }
        /// <summary>
        /// The text the filter searches. Defaults to every cell's text.
        /// </summary>
        public Func<TRow, string>? FilterText { get; init; }
        public string Filter { get; init; } = "";
        /// <summary>
        /// The column sorted by, or null for the default order
        /// </summary>
        public DataGridSort? Sort { get; init; }
        /// <summary>
        /// The selected column, moved with ←/→ and sorted with s
        /// </summary>
        public int Column { get; init; }
        /// <summary>
        /// The selected row's index in <see cref="DataGrid.VisibleRows"/>
        /// </summary>
        public int Cursor { get; init; }
        /// <summary>
        /// The first row shown
        /// </summary>
        public int Offset { get; init; }
        /// <summary>
        /// Rows that fit on screen, used to scroll. Set it with
        /// <see cref="DataGrid.SetViewHeight"/> when the screen is resized.
        /// </summary>
        public int ViewHeight { get; init; } = 10;
        /// <summary>
        /// A style for a whole row, e.g. red for errors
        /// </summary>
        public Func<TRow, Style?>? RowStyle { get; init; }
        /// <summary>
        /// Replaces a row's cells from a column onwards with one piece of text
        /// </summary>
        public Func<TRow, DataGridSpan?>? SpanFrom { get; init; }
        /// <summary>
        /// Shown instead of rows when there are none
        /// </summary>
        public string EmptyText { get; init; } = "No data";
        /// <summary>
        /// Shows <see cref="LoadingText"/> instead of <see cref="EmptyText"/>,
        /// e.g. until the first load finishes
        /// </summary>
        public bool Loading { get; init; }
        public string LoadingText { get; init; } = "Loading…";
    }

    /// <summary>
    /// A selectable, sortable, filterable table. Keys: ↑/↓ (or k/j),
    /// PgUp/PgDn (or Ctrl+B/Ctrl+F) and Home/End (or g/G) move the cursor,
    /// ←/→ select a column and s sorts by it (again to reverse).
    /// </summary>
    public static class DataGrid
    {
        public static DataGridState<TRow> Create<TRow>(params IEnumerable<DataGridColumn<TRow>> columns) =>
            new(columns.ToList());

        /// <summary>
        /// The rows after filtering and sorting, in display order
        /// </summary>
        public static IReadOnlyList<TRow> VisibleRows<TRow>(DataGridState<TRow> state)
        {
            IEnumerable<TRow> rows = state.Rows;
            var filter = state.Filter.Trim();
            if (filter != "")
                rows = rows.Where(r => FilterText(state, r).Contains(filter, StringComparison.OrdinalIgnoreCase));

            if (state.Sort is { } sort && sort.Column >= 0 && sort.Column < state.Columns.Count && state.Columns[sort.Column].Sortable)
            {
                var column = state.Columns[sort.Column];
                rows = sort.Descending ? rows.OrderByDescending(column.SortValue) : rows.OrderBy(column.SortValue);
            }

            return rows.ToList();
        }

        /// <summary>
        /// The row the cursor is on, or default if there are none
        /// </summary>
        public static TRow? Selected<TRow>(DataGridState<TRow> state)
        {
            var rows = VisibleRows(state);
            return state.Cursor >= 0 && state.Cursor < rows.Count ? rows[state.Cursor] : default;
        }

        /// <summary>
        /// Replaces the rows, e.g. on a refresh. The cursor and scroll position
        /// stay where they were (moved up if there are fewer rows now), rather
        /// than following the selected row if the order changed.
        /// </summary>
        public static DataGridState<TRow> SetRows<TRow>(DataGridState<TRow> state, IEnumerable<TRow> rows) =>
            Clamp(state with { Rows = rows.ToList() });

        /// <summary>
        /// Filters the rows, keeping the cursor on the selected row if it still matches
        /// </summary>
        public static DataGridState<TRow> SetFilter<TRow>(DataGridState<TRow> state, string filter) =>
            KeepSelection(state, s => s with { Filter = filter ?? "" });

        public static DataGridState<TRow> SetViewHeight<TRow>(DataGridState<TRow> state, int height) =>
            Clamp(state with { ViewHeight = Math.Max(1, height) });

        /// <summary>
        /// Handles navigation and sort keys. Returns null for keys it doesn't
        /// use, so the caller can handle them.
        /// </summary>
        public static DataGridState<TRow>? Update<TRow>(DataGridState<TRow> state, KeyPressMsg key)
        {
            DataGridState<TRow> Move(int n) => Clamp(state with { Cursor = state.Cursor + n });
            var page = Math.Max(1, state.ViewHeight - 1);

            // Letters match on the character since terminals don't reliably report Shift
            switch (key.KeyChar)
            {
                case 'k': return Move(-1);
                case 'j': return Move(1);
                case 'g': return Clamp(state with { Cursor = 0 });
                case 'G': return Clamp(state with { Cursor = int.MaxValue });
                case 's': return KeepSelection(state, SortSelectedColumn);
            }

            return key.Key switch
            {
                ConsoleKey.UpArrow => Move(-1),
                ConsoleKey.DownArrow => Move(1),
                ConsoleKey.PageUp => Move(-page),
                ConsoleKey.B when key.Ctrl() => Move(-page),
                ConsoleKey.PageDown => Move(page),
                ConsoleKey.F when key.Ctrl() => Move(page),
                ConsoleKey.Home => Clamp(state with { Cursor = 0 }),
                ConsoleKey.End => Clamp(state with { Cursor = int.MaxValue }),
                ConsoleKey.LeftArrow => state with { Column = Math.Max(0, state.Column - 1) },
                ConsoleKey.RightArrow => state with { Column = Math.Max(0, Math.Min(state.Columns.Count - 1, state.Column + 1)) },
                _ => null,
            };
        }

        /// <summary>
        /// Renders the header and visible rows in exactly
        /// <paramref name="height"/> lines of at most <paramref name="width"/>
        /// columns. The cursor row and selected column are only highlighted
        /// when focused.
        /// </summary>
        public static IReadOnlyList<string> Render<TRow>(DataGridState<TRow> state, int width, int height, bool focused, Theme? theme = null)
        {
            theme ??= Theme.Default;
            var widths = ColumnWidths(width, state.Columns);

            var header = string.Join(" ", state.Columns.Select((c, i) =>
            {
                var arrow = state.Sort is { } sort && sort.Column == i ? (sort.Descending ? " ▼" : " ▲") : "";
                // The selected column's title is highlighted so you can see what s will sort
                var style = focused && i == state.Column ? theme.SelectedHeader : theme.Header;
                return style.Apply(Cell(widths[i], c.Align, c.Header + arrow));
            }));

            var rows = VisibleRows(state);
            var lines = new List<string> { header };

            if (rows.Count == 0)
            {
                var text = state.Loading ? state.LoadingText
                    : string.IsNullOrWhiteSpace(state.Filter) ? state.EmptyText
                    : $"No rows match \"{state.Filter}\"";
                lines.Add(theme.Dim.Apply(Ansi.Truncate(text, width)));
            }
            else
            {
                var bodyHeight = Math.Max(0, height - 1);
                for (int i = Math.Min(state.Offset, rows.Count); i < rows.Count && lines.Count <= bodyHeight; i++)
                {
                    var row = rows[i];
                    string line;
                    if (state.SpanFrom?.Invoke(row) is { } span)
                    {
                        var before = state.Columns.Take(span.FromColumn).Select((c, j) => Cell(widths[j], c.Align, c.Cell(row))).ToList();
                        var used = before.Sum(s => Ansi.VisibleLength(s) + 1);
                        line = string.Join(" ", [.. before, Ansi.PadRight(span.Text, width - used)]);
                    }
                    else
                    {
                        line = string.Join(" ", state.Columns.Select((c, j) => Cell(widths[j], c.Align, c.Cell(row))));
                    }

                    if (focused && i == state.Cursor)
                        line = theme.Selected.Apply(Ansi.PadRight(Ansi.Strip(line), width));
                    else if (state.RowStyle?.Invoke(row) is { } style)
                        line = style.Apply(Ansi.Strip(line));

                    lines.Add(line);
                }
            }

            // Columns' minimum widths can add up to more than the width
            return Block.FitHeight(lines.Select(l => Ansi.Cut(l, width)).ToList(), height);
        }

        /// <summary>
        /// "3/12": the cursor's position and the number of visible rows, for status bars
        /// </summary>
        public static string Position<TRow>(DataGridState<TRow> state)
        {
            var count = VisibleRows(state).Count;
            return count == 0 ? "0/0" : $"{state.Cursor + 1}/{count}";
        }

        private static string FilterText<TRow>(DataGridState<TRow> state, TRow row) =>
            state.FilterText?.Invoke(row) ?? string.Join(" ", state.Columns.Select(c => Ansi.Strip(c.Cell(row))));

        private static string RowKey<TRow>(DataGridState<TRow> state, TRow row) =>
            state.RowKey?.Invoke(row) ?? (state.Columns.Count > 0 ? Ansi.Strip(state.Columns[0].Cell(row)) : "");

        /// <summary>
        /// Keeps the cursor on screen and in range
        /// </summary>
        private static DataGridState<TRow> Clamp<TRow>(DataGridState<TRow> state)
        {
            var count = VisibleRows(state).Count;
            var height = Math.Max(1, state.ViewHeight);
            var cursor = count == 0 ? 0 : Math.Clamp(state.Cursor, 0, count - 1);
            var offset = cursor < state.Offset ? cursor
                : cursor >= state.Offset + height ? cursor - height + 1
                : state.Offset;
            return state with { Cursor = cursor, Offset = Math.Clamp(offset, 0, Math.Max(0, count - height)) };
        }

        /// <summary>
        /// Applies a change that may reorder the rows, keeping the cursor on
        /// the selected row if it's still visible
        /// </summary>
        private static DataGridState<TRow> KeepSelection<TRow>(DataGridState<TRow> state, Func<DataGridState<TRow>, DataGridState<TRow>> change)
        {
            var rows = VisibleRows(state);
            var key = state.Cursor >= 0 && state.Cursor < rows.Count ? RowKey(state, rows[state.Cursor]) : null;
            var next = change(state);

            if (key != null)
            {
                var index = VisibleRows(next).Select((r, i) => (r, i)).FirstOrDefault(x => RowKey(next, x.r) == key, (default!, -1)).i;
                if (index >= 0)
                    next = next with { Cursor = index };
            }

            return Clamp(next);
        }

        /// <summary>
        /// Sorts by the selected column ascending, or flips the direction if
        /// it's already sorted by it
        /// </summary>
        private static DataGridState<TRow> SortSelectedColumn<TRow>(DataGridState<TRow> state)
        {
            if (state.Column < 0 || state.Column >= state.Columns.Count || !state.Columns[state.Column].Sortable)
                return state;
            if (state.Sort is { } sort && sort.Column == state.Column)
                return state with { Sort = sort with { Descending = !sort.Descending } };
            return state with { Sort = new DataGridSort(state.Column, false) };
        }

        private static int[] ColumnWidths<TRow>(int width, IReadOnlyList<DataGridColumn<TRow>> columns)
        {
            var gaps = Math.Max(0, columns.Count - 1);
            var spare = Math.Max(0, width - gaps - columns.Sum(c => c.Width));
            var growing = columns.Count(c => c.Grow);
            var firstGrow = columns.ToList().FindIndex(c => c.Grow);

            // Give any remainder to the first growing column
            return columns
                .Select((c, i) => c.Grow ? c.Width + spare / growing + (i == firstGrow ? spare % growing : 0) : c.Width)
                .ToArray();
        }

        private static string Cell(int width, ColumnAlignment align, string text) => align switch
        {
            ColumnAlignment.Right => Ansi.PadLeft(text, width),
            ColumnAlignment.Center => Ansi.Center(text, width),
            _ => Ansi.PadRight(text, width),
        };
    }

    /// <summary>
    /// A <see cref="DataGrid"/> as a program, for C# apps that keep mutable
    /// components. Set <see cref="Width"/> and <see cref="Height"/>, or leave
    /// <see cref="AutoSize"/> on to fill the screen.
    /// </summary>
    public class DataGridView<TRow> : ICalliopeProgram
    {
        private int _height = 10;
        private DataGridState<TRow> _state;

        public DataGridView(params IEnumerable<DataGridColumn<TRow>> columns)
        {
            _state = DataGrid.Create(columns);
            State = _state;
        }

        /// <summary>
        /// The grid's state. Its <see cref="DataGridState{TRow}.ViewHeight"/>
        /// always follows <see cref="Height"/>.
        /// </summary>
        public DataGridState<TRow> State
        {
            get => _state;
            set => _state = DataGrid.SetViewHeight(value, _height - 1);
        }
        public TRow? Selected => DataGrid.Selected(State);
        public int Width { get; set; } = 80;
        /// <summary>
        /// Lines the grid takes, including the header
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                State = _state;
            }
        }
        /// <summary>
        /// Resize to the screen whenever the window is resized
        /// </summary>
        public bool AutoSize { get; set; } = true;
        public bool Focused { get; set; } = true;
        public Theme Theme { get; set; } = Theme.Default;

        /// <summary>
        /// Replaces the rows, keeping the cursor position
        /// </summary>
        public void SetRows(IEnumerable<TRow> rows) => State = DataGrid.SetRows(State, rows);

        public CalliopeCmd? Init() => null;

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is WindowSizeChangeMsg size && AutoSize)
            {
                Width = size.ScreenWidth;
                Height = size.ScreenHeight;
            }
            else if (Focused && msg is KeyPressMsg key && DataGrid.Update(State, key) is { } next)
            {
                State = next;
            }
            return null;
        }

        public string View() => string.Join('\n', DataGrid.Render(State, Width, Height, Focused, Theme));
    }
}
