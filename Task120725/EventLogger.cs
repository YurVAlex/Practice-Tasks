using System.Text.Json;
using System.Threading.Tasks;

namespace Task120725;

internal class EventLogger
{
    private static List<Event> _events = [];

    private static readonly string _filePath = @"D:\Tasks Files\events.json";

    static EventLogger() // needed if we have AppendAllTextAsync?
    {
        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }
    }

    public static async Task SaveEventsToJsonAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Makes the JSON output human-readable
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Converts C# PascalCase to JSON camelCase
        };

        try
        {
            var jsonString = JsonSerializer.Serialize(_events, options);

            await File.AppendAllTextAsync(_filePath, jsonString);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void LogEvent<T>(string message, EventType type, T additionalInfo)
    {
        if (additionalInfo != null) 
        { 
            message = $"{message}. Info: {additionalInfo}";
        }
        _events.Add(new Event(message, type));
        
        Console.WriteLine($"Log: {type}, {message} added to list");
    }

    public static IEnumerable<Event> GetAllEvents()
    { 
        return _events; 
    }

    public static Event? GetLatestEvent()
    { 
        if(_events.Count == 0)
        {
            return null;
        }

        return _events[^1]; 
    }
}
