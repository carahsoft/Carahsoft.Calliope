using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Renderers
{
    public interface IRenderer
    {
        char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options);
    }
}
