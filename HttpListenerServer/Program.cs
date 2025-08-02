using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpListenerServer;

internal class Programm
{
    // A static list to hold the last received cache data.
    public static List<Item> LastReceivedItems { get; set; } = new List<Item>();
    private static HttpListener listener;

    public static async Task Main(string[] args)
    {
        // Create a new HttpListener instance.
        listener = new HttpListener();

        // Add the prefixes (URLs) that the listener will handle.
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Prefixes.Add("http://localhost:5000/api/items/receive-cache/");

        try
        {
            // Start the listener.
            listener.Start();
            Console.WriteLine("Server started. Listening on http://localhost:5000/ and http://localhost:5000/api/items/receive-cache/");

            // A loop to continuously process incoming requests.
            while (true)
            {
                // GetContextAsync waits for an incoming request.
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context)); // Handle the request asynchronously.
            }
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"HttpListenerException: {ex.Message}");
        }
        finally
        {
            // Stop the listener when the application closes.
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    private static async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // Route the request based on the URL and HTTP method.
        if (request.Url.AbsolutePath.Equals("/api/items/receive-cache/", StringComparison.OrdinalIgnoreCase) && request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            await HandlePostRequestAsync(request, response);
        }
        else if (request.Url.AbsolutePath.Equals("/", StringComparison.OrdinalIgnoreCase) && request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            await HandleGetRequestAsync(response);
        }
        else
        {
            // Handle other requests with a 404 Not Found response.
            response.StatusCode = (int)HttpStatusCode.NotFound;
            var notFoundMessage = "404 - Not Found";
            var buffer = Encoding.UTF8.GetBytes(notFoundMessage);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private static async Task HandlePostRequestAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            // Read the request body stream.
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var requestBody = await reader.ReadToEndAsync();

            // Deserialize the JSON data into a List<Item>.
            var items = JsonSerializer.Deserialize<List<Item>>(requestBody);

            if (items == null || items.Count == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                var badRequestMessage = "No items were received.";
                var buffer = Encoding.UTF8.GetBytes(badRequestMessage);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                // Store the received items in the static list.
                LastReceivedItems = items;

                // Log the received items to the console.
                Console.WriteLine($"\nReceived {items.Count} items from the client:");
                foreach (var item in items)
                {
                    Console.WriteLine($"  - SimpleID: {item.SimpleID}, Name: {item.Name}");
                }
                Console.WriteLine("\n");

                // Set the response status and content.
                response.StatusCode = (int)HttpStatusCode.OK;
                var successMessage = $"Successfully received {items.Count} items.";
                var buffer = Encoding.UTF8.GetBytes(successMessage);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        catch (JsonException)
        {
            // Handle JSON deserialization errors.
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            var errorMessage = "Invalid JSON format.";
            var buffer = Encoding.UTF8.GetBytes(errorMessage);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            // Handle other potential errors.
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var errorMessage = $"Internal server error: {ex.Message}";
            var buffer = Encoding.UTF8.GetBytes(errorMessage);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private static async Task HandleGetRequestAsync(HttpListenerResponse response)
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

        var htmlContent = htmlBuilder.ToString();
        var buffer = Encoding.UTF8.GetBytes(htmlContent);

        // Set the content type and content length.
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;

        // Write the HTML content to the response stream.
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
