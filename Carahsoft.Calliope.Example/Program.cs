// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Calliope.Print(
    "carahsoft",
    font: "Ubuntu Mono",
    width: 70,
    height: 14,
    fontColor: ConsoleColor.Magenta,
    //drawChar: '\u2588',
    //spaceChar: '　',
    fontSize: 11,
    drawThreshold: 255,
    antiAliasing: false
);

Calliope.Print("carahsoft", new CalliopeOptions
{
    Font = "Ubuntu Mono",
    Width = 100,
    Height = 20,
    FontStartColor = ConsoleColor.Blue,
   // ConsoleBackgroundColor = ConsoleColor.DarkBlue,
    FontSize = 16,
    AntiAliasing = false,
    Effect = CalliopeEffect.ScanlineGradient
});

Calliope.Print("CARAHSOFT", new CalliopeOptions
{
    Font = "Ubuntu Mono",
    Width = 70,
    Height = 14,
    FontStartColor = ConsoleColor.Red,
    FontSize = 12,
    AntiAliasing = false,
    Effect = CalliopeEffect.Phoenix
});

Calliope.Print("CARAHSOFT", new CalliopeOptions
{
    Font = "Courier New",
    Width = 70,
    Height = 14,
    FontStartColor = ConsoleColor.Red,
    FontSize = 12,
    AntiAliasing = false,
    Effect = CalliopeEffect.Unicorn
});