# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

ProjectManager (ProTimeline) is an ASP.NET Core 9.0 web application that provides a task management system with user authentication, session management, and project tracking. The backend uses Minimal APIs with SQLite database via Entity Framework Core.

## Build and Run Commands

### Building and Running
```powershell
# Build the project
dotnet build ProjectManager.csproj

# Run the application
dotnet run --project ProjectManager.csproj

# Run with specific environment
dotnet run --project ProjectManager.csproj --environment Development
```

### Database Management
```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project ProjectManager.csproj

# Apply migrations to database
dotnet ef database update --project ProjectManager.csproj

# Remove last migration
dotnet ef migrations remove --project ProjectManager.csproj

# Drop database (deletes users.db file)
Remove-Item users.db
```

Note: Database migrations are automatically applied on application startup via `MigrateAsync()` in Program.cs.

### Project Management
```powershell
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore

# Publish for deployment
dotnet publish -c Release -o ./publish
```

## Architecture

### Application Entry Point
- **Program.cs**: Main entry point containing all API endpoint definitions using ASP.NET Core Minimal APIs pattern
- Database auto-migration runs on startup
- CORS configured to allow any origin/method/header
- Static files served from wwwroot/

### Core Components

#### Session Management
- **SessionManager.cs**: Static in-memory session store that maintains active user sessions
- Sessions are identified by secure random session IDs (256-bit entropy)
- Session cookies: `session_id_v1` with HttpOnly, Secure, SameSite=None attributes
- Each session contains a `ProjectsProcessor` instance for managing user's projects

#### Data Layer
- **Data/ApplicationDbContext.cs**: EF Core DbContext managing the Users table
- Database: SQLite (users.db)
- Connection string in appsettings.json

#### Models (Models/)
- **User**: Entity with GUID ID, credentials (email/password), and JSON fields (Settings, Projects, Links)
- **Project**: Top-level DTO matching client JSON payload with:
  - `Id` (Guid): Unique identifier for the project, auto-generated
  - `Tasks` (List<TaskItem>): Collection of tasks in the project
  - `ProjectInfo`: Project metadata (name, description, dates)
  - `LastUpdatedTask`: Most recently changed task (nullable)
  - `ClientTimestamp`: ISO 8601 timestamp for synchronization
- **ProjectInfo**: Project metadata (name, description, etc.)
- **TaskItem**: Individual task with progress tracking
- **Session**: Represents user session with secure ID and ProjectsProcessor
- **LoginModel**: DTO for login endpoint validation
- **Projects**: Container for List<Project> called UserProjects
- **Defaults**: Provides default/starter project configurations

#### Utilities (Utilities/)
- **ProjectsProcessor**: Manages user's project collection in memory during session lifetime
  - Deserializes projects from User.Projects JSON on session creation
  - Key methods:
    - `GetLatestProjectOrDefault()`: Returns project with most recent ClientTimestamp
    - `GetProjectById(Guid)`: Retrieves project by its unique Id
    - `GetProjectByName(string)`: Retrieves project by name (case-insensitive)
    - `ReplaceProject(Project)`: Updates existing project with smart matching:
      - Primary: Searches by Id
      - Fallback 1: If only one project exists, replaces it (handles Id mismatches)
      - Fallback 2: Searches by name for backward compatibility
    - `AddProject(Project)`: Adds new project to collection
    - `RemoveProject(Guid)`: Removes project by Id
- **ProjectsSerializer**: Centralized JSON serialization/deserialization for Project and Projects types using System.Text.Json
- **FileProcessor**: Handles dynamic generation of Users.html
- **ProjectLogger**: Generates console log strings for project operations

#### Generators (Generators/)
Contains HTML generation utilities for dynamic content

### API Endpoints

#### Authentication
- **POST /login**: Validates credentials, creates/retrieves session, sets session cookie
- **POST /register**: Creates new user, validates data, creates session, persists to database

#### Project Management
- **GET /getProject**: Returns TaskManager.html with injected project data for authenticated session
- **POST /projectUpdate**: Receives project updates from client, validates session, updates in-memory cache and persists to database

#### Administrative
- **GET /users**: Displays registered users in HTML format
- **GET /api/users**: Returns JSON list of all users
- **GET /**: Redirects to Index.html

### Key Architectural Patterns

#### Session-based State Management
User projects are cached in-memory within Session objects via ProjectsProcessor. Database writes occur on project updates, not on every read. This reduces database I/O for active sessions.

#### JSON Storage Pattern
User entity stores complex data (projects, settings, links) as JSON text fields in SQLite. The ProjectsSerializer provides type-safe serialization/deserialization between JSON strings and strongly-typed C# models.

#### Minimal API Pattern
All endpoints defined inline in Program.cs using MapPost/MapGet. Business logic mixed with routing (see TODOs for planned refactoring).

### Password Storage Warning
⚠️ **Security Issue**: Passwords are currently stored in plain text. The User model includes validation patterns but passwords should be hashed (e.g., using BCrypt or ASP.NET Core Identity) before production use.

## Development Notes

### TODOs in Codebase
The codebase contains numerous TODO comments indicating planned refactoring:
- Extract validation logic into a Validation class
- Create DataProcessor class for database operations
- Create CookieManager class for cookie configuration
- Move HTML generation into GeneratorHtml methods
- Use Project DTO in /projectUpdate parameters instead of manual deserialization
- Add scheduled database updates instead of immediate writes
- Add session expiration checks

### JSON Field Handling
User.Settings, User.Projects, and User.Links default to "{}" both in model initialization and database schema. When adding new users, these fields may be redundantly set in the registration endpoint.

### Project Identification and Synchronization
Projects are uniquely identified by a GUID `Id` property (auto-generated on creation).

**ReplaceProject Logic:**
The `ReplaceProject` method uses a multi-tiered matching strategy:
1. **By Id (preferred)**: Direct GUID match for reliable identification
2. **Single-project optimization**: If only one project exists in the collection, automatically replaces it (solves client/server Id synchronization issues)
3. **By name (legacy)**: Case-insensitive name matching for backward compatibility

This approach handles the common scenario where client and server initially generate different Ids for the default project. The single-project fallback ensures updates work seamlessly even when Ids don't match, which is typical for new users or after data resets.

**Best Practice**: The client should always use the project Id received from the server's initial `/getProject` response to ensure Id consistency.

### Client Timestamp Tracking
Projects include a ClientTimestamp field for synchronization. The latest project is determined by the most recent timestamp in ProjectsProcessor.GetLatestProjectOrDefault().

### Front-end Integration
The application serves static HTML/CSS/JS from wwwroot/. TaskManager.html receives bootstrapped data via injected `window.__INITIAL_DATA__` script tag containing serialized project data.
