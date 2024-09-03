using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Table
{
    public class ColumnHeader
    {
        string _formatString;
        public ColumnHeader()
        {
            Alignment = ColumnAlignment.Left;
        }

        public int Width { get; set; }
        public string Name { get; set; }

        public ColumnAlignment Alignment { get; set; }
        public int Rows { get; set; }

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
    }
}
