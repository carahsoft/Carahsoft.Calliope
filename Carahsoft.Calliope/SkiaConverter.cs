using SkiaSharp;
using System.Text;

namespace Carahsoft.Calliope
{
    internal class SkiaConverter
    {
        public SkiaConverter(
            string bannerText,
            CalliopeOptions? options = null)
        {
            _options = options ?? new CalliopeOptions();
            BannerText = bannerText;
            Width = _options.Width;
            Height = _options.Height;
            FontName = _options.Font;
            FontSize = _options.FontSize;
            _color = _options.FontColor;
            _drawChar = _options.DrawChar;
            _spaceChar = _options.SpaceChar;
            _drawThreshold = _options.DrawThreshold;
            _antiAliasing = _options.AntiAliasing;
            DrawText();

            _matrix = new int[Height, Width];

            ConvertToMatrix();
        }

        public void Print()
        {
            var fgSnap = Console.ForegroundColor;

            Console.ForegroundColor = _color;
            Console.WriteLine(ToString());

            Console.ForegroundColor = fgSnap;
        }

        private void ConvertToMatrix()
        {
            var offset = 0;

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    _matrix[i, j] = _bytes[offset] >= _drawThreshold ? 0 : 1;
                    offset += 4; //advance 4 bytes (r, g, b, opacity)
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    sb.Append(GetCharAt(i, j, _matrix[i, j]));
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private char GetCharAt(int x, int y, int val)
        {
            if (_options.Effect == CalliopeEffect.ScanlineGradient)
            {
                var line = (int)(((double)x / Height) * 8);
                return (val, line) switch
                {
                    (0, _) => _spaceChar,
                    (_, 0) => '\u2588',
                    (_, 1) => '\u2587',
                    (_, 2) => '\u2586',
                    (_, 3) => '\u2585',
                    (_, 4) => '\u2584',
                    (_, 5) => '\u2583',
                    (_, 6) => '\u2582',
                    (_, 7) => '\u2581',
                };
            }

            return val == 0  ? _spaceChar : _drawChar;
        }

        private void DrawText()
        {
            var skInfo = new SKImageInfo(Width, Height);
            using var surface = SKSurface.Create(skInfo);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // draw left-aligned text, solid
            using (var paint = new SKPaint())
            {
                paint.TextSize = FontSize;
                paint.IsAntialias = _antiAliasing;
                paint.Color = SKColors.Black;
                paint.IsStroke = false;
                paint.Typeface = SKTypeface.FromFamilyName(FontName);

                canvas.DrawText(BannerText, 2, FontSize, paint);
                canvas.Flush();
            }
            canvas.Scale(1, -1, 0, 0);
            canvas.Save();

            using var image = surface.Snapshot();
            var bmp = SKBitmap.FromImage(image);

            var bytes = bmp.Bytes.Where(x => x != 255).ToArray();
            var notWhite = bmp.Pixels.Where(x => x != SKColors.White).ToArray();

            _bytes = bmp.Bytes;
        }

        public string BannerText { get; }
        public int Width { get; }
        public int Height { get; }
        public string FontName { get; }
        public int FontSize { get; }

        private ConsoleColor _color;
        private byte[] _bytes;
        private readonly char _drawChar;
        private readonly char _spaceChar;
        private readonly byte _drawThreshold;
        private readonly bool _antiAliasing;
        private readonly int[,] _matrix;
        private readonly CalliopeOptions _options;
    }
}
