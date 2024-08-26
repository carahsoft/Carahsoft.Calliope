// See https://aka.ms/new-console-template for more information
using Carahsoft.Calliope;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var test = new Calliope(
    "Carahsoft",
    font: "Yu Gothic",
    width: 70,
    height: 14,
    fontColor: ConsoleColor.Magenta,
    drawChar: '\u2588',
    //spaceChar: '　',
    fontSize: 11,
    drawThreshold: 230 
);

test.Print();
