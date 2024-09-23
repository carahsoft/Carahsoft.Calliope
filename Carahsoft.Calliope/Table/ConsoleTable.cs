<<<<<<< HEAD
﻿using Carahsoft.Calliope.Components;
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
=======
﻿using System.Data;
>>>>>>> c8bc53b56a445256b4e72100ca26bb5395b09673
using System.Text;

namespace Carahsoft.Calliope.Table
{
<<<<<<< HEAD


   

    public class ConsoleTable
=======
    /// <summary>
    /// This class is used for outputting a System.Data.DataTable or an IEnnumberable of objects in a table to the console.  
    /// Sorting and text formatting are supported.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConsoleTable<T>
>>>>>>> c8bc53b56a445256b4e72100ca26bb5395b09673
    {

        protected DataTable _table;
        public List<ColumnHeader> Columns;

        /// <summary>
        /// Initializes a ConsoleTable supplying a DataTable onject
        /// </summary>
        /// <param name="table"></param>
        public ConsoleTable(DataTable table)
        {
            this._table = table;
            SetHeaders();
        }

        /// <summary>
        /// Creates and assigns list of ColumnHeader objects containing column 
        /// formatting information.  Alignment assumption is made by contents 
        /// of first cell in each column.  
        /// </summary>
        protected void SetHeaders()
        {
            Columns = new List<ColumnHeader>();

            foreach (DataColumn col in _table.Columns)
            {
                var colHeader = new ColumnHeader(col.ColumnName);

                if (col.DataType == typeof(int))
                {
                    colHeader.Alignment = ColumnAlignment.Center;
                    colHeader.ColumnType = ColumnType.Int;
                }
                else if (col.DataType == typeof(decimal))
                {
                    colHeader.Alignment = ColumnAlignment.Right;
                    colHeader.ColumnType = ColumnType.Decimal;
                }
                else if (col.DataType == typeof(DateTime))
                {
                    colHeader.Alignment = ColumnAlignment.Right;
                    colHeader.ColumnType = ColumnType.Date;
                }

                SetWidth(colHeader);

                Columns.Add(colHeader);
            }

        }

        /// <summary>
        /// Sets the output wodth for each column using the longest element contained in the column
        /// </summary>
        /// <param name="colHeader"></param>
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

        /// <summary>
        /// Sets the format string for a specified column for outputting.  Forexample "MM/dd/yyyy" on a date.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="formatString"></param>
        public void ColOutputFormat(int columnIndex, string formatString)
        {
            ColOutputFormat(Columns[columnIndex].Name, formatString);
        }

        /// <summary>
        /// Sets the format string for a specified column for outputting.  Forexample "MM/dd/yyyy" on a date.
        /// </summary>
        /// <param name="ColumnName">Name of the column to format</param>
        /// <param name="StringFormat"></param>
        public void ColOutputFormat(string columnName, string formatString)
        {
            var col = Columns.Find(x => x.Name == columnName);
            col.OutputFormat = formatString;
            SetWidth(col);
        }

        /// <summary>
        /// Sorts the data table by the column and direction supplied by yser
        /// </summary>
        /// <param name="columnIndex">0 based column index of column to sort by</param>
        /// <param name="direction">Direction to sort (Ascending or Descending.  Defauly value = Ascending</param>
        public void Sort(int columnIndex, SortDirection direction = SortDirection.ASC)
        {
            Sort(Columns[columnIndex].Name, direction);
        }

        /// <summary>
        /// Sorts the data table by the column and direction supplied by yser
        /// </summary>
        /// <param name="columnName">0 ColumnName to sort by</param>
        /// <param name="direction">Direction to sort (Ascending or Descending.  Defauly value = Ascending</param>
        public void Sort(string columnName, SortDirection direction = SortDirection.ASC)
        {
            DataView dv = _table.DefaultView;
            dv.Sort = $"{columnName} {direction}";
            _table = dv.ToTable();
        }

        /// <summary>
        /// Output entire table contents to table applying cell alignmnet and formatting
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder spacer = new StringBuilder();

            sb.Append(" | ");

            spacer.Append(" +-");

            foreach (var c in Columns)
            {
                sb.Append(string.Format(c.AlignString, c.Name));
                sb.Append(" | ");

                spacer.Append(string.Empty.PadLeft(c.Width, '-'));
                spacer.Append("-+-");
            }
            sb.Append('\n');
            sb.Append(spacer.ToString());
            sb.Append('\n');

            foreach (DataRow dr in _table.Rows)
            {
                sb.Append(" | ");

                foreach (var c in Columns)
                {
                    string val = c.FormatValue(dr[c.Name].ToString());

                    if (c.Alignment == ColumnAlignment.Center)
                        val = val.PadRight((int)Math.Ceiling(((decimal)c.Width - val.Length) / 2 + val.Length), ' ');

                    sb.Append(string.Format(c.AlignString, val));

                    sb.Append(" | ");
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns string composed of entire table contents in a text delimited format.
        /// </summary>
        /// <param name="delimiter">Delimiter character.  Default value = ","</param>
        /// <returns></returns>
        public string ToDelimitedString(char delimiter = ',')
        {
            StringBuilder sb = new StringBuilder();

            foreach (var header in Columns)
            {
                sb.Append(header.Name.Replace("\"", "\"\"").Replace("\n", "\r"));
                sb.Append(delimiter);
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("\n");

            foreach (DataRow dr in _table.Rows)
            {
                foreach (var header in Columns)
                {
                    sb.Append("\"");
                    sb.Append((dr[header.Name].ToString() ?? string.Empty).Replace("\"", "\"\"").Replace("\n", "\r"));
                    sb.Append($"\"{delimiter}");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns string containing entire table contents in XML format
        /// </summary>
        /// <returns></returns>
        public string ToXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<TABLE>\n");

            foreach (DataRow dr in _table.Rows)
            {
                sb.Append("<ROW>\n");
                foreach (var header in Columns)
                {
                    sb.Append("<" + header.Name + ">" + dr[header.Name].ToString() + "</" + header.Name + ">");
                }
                sb.Append("\n</ROW>\n");
            }

            sb.Append("</TABLE>");
            return sb.ToString();
        }

        /// <summary>
        /// Returns string containing entire table contents in JSON Format
        /// </summary>
        /// <returns></returns>
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

    /// <summary>
    /// This class is used for outputting a System.Data.DataTable or an IEnnumberable of objects in a table to the console.  
    /// Sorting and text formatting are supported.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConsoleTable<T> : ConsoleTable
    {
        
        /// <summary>
        /// Initializes the table using an IEnumerable collection of objects.
        /// All public properties in the object will become columns.
        /// </summary>
        /// <param name="myList"></param>
        public ConsoleTable(IEnumerable<T> myList) : base (getDataTable(myList))
        {

        }

        private static DataTable getDataTable(IEnumerable<T> myList)
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
                foreach (var whatever in obj.GetType().GetProperties())
                {
                    alist.Add(whatever.GetValue(obj));
                }
                dt.Rows.Add(alist.ToArray());
            }
           
            return dt;
        }
       
    }




}
