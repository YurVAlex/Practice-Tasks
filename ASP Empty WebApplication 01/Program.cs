using ASP_Empty_WebApplication_01;
using ASP_Empty_WebApplication_01.Data;
using ASP_Empty_WebApplication_01.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

// This file defines the complete ASP.NET Core Minimal API server for user registration and login.

// --- Helper Functions ---

/// <summary>
/// Generates the HTML content for displaying all registered users.
/// </summary>
/// <param name="users">A list of User entities.</param>
/// <returns>HTML string for the users page.</returns>
string GenerateUsersHtml(List<User> users)
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine("<html>");
    sb.AppendLine("<head>");
    sb.AppendLine("    <meta charset='utf-8' />");
    sb.AppendLine("    <title>Registered Users</title>");
    sb.AppendLine("    <style>");
    sb.AppendLine("        body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }");
    sb.AppendLine("        .user-card { background-color: #f9f9f9; padding: 15px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #4CAF50; }");
    sb.AppendLine("        .user-name { font-weight: bold; color: #333; font-size: 18px; }");
    sb.AppendLine("        .user-email { color: #666; margin: 5px 0; }");
    sb.AppendLine("        .user-id { color: #888; font-size: 12px; }");
    sb.AppendLine("    </style>");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine("    <h1>Registered Users</h1>");

    if (users.Count == 0)
    {
        sb.AppendLine("    <p>No users registered yet.</p>");
    }
    else
    {
        foreach (var user in users)
        {
            sb.AppendLine("    <div class='user-card'>");
            sb.AppendLine($"        <p class='user-name'>{user.Name}</p>");
            sb.AppendLine($"        <p class='user-email'>Email: {user.Email}</p>");
            sb.AppendLine($"        <p class='user-id'>ID: {user.ID}</p>");
            sb.AppendLine("    </div>");
        }
    }

    sb.AppendLine("</body>");
    sb.AppendLine("</html>");
    return sb.ToString();
}

/// <summary>
/// Updates the static Users.html file with the current list of registered users.
/// </summary>
/// <param name="webRootPath">The path to the wwwroot folder.</param>
/// <param name="dbContext">The application's database context.</param>
/// <returns>A Task representing the asynchronous operation.</returns>
async Task UpdateUsersHtmlFileAsync(string webRootPath, ApplicationDbContext dbContext)
{
    var users = await dbContext.Users.ToListAsync();
    string htmlContent = GenerateUsersHtml(users);
    string filePath = Path.Combine(webRootPath, "Users.html");
    await File.WriteAllTextAsync(filePath, htmlContent);
}

// --- Application Setup ---

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files from wwwroot

// --- API Endpoints ---

// Registration endpoint
app.MapPost("/register/{name}/{email}/{password}", async (string name, string email, string password, ApplicationDbContext dbContext, HttpContext httpContext) =>
{
    // 1. Construct a temporary user for validation
    var newUser = new User
    {
        Name = name,
        Email = email,
        Password = password
    };

    // 2. Perform validation against data annotations in the User model
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
        // 3. Check for existing user with the same email
        if (await dbContext.Users.AnyAsync(u => u.Email == newUser.Email))
        {
            return Results.Conflict(new { error = "User with this email already exists." });
        }

        // 4. Save to database
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        var session = new Session(newUser.ID);
        SessionManager.Sessions.Add(session);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,         // Recommended: Prevents client-side JavaScript access (mitigates XSS)
            SameSite = SameSiteMode.Strict // Recommended for security
        };

        // 4. **Set the Session ID Cookie
        // Use httpContext.Response.Cookies.Append to add the cookie to the response headers.
        httpContext.Response.Cookies.Append("SessionCookieName", session.Id.ToString(), cookieOptions);

        Console.WriteLine($"New user registered: {newUser.Name} ({newUser.Email}) - ID: {newUser.ID}");

        
        // Return the created user as JSON
        return Results.Json(new
        {
            id = newUser.ID,
            name = newUser.Name,
            email = newUser.Email,
            message = $"Registration successful! Session ID: {session.Id}"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during registration: {ex.Message}");
        return Results.Problem("An error occurred during registration.");
    }
});

// ***************************************************************
// NEW: Login endpoint
// ***************************************************************
app.MapPost("/login/{email}/{password}", async (string email, string password, ApplicationDbContext dbContext) =>
{
    // 1. Validation Setup (using User model's constraints)
    var loginAttemptUser = new User 
    {
        // Name is required by the User model, so we provide a dummy value 
        // to allow validation of Email and Password to proceed.
        Name = "LoginAttempt", 
        Email = email,
        Password = password
    };
    
    var validationContext = new ValidationContext(loginAttemptUser, serviceProvider: null, items: null);
    var validationResults = new List<ValidationResult>();
    
    // Check if the provided email/password strings satisfy the model's data annotations
    bool isValid = Validator.TryValidateObject(loginAttemptUser, validationContext, validationResults, true);

    if (!isValid)
    {
        // Extract validation errors and return 400 Bad Request
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        Console.WriteLine($"Login validation failed: {string.Join(", ", errors)}");
        return Results.BadRequest(new { error = "Invalid data format.", details = errors });
    }

    try
    {
        // 2. Database Lookup
        // Find the user by both Email and (plain text) Password
        // NOTE: In a production app, the password should be HASHED in the DB and verified using a hash comparison!
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

        if (user == null)
        {
            // 3. Login Failed
            Console.WriteLine($"Login failed for email: {email} (Invalid credentials)");
            // Use Unauthorized (401) or Forbidden (403) for failed authentication/authorization
            return Results.BadRequest(new { error = "Invalid e-mail or password."});
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
        // Return 500 Internal Server Error for unhandled exceptions
        return Results.Problem("An internal server error occurred during login.");
    }
});


// Users endpoint - display registered users in HTML
app.MapGet("/users", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    // Update the Users.html file
    await UpdateUsersHtmlFileAsync(builder.Environment.WebRootPath, dbContext);
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
        registrationDate = DateTime.Now // You might want to add a registration date field to User model
    }));
});

// Default route to serve the main HTML file
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/Index.html");
    return Task.CompletedTask;
});

app.Run();