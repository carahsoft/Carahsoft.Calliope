using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class PhoenixRenderer : CalliopeRenderer
    {
        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options)
        {
            var line = (int)((((double)(y) / options.Width)) * 8);
            var val = PerceivedBrightness(pixel) > 90;

            return (val, line) switch
            {
                (true, _) => options.SpaceChar,
                (_, 0) => '\u2588',  //█
                (_, 1) => '\u2593',  //▓
                (_, 2) => '\u2592',  //▒
                (_, 3) => '\u2591',  //░
                (_, 4) => '\u2591',  //░
                (_, 5) => '\u2592',  //▒
                (_, 6) => '\u2593',  //▓
                (_, 7) => '\u2588',  //█
            };
        }
    }
}
