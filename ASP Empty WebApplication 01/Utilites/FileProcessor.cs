namespace ASP_Empty_WebApplication_01.Utilites;

using ASP_Empty_WebApplication_01.Data;
using ASP_Empty_WebApplication_01.Generators;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Handles file system operations related to the application.
/// </summary>
public class FileProcessor
{
    /// <summary>
    /// Updates the static Users.html file with the current list of registered users.
    /// </summary>
    /// <param name="webRootPath">The path to the wwwroot folder.</param>
    /// <param name="dbContext">The application's database context.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static async Task UpdateUsersHtmlFileAsync(string webRootPath, ApplicationDbContext dbContext)
    {
        // 1. Fetch data
        var users = await dbContext.Users.ToListAsync();

        // 2. Generate content (using the new GeneratorHtml class)
        string htmlContent = GeneratorHtml.GenerateUsersHtml(users);

        // 3. Write to file
        string filePath = Path.Combine(webRootPath, "Users.html");
        await File.WriteAllTextAsync(filePath, htmlContent);
    }
}