using System;
using System.Diagnostics;

namespace Example_2;

internal class Program
{
    static async Task Main(string[] args)
    {
        var startTime = DateTime.Now;

        var item1 = new Item("Ball", "Green, medium size");
        var item2 = new Item("Box", "Yellow, medium size, paper");
        var item3 = new Item("Plate", "White");
        var item4 = new Item("Ring", "Golden");
        var item5 = new Item("Cube", "Big, steel, gray");

        Console.WriteLine("List of created items:");
        Console.WriteLine(item1);
        Console.WriteLine(item2);
        Console.WriteLine(item3);
        Console.WriteLine(item4);

        var cache = new MemoryCache();

        Console.WriteLine("\nTry to add all 4 items to cache:");

        /*for (int i = 0; i < 10000; i++)
        {*/
            cache.AddItems(item1, item2, item3, item4 /*item5*/);
        /*}*/

        Console.WriteLine("\nTry find item with simpleID = 3 in cache:");
        Console.WriteLine(cache.FindItem(3));

        Console.WriteLine("\nTry find item by name (Ring) in cache:");
        Console.WriteLine(cache.FindItem("Ring"));

        Console.WriteLine("\nTry find item by description (White) in cache:");
        Console.WriteLine(cache.FindItem("White"));

        Console.WriteLine("\nTry find item by description (Black) in cache:");
        Console.WriteLine(cache.FindItem("Black"));

        Console.WriteLine("\nTry find item with simpleID = 5 in cache:");
        Console.WriteLine(cache.FindItem(5));

        Console.WriteLine("\nTry to remove item with simpleID = 2 from cache:");
        cache.RemoveItem(2);

        Console.WriteLine("\nTry to remove item with simpleID = 5 from cache:");
        cache.RemoveItem(5);

        Console.WriteLine("\nTry to ADD item AGAIN TO cache:");
        cache.AddItem(item2);

        Console.WriteLine("\nTry to rewrite item 2 in cache:");
        cache.RewriteItem(2, "Cube", "Big, steel, gray");

        

        Console.ReadKey();
        Console.Clear();

        Console.WriteLine("\nTry to create storage:");
        var storage = new StorageJSON(@"D:\Practice-Tasks\Example 2\Storage Folder\JSONStorage.txt");

        Console.WriteLine("\nTry to save cache to storage:");
        await storage.SaveCache(cache.ReturnCache());

        Console.WriteLine("\nTry to find item with simpleID = 3 in storage:");
        Console.WriteLine(storage.FindItemAsunc(3).Result);

        Console.WriteLine("\nTry to find not existed item with simpleID = 2575 in storage:");
        Console.WriteLine(storage.FindItemAsunc(2575).Result);

        Console.WriteLine("\nTry to save item5 to storage:");
        await storage.SaveItem(item5);

        Console.WriteLine("\nTry to find item with simpleID = 5 in storage:");
        Console.WriteLine(storage.FindItemAsunc(5).Result);

        Console.WriteLine("\nTry to find not existed item with simpleID = 2575 in storage:");
        Console.WriteLine(storage.FindItemAsunc(2575).Result);

        Console.WriteLine("\nTry to find not existed item with simpleID = 2575 in storage:");
        Console.WriteLine(storage.FindItemAsunc(2575).Result);

        Console.WriteLine("\nTry to find not existed item with simpleID = 2575 in storage:");
        Console.WriteLine(storage.FindItemAsunc(2575).Result);

        Console.ReadKey();
        Console.Clear();

        Console.WriteLine($"\nTry to load items from storage: {storage.StoragePath}");
        var loadedList = await Loader.LoadAsync(storage.StoragePath);
        foreach (var item in loadedList)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine($"\nTry to load items from storage: {storage.StoragePath} to cache:");
        await cache.RewriteCacheWithDataFromStorage(storage.StoragePath);
        
        cache.ShowCachedItems();

        Console.WriteLine($"\nCurrent log cache: {CombineLoger._logCache.ToString()}");

        var finishTime = DateTime.Now - startTime;
        Console.WriteLine($"Total time: {(finishTime).TotalMilliseconds} ms");
        Console.ReadKey();
    }

    // The main entry point of the program.
    // The `async` keyword allows the use of `await` for asynchronous operations.
    static async Task Main(string[] args)
    {
        // Define the file path for the storage. This needs to be a valid path on your system.
        const string storagePath = @"D:\Practice-Tasks\Example 2\Storage Folder\JSONStorage.txt";

        // Initialize the MemoryCache and StorageJSON objects.
        var cache = new MemoryCache();
        var storage = new StorageJSON(storagePath);

        // A list of items to be used for testing.
        var testItems = new List<Item>
        {
            new("Ball", "Green, medium size"),
            new("Box", "Yellow, medium size, paper"),
            new("Plate", "White"),
            new("Ring", "Golden"),
            new("Cube", "Big, steel, gray")
        };

        // Display a welcome message and the menu to the user.
        Console.WriteLine("Welcome to the Cache and Storage Test Console!");
        await ShowMenu(cache, storage, testItems);
    }

