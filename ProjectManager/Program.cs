using ProjectManager;
using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Utilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

// This file defines the complete ASP.NET Core Minimal API server for ProTimeline app.
//-----------------------------------------------------------------------------------------
// --- Application Setup ---

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddRouting();

// Configure SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// --- CRITICAL: Ensure the database is created and migrations are applied ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        // Use MigrateAsync() to apply any pending migrations or create the DB if it doesn't exist
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("[DB Setup] Database creation and migration successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Setup] An error occurred during database migration: {ex.Message}");
    }
}

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files from wwwroot

// --- API Endpoints ---
//-----------------------------------------------------------------------------------------
// Login endpoint: Accepts JSON in the body

app.MapPost("/login", async (LoginModel loginData, ApplicationDbContext dbContext, HttpContext context) =>
{
    // 1. Model Binding and Validation (via LoginModel DTO)
    // ASP.NET Core Minimal APIs automatically deserialize the JSON body into the 'loginData' object.
    var validationContext = new ValidationContext(loginData, serviceProvider: null, items: null);
    var validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(loginData, validationContext, validationResults, true);

    if (!isValid)
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        Console.WriteLine($"Login validation failed: {string.Join(", ", errors)}");
        return Results.BadRequest(new { error = "Invalid data format.", details = errors });
    } // TODO Add new class Validation (use overloaded Validation.TryValidate(loginData))

    try
    {
        // 2. Database Lookup
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == loginData.Email && u.Password == loginData.Password);
        // TODO Add new class DataProcessor (use await overloaded DataProcessor.GetUser(loginData))

        if (user == null)
        {
            // 3. Login Failed
            Console.WriteLine($"Login failed for email: {loginData.Email} (Invalid credentials)");
            return Results.BadRequest(new { error = "Invalid e-mail or password." });
        }

        // 4. Login Successful
        Console.WriteLine($"User successfully logged in: {user.Email} - ID: {user.ID}");

        // 3. Manage session and return it to the client

        var session = SessionManager.GetSession(user.ID);
        if (session == null)
        {
            // Session is absent in session's list 

            session = SessionManager.ReturnNewSession(user);

            Console.WriteLine("=========================================================");
            Console.WriteLine($"New session appointed: {session.Id} for {user.Email}");
            Console.WriteLine("=========================================================");
        }  // TODO new method to SessionManager (SessionManager.GetSessionFromCacheOrMakeNew(user.ID))

        var cookieOptions = new CookieOptions
        {
            // HttpOnly: TRUE means JavaScript CANNOT read this cookie.
            // This is the correct setting if the Session ID is only for the server.
            HttpOnly = true,

            // Secure: TRUE means the cookie is only sent over HTTPS. 
            // This is MANDATORY for production security.
            Secure = true,

            // SameSite: Use 'None' if your front-end and back-end are on different domains/ports.
            // Note: SameSite=None REQUIRES Secure=true.
            SameSite = SameSiteMode.None,

            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Domain = null // Set to your domain if needed, null defaults to current host
        };  // TODO new class CookieManager (to use CookieManager.SessionCookieOptions)

        // 3. SET THE COOKIE
        context.Response.Cookies.Append("session_id_v1", session.Id, cookieOptions);

        return Results.Ok(new { success = $"User {user.Name} logged in." });

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during login: {ex.Message}");
        return Results.Problem("An internal server error occurred during login.");
    }
});

