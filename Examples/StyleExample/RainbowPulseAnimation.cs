using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
using System.Text;

namespace StyleExample
{
    public class RainbowPulseAnimation : CalliopeAnimation
    {
        private string _render;
        private int _colorPhase;
        private bool _pulseDirection = true;
        private int _pulseIntensity = 0;
        private readonly RgbColor[] _rainbowColors;

        public RainbowPulseAnimation(string bannerText, CalliopePrintOptions options) : base(bannerText, options)
        {
            Options.Effect = CalliopeEffect.None;
            Options.DrawChar = CalliopeCharacters.FullBlock;
            _render = string.Empty;
            _colorPhase = 0;
            
            // Define rainbow colors
            _rainbowColors = new[]
            {
                new RgbColor(255, 0, 0),     // Red
                new RgbColor(255, 127, 0),   // Orange
                new RgbColor(255, 255, 0),   // Yellow
                new RgbColor(0, 255, 0),     // Green
                new RgbColor(0, 0, 255),     // Blue
                new RgbColor(75, 0, 130),    // Indigo
                new RgbColor(148, 0, 211)    // Violet
            };
        }

        public override CalliopeCmd? Init()
        {
            _render = Calliope.PrintString(RenderText, Options);
            return Animate();
        }

        public override CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return CalliopeCmd.Quit;
                }
                if (kpm.Key == ConsoleKey.Escape)
                {
                    return CalliopeCmd.Quit;
                }
            }

            if (msg is AnimateMsg)
            {
                // Update color phase for rainbow effect
                _colorPhase = (_colorPhase + 1) % _rainbowColors.Length;
                
                // Update pulse intensity
                if (_pulseDirection)
                {
                    _pulseIntensity += 15;
                    if (_pulseIntensity >= 255)
                    {
                        _pulseIntensity = 255;
                        _pulseDirection = false;
                    }
                }
                else
                {
                    _pulseIntensity -= 15;
                    if (_pulseIntensity <= 100)
                    {
                        _pulseIntensity = 100;
                        _pulseDirection = true;
                    }
                }
                
                return Animate();
            }

            return null;
        }

        public override string View()
        {
            if (string.IsNullOrEmpty(_render))
                return "";

            var sb = new StringBuilder();
            var lines = _render.Replace("\r\n", "\n").Split('\n');
            
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                if (string.IsNullOrEmpty(line))
                {
                    sb.AppendLine();
                    continue;
                }

                var styledLine = new StringBuilder();
                
                for (int charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    var character = line[charIndex];
                    
                    // Skip spaces and maintain them as-is
                    if (character == ' ')
                    {
                        styledLine.Append(character);
                        continue;
                    }
                    
                    // Calculate color based on position and phase
                    var colorIndex = (charIndex + lineIndex + _colorPhase) % _rainbowColors.Length;
                    var baseColor = _rainbowColors[colorIndex];
                    
                    // Apply pulse effect by modulating brightness
                    var pulsedColor = new RgbColor(
                        (byte)(baseColor.Red * _pulseIntensity / 255),
                        (byte)(baseColor.Green * _pulseIntensity / 255),
                        (byte)(baseColor.Blue * _pulseIntensity / 255)
                    );
                    
                    // Create style with the pulsed color and apply it
                    var style = Style.Default.WithForegroundColor(pulsedColor).WithBold();
                    styledLine.Append(style.Apply(character.ToString()));
                }
                
                sb.AppendLine(styledLine.ToString());
            }
            
            // Add instructions at the bottom
            var instructions = Style.Default
                .WithForegroundColor(new RgbColor(128, 128, 128))
                .WithItalic()
                .Apply("Press ESC or Ctrl+C to return to demo");
            
            sb.AppendLine();
            sb.AppendLine(instructions);
            
            return sb.ToString();
        }

        private CalliopeCmd Animate()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(100); // Animation speed
                return new AnimateMsg();
            });
        }

        private class AnimateMsg : CalliopeMsg { }
    }
}