using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEffectExample
{
    public class TextEffectPicker : ICalliopeProgram<TextEffectPickerState>
    {
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

        private readonly SelectList _picker = new SelectList(new SelectListProps
        {
            Items =
            [
                new() { Value = "Rainbow" },
                new() { Value = "Matrix" },
                new() { Value = "Twinkle" }
            ]
        });
        private readonly string[] _renderLines;

        public (TextEffectPickerState, CalliopeCmd?) Init()
        {
            return (new(), null);
        }

        public (TextEffectPickerState, CalliopeCmd?) Update(TextEffectPickerState state, CalliopeMsg msg)
        {
            throw new NotImplementedException();
        }

        public string View(TextEffectPickerState state)
        {
            if (state.Selection == null)
            {
                return _picker.View(state.SelectListState);
            }
            if (state.Selection == "Rainbow")
            {
                var renderComponents = _renderLines
                    .Select(line => line.Select((x, i) => new RainbowChar(x, i * 6)).ToList())
                    .ToList();
                var sb = new StringBuilder();
                foreach (var line in renderComponents)
                {
                    foreach (var c in line)
                    {
                        sb.Append(c.View(state.Frame));
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return "";
        }
    }

    public record TextEffectPickerState
    {
        public SelectListState SelectListState { get; init; }
        public string? Selection { get; init; }
        public int Frame { get; init; }
    }
}
