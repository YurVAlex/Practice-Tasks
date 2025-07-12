namespace Task120725;

internal class Event
{
    public Guid Id { get; init; }

    public string Message { get; init; }

    public DateTime Timestamp { get; init; }

    public EventType Type { get; init; }

    public Event (string message, EventType type)
    {
        Message = message; 
        
        Type = type;

        Id = Guid.NewGuid ();

        Timestamp = DateTime.UtcNow;
    }
}
