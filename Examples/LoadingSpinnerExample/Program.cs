using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;

await Calliope.NewProgram(new LoadingSpinnerExample()).RunAsync();

public class LoadingSpinnerExample : ICalliopeProgram
{
    LoadingSpinner _spin = new();

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
                return _spin.Spin();
            }
            if (kpm.Key == ConsoleKey.D2)
            {
                _spin.StopSpin();
                return null;
            }
        }

        return _spin.Update(msg);
    }

    public string View()
    {
        return $"""
            This is a spinner! Press 1 to start the spinner and 2 to stop the spinner.

            {_spin.View()}
            """;
    }
}