// Registration endpoint: Accepts JSON in the body
app.MapPost("/register", async (User userData, ApplicationDbContext dbContext, HttpContext context) =>
{
    // 1. Model Binding and Validation
    // ASP.NET Core Minimal APIs automatically deserialize the JSON body into the 'newUser' object.
    // The validation context is needed to check Data Annotations manually.
    var validationContext = new ValidationContext(userData, serviceProvider: null, items: null);
    var validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(userData, validationContext, validationResults, true);

    if (!isValid)
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        return Results.BadRequest(new { error = "Validation failed.", details = errors });
    }  // TODO Add new class Validation (use overloaded Validation.TryValidate(newUser))

    try
    {
        // 2. Check for existing user with the same email
        if (await dbContext.Users.AnyAsync(u => u.Email == userData.Email))
        {
            return Results.Conflict(new { error = "User with this email already exists." });
        }  // TODO Add new class DataProcessor (use await DataProcessor.ExistingEmailCheck(newUser))

        // Ensure default JSON fields are set if the client didn't provide them (User model handles required properties)
        userData.Settings ??= "{}";  // Redundant?
        userData.Projects ??= "{}";  // Redundant?
        userData.Links ??= "{}";     // Redundant?

        // 3. Save to database
        dbContext.Users.Add(userData);
        await dbContext.SaveChangesAsync();
        // TODO Add new class DataProcessor (use await DataProcessor.AddToBase(newUser))

        Console.WriteLine($"New user registered: {userData.Name} ({userData.Email}) - ID: {userData.ID}");

        // Create a session for this new user
        var session = SessionManager.ReturnNewSession(userData);

        var cookieOptions = new CookieOptions
        {
            // HttpOnly: TRUE means JavaScript CANNOT read this cookie.
            // This is the correct setting if the Session ID is only for the server.
            HttpOnly = true,

            // Secure: TRUE means the cookie is only sent over HTTPS. 
            // This is MANDATORY for production security.
            Secure = true,

            // SameSite: Use 'None' if your front-end and back-end are on different domains/ports.
            // Note: SameSite=None REQUIRES Secure=true.
            SameSite = SameSiteMode.None,

            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Domain = null // Set to your domain if needed, null defaults to current host
        };  // TODO new class CookieManager (to use CookieManager.SessionCookieOptions)

        // SET THE COOKIE
        context.Response.Cookies.Append("session_id_v1", session.Id, cookieOptions);

        return Results.Ok(new { success = $"Welcome {userData.Name}. Registration successful!" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during registration: {ex.Message}");
        return Results.Problem("An error occurred during registration.");
    }
});

// Combined GET and POST endpoint for /getProject
// GET: Returns latest project or default
// POST with Project DTO: Creates/loads specific project
app.MapGet("/getProject", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    await HandleGetProject(context, dbContext, builder.Environment.WebRootPath);
});

async Task HandleGetProject(HttpContext context, ApplicationDbContext dbContext, string webRootPath)
{
    if (!context.Request.Cookies.TryGetValue("session_id_v1", out string? sessionId))
    {
        context.Response.StatusCode = 401;
        return;
    }

    var session = SessionManager.GetSession(sessionId);
    if (session == null)
    {
        context.Response.StatusCode = 401;
        return;
    }

        Project? bootstrap = session.projectsProcessor.GetLatestProjectOrDefault();
        Console.WriteLine($"Srnding latest project: {bootstrap?.ProjectInfo?.Name}");
    
    // Load and inject HTML with bootstrap data
    var path = Path.Combine(webRootPath, "TaskManager.html");
    var html = await File.ReadAllTextAsync(path);

    var injection = "<script>window.__INITIAL_DATA__ = " +
                    ProjectsSerializer.SerializeProject(bootstrap) +
                    ";</script>";

    var idx = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        html = html.Insert(idx, injection);
    }
    else
    {
        html += injection;
    }

    Console.WriteLine("================================================================================");
    Console.WriteLine($"Sending project: {bootstrap?.ProjectInfo?.Name} (Id: {bootstrap?.Id})");
    Console.WriteLine($"Session Id: {sessionId}");
    Console.WriteLine("================================================================================");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
}

app.MapPost("/newProject", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    Project? newProject = null;
    
    // Try to read Project DTO from body
    if (context.Request.ContentLength > 0)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            newProject = await JsonSerializer.DeserializeAsync<Project>(context.Request.Body, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[getProject POST] Error deserializing Project: {ex.Message}");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid Project DTO" });
            return;
        }
    }
    
    await HandlePostProject(context, dbContext, builder.Environment.WebRootPath, newProject);
});

