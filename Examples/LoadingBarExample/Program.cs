using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var program = Calliope.NewProgram(new LoadingBarExample(new LoadingBarExampleProps(
    new RgbPixel { Red = 255, Green = 0, Blue = 255 }
)));

await program.RunAsync();


public record LoadingBarExampleProps(RgbPixel Color);

public class LoadingBarExample : ICalliopeProgram<LoadingBarState>
{
    private readonly LoadingBar _lb;

    public LoadingBarExample(LoadingBarExampleProps props)
    {
        _lb = new LoadingBar(new LoadingBarOptions
        {
            GradientStart = new RgbPixel { Red = 45, Green = 156, Blue = 218 },
            GradientEnd = new RgbPixel { Red = 255, Green = 99, Blue = 88 }
        });
    }

    public (LoadingBarState, CalliopeCmd?) Init()
    {
        return (new() { Percent = 0 }, null);
    }

    public (LoadingBarState, CalliopeCmd?) Update(LoadingBarState state, CalliopeMsg msg)
    {
        if (msg is QuitMsg)
            return (state, CalliopeCmd.Quit);

        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.RightArrow)
            {
                var updatedPercent = state.Percent + 5;
                return _lb.Update(state with { Percent = updatedPercent > 100 ? 100 : updatedPercent }, msg);
            }
            if (kpm.Key == ConsoleKey.LeftArrow)
            {
                var updatedPercent = state.Percent - 5;
                return _lb.Update(state with { Percent = updatedPercent < 0 ? 0 : updatedPercent }, msg);
            }
        }

        return _lb.Update(state, msg);
    }

    public string View(LoadingBarState state)
    {
        return _lb.View(state);
    }
}
