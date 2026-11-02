namespace Carahsoft.Calliope.Components
{
    /// <summary>
    /// Copies of the component records with one property changed, for
    /// Carahsoft.Calliope.Elm: F# can't use C#'s <c>with</c> on records.
    /// </summary>
    internal static class RecordCopies
    {
        public static DataGridColumn<TRow> WithGrow<TRow>(DataGridColumn<TRow> c, bool grow) => c with { Grow = grow };
        public static DataGridColumn<TRow> WithAlign<TRow>(DataGridColumn<TRow> c, global::Carahsoft.Calliope.Table.ColumnAlignment align) => c with { Align = align };
        public static DataGridColumn<TRow> WithSortable<TRow>(DataGridColumn<TRow> c, bool sortable) => c with { Sortable = sortable };
        public static DataGridColumn<TRow> WithSortKey<TRow>(DataGridColumn<TRow> c, Func<TRow, IComparable>? key) => c with { SortKey = key };

        public static DataGridState<TRow> WithRowKey<TRow>(DataGridState<TRow> s, Func<TRow, string>? key) => s with { RowKey = key };
        public static DataGridState<TRow> WithFilterText<TRow>(DataGridState<TRow> s, Func<TRow, string>? text) => s with { FilterText = text };
        public static DataGridState<TRow> WithRowStyle<TRow>(DataGridState<TRow> s, Func<TRow, Style?>? style) => s with { RowStyle = style };
        public static DataGridState<TRow> WithSpanFrom<TRow>(DataGridState<TRow> s, Func<TRow, DataGridSpan?>? span) => s with { SpanFrom = span };
        public static DataGridState<TRow> WithEmptyText<TRow>(DataGridState<TRow> s, string text) => s with { EmptyText = text };
        public static DataGridState<TRow> WithLoading<TRow>(DataGridState<TRow> s, bool loading) => s with { Loading = loading };

        public static FormField WithRequired(FormField f, bool required) => f with { Required = required };
        public static FormField WithDescription(FormField f, string? description) => f with { Description = description };
        public static FormField WithLabel(FormField f, string? label) => f with { Label = label };
        public static FormField WithHidden(FormField f, bool hidden) => f with { Hidden = hidden };
        public static FormField WithInput(FormField f, TextFieldState input) => f with { Input = input };

        public static DropdownState WithRows(DropdownState d, int rows) => d with { Rows = rows };

        public static CommandLineCommand WithAliases(CommandLineCommand c, IReadOnlyList<string> aliases) => c with { Aliases = aliases };
    }
}
