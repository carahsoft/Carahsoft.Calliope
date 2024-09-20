using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Table
{
    public enum SortDirection
    {
        ASC, 
        DESC,
    }
    public class ConsoleTable<T>
    {

        DataTable _table;
        public List<ColumnHeader> Columns;

        public ConsoleTable(DataTable table)
        {
            this._table = table;
            SetHeaders();
        }

        public ConsoleTable(IEnumerable<T> myList)
        {
            var testList = myList.ToList();

            DataTable dt = new DataTable();

            foreach (var o in testList[0].GetType().GetProperties())
            {
                dt.Columns.Add(o.Name, o.PropertyType);
            }
            
            foreach (var obj in testList)
            {
                var alist = new List<object>();
                foreach(var whatever in obj.GetType().GetProperties())
                {
                    alist.Add(whatever.GetValue(obj));
                }
               dt.Rows.Add(alist.ToArray());
            }
            
            _table = dt;
            SetHeaders();
        }

        private void SetHeaders()
        {
            Columns = new List<ColumnHeader>();

            foreach (DataColumn col in _table.Columns)
            {
                var colHeader = new ColumnHeader();

                colHeader.Name = col.ColumnName;

                if (col.DataType == typeof(int))
                {
                    colHeader.Alignment = ColumnAlignment.Center;
                    colHeader.ColumnType = ColumnTypes.Int;
                }
                else if (col.DataType == typeof(decimal))
                {
                    colHeader.Alignment = ColumnAlignment.Right;
                    colHeader.ColumnType = ColumnTypes.Decimal;
                }
                else if (col.DataType == typeof(DateTime))
                {
                    colHeader.Alignment = ColumnAlignment.Right;
                    colHeader.ColumnType = ColumnTypes.Date;
                }
                else
                {
                    colHeader.Alignment = ColumnAlignment.Left;
                }

                SetWidth(colHeader);

                Columns.Add(colHeader);
            }

        }

        private void SetWidth(ColumnHeader colHeader)
        {
            DataColumn col = _table.Columns[colHeader.Name];

            string maxString = colHeader.FormatValue(_table.AsEnumerable()
                                       .Select(row => row[col].ToString())
                                       .OrderByDescending(st => st.Length).FirstOrDefault());
            
            int width = maxString.Length;

            if (width < col.ColumnName.Length)
            {
                width = col.ColumnName.Length;
            }

            colHeader.Width = width; ;
        }

        public void ApplyStyle(int ColumnIndex, string StringFormat)
        {
            Columns[ColumnIndex].OutputFormat = StringFormat;
            SetWidth(Columns[ColumnIndex]);
        }

        public void ApplyStyle(string ColumnName, string StringFormat)
        {
            var col = Columns.Find(x => x.Name == ColumnName);
            col.OutputFormat = StringFormat;
            SetWidth(col);
        }


        public void Sort(int columnnumber, SortDirection direction = SortDirection.ASC)
        {
            DataView dv = _table.DefaultView;
            dv.Sort = $"{Columns[columnnumber].Name} {direction}";
            _table = dv.ToTable();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            StringBuilder _spacer = new StringBuilder();


            sb.Append(" | ");

            _spacer.Append(" +-");
            
            foreach (var c in Columns)
            {
                sb.Append(string.Format(c.FormatString, c.Name));
                sb.Append(" | ");

                _spacer.Append(string.Empty.PadLeft(c.Width, '-'));
                _spacer.Append("-+-");
            }
            sb.Append('\n');
            sb.Append(_spacer.ToString());
            sb.Append('\n');


            foreach (DataRow dr in _table.Rows)
            {
                sb.Append(" | ");

                foreach (var c in Columns)
                {
                    string val = c.FormatValue(dr[c.Name].ToString());

                    if (c.Alignment == ColumnAlignment.Center)
                        val = val.PadRight((int)Math.Ceiling(((decimal)c.Width - val.Length) / 2 + val.Length), ' ');

                    sb.Append(string.Format(c.FormatString, val));

                    sb.Append(" | ");
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public string ToCSV()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var header in Columns)
            {
                sb.Append(header.Name.Replace("\"", "\"\"").Replace("\n", "\r"));
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("\n");

            foreach (DataRow dr in _table.Rows)
            {
                foreach (var header in Columns)
                {
                    sb.Append("\"");
                    sb.Append((dr[header.Name].ToString() ?? string.Empty).Replace("\"", "\"\"").Replace("\n", "\r"));
                    sb.Append("\",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public string ToXML()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<TABLE>\n");

            foreach (DataRow dr in _table.Rows)
            {
                sb.Append("<ROW>\n");
                foreach (var header in Columns)
                {
                    sb.Append("<" + header.Name + ">" + dr[header.Name].ToString()+ "</" + header.Name + ">");
                }
                sb.Append("\n</ROW>\n");
            }

            sb.Append("</TABLE>");
            return sb.ToString();
        }

        public string ToJSON()
        {
            var sb = new StringBuilder();
            if (_table.Rows.Count > 0)
            {
                sb.Append("[");
                for (int i = 0; i < _table.Rows.Count; i++)
                {
                    sb.Append("{");
                    for (int j = 0; j < _table.Columns.Count; j++)
                    {
                        sb.Append("\"" + _table.Columns[j].ColumnName.ToString() + "\":" + "\"" + _table.Rows[i][j].ToString() + "\"");

                        if (j < _table.Columns.Count - 1)
                        {
                            sb.Append(",");
                        }
   
                    }
                    sb.Append("}");

                    if (i < _table.Rows.Count - 1)
                    {
                        sb.Append(",\n");
                    }
              
                }
                sb.Append("]");
            }
            return sb.ToString();
        }
    }




}
