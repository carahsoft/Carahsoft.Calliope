using Carahsoft.Calliope;
using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Messages;
using System.Text;
using TextEffectExample;

Console.ForegroundColor = ConsoleColor.Green;
Console.OutputEncoding = Encoding.UTF8;

/*
await Calliope.PrintEffect("twinkle", new CalliopeOptions
{
    Effect = CalliopeEffect.Twinkle,
    Font = "Trebuchet MS",
    Height = 20,
    Width = 100,
    FontSize = 18
});
*/

await Calliope.NewProgram(new TextEffect()).RunAsync();

public record TextEffectState
{
    public int Frame { get; init; }
}

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

public class TextEffect : ICalliopeProgram<TextEffectState>
{
    private readonly List<List<RainbowChar>> _renderComponents;

    public TextEffect()
    {
        var renderLines = Calliope.PrintString("carahsoft", new CalliopeOptions
        {
            Effect = CalliopeEffect.None,
            Font = "Trebuchet MS",
            Width = 100,
            Height = 20,
            DrawChar = '\u2588'
        }).Replace("\r\n", "\n").Split('\n');

        _renderComponents = renderLines.Select(line => line.Select((x, i) => new RainbowChar(x, i * 6)).ToList()).ToList();
    }

    public (TextEffectState, CalliopeCmd?) Init()
    {
        return (new(), Tick());
    }

    public (TextEffectState, CalliopeCmd?) Update(TextEffectState state, CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
            {
                return (state, CalliopeCmd.Quit);
            }
        }
        if (msg is TickMsg)
        {
            if (state.Frame > 100000)
                return (state, CalliopeCmd.Quit);

            return (state with { Frame = state.Frame + 2 }, Tick());
        }

        return (state, null);
    }

    public string View(TextEffectState state)
    {
        var sb = new StringBuilder();
        foreach (var line in _renderComponents)
        {
            foreach (var c in line)
            {
                sb.Append(c.View(state.Frame));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private CalliopeCmd Tick()
    {
        return CalliopeCmd.Make(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1) / 60);
            return new TickMsg();
        });
    }
}
