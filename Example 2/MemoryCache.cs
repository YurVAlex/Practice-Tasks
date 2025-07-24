
namespace Example_2;

internal class MemoryCache : ICache
{
    private static List<Item> _items = [];    

    public async Task AddItem(Item item)
    {
        if (item != null) 
        { 
            _items.Add(item);
            await CombineLoger.Log($"{item} ==> added to cache.");
        }
    }

    public void AddItems(params Item[] items)
    {
        if (items.Length > 0)
        {
            foreach (Item item in items)
            {
                AddItem(item).Wait();
            }
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public async Task<Item?> FindItem(int simpleId)
    {
        var temp = _items.FirstOrDefault(_ => _.SimpleID == simpleId);

        if (temp == null)
        {
            await CombineLoger.Log($"Item with ID:{simpleId} does not exists in the cache...");
        }

        return temp;
    }

    public async Task<Item?> FindItem(string description)
    {
        var temp = _items.FirstOrDefault(_ => (_.Name == description) || (_.Description == description));
        
        if (temp == null)
        {
            await CombineLoger.Log($"Item with name or description ({description}) does not exists in the cache...");
        }

        return temp;
    }

    public async Task RemoveItem(int simpleId)
    {
        var temp = FindItem(simpleId).Result;

        if (temp != null)
        {
            _items.Remove(temp);
            await CombineLoger.Log($"{temp} ==> removed from cache.");
        }
    }

    public IEnumerable<Item> ReturnCache()
    {
        return _items;
    }

    public async Task RewriteItem(int simpleId, string name, string description)
    {
        var temp = FindItem(simpleId).Result;

        if (temp != null)
        {
            var logMessage = $"Item {simpleId}: {temp.Name}, {temp.Description}";

            temp.Name = name;
            temp.Description = description;

            await CombineLoger.Log(logMessage + $" ==> changed to:\nItem {simpleId}: {temp.Name}, {temp.Description}\n");
        }
    }
}
