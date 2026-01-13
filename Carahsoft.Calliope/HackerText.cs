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

        public bool Hackify { get; set; }

        public HackerWriter(TextWriter standardOut)
        {
            StandardOut = standardOut;
        }
        public override void Write(char value)
        {
            StandardOut.Write(value);
        }

        public override void Write(string? value)
        {
            if (value == null) return;

            if (Hackify)
            {
                int i = 0;

                foreach (char c in value)
                {
                    if ((c == '\n'))
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
            }
            else
            {
                StandardOut.Write(value);
            }
        }

        public override void WriteLine(string? value)
        {
            Write(value);
            Write('\n');
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
