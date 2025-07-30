
using System.Threading.Tasks;

namespace Example_2;

internal class MemoryCache 
{
    private static List<Item> _items = [];    

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
}
