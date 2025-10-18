using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Infrastructure.Database;

/// <summary>
/// Represents the Entity Framework Core database context for the Chirp application.
/// This context defines entity sets, configures table mappings, and manages the connection
/// between C# domain models and the underlying SQLite database.
/// </summary>
/// <remarks>
/// The <see cref="ChirpDbContext"/> class is registered with dependency injection and is
/// used by repositories and services to query and persist entities such as
/// <see cref="Author"/> and <see cref="Cheep"/>.
/// </remarks>
public class ChirpDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChirpDbContext"/> class.
    /// </summary>
    /// <param name="options">The options used to configure the database connection and behavior.</param>
    public ChirpDbContext(DbContextOptions<ChirpDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the collection of authors (users) stored in the database.
    /// </summary>
    /// <remarks>
    /// This is mapped to the <c>user</c> table in SQLite.
    /// Each <see cref="Author"/> can have multiple related <see cref="Cheep"/> entities.
    /// </remarks>
    public DbSet<Author> Authors { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of cheeps (posts) stored in the database.
    /// </summary>
    /// <remarks>
    /// This is mapped to the <c>message</c> table in SQLite.
    /// Each <see cref="Cheep"/> has one <see cref="Author"/> as its creator.
    /// </remarks>
    public DbSet<Cheep> Cheeps { get; set; } = null!;

    /// <summary>
    /// Configures entity relationships, table names, and column mappings for the database schema.
    /// </summary>
    /// <param name="modelBuilder">Provides a fluent API for configuring entity mappings.</param>
    /// <remarks>
    /// This method is automatically invoked by Entity Framework when the model is being created.
    /// It ensures that C# property names align with SQLite column names and that relationships
    /// such as one-to-many between <see cref="Author"/> and <see cref="Cheep"/> are properly defined.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Author entity mapping ---
        modelBuilder.Entity<Author>(entity =>
        {
            // Table name
            entity.ToTable("user");

            // Primary key
            entity.HasKey(a => a.Id);

            // Column mappings
            entity.Property(a => a.Id).HasColumnName("user_id");
            entity.Property(a => a.Name).HasColumnName("username");
            entity.Property(a => a.Email).HasColumnName("email");

            // Ignore properties not persisted to the database
            entity.Ignore(a => a.AuthorId);
        });

        // --- Cheep entity mapping ---
        modelBuilder.Entity<Cheep>(entity =>
        {
            // Table name
            entity.ToTable("message");

            // Primary key
            entity.HasKey(c => c.Id);

            // Column mappings
            entity.Property(c => c.Id).HasColumnName("message_id");
            entity.Property(c => c.Text).HasColumnName("text");

            // Map and convert the timestamp column between DateTime and string
            entity.Property(c => c.TimeStamp)
                .HasColumnName("pub_date")
                .HasConversion(
                    // Convert DateTime → string when saving to the database
                    v => v.ToString("yyyy-MM-dd HH:mm:ss"),
                    // Convert string → DateTime when reading from the database
                    v => ParseOrDefault(v)
                );

            entity.Property(c => c.AuthorId).HasColumnName("author_id");

            // Ignore properties not persisted to the database
            entity.Ignore(c => c.CheepId);

            // Define one-to-many relationship: Author → Cheeps
            entity.HasOne(c => c.Author)
                .WithMany(a => a.Cheeps)
                .HasForeignKey(c => c.AuthorId);
        });
    }

    /// <summary>
    /// Attempts to parse a string into a <see cref="DateTime"/> object.
    /// Returns <see cref="DateTime.UtcNow"/> if parsing fails.
    /// </summary>
    /// <param name="value">The string representation of the date/time value.</param>
    /// <returns>
    /// A valid <see cref="DateTime"/> object if parsing succeeds;
    /// otherwise, <see cref="DateTime.UtcNow"/>.
    /// </returns>
    private static DateTime ParseOrDefault(string value)
    {
        if (DateTime.TryParse(value, out var parsed))
            return parsed;
        return DateTime.UtcNow;
    }
}
