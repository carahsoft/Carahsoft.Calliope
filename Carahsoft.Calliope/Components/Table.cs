using Carahsoft.Calliope.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class Table : ICalliopeProgram
    {
        private readonly DataTable _table;
        private ScrollView _sv = new();

        public ScrollView ScrollView { get { return _sv; } }

        public Table(DataTable table)
        {
            _table = table;
        }

        public CalliopeCmd? Init()
        {
            _sv.RenderView = RenderTable();
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

        private string RenderTable()
        {
            var ct = new ConsoleTable(_table);
            return ct.ToString();
        }
    }
}
