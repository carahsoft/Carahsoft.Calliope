using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class UnicornRenderer : CalliopeRenderer
    {
        private readonly Random _rand = new Random();
        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options)
        {
            var line = _rand.Next() % 15;
            var val = PerceivedBrightness(pixel) > 90;

            return (val, line) switch
            {
                (true, _) => options.SpaceChar,
                (_, 0) => '\u2724',  //
                (_, 1) => '\u2725',  //
                (_, 2) => '\u2726',  //
                (_, 3) => '\u2727',  //
                (_, 4) => '\u2739',  //
                (_, 5) => '\u2729',  //
                (_, 6) => '\u2740',  //
                (_, 7) => '\u2741',  //
                (_, 8) => '\u2742',  //
                (_, 9) => '\u2743',  //
                //(_, 10) => '\u2744', //
                (_, 10) => '\u2745', //
                (_, 11) => '\u2746', //
                (_, 12) => '\u2765', //
                (_, 13) => '\u2748', //
                (_, 14) => '\u2749', //
                //(_, 16) => '\u2764', //
            };
        }
    }
}
