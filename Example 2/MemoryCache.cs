
using System.Threading.Tasks;

namespace Example_2;

internal class MemoryCache : ICache
{
    private static List<Item> _items = [];    

    public async Task AddItemAsunc(Item item)
    {   
        if (item != null) 
        { 
            _items.Add(item);
            await CombineLoger.LogAsunc($"{item} ==> added to cache.");
        }
    }

    public async Task AddItemsAsunc(params Item[] items)
    {
        if (items.Length > 0)
        {
            Task[] tasks = new Task[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                tasks[i] = AddItemAsunc(items[i]);
            }

            for (int i = 0; i < items.Length; i++)
            {
                await tasks[i];
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
            await CombineLoger.LogAsunc($"Item with ID:{simpleId} does not exists in the cache...");
        }

        return temp;
    }

    public async Task<Item?> FindItem(string description)
    {
        var temp = _items.FirstOrDefault(_ => (_.Name == description) || (_.Description == description));
        
        if (temp == null)
        {
            await CombineLoger.LogAsunc($"Item with name or description ({description}) does not exists in the cache...");
        }

        return temp;
    }

    public async Task RemoveItem(int simpleId)
    {
        var temp = FindItem(simpleId).Result;

        if (temp != null)
        {
            _items.Remove(temp);
            await CombineLoger.LogAsunc($"{temp} ==> removed from cache.");
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

            await CombineLoger.LogAsunc(logMessage + $" ==> changed to:\nItem {simpleId}: {temp.Name}, {temp.Description}\n");
        }
    }
}
