using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var program = Calliope.NewProgram(new LoadingBarExample());

await program.RunAsync();


public class LoadingBarExample : ICalliopeProgram
{
    private readonly LoadingBar _lb;

    public LoadingBarExample()
    {
        _lb = new LoadingBar(new LoadingBarOptions
        {
            GradientStart = new RgbPixel { Red = 45, Green = 156, Blue = 218 },
            GradientEnd = new RgbPixel { Red = 255, Green = 99, Blue = 88 }
        });
    }

    public CalliopeCmd? Init()
    {
        _lb.Percent = 0;
        return null;
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
            {
                return CalliopeCmd.Quit;
            }
            if (kpm.Key == ConsoleKey.RightArrow)
            {
                var updatedPercent = _lb.Percent + 5;
                _lb.Percent = updatedPercent > 100 ? 100 : updatedPercent;
                return null;
            }
            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                var updatedPercent = _lb.Percent - 5;
                _lb.Percent = updatedPercent < 0 ? 0 : updatedPercent;
                return null;
            }
        }

        return _lb.Update(msg);
    }

    public string View()
    {
        return $"""
            Gradient loading bar example: press right arrow to progress the
            loading bar, and left arrow to decrease progress.
            {_lb.View()}
            """;
    }
}
