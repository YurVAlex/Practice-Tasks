namespace Example_2;

internal interface IStorage
{
    Task SaveItem (Item item);

    /*void DeleteItem (Item item);   

    void FindItem (int simpleID);*/

    void SaveCache(IEnumerable<Item> items);
}
