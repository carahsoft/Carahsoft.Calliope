using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class HackerWriter : TextWriter
    {
        TextWriter StandardOut;

     
        public RgbPixel foreground { get; set; }
        public RgbPixel background { get; set; }

        public HackerWriter(TextWriter standardOut)
        {
            StandardOut = standardOut;

        }
        public override void Write(char value)
        {
            StandardOut.Write(value);
        }

        public override void Write(string value)
        {
            int i = 0;

            StandardOut.Write(GetANSIColorTag());

            foreach (char c in value)
            {
                if ((c == '\n') )
                {
                    Thread.Sleep(1);
                    i++;
                }
                Write(c);

                if (i > 2000)
                {
                    StandardOut.Write(value.Substring(i));
                    break;
                }
            }
            StandardOut.Write("\x1b[0m");
        }

        public void ColorWrite(string value, string foreground, string background = "000000")
        {
            string s1 = $"\x1b[38;2;{fromHex(foreground.Substring(0, 2))};{fromHex(foreground.Substring(2, 2))};{fromHex(foreground.Substring(4, 2))};" +
                  $"48;2;{fromHex(background.Substring(0, 2))};{fromHex(background.Substring(2, 2))};{fromHex(background.Substring(4, 2))}m";
            string s2 = "\x1b[0m";

            StandardOut.Write(s1 + value + s2);
        }

        public void ColorWriteLine(string value, string foreground, string background = "000000")
        {
            ColorWrite(value, foreground, background);
            StandardOut.Write("\n");
        }

        private int fromHex(string value)
        {
            return int.Parse(value, System.Globalization.NumberStyles.HexNumber);
        }

        public override void WriteLine(string value)
        {
            Write(value);
            Write('\n');
        }

        private string GetANSIColorTag()
        {
            if (foreground == null && background == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append("\x1b[");

            if (foreground != null)
            {
                sb.Append($"38;2;{(int)foreground.Red};{(int)foreground.Green};{(int)foreground.Blue}");
            }
            if (background == null)
            {
                sb.Append("m");
            }
            else
            {
                if(foreground != null)
                {
                    sb.Append(';');
                }
                sb.Append($"48;2;{(int)background.Red};{(int)background.Green};{(int)background.Blue}m");
            }

            return sb.ToString(); 
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
