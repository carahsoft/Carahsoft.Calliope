// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;

var table = new FullscreenTable(
    """
    X
    XX
    XXX
    XXXX
    XXXXX
    XXXXXX
    XXXXXXX
    XXXXXXXX
    XXXXXXXXX
    XXXXXXXXXX
    XXXXXXXXXXX
    XXXXXXXXXXXX
    XXXXXXXXXXXXX
    XXXXXXXXXXXXXX
    XXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXXX
    XXXXXXXXXXXXXXX
    XXXXXXXXXXXXXX
    XXXXXXXXXXXXX
    XXXXXXXXXXXX
    XXXXXXXXXXX
    XXXXXXXXXX
    XXXXXXXXX
    XXXXXXXX
    XXXXXXX
    XXXXXX
    XXXXX
    XXXX
    XXX
    XX
    X
    """);

await Calliope.NewProgram(table).Fullscreen().RunAsync();


public class FullscreenTable : ICalliopeProgram
{
    private readonly string _view;
    private readonly ScrollView _sv = new();

    public FullscreenTable(string view)
    {
        _view = view;
    }

    public CalliopeCmd? Init()
    {
        _sv.RenderView = _view;
        return null;
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        return _sv.Update(msg);
    }

    public string View()
    {
        return _sv.View();
    }
}
