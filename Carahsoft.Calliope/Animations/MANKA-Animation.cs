using Carahsoft.Calliope.AnsiConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    /// <summary>
    /// Pander to judges
    /// </summary>
    public class MANKA_Animation : CalliopeAnimation
    {
        private string Render { get; set; }

        private List<char[]> WordMap { get; set; }
        private string rendered { get; set; }

        private (int, int) CurrentBlock { get; set; }

        private int DefenderX { get; set; }

        private bool left = false;

        private Point? interceptPt = null;

        private int? InvaderY = null;

        private int? ProjectileY = null;

        private Random _rand { get; set; }

        private bool end = false;

        private string _lastRenderState;

        private int _redIdx = 0;

        private bool ending = false;

        public MANKA_Animation(string renderText, MANKAPrintOptions options)
            : base(renderText, options)
        {
            Options.Effect = CalliopeEffect.None;
            DefenderX = options.Width / 2;
            _rand = new Random();
            var swThread = new Thread(() => PlayImperialMarch());
            swThread.SetApartmentState(ApartmentState.STA);
            swThread.Start();
        }

        public override CalliopeCmd? Init()
        {
            end = false;
            rendered = Calliope.PrintString(RenderText, Options);
            WordMap = new List<char[]>();
            var lines = rendered.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);

            WordMap = new List<char[]>();
            foreach (var line in lines)
            {
                WordMap.Add(line.ToCharArray());
            }
            Render = GenerateText();
            return Invade();
        }

        public override CalliopeCmd? Update(CalliopeMsg msg)
        {
            if (msg is KeyPressMsg kpm)
            {
                if (kpm.Key == ConsoleKey.C && kpm.Modifiers == ConsoleModifiers.Control)
                {
                    return CalliopeCmd.Quit;
                }
            }

            if (msg is InvadeMsg)
            {
                Render = GenerateText();
                return Invade();
            }

            return null;
        }

        public override string View()
        {
            return Render;
        }

        private string GenerateText()
        {
            StringBuilder sb = new StringBuilder();
            if (_lastRenderState == null)
            {
                int i = 0;
                int j = 0;

                if (ending)
                    Thread.Sleep(50);

                if (interceptPt == null)
                {
                    var cur = WordMap.FirstOrDefault(x => x.Any(x => x != ' ' && x != 'X'));
                    if (cur == null && !end)
                    {
                        ending = true;
                        interceptPt = new Point
                        {
                            X = DefenderX,
                            Y = WordMap.Count + 1
                        };
                        goto est;
                    }
                    var ct = cur.Count(x => x != ' ' && x != 'X');

                    var skipped = 0;
                    var skip = _rand.Next(0, ct);
                    var xcoord = 0;
                    for (int x = 0; x < cur.Length; x++)
                    {
                        if (cur[x] != ' ' && cur[x] != 'X')
                        {
                            if (skip == skipped)
                            {
                                xcoord = x;
                                break;
                            }
                            else
                            {
                                skipped++;
                            }
                        }
                    }

                    interceptPt = new Point
                    {
                        X = xcoord,
                        Y = WordMap.IndexOf(cur)
                    };
                }

            est:

                if (InvaderY == null)
                {
                    InvaderY = 0;
                }
                else
                {
                    InvaderY++;
                }


                if (interceptPt.Y != WordMap.Count + 1 && Math.Abs(interceptPt.Y - InvaderY.Value) <= Math.Abs(WordMap.Count - interceptPt.Y))
                    ProjectileY = interceptPt?.Y + Math.Abs(interceptPt!.Y - InvaderY.Value);

                foreach (var line in WordMap)
                {
                    foreach (var c in line)
                    {
                        if (DefenderX == interceptPt?.X && interceptPt?.Y == WordMap.Count + 1 && interceptPt?.X == i && j == WordMap.Count - 1 && InvaderY == WordMap.Count - 1)
                        {
                            sb.Append("💥");
                            interceptPt = null;
                            InvaderY = null;
                            ProjectileY = null;
                            end = true;
                        }
                        if (j == ProjectileY && interceptPt?.X == i && j == InvaderY && interceptPt.X == i)
                        {
                            sb.Append("💥");
                            WordMap[interceptPt.Y][interceptPt.X] = 'X';
                            interceptPt = null;
                            InvaderY = null;
                            ProjectileY = null;

                        }
                        else if (j == InvaderY && interceptPt?.X == i)
                        {
                            sb.Append("👾");
                        }
                        else if (j == ProjectileY && interceptPt?.X == i)
                        {
                            sb.Append('!');
                        }
                        else if (c == ' ')
                        {
                            sb.Append(' ');
                        }
                        else if (c == 'X')
                        {
                            sb.Append(AnsiTextHelper.ColorText('▀'.ToString(), new((byte)255, (byte)232, (byte)31)));
                        }
                        else
                        {
                            sb.Append(' ');
                        }
                        i++;
                    }
                    i = 0;
                    j++;
                    sb.Append('\n');
                    continue;
                }

                int? moves = null;
                int? dist = null;
                int inc = 0;
                if (interceptPt != null && InvaderY != null)
                {
                    dist = Math.Abs(interceptPt.X - DefenderX);
                    moves = Math.Abs(interceptPt.Y - InvaderY.Value);
                    if (dist * 2 > moves)
                    {
                        inc = 3;
                    }
                    else if (Math.Abs(interceptPt.X - DefenderX) == 1)
                    {
                        inc = 1;
                    }
                    else
                    {
                        inc = 2;
                    }
                }

                if (!end)
                {
                    sb.Append('\n');
                    bool placed = false;
                    for (int x = 0; x <= Options.Width; x++)
                    {
                        if (interceptPt == null || InvaderY == null)
                        {
                            if (x == DefenderX && !placed)
                            {
                                sb.Append("👨‍💻");
                                placed = true;
                            }
                            else
                            {
                                sb.Append(' ');
                            }
                            continue;
                        }

                        if (interceptPt.X == DefenderX && x == DefenderX && !placed)
                        {
                            sb.Append("👨‍💻");
                            placed = true;
                        }
                        else if (interceptPt.X < DefenderX && x == DefenderX + inc && !placed)
                        {
                            sb.Append("👨‍💻");
                            DefenderX = EnforceBounds(DefenderX - inc);
                            placed = true;
                        }
                        else if (interceptPt.X > DefenderX && x == DefenderX - inc && !placed)
                        {
                            sb.Append("👨‍💻");
                            DefenderX = EnforceBounds(DefenderX + inc);
                            placed = true;
                        }
                        else
                        {
                            sb.Append(' ');
                        }
                    }
                }

                int EnforceBounds(int i)
                {
                    return Math.Max(1, Math.Min(100, i));
                }


                if (end)
                    _lastRenderState = sb.ToString();
                return sb.ToString();
            }
            else if (_redIdx < WordMap.Count + 1)
            {
                Thread.Sleep(100);
                var state = _lastRenderState.Split("\n");
                StringBuilder line = new StringBuilder();
                for(int x = 0; x < Options.Width; x++)
                {
                    line.Append(AnsiTextHelper.ColorText('▀'.ToString(), new((byte)255, (byte)0, (byte)0)));
                }
                state[_redIdx++] = line.ToString();
                _lastRenderState = string.Join("\n", state);
                return _lastRenderState;
            }
            else
            {
                Environment.Exit(1);
                return null;
            }
        }

        private CalliopeCmd Invade()
        {
            return CalliopeCmd.Make(async () =>
            {
                await Task.Delay(1);
                return new InvadeMsg();
            });
        }

        private class InvadeMsg : CalliopeMsg { }

        private class Point
        {
            public int X, Y;
        }

        static async Task PlayImperialMarch(bool skip = false)
        {
            // Define the notes and durations based on the provided `beep` command
            (int frequency, int duration)[] notes = new (int frequency, int duration)[]
            {
            (392, 700), (392, 700), (392, 700),
            (311, 500), (466, 50),
            (392, 700), (311, 500), (466, 50),
            (392, 1400), (587, 700), (587, 700), (587, 700),
            (622, 500), (466, 50),
            (370, 700), (311, 500), (466, 50),
            (392, 1400), (784, 700), (392, 500), (392, 50),
            (784, 700), (740, 500), (698, 50), (659, 50), (622, 50),
            (659, 100), (415, 50), (554, 700), (523, 500),
            (494, 50), (466, 50), (440, 50),
            (466, 100), (311, 50), (370, 700), (311, 500),
            (392, 50), (466, 700), (392, 500), (466, 50),
            (587, 1400), (784, 700), (392, 500), (392, 50),
            (784, 700), (740, 500), (698, 50), (659, 50), (622, 50),
            (659, 100), (415, 50), (554, 700), (523, 500),
            (494, 50), (466, 50), (440, 50),
            (466, 100), (311, 50), (392, 700), (311, 500),
            (466, 50), (392, 600), (311, 500), (466, 50),
            (392, 1400)
            };

            for (int i = 1; i < notes.Length; i++)
            {
                if (notes[i].duration == 50)
                {
                    notes[i].duration *= 4; // Quadruple the duration
                }
            }

            // Play each note in the sequence
            foreach (var (frequency, duration) in notes)
            {
                Console.Beep(frequency, duration);
                await Task.Delay(100); // Short pause between notes
            }

            await Task.Delay(1000);
            PlayImperialMarch();
        }
    }
}
