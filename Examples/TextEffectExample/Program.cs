using Carahsoft.Calliope;
using Carahsoft.Calliope.Animations;
using Carahsoft.Calliope.AnsiConsole;
using System.Text;
using TextEffectExample;

await Calliope.NewProgram(new TextEffectPicker()).Fullscreen().RunAsync();

/*
Console.ForegroundColor = ConsoleColor.Blue;
Console.OutputEncoding = Encoding.UTF8;


//await Calliope.PrintAnimatedEffect(CalliopeAnimation.TwinkleAnimation("GALACTIC CRUSH", new CalliopeOptions
//{
//    Effect = CalliopeEffect.None,
//    Font = "Comic Sans MS",
//    Height = 20,
//    Width = Console.BufferWidth,
//    FontSize = 18
//}));

await Calliope.PrintAnimatedEffect(CalliopeAnimation.RainbowAnimation("Calliope", new CalliopePrintOptions
{
    Effect = CalliopeEffect.None,
    Font = "Comic Sans MS",
    Width = 100,
    Height = 34,
    DrawChar = '\u2588',
    FontSize = 28
}));

await Calliope.PrintAnimatedEffect(CalliopeAnimation.RainAnimation("Calliope", new CalliopePrintOptions
{
    Effect = CalliopeEffect.None,
    Font = "Comic Sans MS",
    Width = 100,
    Height = 34,
    FontSize = 28
}));

*/
