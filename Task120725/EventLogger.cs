using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Task120725;

internal static class EventLogger
{
    private static List<Event> _events = []; // Initialize as empty list

    public static List<Event> LoadedEvents = []; // Initialize as empty list

    private static readonly string _filePath = @"D:\Tasks Files\events.json";

    public static async Task SaveEventsToJsonAsync()
    {
        try
        {
            foreach (var item in _events)
            {
                var jsonString = JsonSerializer.Serialize(item) + "\n";
                await File.AppendAllTextAsync(_filePath, jsonString);
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving events: {ex}");
        }
    }

    public static async Task LoadEventsFromFileAsync()
    {
        LoadedEvents.Clear();

        if (File.Exists(_filePath) && (new FileInfo(_filePath).Length != 0))
        {
            try
            {
                var jsonString = await File.ReadAllLinesAsync(_filePath);

                foreach (var item in jsonString)
                {
                    var loadedEvent = JsonSerializer.Deserialize<Event>(item);

                    if (loadedEvent != null)
                    {
                        LoadedEvents.Add(loadedEvent);
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing events from file: {ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading events file: {ex}");
            }
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
        // Return a copy to prevent external modification of the internal list directly
        return new List<Event>(_events);
    }

    public static Event? GetLatestEvent()
    {
        if (_events.Count == 0)
        {
            return null;
        }

        return _events[^1];
    }
}