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
    public class ConsoleTable<T>
    {

        DataTable tab;
        List<ColumnHeader> cols;

        public ConsoleTable(DataTable table)
        {
            tab = table;
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
            
            tab = dt;
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
                    colHeader.Width = maxString.Length ;
                }
                else
                {
                    colHeader.Width = col.ColumnName.Length ;
                }


                if (col.DataType == typeof(int))
                {
                    colHeader.Alignment = ColumnAlignment.Center;
                }
                else if (col.DataType == typeof(decimal))
                {
                    colHeader.Alignment = ColumnAlignment.Right;
                }
                else
                {
                    colHeader.Alignment = ColumnAlignment.Left;
                }

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

            foreach (var header in cols)
            {
                sb.Append(header.Name.Replace("\"", "\"\"").Replace("\n", "\r"));
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("\n");

            foreach (DataRow dr in tab.Rows)
            {
                foreach (var header in cols)
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

            foreach (DataRow dr in tab.Rows)
            {
                sb.Append("<ROW>\n");
                foreach (var header in cols)
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
            if (tab.Rows.Count > 0)
            {
                sb.Append("[");
                for (int i = 0; i < tab.Rows.Count; i++)
                {
                    sb.Append("{");
                    for (int j = 0; j < tab.Columns.Count; j++)
                    {
                        sb.Append("\"" + tab.Columns[j].ColumnName.ToString() + "\":" + "\"" + tab.Rows[i][j].ToString() + "\"");

                        if (j < tab.Columns.Count - 1)
                        {
                            sb.Append(",");
                        }
   
                    }
                    sb.Append("}");

                    if (i < tab.Rows.Count - 1)
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
