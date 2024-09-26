using Carahsoft.Calliope;
using Carahsoft.Calliope.AnsiConsole;
using Carahsoft.Calliope.Components;
using System.Text;

await Calliope.NewProgram(new LoadingSpinnerExample()).RunAsync();

public class LoadingSpinnerExample : ICalliopeProgram
{
    private LoadingSpinner _spin = new() { Frames = LoadingSpinner.DotFrames };
    private int _selectedIndex = 1;

    public CalliopeCmd? Init()
    {
        return _spin.Init();
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        if (msg is KeyPressMsg kpm)
        {
            if (kpm.Key == ConsoleKey.D1)
            {
                _spin.Frames = LoadingSpinner.DotFrames;
                _selectedIndex = 1;
                return null;
            }
            if (kpm.Key == ConsoleKey.D2)
            {
                _spin.Frames = LoadingSpinner.DotSpinFrames;
                _selectedIndex = 2;
                return null;
            }
            if (kpm.Key == ConsoleKey.D3)
            {
                _spin.Frames = LoadingSpinner.SingleDotFrames;
                _selectedIndex = 3;
                return null;
            }
            if (kpm.Key == ConsoleKey.D4)
            {
                _spin.Frames = LoadingSpinner.DashFrames;
                _selectedIndex = 4;
                return null;
            }
        }

        return _spin.Update(msg);
    }

    public string View()
    {
        var helpSb = new StringBuilder();
        helpSb.Append(WrapIfSelected(nameof(LoadingSpinner.DotFrames), 1));
        helpSb.Append(WrapIfSelected(nameof(LoadingSpinner.DotSpinFrames), 2));
        helpSb.Append(WrapIfSelected(nameof(LoadingSpinner.SingleDotFrames), 3));
        helpSb.Append(WrapIfSelected(nameof(LoadingSpinner.DashFrames), 4));

        return $"""
            This is a spinner! Press 1-4 to cycle between different spinner types
            {helpSb}

            {_spin.View()}
            """;
    }

    private string WrapIfSelected(string name, int index)
    {
        if (index == _selectedIndex)
        {
            return AnsiTextHelper.ColorText($"({index}) {name} ", new(45, 156, 218));
        }
        else
        {
            return $"({index}) {name} ";
        }
    }
}

