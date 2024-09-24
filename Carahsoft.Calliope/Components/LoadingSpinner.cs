using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Components
{
    public class LoadingSpinner : ICalliopeProgram
    {
        private const string BRAILLE_LOADER = "⣽⣾⣷⣯⣟⡿⢿⣻";

        private int _frame;
        private bool _spinning;

        public CalliopeCmd? Init()
        {
            return Spin();
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is SpinMsg && _spinning)
            {
                _frame++;
                if (_frame >= BRAILLE_LOADER.Length)
                {
                    _frame = 0;
                }
                return Spin();
            }

            return null;
        }

        public string View()
        {
            if (_spinning)
                return BRAILLE_LOADER[_frame].ToString();

            return "";
        }

        public CalliopeCmd Spin()
        {
            _spinning = true;
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(100);
                return new SpinMsg();
            });
        }

        public void StopSpin()
        {
            _spinning = false;
            _frame = 0;
        }

        private class SpinMsg : CalliopeMsg { }
    }
}
