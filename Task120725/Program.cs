namespace Task120725;

internal class Program
{
    static async Task Main(string[] args) // Changed to async Task to allow await
    {
        Console.WriteLine("Starting EventLogger Test...");

        /*// Log some information events
        EventLogger.LogEvent<Object>(" 1. Application started successfully.", EventType.Information, null);
        EventLogger.LogEvent<Object>(" 2. User 'JohnDoe' logged in.", EventType.Information, "User ID: 123");

        // Log a warning event
        EventLogger.LogEvent<Object>(" 3. Low disk space warning.", EventType.Warning, "Drive C: 10% free");

        // Log an error event
        EventLogger.LogEvent<Object>(" 4. Failed to connect to database.", EventType.Error, new { ErrorCode = 500, Details = "Connection refused" });

        // Log another information event
        EventLogger.LogEvent<Object>(" 5. Data processing completed.", EventType.Information, null);*/

        // Log some information events
        EventLogger.LogEvent<Object>(" 6. Application started once again.", EventType.Information, null);
        EventLogger.LogEvent<Object>(" 7. User 'Carabas' logged in.", EventType.Information, "User ID: 153");

        // Log a warning event
        EventLogger.LogEvent<Object>(" 8. Low disk space!!!", EventType.Warning, "Drive C: 5% free");

        // Log an error event
        EventLogger.LogEvent<Object>(" 9. Database linked, status - ok.", EventType.Error, new { StatusCode = 200, Details = "Connection OK" });

        // Log another information event
        EventLogger.LogEvent<Object>(" 5. Data processing started.", EventType.Information, null);

        Console.WriteLine("\n--- All Logged Events ---");
        // Get and display all events
        IEnumerable<Event> allEvents = EventLogger.GetAllEvents();
        foreach (var ev in allEvents)
        {
            Console.WriteLine(ev);
        }

        Console.WriteLine("\n--- Latest Event ---");
        // Get and display the latest event
        Event latestEvent = EventLogger.GetLatestEvent();
        if (latestEvent != null)
        {
            Console.WriteLine(latestEvent);
        }
        else
        {
            Console.WriteLine("No events logged yet.");
        }

        // Save all events to the JSON file
        Console.WriteLine("\nSaving events to file...");
        await EventLogger.SaveEventsToJsonAsync();
        Console.WriteLine("Events saved. Check your 'D:\\Tasks Files\\events.json' file.");

        Console.WriteLine("Load and output events from stoeage:");

        await EventLogger.LoadEventsFromFileAsync();

        foreach (var ev in EventLogger.LoadedEvents)
        {
            Console.WriteLine(ev);
        }

        Console.WriteLine("\nEventLogger Test Finished.");
        Console.ReadKey();
    }
}