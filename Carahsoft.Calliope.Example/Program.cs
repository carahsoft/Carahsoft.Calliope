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
    Width = 70,
    Height = 14,
    FontColor = ConsoleColor.Magenta,
    FontSize = 11,
    AntiAliasing = false,
    Effect = CalliopeEffect.ScanlineGradient
});
