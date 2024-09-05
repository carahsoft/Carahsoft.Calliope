using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class HackerWriter : TextWriter
    {
        TextWriter StandardOut;
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
                foreach (char c in value)
                {
                    i++;
                    if ((char.IsLetterOrDigit(c) || c == ' ') && i % 5 == 0)
                        Thread.Sleep(1);
                    Write(c);
                }
        }

        public override void WriteLine(string value)
        {
            Write(value);
            Write("\n");
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
