using SkiaSharp;
using System.Text;

namespace Carahsoft.Calliope
{
    public class Calliope
    {
        public Calliope(
            string bannerText,
            int width = 120,
            int height = 28,
            string font = "Roboto",
            int fontSize = 18,
            ConsoleColor fontColor = ConsoleColor.DarkRed,
            char drawChar = '\u2580',
            char spaceChar = ' ',
            byte drawThreshold = 255)
        {
            BannerText = bannerText.ToLower();
            Width = width;
            Height = height;
            FontName = font;
            FontSize = fontSize;
            _color = fontColor;
            _drawChar = drawChar;
            _spaceChar = spaceChar;
            _drawThreshold = drawThreshold;
            DrawText();

            _matrix = new int[height, width];

            ConvertToMatrix();
        }

        public void Print()
        {
            Console.ForegroundColor = _color;
            Console.WriteLine(ToString());

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
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
                    sb.Append(_matrix[i, j] == 0 ? _spaceChar : _drawChar);
                }
                sb.Append('\n');
            }

            return sb.ToString();
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
                paint.IsAntialias = true;
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
        private readonly int[,] _matrix;
    }
}
