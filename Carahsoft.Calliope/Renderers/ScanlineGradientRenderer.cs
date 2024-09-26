using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class ScanlineGradientRenderer : CalliopeRenderer
    {
        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopeOptions options)
        {
            var line = (int)(((double)x / options.Height) * 8);
            var val = PerceivedBrightness(pixel) > 90;

            return (val, line) switch
            {
                (true, _) => options.SpaceChar,
                (_, 0) => '\u2588',  //█
                (_, 1) => '\u2587',  //▇
                (_, 2) => '\u2586',  //▆
                (_, 3) => '\u2585',  //▅
                (_, 4) => '\u2584',  //▄  
                (_, 5) => '\u2583',  //▃        
                (_, 6) => '\u2582',  //▂
                (_, 7) => '\u2581',  //▁
            };
        }
    }
}
