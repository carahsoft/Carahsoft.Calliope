using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Table
{
    public class ConsoleTable
    {

        DataTable tab;
        List<ColumnHeader> cols;
        public ConsoleTable(DataTable table)
        {
            tab = table;
            SetHeaders();
        }

        private void SetHeaders()
        {
            cols = new List<ColumnHeader>();

            foreach (DataColumn col in tab.Columns)
            {
                var colHeader = new ColumnHeader();

                colHeader.Name = col.ColumnName;

                string maxString = tab.AsEnumerable()
                                        .Select(row => row[col].ToString())
                                        .OrderByDescending(st => st.Length).FirstOrDefault();

                if (maxString.Length > col.ColumnName.Length)
                {
                    colHeader.Width = maxString.Length + 2;
                }
                else
                    colHeader.Width = col.ColumnName.Length + 2;

                cols.Add(colHeader);

            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            StringBuilder spacer = new StringBuilder();


            sb.Append(" | ");

            spacer.Append(" +-");
            foreach (var c in cols)
            {
                sb.Append(string.Format(c.FormatString, c.Name));
                sb.Append(" | ");

                spacer.Append(string.Empty.PadLeft(c.Width, '-'));
                spacer.Append("-+-");
            }
            sb.Append('\n');
            sb.Append(spacer.ToString());
            sb.Append('\n');


            foreach (DataRow dr in tab.Rows)
            {
                sb.Append(" | ");

                foreach (var c in cols)
                {

                    string val = dr[c.Name].ToString();

                    if (c.Alignment == ColumnAlignment.Center)
                        val = val.PadRight((c.Width - val.Length) / 2 + val.Length, ' ');

                    sb.Append(string.Format(c.FormatString, val));

                    sb.Append(" | ");
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }

    }




}
