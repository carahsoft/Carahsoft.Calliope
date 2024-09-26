using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class GreyscaleRenderer : CalliopeRenderer
    {
        private const char LIGHT = '\u2591';
        private const char MEDIUM = '\u2592';
        private const char DARK = '\u2593';
        //private const char FULL = '\u2588';

        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopeOptions options)
        {
            var brightness = PerceivedBrightness(pixel);

            if (brightness > 90)
                return ' ';
            if (brightness > 80)
                return LIGHT;
            if (brightness > 65)
                return MEDIUM;
            return DARK;
        }

    }
}
