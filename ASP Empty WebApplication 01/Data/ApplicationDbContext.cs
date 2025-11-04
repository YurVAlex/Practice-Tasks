using Microsoft.EntityFrameworkCore;
using ASP_Empty_WebApplication_01.Models;

namespace ASP_Empty_WebApplication_01.Data;

/// <summary>
/// Entity Framework DbContext for the application.
/// Manages database operations for User entities.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table in the database.
    /// </summary>
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.ID);
            
            // Configure Name field
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30);

            // Configure Email field with unique index
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasIndex(e => e.Email)
                .IsUnique();

            // Configure Password field
            entity.Property(e => e.Password)
                .IsRequired();

            // Configure JSON fields with default values
            entity.Property(e => e.Settings)
                .IsRequired()
                .HasDefaultValue("{}");

            entity.Property(e => e.Projects)
                .IsRequired()
                .HasDefaultValue("{}");

            entity.Property(e => e.Links)
                .IsRequired()
                .HasDefaultValue("{}");

            // Configure table name
            entity.ToTable("Users");
        });
    }
}
