// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Calliope.Print(
    "carahsoft",
    font: "Trebuchet MS",
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
    //Font = "Ubuntu Mono",
    Font = "Trebuchet MS",
    Width = 100,
    Height = 20,
    FontStartColor = ConsoleColor.Blue,
   // ConsoleBackgroundColor = ConsoleColor.DarkBlue,
    FontSize = 16,
    AntiAliasing = false,
    DrawChar = '\u2588'
});

Calliope.Print("carahsoft", new CalliopeOptions
{
    //Font = "Ubuntu Mono",
    Font = "Trebuchet MS",
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
    Font = "Trebuchet MS",
    Width = 65,
    Height = 14,
    FontStartColor = ConsoleColor.Red,
    FontSize = 12,
    AntiAliasing = false,
    Effect = CalliopeEffect.Phoenix
});

Calliope.Print("UNICORN", new CalliopeOptions
{
    Font = "Fira Code NF",
    Width = 70,
    Height = 14,
    FontStartColor = ConsoleColor.Red,
    FontSize = 12,
    AntiAliasing = true,
    Effect = CalliopeEffect.Unicorn
});