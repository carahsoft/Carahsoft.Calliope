// See https://aka.ms/new-console-template for more information

using Carahsoft.Calliope;
using Carahsoft.Calliope.Components;

var program2 = Calliope.NewProgram(new FilterableSelectList([
        new() { Value = "Option 1!" },
        new() { Value = "Option 2!" },
        new() { Value = "Option 3!" },
        new() { Value = "Option 4!" },
        new() { Value = "Option 5!" },
        new() { Value = "Option 6!" },
        new() { Value = "Option 7!" },
        new() { Value = "Option 8!" },
        new() { Value = "Option 9!" },
        new() { Value = "Option 10!" },
        new() { Value = "Option 11!" },
        new() { Value = "Option 12!" },
        new() { Value = "Option 13!" },
        new() { Value = "Option 14!" },
        new() { Value = "Option 15!" },
        new() { Value = "Option 16!" },
        new() { Value = "Option 17!" },
        new() { Value = "Option 18!" },
        new() { Value = "Option 19!" },
        new() { Value = "Option 20!" },
        new() { Value = "Option 21!" },
        new() { Value = "Option 22!" },
        new() { Value = "Secret option!!" },
        new() { Value = "Don't pick this" },
]));

var choice = await program2.RunAsync();
Console.WriteLine("You chose " + choice.SelectListState.Choice);
