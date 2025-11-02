using ASP_Empty_WebApplication_01.Data;
using ASP_Empty_WebApplication_01.Models;
using ASP_Empty_WebApplication_01.Utilites;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

// This file defines the complete ASP.NET Core Minimal API server for ProTimeline app.

// --- Application Setup ---

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddRouting();

// Configure SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Ensure the database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files from wwwroot

// --- API Endpoints ---

// Registration endpoint: Accepts JSON in the body
app.MapPost("/register", async (User newUser, ApplicationDbContext dbContext) =>
{
    // 1. Model Binding and Validation
    // ASP.NET Core Minimal APIs automatically deserialize the JSON body into the 'newUser' object.
    // The validation context is needed to check Data Annotations manually.
    var validationContext = new ValidationContext(newUser, serviceProvider: null, items: null);
    var validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(newUser, validationContext, validationResults, true);

    if (!isValid)
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        return Results.BadRequest(new { error = "Validation failed.", details = errors });
    }

    try
    {
        // 2. Check for existing user with the same email
        if (await dbContext.Users.AnyAsync(u => u.Email == newUser.Email))
        {
            return Results.Conflict(new { error = "User with this email already exists." });
        }

        // Ensure default JSON fields are set if the client didn't provide them (User model handles required properties)
        if (newUser.Settings == null) newUser.Settings = "{}";
        if (newUser.Pages == null) newUser.Pages = "{}";
        if (newUser.Links == null) newUser.Links = "{}";

        // 3. Save to database
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"New user registered: {newUser.Name} ({newUser.Email}) - ID: {newUser.ID}");

        // Return the created user as JSON
        return Results.Json(new
        {
            id = newUser.ID,
            name = newUser.Name,
            email = newUser.Email,
            message = "Registration successful!"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during registration: {ex.Message}");
        return Results.Problem("An error occurred during registration.");
    }
});

// Login endpoint: Accepts JSON in the body
app.MapPost("/login", async (LoginModel loginData, ApplicationDbContext dbContext) =>
{
    // 1. Model Binding and Validation (via LoginModel DTO)
    var validationContext = new ValidationContext(loginData, serviceProvider: null, items: null);
    var validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(loginData, validationContext, validationResults, true);

    if (!isValid)
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        Console.WriteLine($"Login validation failed: {string.Join(", ", errors)}");
        return Results.BadRequest(new { error = "Invalid data format.", details = errors });
    }

    try
    {
        // 2. Database Lookup
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == loginData.Email && u.Password == loginData.Password);

        if (user == null)
        {
            // 3. Login Failed
            Console.WriteLine($"Login failed for email: {loginData.Email} (Invalid credentials)");
            return Results.BadRequest(new { error = "Invalid e-mail or password." });
        }

        // 4. Login Successful
        Console.WriteLine($"User successfully logged in: {user.Email} - ID: {user.ID}");

        // Return the essential user details
        return Results.Json(new
        {
            id = user.ID,
            name = user.Name,
            email = user.Email,
            message = "Login successful!"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during login: {ex.Message}");
        return Results.Problem("An internal server error occurred during login.");
    }
});


// Users endpoint - display registered users in HTML
app.MapGet("/users", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    await FileProcessor.UpdateUsersHtmlFileAsync(builder.Environment.WebRootPath, dbContext);
    await context.Response.SendFileAsync(builder.Environment.WebRootPath + "/Users.html");
});

// API endpoint to get all registered users as JSON
app.MapGet("/api/users", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users.ToListAsync();
    return Results.Json(users.Select(u => new
    {
        id = u.ID,
        name = u.Name,
        email = u.Email,
        registrationDate = DateTime.Now // Placeholder date
    }));
});

// projectUpdate and default route remain unchanged
app.MapPost("/projectUpdate", async (HttpRequest req) =>
{
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    Project? payload;
    try
    {
        payload = await JsonSerializer.DeserializeAsync<Project>(req.Body, options);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error deserializing /projectUpdate payload: " + ex);
        return Results.BadRequest(new { success = false, error = "Invalid JSON payload" });
    }

    if (payload == null)
    {
        Console.WriteLine("Received empty payload on /projectUpdate");
        return Results.BadRequest(new { success = false, error = "Empty payload" });
    }

    // Ensure tasks are normalized (enforces invariants like progress/completed)
    try
    {
        payload.NormalizeTasks();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Normalization error: " + ex);
    }

    // Show received summary on console
    Console.WriteLine(ProjectLogger.GenerateLogString(payload));

    // Return a simple acknowledgement. Replace this with the appropriate DTO.
    return Results.Ok(new { success = true, receivedTasks = payload.Tasks.Count });
});

app.MapGet("/getProject", async (HttpContext context) =>
{
    await context.Response.SendFileAsync(builder.Environment.WebRootPath + "/TaskManager.html");
});

// Default route to serve the main HTML file
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/Index.html");
    return Task.CompletedTask;
});

app.Run();