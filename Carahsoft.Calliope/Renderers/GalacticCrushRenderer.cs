using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class GalacticCrushRenderer : CalliopeRenderer
    {
        private readonly Random _rand = new Random();

        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options)
        {
            var line = _rand.Next(100);

            if (PerceivedBrightness(pixel) > 90)
                return options.SpaceChar;

            return line switch
            {
                int i when i < 80 => '\u2605',
                int i when i < 85 => '\u2606',
                int i when i < 90 => '\u272F',
                int i when i < 95 => '\u272D',
                _ => '\u272B'
            };
        }
    }
}
