using Carahsoft.Calliope.AnsiConsole;
using System.Text;

namespace Carahsoft.Calliope
{
    public class Style
    {
        public RgbColor? ForegroundColor { get; private set; }
        public RgbColor? BackgroundColor { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public bool Underline { get; private set; }
        public bool Strikethrough { get; private set; }

        public Style() { }

        private Style(RgbColor? foreground, RgbColor? background, bool bold, bool italic, bool underline, bool strikethrough)
        {
            ForegroundColor = foreground;
            BackgroundColor = background;
            Bold = bold;
            Italic = italic;
            Underline = underline;
            Strikethrough = strikethrough;
        }

        public Style WithForegroundColor(RgbColor color)
        {
            return new Style(color, BackgroundColor, Bold, Italic, Underline, Strikethrough);
        }

        public Style WithBackgroundColor(RgbColor color)
        {
            return new Style(ForegroundColor, color, Bold, Italic, Underline, Strikethrough);
        }

        public Style WithBold(bool bold = true)
        {
            return new Style(ForegroundColor, BackgroundColor, bold, Italic, Underline, Strikethrough);
        }

        public Style WithItalic(bool italic = true)
        {
            return new Style(ForegroundColor, BackgroundColor, Bold, italic, Underline, Strikethrough);
        }

        public Style WithUnderline(bool underline = true)
        {
            return new Style(ForegroundColor, BackgroundColor, Bold, Italic, underline, Strikethrough);
        }

        public Style WithStrikethrough(bool strikethrough = true)
        {
            return new Style(ForegroundColor, BackgroundColor, Bold, Italic, Underline, strikethrough);
        }

        public string Apply(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder();
            
            // Start ANSI sequence
            sb.Append("\x1b[");
            
            var hasStyles = false;

            // Text formatting
            if (Bold)
            {
                sb.Append("1");
                hasStyles = true;
            }
            
            if (Italic)
            {
                if (hasStyles) sb.Append(";");
                sb.Append("3");
                hasStyles = true;
            }
            
            if (Underline)
            {
                if (hasStyles) sb.Append(";");
                sb.Append("4");
                hasStyles = true;
            }
            
            if (Strikethrough)
            {
                if (hasStyles) sb.Append(";");
                sb.Append("9");
                hasStyles = true;
            }

            // Foreground color
            if (ForegroundColor.HasValue)
            {
                if (hasStyles) sb.Append(";");
                sb.Append($"38;2;{ForegroundColor.Value.Red};{ForegroundColor.Value.Green};{ForegroundColor.Value.Blue}");
                hasStyles = true;
            }

            // Background color
            if (BackgroundColor.HasValue)
            {
                if (hasStyles) sb.Append(";");
                sb.Append($"48;2;{BackgroundColor.Value.Red};{BackgroundColor.Value.Green};{BackgroundColor.Value.Blue}");
                hasStyles = true;
            }

            if (!hasStyles)
            {
                return text;
            }

            sb.Append("m");
            sb.Append(text);
            sb.Append("\x1b[0m");
            
            return sb.ToString();
        }

        public string ApplyGradient(string text, RgbColor endColor)
        {
            if (string.IsNullOrEmpty(text) || !ForegroundColor.HasValue)
                return Apply(text);

            return AnsiTextHelper.ColorTextGradient(text, ForegroundColor.Value, endColor, BackgroundColor);
        }

        // Static factory methods for common styles
        public static Style DefaultStyle => new Style();
        
        public static Style BoldStyle => new Style().WithBold();
        
        public static Style ItalicStyle => new Style().WithItalic();
        
        public static Style UnderlineStyle => new Style().WithUnderline();

        public static Style Red => DefaultStyle.WithForegroundColor(RgbColors.Red);
        
        public static Style Green => DefaultStyle.WithForegroundColor(RgbColors.Green);
        
        public static Style Blue => DefaultStyle.WithForegroundColor(RgbColors.Blue);
        
        public static Style Yellow => DefaultStyle.WithForegroundColor(RgbColors.Yellow);
        
        public static Style Cyan => DefaultStyle.WithForegroundColor(RgbColors.Cyan);
        
        public static Style Magenta => DefaultStyle.WithForegroundColor(RgbColors.Magenta);
        
        public static Style White => DefaultStyle.WithForegroundColor(RgbColors.White);
        
        public static Style Black => DefaultStyle.WithForegroundColor(RgbColors.Black);

        // Convenience methods for creating styles
        public static Style FromColor(RgbColor color) => DefaultStyle.WithForegroundColor(color);
        
        public static Style FromColors(RgbColor foreground, RgbColor background) => 
            DefaultStyle.WithForegroundColor(foreground).WithBackgroundColor(background);
    }
}