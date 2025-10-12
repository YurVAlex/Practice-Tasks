using System;
using System.Text;
using ASP_Empty_WebApplication_01.Models;
using ASP_Empty_WebApplication_01.Data;
using Microsoft.EntityFrameworkCore;

// This file defines the complete ASP.NET Core Minimal API server for user registration.

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
    sb.AppendLine("        .registration-count { background-color: #4CAF50; color: white; padding: 10px; border-radius: 5px; text-align: center; margin-bottom: 20px; }");
    sb.AppendLine("        h1 { text-align: center; color: #333; }");
    sb.AppendLine("    </style>");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine("    <h1>Registered Users</h1>");
    sb.AppendLine($"    <div class='registration-count'>Total Registrations: {users.Count}</div>");

    foreach (var user in users)
    {
        sb.AppendLine("    <div class='user-card'>");
        sb.AppendLine($"        <div class='user-name'>{user.Name}</div>");
        sb.AppendLine($"        <div class='user-email'>{user.Email}</div>");
        sb.AppendLine($"        <div class='user-id'>ID: {user.ID}</div>");
        sb.AppendLine("    </div>");
    }

    sb.AppendLine("</body>");
    sb.AppendLine("</html>");
    return sb.ToString();
}

async Task UpdateUsersHtmlFileAsync(string webRootPath, ApplicationDbContext context)
{
    try
    {
        var users = await context.Users.ToListAsync();
        var htmlContent = GenerateUsersHtml(users);
        File.WriteAllText(Path.Combine(webRootPath, "Users.html"), htmlContent);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating Users.html: {ex.Message}");
    }
}

// --- 2. Build the Web Application ---
var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=users.db"));

// Configure Kestrel to use the specific port (http://localhost:5146)
builder.WebHost.UseUrls("http://localhost:5146");

var app = builder.Build();

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();

// --- 3. Define Endpoints ---

// Root endpoint - serve the registration form
app.MapGet("/", async (context) => 
    await context.Response.SendFileAsync(builder.Environment.WebRootPath + "/Index.html"));

// Registration endpoint - handle user registration with route-level validation
app.MapPost("/register/{name:minlength(3):maxlength(30)}/{email:minlength(5):maxlength(255)}/{password:minlength(8):maxlength(128)}", 
    async (string name, string email, string password, HttpContext context, ApplicationDbContext dbContext) =>
{
    try
    {
        // URL decode the parameters to handle special characters
        var decodedName = Uri.UnescapeDataString(name);
        var decodedEmail = Uri.UnescapeDataString(email);
        var decodedPassword = Uri.UnescapeDataString(password);
        
        // Route-level validation constraints (matching model and client levels)
        
        // 1. Name validation: 3-30 characters, no whitespace-only
        if (string.IsNullOrWhiteSpace(decodedName) || decodedName.Trim().Length < 3 || decodedName.Trim().Length > 30)
        {
            return Results.BadRequest(new { error = "Name must be between 3 and 30 characters long." });
        }
        
        // 2. Email validation: valid email format, max 255 characters
        if (string.IsNullOrWhiteSpace(decodedEmail) || decodedEmail.Trim().Length > 255)
        {
            return Results.BadRequest(new { error = "Email address is too long (maximum 255 characters)." });
        }
        
        // Email format validation using regex (matching client-side validation)
        var emailRegex = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(decodedEmail.Trim(), emailRegex))
        {
            return Results.BadRequest(new { error = "Please enter a valid email address." });
        }
        
        // 3. Password validation: 8-128 characters with complexity requirements
        if (string.IsNullOrWhiteSpace(decodedPassword) || decodedPassword.Length < 8 || decodedPassword.Length > 128)
        {
            return Results.BadRequest(new { error = "Password must be between 8 and 128 characters long." });
        }
        
        // Password complexity validation (matching model and client levels)
        var hasUppercase = System.Text.RegularExpressions.Regex.IsMatch(decodedPassword, @"[A-Z]");
        var hasLowercase = System.Text.RegularExpressions.Regex.IsMatch(decodedPassword, @"[a-z]");
        var hasDigit = System.Text.RegularExpressions.Regex.IsMatch(decodedPassword, @"[0-9]");
        var hasSpecial = System.Text.RegularExpressions.Regex.IsMatch(decodedPassword, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");
        
        if (!hasUppercase || !hasLowercase || !hasDigit || !hasSpecial)
        {
            var missingRequirements = new List<string>();
            if (!hasUppercase) missingRequirements.Add("one uppercase letter");
            if (!hasLowercase) missingRequirements.Add("one lowercase letter");
            if (!hasDigit) missingRequirements.Add("one number");
            if (!hasSpecial) missingRequirements.Add("one special character");
            
            return Results.BadRequest(new { 
                error = $"Password must contain: {string.Join(", ", missingRequirements)}." 
            });
        }
        
        // 4. Business logic validation: Check for duplicate email in database
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == decodedEmail.Trim().ToLower());
        
        if (existingUser != null)
        {
            return Results.Conflict(new { error = "User with this email already exists." });
        }
        
        // 5. Create and store the user in database
        var newUser = new User
        {
            Name = decodedName.Trim(),
            Email = decodedEmail.Trim(),
            Password = decodedPassword
        };
        
        // Add to the database
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

// Users endpoint - display registered users
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

// Fallback to serve the registration form for any other routes
app.MapFallbackToFile("Index.html");

app.Run();

