namespace ProjectManager.Models;
using System.Security.Cryptography;

public class Session
{
    public string Id { get; init; }

    public Guid UserID { get; init; }

    public Session(Guid userID)
    {
        this.UserID = userID;

        Id = GenerateSecureSessionId();
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
