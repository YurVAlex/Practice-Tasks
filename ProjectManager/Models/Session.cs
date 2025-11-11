namespace ProjectManager.Models;

using ProjectManager.Utilities;
using System.Security.Cryptography;

public class Session
{
    public string Id { get; init; }

    public Guid UserID { get; init; }

    public ProjectsProcessor projectsProcessor { get; set; }

    public Session(User user) 
    {
        this.UserID = user.ID;

        Id = GenerateSecureSessionId();

        projectsProcessor = new ProjectsProcessor(user);
    }

    static string GenerateSecureSessionId()
    {
        var data = new byte[32]; // 32 bytes = 256 bits of entropy
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }
        // Encode as Base64 string for safe transmission
        return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
