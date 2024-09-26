using Carahsoft.Calliope.AnsiConsole;

namespace Carahsoft.Calliope.Components
{
    public class LoadingSpinner : ICalliopeProgram
    {
        public const string DotFrames = "⣽⣾⣷⣯⣟⡿⢿⣻";
        public const string DotSpinFrames = "⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏";
        public const string SingleDotFrames = "⠈⠐⠠⢀⡀⠄⠂⠁";
        public const string DashFrames = @"\|/-";

        private uint _frame;
        private bool _spinning;

        /// <summary>
        /// String of characters that correspond to the different frames
        /// displayed as the loading spinner. Defaults to <see cref="DashFrames"/>.
        /// The <see cref="LoadingSpinner"/> class has several other available
        /// default spinners, or you can supply your own here.
        /// </summary>
        public string? Frames { get; set; }
        /// <summary>
        /// If supplied, this is the color that the spinner will be rendered as.
        /// </summary>
        public RgbPixel? Color { get; set; }
        /// <summary>
        /// Defaults to 100ms
        /// </summary>
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
            var frameChar = frames[(int)(_frame % frames.Length)].ToString();
            if (Color != null)
                return AnsiTextHelper.ColorText(frameChar, Color.Value);
            return frameChar;
        }

        private class SpinMsg : CalliopeMsg { }
    }
}
