using Carahsoft.Calliope.AnsiConsole;
using System.Text;

namespace Carahsoft.Calliope
{
    public class Style
    {
        public RgbColor? ForegroundColor { get; private set; }
        public RgbColor? BackgroundColor { get; private set; }
        public bool IsBold { get; private set; }
        public bool IsItalic { get; private set; }
        public bool IsUnderline { get; private set; }
        public bool IsStrikethrough { get; private set; }

        public Style() { }

        private Style(RgbColor? foreground, RgbColor? background, bool bold, bool italic, bool underline, bool strikethrough)
        {
            ForegroundColor = foreground;
            BackgroundColor = background;
            IsBold = bold;
            IsItalic = italic;
            IsUnderline = underline;
            IsStrikethrough = strikethrough;
        }

        public Style WithForegroundColor(RgbColor color)
        {
            return new Style(color, BackgroundColor, IsBold, IsItalic, IsUnderline, IsStrikethrough);
        }

        public Style WithForegroundColor(AdaptiveColor color)
        {
            return new Style(color.GetColor(), BackgroundColor, IsBold, IsItalic, IsUnderline, IsStrikethrough);
        }

        public Style WithBackgroundColor(RgbColor color)
        {
            return new Style(ForegroundColor, color, IsBold, IsItalic, IsUnderline, IsStrikethrough);
        }

        public Style WithBackgroundColor(AdaptiveColor color)
        {
            return new Style(ForegroundColor, color.GetColor(), IsBold, IsItalic, IsUnderline, IsStrikethrough);
        }

        public Style WithBold(bool bold = true)
        {
            return new Style(ForegroundColor, BackgroundColor, bold, IsItalic, IsUnderline, IsStrikethrough);
        }

        public Style WithItalic(bool italic = true)
        {
            return new Style(ForegroundColor, BackgroundColor, IsBold, italic, IsUnderline, IsStrikethrough);
        }

        public Style WithUnderline(bool underline = true)
        {
            return new Style(ForegroundColor, BackgroundColor, IsBold, IsItalic, underline, IsStrikethrough);
        }

        public Style WithStrikethrough(bool strikethrough = true)
        {
            return new Style(ForegroundColor, BackgroundColor, IsBold, IsItalic, IsUnderline, strikethrough);
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
            if (IsBold)
            {
                sb.Append("1");
                hasStyles = true;
            }
            
            if (IsItalic)
            {
                if (hasStyles) sb.Append(";");
                sb.Append("3");
                hasStyles = true;
            }
            
            if (IsUnderline)
            {
                if (hasStyles) sb.Append(";");
                sb.Append("4");
                hasStyles = true;
            }
            
            if (IsStrikethrough)
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
        public static Style Default => new Style();
        
        public static Style Bold => new Style().WithBold();
        
        public static Style Italic => new Style().WithItalic();
        
        public static Style Underline => new Style().WithUnderline();

        public static Style Red => Default.WithForegroundColor(RgbColors.Red);
        
        public static Style Green => Default.WithForegroundColor(RgbColors.Green);
        
        public static Style Blue => Default.WithForegroundColor(RgbColors.Blue);
        
        public static Style Yellow => Default.WithForegroundColor(RgbColors.Yellow);
        
        public static Style Cyan => Default.WithForegroundColor(RgbColors.Cyan);
        
        public static Style Magenta => Default.WithForegroundColor(RgbColors.Magenta);
        
        public static Style White => Default.WithForegroundColor(RgbColors.White);
        
        public static Style Black => Default.WithForegroundColor(RgbColors.Black);

        // Convenience methods for creating styles
        public static Style FromColor(RgbColor color) => Default.WithForegroundColor(color);
        
        public static Style FromColor(AdaptiveColor color) => Default.WithForegroundColor(color);
        
        public static Style FromColors(RgbColor foreground, RgbColor background) => 
            Default.WithForegroundColor(foreground).WithBackgroundColor(background);
        
        public static Style FromColors(AdaptiveColor foreground, RgbColor background) => 
            Default.WithForegroundColor(foreground).WithBackgroundColor(background);
        
        public static Style FromColors(RgbColor foreground, AdaptiveColor background) => 
            Default.WithForegroundColor(foreground).WithBackgroundColor(background);
        
        public static Style FromColors(AdaptiveColor foreground, AdaptiveColor background) => 
            Default.WithForegroundColor(foreground).WithBackgroundColor(background);
    }
}