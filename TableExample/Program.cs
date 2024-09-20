// See https://aka.ms/new-console-template for more information

using Carahsoft.Calliope;
using Carahsoft.Calliope.Table;


List<Fruit> FruitCart = new List<Fruit>();

FruitCart.Add(new Fruit() { Type = "Apple", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Orange", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Grape", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Watermelon", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Dragon Fruit", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Cantelope", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Durian", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Avocado", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Strawberry", Quantity = 5, Price = 1.65m });


var myTable = new ConsoleTable<Fruit>(FruitCart);



Console.WriteLine("Table output:\n\n");
Console.WriteLine(myTable.ToString());

Console.WriteLine("\n\nPress Enter to see date in CSV Format:\n\n");
Console.ReadLine();

Console.WriteLine(myTable.ToCSV());

Console.WriteLine("\n\nPress Enter to see date in XML Format:\n\n");
Console.ReadLine();

Console.WriteLine(myTable.ToXML());


Console.WriteLine("\n\nPress Enter to see date in JSON Format:\n\n");
Console.ReadLine();

Console.WriteLine(myTable.ToJSON());


public class Fruit
{
    public string Type { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}