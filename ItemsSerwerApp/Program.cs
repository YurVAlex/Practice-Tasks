using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ItemsSerwerApp;

public class Program
{
    // A static list to hold the last received cache data.
    // This will be accessible by both the POST and GET endpoints.
    public static List<Item> LastReceivedItems { get; set; } = new List<Item>();
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Define the API endpoint using a minimal API approach.
        // It responds to HTTP POST requests at the specified URL.
        // Endpoint to receive data via POST request.
        app.MapPost("/api/items/receive-cache", ([FromBody] List<Item> items) =>
        {
            if (items == null || items.Count == 0)
            {
                return Results.BadRequest("No items were received.");
            }

            // Store the received items in the static list.
            LastReceivedItems = items;

            // Log the received items to the server's console.
            Console.WriteLine($"\nReceived {items.Count} items from the client:");
            foreach (var item in items)
            {
                Console.WriteLine($"  - SimpleID: {item.SimpleID}, Name: {item.Name}");
            }
            Console.WriteLine("\n");

            return Results.Ok($"Successfully received {items.Count} items.");
        });

        // Endpoint to output the data to the browser.
        // This will be the default page when the application starts.
        app.MapGet("/", () =>
        {
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head><title>Received Items</title></head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("<h1>Last Received Cache Items</h1>"); 

            if (LastReceivedItems.Count == 0)
            {
                htmlBuilder.AppendLine("<p>No items have been received yet. Please send a POST request to /api/items/receive-cache.</p>");
            }
            else
            {
                htmlBuilder.AppendLine("<ul>");
                foreach (var item in LastReceivedItems)
                {
                    htmlBuilder.AppendLine($"<li><b>SimpleID:</b> {item.SimpleID}, <b>Name:</b> {item.Name}</li>");
                }
                htmlBuilder.AppendLine("</ul>");
            }

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            // Return the generated HTML content.
            return Results.Content(htmlBuilder.ToString(), "text/html");
        });

        // Start the application.
        app.Run();
    }
}
