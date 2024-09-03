using SkiaSharp;
﻿using Carahsoft.Calliope.Renderers;
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
            _color = _options.FontStartColor;
            _drawChar = _options.DrawChar;
            _spaceChar = _options.SpaceChar;
            _drawThreshold = _options.DrawThreshold;
            _antiAliasing = _options.AntiAliasing;
            _bgcolor = _options.ConsoleBackgroundColor;
            _renderer = _options.Renderer ?? CalliopeRenderer.RendererFromEffect(_options.Effect);
            DrawText();
            
            _outputChars = new char[Width * Height];
            _matrix = new int[Height, Width];


            ConvertToMatrix();
            RemovePadding();
        }

        public void Print()
        {
            var fgSnap = Console.ForegroundColor;
            var bgSnap = Console.BackgroundColor;

            Console.ForegroundColor = _color;
            Console.BackgroundColor = _bgcolor;

            Console.WriteLine(ToString());

            Console.ForegroundColor = fgSnap;
            Console.BackgroundColor = bgSnap;
        }

        private void ConvertToMatrix()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var idx = (i * Width) + j;
                    var px = _pixels[idx];
                    _outputChars[idx] = _renderer.CharAtPoint(i, j, px, _options);
                }
            }

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

        private void RemovePadding()
        {
            List<int[]> lines = new();
            for (var i = 0; i < Height; i++)
            {
                var blankLine = true;
                int[] line = new int[Width];
                for (var j = 0; j < Width; j++)
                {
                    if (_matrix[i, j] > 0)
                    {
                        blankLine = false;
                        break;
                    }
                    line[j] = _matrix[i, j];
                }
                if (!blankLine)
                {
                    lines.Add(line);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            /*
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    sb.Append(GetCharAt(i, j, _matrix[i, j]));
                }
                sb.Append('\n');
            }
            */
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    sb.Append(_outputChars[(i * Width) + j]);
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
                paint.IsAntialias = _antiAliasing;
                paint.Color = SKColors.Black;
                paint.IsStroke = false;
                paint.Typeface = SKTypeface.FromFamilyName(FontName);

                canvas.DrawText(BannerText, 2, FontSize, paint);
                canvas.Flush();
            }
            canvas.Save();

            using var image = surface.Snapshot();
            var bmp = SKBitmap.FromImage(image);

            _pixels = bmp.Pixels
                .Select(x => new RgbPixel { Red = x.Red, Green = x.Green, Blue = x.Blue })
                .ToArray();
            _bytes = bmp.Bytes;
        }

        public string BannerText { get; }
        public int Width { get; }
        public int Height { get; }
        public string FontName { get; }
        public int FontSize { get; }

        private ConsoleColor _color;
        private ConsoleColor _bgcolor;
        private byte[] _bytes;
        private RgbPixel[] _pixels;
        private char[] _outputChars;
        private readonly char _drawChar;
        private readonly char _spaceChar;
        private readonly byte _drawThreshold;
        private readonly bool _antiAliasing;
        private readonly int[,] _matrix;
        private readonly CalliopeOptions _options;
        private readonly IRenderer _renderer;
    }

    public record RgbPixel
    {
        public byte Red { get; init; }
        public byte Green { get; init; }
        public byte Blue { get; init; }
    }
}
