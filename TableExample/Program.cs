// See https://aka.ms/new-console-template for more information

using Carahsoft.Calliope;
using Carahsoft.Calliope.Table;


List<Fruit> FruitCart = new List<Fruit>();

FruitCart.Add(new Fruit() { Type = "Apple", Quantity = 5, Price = 1.65m });
FruitCart.Add(new Fruit() { Type = "Orange", Quantity = 6, Price = 1.84m });
FruitCart.Add(new Fruit() { Type = "Grape", Quantity = 43, Price = 1.23m });
FruitCart.Add(new Fruit() { Type = "Watermelon", Quantity = 1, Price = 8.08m });
FruitCart.Add(new Fruit() { Type = "Dragon Fruit", Quantity = 2, Price = 2.79m });
FruitCart.Add(new Fruit() { Type = "Cantelope", Quantity = 4, Price = 3.44m });
FruitCart.Add(new Fruit() { Type = "Durian", Quantity = 3, Price = 1.00m });
FruitCart.Add(new Fruit() { Type = "Avocado", Quantity = 1, Price = 4500.00m });
FruitCart.Add(new Fruit() { Type = "Strawberry", Quantity =85, Price = 1.00m });


var myTable = new ConsoleTable<Fruit>(FruitCart);



Console.WriteLine("Table output:\n\n");
Console.WriteLine(myTable.ToString());

Console.ReadLine();
Console.WriteLine("\n\nPress Enter to sort:\n\n");
myTable.Sort();
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