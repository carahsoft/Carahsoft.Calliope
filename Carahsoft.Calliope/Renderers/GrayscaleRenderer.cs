namespace Carahsoft.Calliope.Renderers
{
    public class GrayscaleRenderer : CalliopeRenderer
    {
        private const char LIGHT = '░';
        private const char MEDIUM = '▒';
        private const char DARK = '▓';
        //private const char FULL = '█';

        public override char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options)
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
