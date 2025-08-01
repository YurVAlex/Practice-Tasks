namespace ItemsSerwerApp;

public class Item
{
    public int SimpleID { get; set; }
    public string GuidID { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int UsedCount { get; set; }
}
