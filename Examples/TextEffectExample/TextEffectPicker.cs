using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
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
        private readonly MANKA_Animation _manka;

        public TextEffectPicker()
        {
            var mankaOptions = new MANKAPrintOptions
            {
                Font = "Trebuchet MS",
                Width = 100,
                Height = 50,
                RemovePadding = false
            };
            _manka = new MANKA_Animation("carahsoft", mankaOptions);
        }

        public CalliopeCmd? Init()
        {
            return CalliopeCmd.Combine(
                _manka.Init()
            );
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return CalliopeCmd.Quit;
                }
            }

            return CalliopeCmd.Combine(
                _manka.Update(msg)
            );
        }

        public string View()
        {
            return _manka.View();
        }
    }
}
