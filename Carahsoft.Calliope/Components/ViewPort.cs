using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class ViewPort
    {
        private int _height;

        public List<string> Text { get; set; } = [];
        /// <summary>
        /// The height of the <see cref="ViewPort"/>. If less than the amount of rows in
        /// <see cref="Text"/>, the viewport will be truncated based on the
        /// value of the <see cref="RowIndex"/>. If greater than the amount of
        /// rows in <see cref="Text"/>, padding will be applied based on the
        /// value of <see cref="PadTop"/>.
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                SetRowIndex(RowIndex);
            }
        }
        public int Width { get; set; }
        public bool PadTop { get; set; }

        /// <summary>
        /// <see cref="RowIndex"/> is the current 0-based index of the
        /// <see cref="ViewPort"/> from the top.
        /// </summary>
        public int RowIndex { get; private set; }

        public void SetRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || Text.Count - _height < 0)
                rowIndex = 0;
            else if (rowIndex > Text.Count - _height)
                rowIndex = Text.Count - _height;

            RowIndex = rowIndex;
        }

        public string View()
        {
            var sb = new StringBuilder();
            var padAmt = _height - Text.Count;
            if (padAmt > 0)
            {
                if (PadTop)
                {
                    for (int i = 0; i < padAmt; i++)
                    {
                        sb.AppendLine();
                    }
                }
                for (int i = 0; i < Text.Count; i++)
                {
                    sb.AppendLine(Text[i]);
                }
                if (!PadTop)
                {
                    for (int i = 0; i < padAmt; i++)
                    {
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                for (int i = RowIndex; i < RowIndex + _height; i++)
                {
                    sb.AppendLine(Text[i]);
                }
            }
            return sb.ToString();
        }
    }
}
