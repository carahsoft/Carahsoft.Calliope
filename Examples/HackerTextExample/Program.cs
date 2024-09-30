// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.ForegroundColor = ConsoleColor.Green;
var mywriter = new HackerWriter(Console.Out);
Console.SetOut(mywriter);

Calliope.Print("HACKERMAN", new CalliopePrintOptions() { Effect = CalliopeEffect.Phoenix, Height = 20, Width = 120, StartColor = RgbColors.Green });

Calliope.Print("carahsoft", new CalliopePrintOptions() { Effect = CalliopeEffect.ScanlineGradient, Height = 20, Width = 90, StartColor = RgbColors.Green });

