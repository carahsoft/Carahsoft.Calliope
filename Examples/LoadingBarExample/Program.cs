using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var program2 = Calliope.NewProgram(new SelectList(new SelectListProps
{
    Items =
    [
        new() { Value = "Option 1!" },
        new() { Value = "Option 2!" },
        new() { Value = "Option 3!" },
        new() { Value = "Option 4!" },
        new() { Value = "Option 5!" },
        new() { Value = "Option 6!" },
        new() { Value = "Option 7!" },
        new() { Value = "Option 8!" },
        new() { Value = "Option 9!" },
        new() { Value = "Option 10!" },
        new() { Value = "Option 11!" },
        new() { Value = "Option 12!" },
        new() { Value = "Option 13!" },
        new() { Value = "Option 14!" },
        new() { Value = "Option 15!" },
        new() { Value = "Option 16!" },
        new() { Value = "Option 17!" },
        new() { Value = "Option 18!" },
        new() { Value = "Option 19!" },
        new() { Value = "Option 20!" },
        new() { Value = "Option 21!" },
        new() { Value = "Option 22!" },
        new() { Value = "Secret option!!" },
        new() { Value = "Don't pick this" },
    ]
}));

var choice = await program2.RunAsync();
Console.WriteLine("You chose " + choice.Choice);

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
