using Carahsoft.Calliope.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public record TableState
    {
        public ScrollViewState ScrollViewState { get; init; }
    }

    public class Table<T> : ICalliopeProgram<TableState>
    {
        private readonly DataTable _table;
        private ScrollView _sv = new();

        public Table(DataTable table)
        {
            _table = table;
        }

        public (TableState, CalliopeCmd?) Init()
        {
            return (new()
            {
                ScrollViewState = new()
                {
                    View = RenderTable()
                }
            }, null);
        }

        public (TableState, CalliopeCmd?) Update(TableState state, CalliopeMsg msg)
        {
            var (svState, svCmd) = _sv.Update(state.ScrollViewState, msg);

            return (state with { ScrollViewState = svState }, svCmd);
        }

        public string View(TableState state)
        {
            return _sv.View(state.ScrollViewState);
        }

        private string RenderTable()
        {
            var ct = new ConsoleTable<T>(_table);
            return ct.ToString();
        }
    }
}
