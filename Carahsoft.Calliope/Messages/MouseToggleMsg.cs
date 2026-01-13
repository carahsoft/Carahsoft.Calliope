namespace Carahsoft.Calliope
{
    /// <summary>
    /// Message to toggle mouse tracking on or off at runtime
    /// </summary>
    public class MouseToggleMsg : CalliopeMsg
    {
        public bool Enable { get; init; }
    }
}
