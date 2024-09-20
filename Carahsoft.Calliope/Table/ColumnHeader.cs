using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Table
{
    public enum ColumnTypes
    {
        String,
        Int, 
        Decimal,
        Date
    }

    public class ColumnHeader
    {
        string _formatString;

        public ColumnTypes ColumnType { get; set; }
    
        public ColumnHeader()
        {
            Alignment = ColumnAlignment.Left;
        }

        public ColumnHeader(string Name)
        {
            this.Name = Name;
            Alignment = ColumnAlignment.Left;
        }

        public int Width { get; set; }
        
        public string Name { get; set; }
        
        public ColumnAlignment Alignment { get; set; }

        public string FormatString
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

        public string FormatValue(string value)
        {
            try
            {
                switch (ColumnType)
                {
                    case ColumnTypes.String:
                        return value;

                    case ColumnTypes.Decimal:
                        return Decimal.Parse(value).ToString(OutputFormat ?? string.Empty);

                    case ColumnTypes.Date:
                        return DateTime.Parse(value).ToString(OutputFormat ?? string.Empty);

                    case ColumnTypes.Int:
                        return Int32.Parse(value).ToString(OutputFormat ?? string.Empty);

                    default: return value;
                }
            }
            catch
            {
                return value;
            }
        }

        public string OutputFormat { get; set; }
    }
}
