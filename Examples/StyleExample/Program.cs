using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;

await Calliope.NewProgram(new StyleDemoProgram()).RunAsync();

public class StyleDemoProgram : ICalliopeProgram
{
    private int _currentDemo = 0;
    private readonly string[] _demoNames = 
    {
        "Basic Colors",
        "Text Formatting", 
        "Styled Components",
        "Custom Colors",
        "Gradients"
    };

    public CalliopeCmd? Init()
    {
        return null;
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                return CalliopeCmd.Quit;

            if (kpm.Key == ConsoleKey.RightArrow || kpm.Key == ConsoleKey.N)
            {
                _currentDemo = (_currentDemo + 1) % _demoNames.Length;
                return null;
            }

            if (kpm.Key == ConsoleKey.LeftArrow || kpm.Key == ConsoleKey.P)
            {
                _currentDemo = (_currentDemo - 1 + _demoNames.Length) % _demoNames.Length;
                return null;
            }
        }

        return null;
    }

    public string View()
    {
        var title = Style.Bold.WithForegroundColor(new RgbColor(255, 215, 0))
            .Apply($"Style Demo - {_demoNames[_currentDemo]}");
        
        var navigation = Style.Italic.WithForegroundColor(new RgbColor(128, 128, 128))
            .Apply("Press ← → (or P/N) to navigate, Ctrl+C to quit");

        var demo = _currentDemo switch
        {
            0 => BasicColorsDemo(),
            1 => TextFormattingDemo(),
            2 => StyledComponentsDemo(),
            3 => CustomColorsDemo(),
            4 => GradientsDemo(),
            _ => "Unknown demo"
        };

        return $"""
        {title}
        {navigation}
        
        {demo}
        """;
    }

    private string BasicColorsDemo()
    {
        return $"""
        Basic Predefined Colors:
        
        {Style.Red.Apply("Red text")}
        {Style.Green.Apply("Green text")}
        {Style.Blue.Apply("Blue text")}
        {Style.Yellow.Apply("Yellow text")}
        {Style.Cyan.Apply("Cyan text")}
        {Style.Magenta.Apply("Magenta text")}
        
        With background colors:
        {Style.White.WithBackgroundColor(new RgbColor(255, 0, 0)).Apply("White on red")}
        {Style.Black.WithBackgroundColor(new RgbColor(0, 255, 0)).Apply("Black on green")}
        """;
    }

    private string TextFormattingDemo()
    {
        return $"""
        Text Formatting Options:
        
        {Style.Bold.Apply("Bold text")}
        {Style.Italic.Apply("Italic text")}
        {Style.Underline.Apply("Underlined text")}
        {Style.Default.WithStrikethrough().Apply("Strikethrough text")}
        
        Combined formatting:
        {Style.Bold.WithItalic().WithUnderline().WithForegroundColor(new RgbColor(255, 100, 50)).Apply("Bold italic underlined colored text")}
        """;
    }

    private string StyledComponentsDemo()
    {
        var selectList = new SelectList
        {
            Items = new List<SelectListItem>
            {
                new() { Value = "Option 1" },
                new() { Value = "Option 2" },
                new() { Value = "Option 3" },
            },
            SelectedStyle = Style.Bold.WithForegroundColor(new RgbColor(0, 255, 0)),
            ItemStyle = Style.Default.WithForegroundColor(new RgbColor(200, 200, 200))
        };

        var textInput = new TextInput
        {
            Text = "Styled input text",
            Style = Style.Default.WithForegroundColor(new RgbColor(100, 200, 255))
                .WithBackgroundColor(new RgbColor(30, 30, 50))
        };

        return $"""
        Styled Components Demo:
        
        Select List with custom styles:
        {selectList.View()}
        
        Text Input with custom style:
        > {textInput.View()}
        """;
    }

    private string CustomColorsDemo()
    {
        return $"""
        Custom RGB Colors:
        
        {Style.FromColor(new RgbColor(255, 100, 150)).Apply("Pink custom color")}
        {Style.FromColor(new RgbColor(100, 255, 100)).Apply("Light green custom color")}
        {Style.FromColor(new RgbColor(150, 100, 255)).Apply("Purple custom color")}
        
        Custom foreground and background:
        {Style.FromColors(new RgbColor(255, 255, 0), new RgbColor(128, 0, 128)).Apply("Yellow on purple")}
        {Style.FromColors(new RgbColor(0, 255, 255), new RgbColor(255, 100, 0)).Apply("Cyan on orange")}
        """;
    }

    private string GradientsDemo()
    {
        var startColor = new RgbColor(255, 0, 0);
        var endColor = new RgbColor(0, 0, 255);
        
        var gradientText = "This text transitions from red to blue!";
        var styledGradient = Style.FromColor(startColor).ApplyGradient(gradientText, endColor);
        
        return $"""
        Gradient Text Demo:
        
        {styledGradient}
        
        Another gradient example:
        {Style.FromColor(new RgbColor(255, 255, 0)).ApplyGradient("Yellow to cyan gradient", new RgbColor(0, 255, 255))}
        """;
    }
}