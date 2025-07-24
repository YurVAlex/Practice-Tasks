namespace Example_2;

internal class Program
{
    static void Main(string[] args)
    {
        var startTime = DateTime.Now;

        var item1 = new Item("Ball", "Green, medium size");
        var item2 = new Item("Box", "Yellow, medium size, paper");
        var item3 = new Item("Plate", "White");
        var item4 = new Item("Ring", "Golden");

        Console.WriteLine("List of created items:");
        Console.WriteLine(item1);
        Console.WriteLine(item2);
        Console.WriteLine(item3);
        Console.WriteLine(item4);

        ICache cache = new MemoryCache();

        Console.WriteLine("\nTry o add all items to cache:");
        cache.AddItems(item1, item2, item3, item4);


        Console.WriteLine("\nTry find item with simpleID = 3 in cache:");
        Console.WriteLine(cache.FindItem(3).Result);

        Console.WriteLine("\nTry find item by name (Ring) in cache:");
        Console.WriteLine(cache.FindItem("Ring").Result);

        Console.WriteLine("\nTry find item by description (White) in cache:");
        Console.WriteLine(cache.FindItem("White").Result);

        Console.WriteLine("\nTry find item by description (Black) in cache:");
        Console.WriteLine(cache.FindItem("Black").Result);

        Console.WriteLine("\nTry find item with simpleID = 5 in cache:");
        Console.WriteLine(cache.FindItem(5).Result);

        Console.WriteLine("\nTry to remove item with simpleID = 2 from cache:");
        cache.RemoveItem(2);

        Console.WriteLine("\nTry to remove item with simpleID = 5 from cache:");
        cache.RemoveItem(5);

        Console.WriteLine("\nTry to ADD item AGAIN TO cache:");
        cache.AddItem(item2);

        Console.WriteLine("\nTry to rewrite item 2 in cache:");
        cache.RewriteItem(2, "Cube", "Big, steel, gray");

        Console.WriteLine("\nTry to create storage:");
        var storage = new StorageJSON(@"D:\Practice-Tasks\Example 2\Storage Folder\JSONStorage.txt");

        Console.WriteLine("\nTry to save cache:");
        storage.SaveCache(cache.ReturnCache());

        var timeDifference = DateTime.Now - startTime;
        Console.WriteLine(timeDifference.Milliseconds.ToString());

        Console.ReadKey();
    }
}
 