namespace Carahsoft.Calliope.Table
{
    /// <summary>
    /// Defines the border characters used for rendering console tables.
    /// </summary>
    public class TableBorder
    {
        public char TopLeft { get; init; }
        public char TopRight { get; init; }
        public char BottomLeft { get; init; }
        public char BottomRight { get; init; }
        public char Horizontal { get; init; }
        public char Vertical { get; init; }
        public char LeftT { get; init; }
        public char RightT { get; init; }
        public char TopT { get; init; }
        public char BottomT { get; init; }
        public char Cross { get; init; }

        /// <summary>
        /// Classic ASCII borders using +, -, and | characters.
        /// </summary>
        public static TableBorder Ascii { get; } = new TableBorder
        {
            TopLeft = '+',
            TopRight = '+',
            BottomLeft = '+',
            BottomRight = '+',
            Horizontal = '-',
            Vertical = '|',
            LeftT = '+',
            RightT = '+',
            TopT = '+',
            BottomT = '+',
            Cross = '+'
        };

        /// <summary>
        /// Unicode box-drawing characters with sharp corners.
        /// </summary>
        public static TableBorder Unicode { get; } = new TableBorder
        {
            TopLeft = '┌',
            TopRight = '┐',
            BottomLeft = '└',
            BottomRight = '┘',
            Horizontal = '─',
            Vertical = '│',
            LeftT = '├',
            RightT = '┤',
            TopT = '┬',
            BottomT = '┴',
            Cross = '┼'
        };

        /// <summary>
        /// Unicode box-drawing characters with rounded corners.
        /// </summary>
        public static TableBorder Rounded { get; } = new TableBorder
        {
            TopLeft = '╭',
            TopRight = '╮',
            BottomLeft = '╰',
            BottomRight = '╯',
            Horizontal = '─',
            Vertical = '│',
            LeftT = '├',
            RightT = '┤',
            TopT = '┬',
            BottomT = '┴',
            Cross = '┼'
        };

        /// <summary>
        /// Unicode double-line box-drawing characters.
        /// </summary>
        public static TableBorder Double { get; } = new TableBorder
        {
            TopLeft = '╔',
            TopRight = '╗',
            BottomLeft = '╚',
            BottomRight = '╝',
            Horizontal = '═',
            Vertical = '║',
            LeftT = '╠',
            RightT = '╣',
            TopT = '╦',
            BottomT = '╩',
            Cross = '╬'
        };

        /// <summary>
        /// Heavy Unicode box-drawing characters.
        /// </summary>
        public static TableBorder Heavy { get; } = new TableBorder
        {
            TopLeft = '┏',
            TopRight = '┓',
            BottomLeft = '┗',
            BottomRight = '┛',
            Horizontal = '━',
            Vertical = '┃',
            LeftT = '┣',
            RightT = '┫',
            TopT = '┳',
            BottomT = '┻',
            Cross = '╋'
        };

        /// <summary>
        /// No borders - just spaced columns.
        /// </summary>
        public static TableBorder None { get; } = new TableBorder
        {
            TopLeft = '\0',
            TopRight = '\0',
            BottomLeft = '\0',
            BottomRight = '\0',
            Horizontal = '\0',
            Vertical = ' ',
            LeftT = '\0',
            RightT = '\0',
            TopT = '\0',
            BottomT = '\0',
            Cross = '\0'
        };

        /// <summary>
        /// Returns true if this border style has no visible borders.
        /// </summary>
        public bool IsNone => Horizontal == '\0';
    }
}
