using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Table
{


    public class ColumnHeader
    {
        string _formatString;

        /// <summary>
        /// Column Data Type.  
        /// </summary>
        public ColumnType ColumnType { get; set; }
    

        /// <summary>
        /// Initializes a ColumnHeader object for use in a ConsoleTable
        /// ColumnHeader contains data type and output formatting 
        /// </summary>
        /// <param name="Name">Name of the column.  Name is used for the header text</param>
        public ColumnHeader(string Name)
        {
            this.Name = Name;
            Alignment = ColumnAlignment.Left;
        }

        /// <summary>
        /// Width in characters of the column for output 
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// Name of the column.  Used for accessing the column by name and to supply the column name when outputting the table.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Specifies how to align the cell date on screen.  Left, Right or Center
        /// </summary>
        public ColumnAlignment Alignment { get; set; }

        /// <summary>
        /// Provides the format string that is used to position/align the column data in a call
        /// </summary>
        public string AlignString
        {
            get
            {
                if (string.IsNullOrEmpty(_formatString))
                {
                    _formatString = "{0," + Width * (Alignment == ColumnAlignment.Left ? -1 : 1) + "}";
                }

                return _formatString;
            }
        }

        /// <summary>
        /// Converts the value of the current object to a string representation using formating conventions.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FormatValue(string value)
        {
            try
            {
                switch (ColumnType)
                {
                    case ColumnType.String:
                        return value;

                    case ColumnType.Decimal:
                        return Decimal.Parse(value).ToString(OutputFormat ?? string.Empty);

                    case ColumnType.Date:
                        return DateTime.Parse(value).ToString(OutputFormat ?? string.Empty);

                    case ColumnType.Int:
                        return Int32.Parse(value).ToString(OutputFormat ?? string.Empty);

                    default: return value;
                }
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Composit format string for data output
        /// </summary>
        public string OutputFormat { get; set; }
    }
}
