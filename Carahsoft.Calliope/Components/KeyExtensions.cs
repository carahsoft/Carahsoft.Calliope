namespace Carahsoft.Calliope.Components
{
    internal static class KeyExtensions
    {
        public static bool Ctrl(this KeyPressMsg key) => key.Modifiers.HasFlag(ConsoleModifiers.Control);
        public static bool Shift(this KeyPressMsg key) => key.Modifiers.HasFlag(ConsoleModifiers.Shift);
        public static bool Alt(this KeyPressMsg key) => key.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}
