using Carahsoft.Calliope.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class Table : ICalliopeProgram
    {
        private DataTable _table;
        private ScrollView _sv = new();

        public ScrollView ScrollView { get { return _sv; } }
        public Style? HeaderStyle { get; set; }
        public Style? RowStyle { get; set; }
        public TableBorder Border { get; set; } = TableBorder.Ascii;

        public Table(DataTable table)
        {
            _table = table;
        }

        public CalliopeCmd? Init()
        {
            _sv.RenderView = RenderTable();
            return null;
        }

        /// <summary>
        /// Replaces the table's rows and redraws it, keeping the scroll
        /// position where possible
        /// </summary>
        public void SetRows(DataTable table)
        {
            _table = table;
            _sv.RenderView = RenderTable();

            var lineCount = _sv.RenderView.Split('\n').Length;
            _sv.Index = Math.Clamp(_sv.Index, 0, Math.Max(lineCount - _sv.Height, 0));
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            return _sv.Update(msg);
        }

        public string View()
        {
            return _sv.View();
        }

        private string RenderTable()
        {
            var ct = new ConsoleTable(_table);
            var lines = ct.ToString(Border).Split('\n');

            // With borders, the header is between the top border and a separator
            var headerLine = Border.IsNone ? 0 : 1;
            var firstRowLine = Border.IsNone ? 1 : 3;

            if (HeaderStyle != null)
                lines[headerLine] = HeaderStyle.Apply(lines[headerLine]);

            if (RowStyle != null)
            {
                for (int i = firstRowLine; i < firstRowLine + _table.Rows.Count; i++)
                    lines[i] = RowStyle.Apply(lines[i]);
            }

            return string.Join('\n', lines);
        }
    }
}
