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
                new() { Value = "Twinkle" }
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
            if (Selection == null)
            {
                return _picker.View();
            }
            if (Selection == "Rainbow")
            {
                var renderComponents = _renderLines
                    .Select(line => line.Select((x, i) => new RainbowChar(x, i * 6)).ToList())
                    .ToList();
                var sb = new StringBuilder();
                foreach (var line in renderComponents)
                {
                    foreach (var c in line)
                    {
                        sb.Append(c.View(Frame));
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return "";
        }
    }
}
