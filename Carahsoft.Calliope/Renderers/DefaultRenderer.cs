using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public class DefaultRenderer : CalliopeRenderer
    {
        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopeOptions options)
        {
            return PerceivedBrightness(pixel) > 90 ? options.SpaceChar : options.DrawChar;
        }
    }
}
