using Carahsoft.Calliope.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class CalliopePrintOptions
    {
        public int Width { get; set; } = 120;
        public int Height { get; set; } = 28;
        public string Font { get; set; } = "Roboto";
        public int FontSize { get; set; } = 18;
        public RgbColor? StartColor { get; set; }
        public RgbColor? EndColor { get; set; }
        public RgbColor? BackgroundColor { get; set; }
        public char DrawChar { get; set; } = '\u2580';
        public char SpaceChar { get; set; } = ' ';
        public bool AntiAliasing { get; set; } = true;
        public CalliopeEffect Effect { get; set; } = CalliopeEffect.None;
        public IRenderer? Renderer { get; set; } = null;
        public bool RemovePadding { get; set; } = false;
    }
}