async Task HandlePostProject(HttpContext context, ApplicationDbContext dbContext, string webRootPath, Project? newProject)
{
    if (!context.Request.Cookies.TryGetValue("session_id_v1", out string? sessionId))
    {
        context.Response.StatusCode = 401;
        return;
    }

    var session = SessionManager.GetSession(sessionId);
    if (session == null)
    {
        context.Response.StatusCode = 401;
        return;
    }

    Project? bootstrap;

    if (newProject == null)
    {
        // Scenario 1: No Project DTO provided - return latest or default
        bootstrap = session.projectsProcessor.GetLatestProjectOrDefault();
        Console.WriteLine($"[getProject] No DTO provided, using latest: {bootstrap?.ProjectInfo?.Name}");
    }
    else
    {
        // Project DTO was provided
        Console.WriteLine($"[getProject] Project DTO received - Id: {newProject.Id}, Name: {newProject.ProjectInfo?.Name}");

        // Check if project exists in session
        var existingProject = session.projectsProcessor.GetProjectById(newProject.Id);

        if (existingProject != null)
        {
            // Scenario 2: Project exists - use it
            bootstrap = existingProject;
            Console.WriteLine($"[getProject] Project exists in session, using it: {bootstrap.ProjectInfo?.Name}");
        }
        else
        {
            // Scenario 3: Project doesn't exist - add it to session
            newProject.NormalizeTasks();

            if (session.projectsProcessor.AddProject(newProject))
            {
                bootstrap = newProject;
                Console.WriteLine($"[getProject] New project added to session: {bootstrap.ProjectInfo?.Name}");

                // Persist to database
                try
                {
                    string projectsJson = ProjectsSerializer.SerializeProjects(session.projectsProcessor.Projects);
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ID == session.UserID);
                    if (user != null)
                    {
                        user.Projects = projectsJson;
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"[getProject] Project persisted to database for user {user.Email}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[getProject] Error persisting project: {ex.Message}");
                }
            }
            else
            {
                // Failed to add (shouldn't happen unless duplicate Id)
                Console.WriteLine($"[getProject] Failed to add project to session");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "Failed to add project - duplicate Id?" });
                return;
            }
        }
    }

    // Load and inject HTML with bootstrap data
    var path = Path.Combine(webRootPath, "TaskManager.html");
    var html = await File.ReadAllTextAsync(path);

    var injection = "<script>window.__INITIAL_DATA__ = " +
                    ProjectsSerializer.SerializeProject(bootstrap) +
                    ";</script>";

    var idx = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        html = html.Insert(idx, injection);
    }
    else
    {
        html += injection;
    }

    Console.WriteLine("================================================================================");
    Console.WriteLine($"Sending project: {bootstrap?.ProjectInfo?.Name} (Id: {bootstrap?.Id})");
    Console.WriteLine($"Session Id: {sessionId}");
    Console.WriteLine("================================================================================");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
}


// Helper method to handle both GET and POST /getProject logic


app.MapPost("/projectUpdate", async (HttpContext context, ApplicationDbContext dbContext) =>
{   // TODO use Project object as DTO in MapPost parameters
    var request = context.Request;

    if (context.Request.Cookies.TryGetValue("session_id_v1", out string? sessionId))
    {
        var session = SessionManager.GetSession(sessionId);
        if (session == null)
        {
            return Results.Unauthorized();
        }
        else
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            Project? payload;  // TODO use Project object as DTO in MapPost parameters
            try
            {
                payload = await JsonSerializer.DeserializeAsync<Project>(request.Body, options);
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

            // Identify the user by session id provided either as query string or header

            // Find the user
            // TODO Add new class DataProcessor (use await overloaded DataProcessor.GetUser(UserID))
            // TODO Make further operations with currentProject in accord Session of sessionManager list
            // TODO Add asunc function to DataProcessor class which should update database using sessionManager currentProjects clientTimestamps 

            if(session.projectsProcessor.ReplaceProject(payload))
            {
                // Persist the received Project payload as a JSON string in the Projects field
                string projectsJson;
                try
                {
                    projectsJson = ProjectsSerializer.SerializeProjects(session.projectsProcessor.Projects);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error serializing Projects from session cache: " + ex);
                    return Results.Problem("Failed to serialize Projects from session cache.");
                }

                // Database Lookup - TODO Delete that add scheduled database update in DataProcessor class)
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.ID == session.UserID);
                user.Projects = projectsJson;
                await dbContext.SaveChangesAsync();

                // TODO Delete that add scheduled database update in DataProcessor class)
                // TODO Add session expirsoon check 
            }
            else
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine($"Can't replace project {payload.ProjectInfo.Name} in user session cache.");
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
                // TODO Add payload as Project to projects list in User's Projects property

                // Return a simple acknowledgement including which user was updated
                return Results.Ok(new { success = true, receivedTasks = payload.Tasks.Count, savedFor = payload.ProjectInfo.Name });
        }
    }
    else
    {
        return Results.Unauthorized();
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

// List projects from current session (id, name, dates)
app.MapGet("/api/projects", (HttpContext context) =>
{
    if (!context.Request.Cookies.TryGetValue("session_id_v1", out string? sessionId))
    {
        return Results.Unauthorized();
    }
    var session = SessionManager.GetSession(sessionId);
    if (session == null)
    {
        return Results.Unauthorized();
    }
    var projects = session.projectsProcessor.Projects.UserProjects
        .Select(p => new
        {
            id = p.Id,
            name = p.ProjectInfo?.Name ?? "(unnamed)",
            timestamp = p.ClientTimestamp
        }).ToList();
    return Results.Json(projects);
});

// Default route to serve the main HTML file
app.MapGet("/", () =>
{
    return Results.Redirect("/Index.html");
});

app.Run();
