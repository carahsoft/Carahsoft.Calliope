using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Components
{
    public class LoadingSpinner : ICalliopeProgram
    {
        public const string DotFrames = "⣽⣾⣷⣯⣟⡿⢿⣻";
        public const string DotSpinFrames = "⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏";
        public const string SingleDotFrames = "⠈⠐⠠⢀⡀⠄⠂⠁";
        public const string DashFrames = @"\|/-";

        private int _frame;
        private bool _spinning;

        public string? Frames { get; set; }
        public RgbPixel? Color { get; set; }
        public TimeSpan? TimeBetweenFrames { get; set; }

        public CalliopeCmd? Init()
        {
            return Spin();
        }

        public CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is SpinMsg && _spinning)
            {
                _frame++;
                return Spin();
            }

            return null;
        }

        public string View()
        {
            if (_spinning)
            {
                return GetFrame(Frames ?? DashFrames);
            }

            return "";
        }

        public CalliopeCmd Spin()
        {
            _spinning = true;
            return CalliopeCmd.Make(async () =>
            {
                if (TimeBetweenFrames != null)
                    await Task.Delay(TimeBetweenFrames.Value);
                else
                    await Task.Delay(100);

                return new SpinMsg();
            });
        }

        public void StopSpin()
        {
            _spinning = false;
            _frame = 0;
        }

        private string GetFrame(string frames)
        {
            var frameChar = frames[_frame % frames.Length].ToString();
            if (Color != null)
                return AnsiTextHelper.ColorText(frameChar, Color.Value);
            return frameChar;
        }

        private class SpinMsg : CalliopeMsg { }
    }
}
