// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Calliope.Print("Calliope", new CalliopePrintOptions
{
    Font = "Trebuchet MS",
    Width = 70,
    Height = 14,
    StartColor = RgbColors.Magenta,
    FontSize = 12,
    AntiAliasing = false
});

Calliope.Print("Calliope", new CalliopePrintOptions
{
    Font = "Trebuchet MS",
    Width = 100,
    Height = 20,
    StartColor = RgbColors.Blue,
    FontSize = 16,
    AntiAliasing = false,
    DrawChar = '\u2588'
});

Calliope.Print("Calliope", new CalliopePrintOptions
{
    Font = "Trebuchet MS",
    Width = 100,
    Height = 20,
    StartColor = RgbColors.Blue,
    FontSize = 16,
    AntiAliasing = false,
    Effect = CalliopeEffect.ScanlineGradient
});

Calliope.Print("Calliope", new CalliopePrintOptions
{
    Font = "Trebuchet MS",
    Width = 65,
    Height = 14,
    StartColor = RgbColors.Red,
    FontSize = 12,
    AntiAliasing = false,
    Effect = CalliopeEffect.Phoenix
});

Calliope.Print("UNICORN", new CalliopePrintOptions
{
    Font = "Fira Code NF",
    Width = 70,
    Height = 14,
    StartColor = RgbColors.Red,
    FontSize = 12,
    AntiAliasing = true,
    Effect = CalliopeEffect.Unicorn
});