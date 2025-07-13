using System;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List<Event> iteration

namespace Task120725;

internal class Program
{
    static async Task Main(string[] args) // Changed to async Task to allow await
    {
        Console.WriteLine("Starting EventLogger Test...");

        // Log some information events
        EventLogger.LogEvent<Object>("Application started successfully.", EventType.Information, null);
        EventLogger.LogEvent<Object>("User 'JohnDoe' logged in.", EventType.Information, "User ID: 123");

        // Log a warning event
        EventLogger.LogEvent<Object>("Low disk space warning.", EventType.Warning, "Drive C: 10% free");

        // Log an error event
        EventLogger.LogEvent<Object>("Failed to connect to database.", EventType.Error, new { ErrorCode = 500, Details = "Connection refused" });

        // Log another information event
        EventLogger.LogEvent<Object>("Data processing completed.", EventType.Information, null);

        Console.WriteLine("\n--- All Logged Events ---");
        // Get and display all events
        IEnumerable<Event> allEvents = EventLogger.GetAllEvents();
        foreach (var ev in allEvents)
        {
            Console.WriteLine($"[{ev.Timestamp:HH:mm:ss}] {ev.Type}: {ev.Message} (ID: {ev.Id})");
        }

        Console.WriteLine("\n--- Latest Event ---");
        // Get and display the latest event
        Event latestEvent = EventLogger.GetLatestEvent();
        if (latestEvent != null)
        {
            Console.WriteLine($"[{latestEvent.Timestamp:HH:mm:ss}] {latestEvent.Type}: {latestEvent.Message} (ID: {latestEvent.Id})");
        }
        else
        {
            Console.WriteLine("No events logged yet.");
        }

        // Save all events to the JSON file
        Console.WriteLine("\nSaving events to file...");
        await EventLogger.SaveEventsToJsonAsync();
        Console.WriteLine("Events saved. Check your 'D:\\Tasks Files\\events.json' file.");

        Console.WriteLine("\nEventLogger Test Finished.");
    }
}