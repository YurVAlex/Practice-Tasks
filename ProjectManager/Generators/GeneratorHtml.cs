namespace ProjectManager.Generators;

using ProjectManager.Models;
using System.Text;

/// <summary>
/// Contains methods for generating HTML content, specifically for displaying a list of users.
/// </summary>
public static class GeneratorHtml
{
    /// <summary>
    /// Generates the HTML content for displaying all registered users.
    /// </summary>
    /// <param name="users">A list of User entities.</param>
    /// <returns>HTML string for the users page.</returns>
    public static string GenerateUsersHtml(List<User> users)
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
}