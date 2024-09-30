using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.AnsiConsole
{
    internal class WindowsConsole
    {
        private const int STD_OUT = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void SetWindowsConsoleMode()
        {
            var handle = GetStdHandle(STD_OUT);
            uint mode;
            GetConsoleMode(handle, out mode);
            if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != ENABLE_VIRTUAL_TERMINAL_PROCESSING)
            {
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, mode);
            }
        }
    }
}
