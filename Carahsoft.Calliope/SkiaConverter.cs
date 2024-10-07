using SkiaSharp;
﻿using Carahsoft.Calliope.Renderers;
using System.Text;
using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope
{
    internal class SkiaConverter
    {
        public SkiaConverter(
            string bannerText,
            CalliopePrintOptions? options = null)
        {
            _options = options ?? new CalliopePrintOptions();
            BannerText = bannerText;
            Width = _options.Width;
            Height = _options.Height;
            FontName = _options.Font;
            FontSize = _options.FontSize;
            _renderer = _options.Renderer ?? CalliopeRenderer.RendererFromEffect(_options.Effect);
        }

        public void Print()
        {
            Console.WriteLine(ToString());
        }

        private char[] ConvertToMatrix(RgbColor[] pixels)
        {
            var outputChars = new char[Width * Height];
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var idx = (i * Width) + j;
                    var px = pixels[idx];
                    outputChars[idx] = _renderer.CharAtPoint(i, j, px, _options);
                }
            }
            return outputChars;
        }

        private string RenderWithoutPadding()
        {
            var pixels = DrawText();
            var outputChars = ConvertToMatrix(pixels);
            var sb = new StringBuilder();
            if (_options.RemovePadding)
            {
                for (var i = 0; i < Height; i++)
                {
                    var blankLine = true;
                    char[] line = new char[Width];
                    for (var j = 0; j < Width; j++)
                    {
                        var cur = outputChars[(i * Width) + j];
                        if (cur != _options.SpaceChar)
                        {
                            blankLine = false;
                        }
                        line[j] = cur;
                    }

                    if (!blankLine)
                    {
                        foreach (var c in line)
                            sb.Append(c);
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                // Don't mind me there is a time crunch
                if (_options is MANKAPrintOptions)
                {
                    for (var i = 0; i < Height / 2; i++)
                    {
                        for (var j = 0; j < Width; j++)
                        {
                            sb.Append(' ');
                        }
                        sb.AppendLine();
                    }
                    for (var i = 0; i < Height/2; i++)
                    {
                        for (var j = 0; j < Width; j++)
                        {
                            var cur = outputChars[(i * Width) + j];
                             sb.Append(cur);
                        }
                        sb.AppendLine();
                    }
                }
                else
                {
                    for (var i = 0; i < Height; i++)
                    {
                        for (var j = 0; j < Width; j++)
                        {
                            var cur = outputChars[(i * Width) + j];
                            sb.Append(cur);
                        }
                        sb.AppendLine();
                    }
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            var render = RenderWithoutPadding();

            if (_options.StartColor != null)
            {
                if (_options.EndColor != null)
                {
                    return AnsiTextHelper.ColorTextGradient(
                        render,
                        _options.StartColor.Value,
                        _options.EndColor.Value,
                        _options.BackgroundColor);
                }
                return AnsiTextHelper.ColorText(
                    render,
                    _options.StartColor.Value,
                    _options.BackgroundColor);
            }

            return render;
        }

        private RgbColor[] DrawText()
        {
            var skInfo = new SKImageInfo(Width, Height);
            using var surface = SKSurface.Create(skInfo);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // draw left-aligned text, solid
            using (var paint = new SKPaint())
            {
                paint.TextSize = FontSize;
                paint.IsAntialias = _options.AntiAliasing;
                paint.Color = SKColors.Black;
                paint.IsStroke = false;
                paint.Typeface = SKTypeface.FromFamilyName(FontName);

                canvas.DrawText(BannerText, 2, FontSize, paint);
                canvas.Flush();
            }
            canvas.Save();

            using var image = surface.Snapshot();
            var bmp = SKBitmap.FromImage(image);

            return bmp.Pixels
                .Select(x => new RgbColor { Red = x.Red, Green = x.Green, Blue = x.Blue })
                .ToArray();
        }

        public string BannerText { get; }
        public int Width { get; }
        public int Height { get; }
        public string FontName { get; }
        public int FontSize { get; }

        private readonly CalliopePrintOptions _options;
        private readonly IRenderer _renderer;
    }


    /// <summary>
    /// Struct representing console colors available to print in
    /// consoles supporting 24 bit TrueColor
    /// </summary>
    public readonly record struct RgbColor
    {
        public RgbColor() { }

        public RgbColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Red { get; init; }
        public byte Green { get; init; }
        public byte Blue { get; init; }

        /// <summary>
        /// Implicitly convert from SKColor to RgbColor. Does not support
        /// alpha channel.
        /// </summary>
        /// <param name="color"></param>
        public static implicit operator RgbColor(SKColor color)
        {
            return new(color.Red, color.Green, color.Blue);
        }

        /// <summary>
        /// Explicitly convert to SKColor
        /// </summary>
        /// <param name="color"></param>
        public static explicit operator SKColor(RgbColor color)
        {
            return new(color.Red, color.Green, color.Blue);
        }
    }

    /// <summary>
    /// Named colors
    /// </summary>
    public static class RgbColors
    {
        public static RgbColor Red { get; } = new RgbColor(255, 0, 0);
        public static RgbColor Green { get; } = new RgbColor(0, 175, 0);
        public static RgbColor Blue { get; } = new RgbColor(0, 0, 255);
        public static RgbColor CarahBlue { get; } = new RgbColor(49, 85, 164);
        public static RgbColor White { get; } = new RgbColor(255, 255, 255);
        public static RgbColor Black { get; } = new RgbColor(0, 0, 0);
        public static RgbColor Grey { get; } = new RgbColor(100, 100, 100);
    }
}