    // Displays the main menu and handles user input.
    private static async Task ShowMenu(MemoryCache cache, StorageJSON storage, List<Item> testItems)
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("           TESTING MENU");
            Console.WriteLine("======================================");
            Console.WriteLine("1.  Add test items to cache");
            Console.WriteLine("2.  Show all items in cache");
            Console.WriteLine("3.  Find an item in cache by SimpleID");
            Console.WriteLine("4.  Find an item in cache by Name or Description");
            Console.WriteLine("5.  Remove an item from cache by SimpleID");
            Console.WriteLine("6.  Rewrite an item in cache by SimpleID");
            Console.WriteLine("7.  Save cache to storage");
            Console.WriteLine("8.  Load storage to cache");
            Console.WriteLine("9.  Find an item in storage by SimpleID");
            Console.WriteLine("10. Add a new item manually to cache");
            Console.WriteLine("0.  Exit");
            Console.WriteLine("======================================");

            Console.Write("Enter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("Adding test items to cache...");
                    cache.AddItems(testItems.ToArray());
                    Console.ReadKey();
                    break;
                case "2":
                    Console.Clear();
                    Console.WriteLine("Current items in cache:");
                    cache.ShowCachedItems();
                    Console.ReadKey();
                    break;
                case "3":
                    Console.Clear();
                    Console.Write("Enter SimpleID to find in cache: ");
                    if (int.TryParse(Console.ReadLine(), out int simpleIdToFind))
                    {
                        var item = cache.FindItem(simpleIdToFind);
                        Console.WriteLine(item != null ? $"Found item: {item}" : "Item not found.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid integer.");
                    }
                    Console.ReadKey();
                    break;
                case "4":
                    Console.Clear();
                    Console.Write("Enter Name or Description to find in cache: ");
                    string? descriptionToFind = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(descriptionToFind))
                    {
                        var item = cache.FindItem(descriptionToFind);
                        Console.WriteLine(item != null ? $"Found item: {item}" : "Item not found.");
                    }
                    else
                    {
                        Console.WriteLine("Input cannot be empty.");
                    }
                    Console.ReadKey();
                    break;
                case "5":
                    Console.Clear();
                    Console.Write("Enter SimpleID to remove from cache: ");
                    if (int.TryParse(Console.ReadLine(), out int simpleIdToRemove))
                    {
                        cache.RemoveItem(simpleIdToRemove);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid integer.");
                    }
                    Console.ReadKey();
                    break;
                case "6":
                    Console.Clear();
                    Console.Write("Enter SimpleID of item to rewrite: ");
                    if (int.TryParse(Console.ReadLine(), out int simpleIdToRewrite))
                    {
                        Console.Write("Enter new name: ");
                        string? newName = Console.ReadLine();
                        Console.Write("Enter new description: ");
                        string? newDescription = Console.ReadLine();
                        cache.RewriteItem(simpleIdToRewrite, newName ?? "", newDescription ?? "");
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid integer.");
                    }
                    Console.ReadKey();
                    break;
                case "7":
                    Console.Clear();
                    Console.WriteLine("Saving current cache to storage...");
                    await storage.SaveCache(cache.ReturnCache());
                    Console.ReadKey();
                    break;
                case "8":
                    Console.Clear();
                    Console.WriteLine("Loading items from storage into cache...");
                    await cache.RewriteCacheWithDataFromStorage(storage.StoragePath);
                    Console.WriteLine("Loading complete. Cache items:");
                    cache.ShowCachedItems();
                    Console.ReadKey();
                    break;
                case "9":
                    Console.Clear();
                    Console.Write("Enter SimpleID to find in storage: ");
                    if (int.TryParse(Console.ReadLine(), out int simpleIdToFindInStorage))
                    {
                        var item = await storage.FindItemAsunc(simpleIdToFindInStorage);
                        Console.WriteLine(item != null ? $"Found item: {item}" : "Item not found in storage.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid integer.");
                    }
                    Console.ReadKey();
                    break;
                case "10":
                    Console.Clear();
                    Console.Write("Enter a name for the new item: ");
                    string? newItemName = Console.ReadLine();
                    Console.Write("Enter a description for the new item: ");
                    string? newItemDescription = Console.ReadLine();
                    var newItem = new Item(newItemName ?? "New Item", newItemDescription ?? "");
                    cache.AddItem(newItem);
                    Console.WriteLine($"Added new item: {newItem}");
                    Console.ReadKey();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.Clear();
                    Console.WriteLine("Invalid option. Press any key to try again.");
                    Console.ReadKey();
                    break;
            }
        }

        Console.WriteLine("Exiting program. Goodbye!");
    }
}
 