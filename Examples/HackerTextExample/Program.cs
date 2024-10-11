// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Calliope.EnableHackerText();
Calliope.Print("HACKERMAN", new CalliopePrintOptions()
{
    Effect = CalliopeEffect.Phoenix,
    Height = 20,
    Width = 120,
    StartColor = RgbColors.Green,
});

Calliope.Print("Calliope", new CalliopePrintOptions()
{
    Effect = CalliopeEffect.ScanlineGradient,
    Height = 20,
    Width = 90,
    StartColor = RgbColors.Green
});

Calliope.DisableHackerText();
