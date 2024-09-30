namespace Carahsoft.Calliope.Renderers
{
    public abstract class CalliopeRenderer : IRenderer
    {
        public abstract char CharAtPoint(int x, int y, RgbColor pixel, CalliopePrintOptions options);

        /// <summary>
        /// Calculate the perceived brightness of the given pixel
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns>
        /// A byte from 0-100, 0 being completely black and
        /// 100 being completely white
        /// </returns>
        internal static byte PerceivedBrightness(RgbColor pixel)
        {
            var dr = (decimal)pixel.Red / 255;
            var dg = (decimal)pixel.Green / 255;
            var db = (decimal)pixel.Blue / 255;
            return (byte)(100 * ((0.2126M * dr) + (0.7152M * dg) + (0.0722M * db)));
        }

        internal static CalliopeRenderer RendererFromEffect(CalliopeEffect effect)
        {
            return effect switch
            {
                CalliopeEffect.Greyscale => GreyscaleRenderer,
                CalliopeEffect.ScanlineGradient => ScanlineGradientRenderer,
                CalliopeEffect.Phoenix => PhoenixRenderer,
                CalliopeEffect.Unicorn => UnicornRenderer,
                CalliopeEffect.GalacticCrush => GalacticCrushRenderer,
                _ => DefaultRenderer
            };
        }

        public static CalliopeRenderer DefaultRenderer = new DefaultRenderer();
        public static CalliopeRenderer GreyscaleRenderer = new GreyscaleRenderer();
        public static CalliopeRenderer PhoenixRenderer = new PhoenixRenderer();
        public static CalliopeRenderer ScanlineGradientRenderer = new ScanlineGradientRenderer();
        public static CalliopeRenderer UnicornRenderer = new UnicornRenderer();
        public static CalliopeRenderer GalacticCrushRenderer = new GalacticCrushRenderer();
    }
}