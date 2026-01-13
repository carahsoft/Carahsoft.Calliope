namespace Carahsoft.Calliope
{
    public class MouseWheelMsg : CalliopeMsg
    {
        public MouseWheelDirection Direction { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
    }

    public enum MouseWheelDirection
    {
        Up,
        Down
    }
}
