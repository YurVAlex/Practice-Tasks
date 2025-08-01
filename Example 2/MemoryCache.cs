using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Example_2;

internal class MemoryCache
{
    private static List<Item> _items = [];

    public string Name { get; private set; }

    private static int CacheId = 0;

    // It is a best practice to reuse a single instance of HttpClient.
    private static readonly HttpClient _httpClient = new();

    public MemoryCache(string name = "")
    {
        CacheId++;

        if (string.IsNullOrWhiteSpace(name))
        {
            Name = "Noname" + CacheId.ToString();
        }
        else
        {
            Name = name;
        }
        
    }

    public void AddItem(Item item)
    {
        if (item != null)
        {
            _items.Add(item);
            CombineLoger.Log($"{item} ==> added to cache.");
        }
    }

    public void AddItems(params Item[] items)
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public Item? FindItem(int simpleId)
    {
        var temp = _items.FirstOrDefault(_ => _.SimpleID == simpleId);

        if (temp == null)
        {
            CombineLoger.Log($"Item with ID:{simpleId} does not exists in the cache...");
        }

        return temp;
    }

    public Item? FindItem(string description)
    {
        var temp = _items.FirstOrDefault(_ => (_.Name == description) || (_.Description == description));

        if (temp == null)
        {
            CombineLoger.Log($"Item with name or description ({description}) does not exists in the cache...");
        }

        return temp;
    }

    public void RemoveItem(int simpleId)
    {
        var temp = FindItem(simpleId);

        if (temp != null)
        {
            _items.Remove(temp);
            CombineLoger.Log($"{temp} ==> removed from cache.");
        }
    }

    public List<Item> ReturnCache()
    {
        return _items;
    }

    public void RewriteItem(int simpleId, string name, string description)
    {
        var temp = FindItem(simpleId);

        if (temp != null)
        {
            var logMessage = $"Item {simpleId}: {temp.Name}, {temp.Description}";

            temp.Name = name;
            temp.Description = description;

            CombineLoger.Log(logMessage + $" ==> changed to:\nItem {simpleId}: {temp.Name}, {temp.Description}\n");
        }
    }

    public async Task RewriteCacheWithDataFromStorage(string storagePath)
    {
        Clear();

        var temp = await Loader.LoadAsync(storagePath);
        _items = temp ?? [];

        CombineLoger.Log($"Data loaded to cache from {storagePath}");
    }

    public void ShowCachedItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            Console.WriteLine($"{i+1}. Cache item - " + _items[i]);
        }
    }

    public async Task SendCacheToUrlAsync(string url = "http://localhost:5010/api/items/receive-cache")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            CombineLoger.Log("URL cannot be null or empty. Sending to: http://localhost:5010/api/items/receive-cache");
            url = "http://localhost:5010/api/items/receive-cache";
        }

        try
        {
            // Serialize the list of items to a JSON string.
            var jsonContent = JsonSerializer.Serialize(_items);

            // Create StringContent with the JSON and set the content type header.
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Send the JSON content to the specified URL using a POST request.
            var response = await _httpClient.PostAsync(url, content);

            // Check if the request was successful.
            if (response.IsSuccessStatusCode)
            {
                CombineLoger.Log($"Successfully sent cache data to {url}. Status code: {response.StatusCode}");
            }
            else
            {
                CombineLoger.Log($"Failed to send cache data to {url}. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log any exceptions that occur during the HTTP request.
            CombineLoger.Log($"An error occurred while sending data: {ex.Message}");
        }
    }
}
