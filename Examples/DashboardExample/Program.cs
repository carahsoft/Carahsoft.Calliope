// A small dashboard showing the components working together: a table that refreshes itself,
// a confirm prompt, a form with a dropdown in a popup, a : command line and a help window.
// The components are immutable state records; the program keeps the current ones and replaces
// them with the result of each update.

using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Table;
using Carahsoft.Calliope.Text;

await Calliope.NewProgram(new Dashboard()).Fullscreen().RunAsync();

public record Service(string Name, string Environment, int Replicas, int Ready);

public class RefreshMsg : CalliopeMsg { }

public class LoadedMsg(IReadOnlyList<Service> services) : CalliopeMsg
{
    public IReadOnlyList<Service> Services { get; } = services;
}

public class Dashboard : ICalliopeProgram
{
    private static readonly Theme Theme = Theme.Default;
    private static readonly string[] Environments = ["dev", "test", "staging", "prod"];

    private readonly List<Service> _services =
    [
        new("billing", "prod", 3, 3),
        new("search", "prod", 2, 1),
        new("notifications", "staging", 1, 1),
        new("reports", "dev", 1, 0),
    ];

    private DataGridState<Service> _grid = DataGrid.Create(
        new DataGridColumn<Service>("NAME", 16, s => s.Name) { Grow = true },
        new DataGridColumn<Service>("ENVIRONMENT", 12, s => s.Environment),
        new DataGridColumn<Service>("READY", 6, s => $"{s.Ready}/{s.Replicas}") { Align = ColumnAlignment.Right, SortKey = s => s.Ready })
        with
        {
            RowKey = s => s.Name,
            Loading = true,
            RowStyle = s => s.Ready == 0 ? Theme.Error : null,
        };

    private CommandLineState _commandLine = CommandLine.Create(
        new CommandLineCommand("quit", "quit the dashboard") { Aliases = ["q"] },
        new CommandLineCommand("refresh", "reload the services") { Aliases = ["r"] },
        new CommandLineCommand("help", "show the keys and commands") { Aliases = ["h"] });

    private ConfirmState? _confirm;
    private FormState? _form;
    private bool _showHelp;
    private string? _notice;
    private DateTime? _loadedAt;
    private int _width = 80;
    private int _height = 24;

    public CalliopeCmd? Init() =>
        CalliopeCmd.Combine(Load(), CalliopeCmd.Every(TimeSpan.FromSeconds(5), () => new RefreshMsg()).WithKey("auto-refresh"));

    // Pretends to call an API. Keyed, so a refresh started while one is running replaces it.
    private CalliopeCmd Load()
    {
        var snapshot = _services.ToList();
        return CalliopeCmd.Make(async token =>
        {
            await Task.Delay(300, token);
            return (CalliopeMsg)new LoadedMsg(snapshot);
        }).WithKey("load");
    }

    public CalliopeCmd? Update(CalliopeMsg msg)
    {
        switch (msg)
        {
            case WindowSizeChangeMsg size:
                _width = size.ScreenWidth;
                _height = size.ScreenHeight;
                // Title, box borders, hints and status take 5 lines; the header takes one more
                _grid = DataGrid.SetViewHeight(_grid, _height - 6);
                return null;
            case RefreshMsg:
                return Load();
            case LoadedMsg loaded:
                _grid = DataGrid.SetRows(_grid, loaded.Services) with { Loading = false };
                _loadedAt = DateTime.Now;
                return null;
            case KeyPressMsg key:
                return OnKey(key);
        }
        return null;
    }

