namespace Example_2;

interface ICache
{
    Task AddItemAsunc(Item item);

    Task AddItemsAsunc(params Item[] items);

    //void RemoveItem(Item item);

    Task RemoveItem(int simpleId);

    void Clear();

    Task<Item?> FindItem(int simpleId);

    Task<Item?> FindItem(string property);

    Task RewriteItem (int simpleId, string name, string description);

    IEnumerable<Item> ReturnCache();
}
