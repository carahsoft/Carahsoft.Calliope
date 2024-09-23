using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEffectExample
{
    public class TextEffectPicker : ICalliopeProgram
    {
        private readonly FilterableSelectList _picker = new FilterableSelectList(
            [
                new() { Value = "Rainbow" },
                new() { Value = "Matrix" },
                new() { Value = "Twinkle" },
                new() { Value = "Rain"}
            ]
        );
        private readonly string[] _renderLines;

        public string? Selection { get; init; }
        public int Frame { get; init; }

        public TextEffectPicker()
        {
            _renderLines = Calliope.PrintString("carahsoft", new CalliopeOptions
            {
                Effect = CalliopeEffect.None,
                Font = "Trebuchet MS",
                Width = 100,
                Height = 20,
                DrawChar = '\u2588'
            }).Replace("\r\n", "\n").Split('\n');
        }

        public CalliopeCmd? Init()
        {
            return null;
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            throw new NotImplementedException();
        }

        public string View()
        {

            return "";
        }
    }
}
