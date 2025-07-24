namespace Example_2;

internal class Item
{
    private static int _totalItemsCount;

    public int SimpleID { get; set; }

    public string GuidID { get; set; }

    public DateTime TimeStamp { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
        
    public int UsedCount { get; set; }

    public Item()
    { }    

    public Item(string name = "Unknown", string description = "")
    {
        SimpleID = ++_totalItemsCount;

        GuidID = Guid.NewGuid().ToString();
        
        TimeStamp = DateTime.UtcNow;

        UsedCount = 0;

        Name = name;

        Description = description;
    }

    public override string ToString()
    {
        return $"{SimpleID}: [{TimeStamp}] - {Name} ({Description})";
    }
}
