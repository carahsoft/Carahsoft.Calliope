using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
using System.Text;

await Calliope.PrintAnimatedEffect(new CustomAnimation("carahsoft", new CalliopePrintOptions
{
    Height = 28,
    Width = 100,
    Font = "Trebuchet MS",
    FontSize = 18
}));


public class CustomAnimation : CalliopeAnimation
{
    private string _render;
    private bool _blinkOn;

    public CustomAnimation(string bannerText, CalliopePrintOptions options) : base(bannerText, options)
    {
        Options.Effect = CalliopeEffect.None;
        Options.DrawChar = CalliopeCharacters.FullBlock;
        _render = string.Empty;
    }

    public override CalliopeCmd? Init()
    {
        _render = Calliope.PrintString(RenderText, Options);
        return Blink();
    }

    public override CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
            {
                return CalliopeCmd.Quit;
            }
        }

        if (msg is BlinkMsg)
        {
            _blinkOn = !_blinkOn;
            return Blink();
        }

        return null;
    }

    public override string View()
    {
        var sb = new StringBuilder();
        foreach (var line in _render.Replace("\r\n", "\n").Split('\n'))
        {
            if (!_blinkOn)
            {
                sb.AppendLine(Calliope.ColorText(line, RgbColors.CarahBlue));
            }
            else
            {
                sb.AppendLine(Calliope.ColorText(line, RgbColors.White));
            }
        }
        return sb.ToString();
    }

    private CalliopeCmd Blink()
    {
        return CalliopeCmd.Make(async () =>
        {
            await Task.Delay(1000);
            return new BlinkMsg();
        });
    }

    private class BlinkMsg : CalliopeMsg { }
}
