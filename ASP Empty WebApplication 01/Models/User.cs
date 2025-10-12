namespace ASP_Empty_WebApplication_01.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity model for a User, designed for compatibility with SQLite using EF Core.
/// </summary>
public class User
{
    /// <summary>
    /// Primary Key: Globally Unique Identifier (GUID) stored as a string.
    /// In C#, we use Guid for type safety, and EF Core handles the mapping
    /// to a database-compatible type (typically TEXT/BLOB in SQLite).
    /// </summary>
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ID { get; set; }

    /// <summary>
    /// User's display name.
    /// Constraints: Required, minimum length 3, maximum length 30.
    /// </summary>
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    /// <summary>
    /// User's password (Note: In a real application, this should always be stored as a hash).
    /// Requirements: 8-128 characters, must contain uppercase, lowercase, digit, and special character.
    /// </summary>
    [Required]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,128}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
    public string Password { get; set; }

    /// <summary>
    /// User's email address.
    /// Note: You might want to add a unique index configuration in your DbContext.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)] // Standard length for email addresses
    public string E_mail { get; set; }

    // --- JSON Text Fields for Complex Data ---

    /// <summary>
    /// User's application settings stored as a JSON string (Text key-value set).
    /// SQLite is excellent for storing large text fields like this.
    /// </summary>
    [Required]
    public string Settings { get; set; } = "{}";

    /// <summary>
    /// Key-value set representing user-specific page configurations, stored as a JSON string.
    /// </summary>
    [Required]
    public string Pages { get; set; } = "{}";

    /// <summary>
    /// Key-value set representing external or internal links, stored as a JSON string.
    /// </summary>
    [Required]
    public string Links { get; set; } = "{}";

    // You could also add a constructor to initialize the GUID and default JSON values
    public User()
    {
        // Initializes the ID automatically when a new entity is created.
        if (ID == Guid.Empty)
        {
            ID = Guid.NewGuid();
        }
    }
}