    private CalliopeCmd? OnKey(KeyPressMsg key)
    {
        if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            return CalliopeCmd.Quit;

        _notice = null;

        if (_showHelp)
        {
            _showHelp = false;
            return null;
        }

        if (_confirm is { } confirm)
        {
            confirm = Confirm.Update(confirm, key);
            _confirm = confirm.Result == ConfirmResult.Pending ? confirm : null;
            if (confirm.Result == ConfirmResult.Confirmed && DataGrid.Selected(_grid) is { } service)
            {
                _services.Remove(service);
                _notice = $"Deleted {service.Name}";
                return Load();
            }
            return null;
        }

        if (_form is { } form)
        {
            form = Form.Update(form, key);
            _form = form.Status == FormStatus.Editing ? form : null;
            if (form.Status == FormStatus.Submitted)
            {
                var values = form.Values!;
                var replicas = (int)(long)values["replicas"]!;
                _services.Add(new Service((string)values["name"]!, (string)values["environment"]!, replicas, 0));
                _notice = $"Added {values["name"]}";
                return Load();
            }
            return null;
        }

        if (CommandLine.Update(_commandLine, key) is { } commandLine)
        {
            _commandLine = commandLine;
            _notice = commandLine.Message;
            return commandLine.Invoked?.Name switch
            {
                "quit" => CalliopeCmd.Quit,
                "refresh" => Load(),
                "help" => ShowHelp(),
                _ => null,
            };
        }

        switch (key.KeyChar)
        {
            case 'q':
                return CalliopeCmd.Quit;
            case '?':
                return ShowHelp();
            case 'r':
                return Load();
            case 'n':
                _form = Form.Create(
                    FormField.Text("name") with { Required = true, Description = "A name for the service, e.g. payments" },
                    FormField.Choice("environment", Environments, "dev") with { Description = "Press space to choose from the list" },
                    FormField.Integer("replicas", 1) with { Required = true, Description = "How many copies to run" });
                return null;
            case 'd' when DataGrid.Selected(_grid) is { } service:
                _confirm = Confirm.Create("Delete", $"Delete {service.Name}?");
                return null;
        }

        if (DataGrid.Update(_grid, key) is { } grid)
            _grid = grid;
        return null;
    }

    private CalliopeCmd? ShowHelp()
    {
        _showHelp = true;
        return null;
    }

    public string View()
    {
        var inner = _width - 4;
        var title = Frame.Bar(Theme.Selected.WithBold(), _width, "Dashboard", $"{_services.Count} services");
        var body = Frame.Box("Services", _width, DataGrid.Render(_grid, inner, _height - 5, _form == null && _confirm == null, Theme));

        var hints = _commandLine.IsOpen
            ? CommandLine.Render(_commandLine, _width, Theme)
            : " " + string.Join("  ", new[] { ("n", "new"), ("d", "delete"), ("s", "sort"), ("r", "refresh"), (":", "command"), ("?", "help"), ("q", "quit") }
                .Select(h => Theme.Key.Apply(h.Item1) + " " + Theme.Dim.Apply(h.Item2)));

        var status = _commandLine.IsOpen ? CommandLine.RenderSuggestions(_commandLine, _width - 10, Theme)
            : _notice != null ? Theme.Warning.Apply(_notice)
            : _loadedAt is { } at ? Theme.Dim.Apply($"Updated {at:T}")
            : "Loading…";
        var position = DataGrid.Position(_grid);
        var statusLine = Ansi.PadRight(" " + status, _width - position.Length - 1) + " " + Theme.Dim.Apply(position);

        IReadOnlyList<string> lines = [title, .. body, Ansi.Truncate(hints, _width), statusLine];

        if (_confirm is { } confirm)
        {
            lines = Confirm.Render(confirm, _width, _height, lines, Theme);
        }
        else if (_form is { } form)
        {
            var content = Block.JoinVertical(Form.Render(form, 56, Theme), Form.Description(form, 56, Theme));
            var popup = Window.Popup(_width, _height, "New service", Form.Footer, content, lines, Theme.Dialog);
            lines = popup.Lines;

            // The open dropdown floats over the popup, so the popup doesn't grow to fit it
            if (Form.RenderDropdown(form, 56, Theme) is { } dropdown)
                lines = Window.FloatBelow(_height, dropdown, lines, popup.ContentRow, popup.ContentColumn);
        }

        if (_showHelp)
        {
            string[] keys =
            [
                Theme.Key.Apply("↑/↓ j/k") + "  move",
                Theme.Key.Apply("←/→    ") + "  select a column",
                Theme.Key.Apply("s      ") + "  sort by it",
                "",
                Theme.Label.Apply("Commands"),
            ];
            lines = Window.Popup(_width, _height, "Keys", "any key to close", [.. keys, .. CommandLine.RenderHelp(_commandLine, Theme)], lines, Theme.Dialog).Lines;
        }

        return string.Join('\n', lines);
    }
}
