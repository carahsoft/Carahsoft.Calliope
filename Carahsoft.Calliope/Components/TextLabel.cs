using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Components
{
    public enum SizeKind
    {
        Relative = 0, // default if none is selected
        Absolute,
    };

    public struct SizeSpecification
    {
        public int Width, Height;
        public SizeKind WidthKind, HeightKind;
    }

    /// <summary>
    /// Display some text, possibly cropped to a width
    /// </summary>
    public class TextLabel : ICalliopeProgram
    {
        public SizeSpecification Size;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (Size.WidthKind == SizeKind.Absolute)
                { 
                    Size.Width = text.Length; 
                }
            }
        }
        private string text = "";
        public RgbColor? Color;
        public RgbColor? Background;

        public CalliopeCmd? Init()
        {
            return null;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is WindowSizeChangeMsg wscm)
            {
                if (Size.WidthKind == SizeKind.Relative)
                {
                    Size.Width = wscm.ScreenWidth;
                }
            }
            return null;
        }

        public string View()
        {
            string output = Text;

            if (Text.Length > Size.Width)
                output = Text.Substring(0, Size.Width);
            else
                output = Text.PadRight(Size.Width);

            return AnsiTextHelper.ColorText(output, Color, Background);
        }
    }
}
