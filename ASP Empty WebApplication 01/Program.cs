using System;
using System.Text;

// This file defines the complete ASP.NET Core Minimal API server.
// It is typically named Program.cs in an ASP.NET Core project.

// You need to run this code using the .NET CLI: 'dotnet run' in your project directory.

// --- 1. Define the Data Structure (Model) ---
// This 'record' structure is the C# object that will automatically hold 
// the data sent from the HTML form. The names (username, email, age) 
// MUST exactly match the 'name' attributes in your HTML <input> tags.
List<String> data = [];

string GetData()
{
    StringBuilder sb = new StringBuilder();

    foreach (var item in data)
    {
        sb.Append("<h1>");
        sb.Append(item.ToString());
        sb.Append("</h1>");
        sb.Append('\n');
    }
    return sb.ToString();
}

// --- 2. Build the Web Application ---
var builder = WebApplication.CreateBuilder(args);

// FIX 1: Add Anti-Forgery services to the container.
builder.Services.AddAntiforgery();

// CRITICAL: Configure Kestrel to use the specific port (http://localhost:5146) 
// that your client's HTML form is targeting.
builder.WebHost.UseUrls("http://localhost:5146");

var app = builder.Build();

app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();


// --- 3. Define the POST Endpoint (The Form Receiver) ---
// [FromForm] tells the framework to bind the data from the form payload.
app.MapPost("/", async (context) =>
    {
        var form = context.Request.Form;
        var name = form["username"];
        var email = form["email"];
        var age = form["age"];

        string result = name + email + age;

        data.Add(result);

        await context.Response.SendFileAsync(builder.Environment.WebRootPath + "/index.html");
    });

// A simple GET endpoint for testing the root URL
app.MapGet("/", () => Results.Content("<h1>Registration Server is Running</h1><p>Open index.html and submit the form to POST data here.</p>", contentType: "text/html"));

app.MapGet("/data", () => Results.Content(GetData(), contentType: "text/html"));

app.MapFallbackToFile("index.html");

app.Run();

