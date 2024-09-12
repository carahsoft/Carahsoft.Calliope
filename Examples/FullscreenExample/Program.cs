// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Messages;

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

await Calliope.NewProgram(table).RunAsync();


public record FullscreenTableState
{
    public required ScrollViewState ScrollViewState { get; set; }
}
public class FullscreenTable : ICalliopeProgram<FullscreenTableState>
{
    private readonly string _view;
    private readonly ScrollView _sv = new();

    public FullscreenTable(string view)
    {
        _view = view;
    }

    public (FullscreenTableState, CalliopeCmd?) Init()
    {
        return (new()
        {
            ScrollViewState = new()
            {
                View = _view,
            }
        }, null);
    }

    public (FullscreenTableState, CalliopeCmd?) Update(FullscreenTableState state, CalliopeMsg msg)
    {
        var (svState, svMsg) = _sv.Update(state.ScrollViewState, msg);

        return (state with { ScrollViewState = svState }, svMsg);
    }

    public string View(FullscreenTableState state)
    {
        return _sv.View(state.ScrollViewState);
    }
}
