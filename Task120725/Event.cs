namespace Task120725;

internal class Event
{
    private static long _totalCount;

    public long Number { get; private set; }

    public Guid Id { get; init; }

    public string Message { get; init; }

    public DateTime Timestamp { get; init; }

    public EventType Type { get; init; }

    public Event (string message, EventType type)
    {
        _totalCount++;

        Message = message; 
        
        Type = type;

        Id = Guid.NewGuid ();

        Timestamp = DateTime.UtcNow;

        Number = _totalCount;
    }

    public override string ToString()
    {
        return $"{Number}: [{Timestamp}] - {Type}. Message{Message}.";
    }
}
