using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
using Carahsoft.Calliope.AnsiConsole;
using System.Text;

Console.ForegroundColor = ConsoleColor.Blue;
Console.OutputEncoding = Encoding.UTF8;


//await Calliope.PrintAnimatedEffect(CalliopeAnimation.TwinkleAnimation("GALACTIC CRUSH", new CalliopeOptions
//{
//    Effect = CalliopeEffect.None,
//    Font = "Comic Sans MS",
//    Height = 20,
//    Width = Console.BufferWidth,
//    FontSize = 18
//}));

//await Calliope.PrintAnimatedEffect(CalliopeAnimation.RainbowAnimation("Calliope", new CalliopeOptions
//{
//    Effect = CalliopeEffect.None,
//    Font = "Comic Sans MS",
//    Width = 100,
//    Height = 40,
//    DrawChar = '\u2588',
//    FontSize = 28
//}));

await Calliope.PrintAnimatedEffect(CalliopeAnimation.RainAnimation("Calliope", new CalliopePrintOptions
{
    Effect = CalliopeEffect.None,
    Font = "Comic Sans MS",
    Width = 100,
    Height = 34,
    FontSize = 28
}));


public class TickMsg : CalliopeMsg { }

public class AnimatedChar
{
    private readonly char _initChar;
    private readonly int _initThreshold;
    private readonly Random _random;

    public AnimatedChar(char initChar)
    {
        _initChar = initChar;
        _random = new Random();
        _initThreshold = _random.Next(100);
    }

    public char View(int frame)
    {
        if (_initThreshold > frame)
        {
            /*
            var pick = r.Next(5);
            var renderChar = pick switch
            {
                0 => '*',
                1 => '$',
                2 => '#',
                3 => '@',
                4 => '&'
            };
            */
            var pick = _random.Next(2);
            var renderChar = pick switch
            {
                0 => '0',
                1 => '1',
            };
            return renderChar;
        }
        else
        {
            return _initChar;
        }
    }
}

public class FadeInChar
{
    private readonly char _initChar;
    private readonly int _initThreshold = 100;

    public FadeInChar(char initChar)
    {
        _initChar = initChar;
    }

    public string View(int frame)
    {
        var pct = (decimal)frame / _initThreshold;
        var grn = (byte)(pct * 148);
        var blu = (byte)(pct * 20);
        var red = (byte)(pct * 20);

        return AnsiTextHelper.ColorText(_initChar.ToString(), new() { Red = red, Green = grn, Blue = blu });
    }
}
