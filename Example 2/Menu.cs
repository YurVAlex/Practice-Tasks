namespace Example_2;

internal static class Menu
{
        // A list of items to be used for testing.
        public static List<Item> testItems =
        [
            new("Ball", "Green, medium size"),
            new("Box", "Yellow, medium size, paper"),
            new("Plate", "White"),
            new("Ring", "Golden"),
            new("Cube", "Big, steel, gray")
        ];

    public static async Task ShowMenu(MemoryCache cache, StorageJSON storage)
    {
        Console.WriteLine("Welcome to the Cache and Storage Test Console!\n ");

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
            Console.WriteLine("11. Send cache to URL");
            Console.WriteLine("0.  Exit");
            Console.WriteLine("======================================");

            Console.Write("Enter your choice: ");

            Console.WriteLine($"\n\nCurrent log cache:\n{CombineLoger._logCache.ToString()}");
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
                case "11":
                    Console.Clear();
                    Console.Write("Enter a URL to send cache data: ");
                    string? url = Console.ReadLine();
                    cache.SendCacheToUrlAsync(url);
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
