using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Example_2;

internal class Program
{
    static async Task Main(string[] args)
    {
        var startTime = DateTime.Now;

        /*        var item1 = new Item("Ball", "Green, medium size");
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

                *//*for (int i = 0; i < 10000; i++)
                {*//*
                    cache.AddItems(item1, item2, item3, item4 *//*item5*//*);
                *//*}*//*

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
        */

        var cache = new MemoryCache();
        var storage = new StorageJSON(@"D:\Practice-Tasks\Example 2\Storage Folder\Storage.txt");

        await Menu.ShowMenu(cache, storage);

        var finishTime = DateTime.Now - startTime;
        Console.WriteLine($"Total time: {(finishTime).TotalMilliseconds} ms");
        Console.ReadKey();
    }

    
}
 