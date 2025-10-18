using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Infrastructure.Database;

/// <summary>
/// Represents the Entity Framework Core database context for the Chirp application.
/// Defines entity sets and configures mappings between C# models and SQLite database tables.
/// </summary>
public class ChirpDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChirpDbContext"/> class.
    /// </summary>
    /// <param name="options">The options used to configure the database connection.</param>
    public ChirpDbContext(DbContextOptions<ChirpDbContext> options) : base(options) { }

    /// <summary>
    /// Represents the collection of authors (users) stored in the database.
    /// Mapped to the <c>user</c> table in SQLite.
    /// </summary>
    public DbSet<Author> Authors { get; set; } = null!;

    /// <summary>
    /// Represents the collection of cheeps (posts) stored in the database.
    /// Mapped to the <c>message</c> table in SQLite.
    /// </summary>
    public DbSet<Cheep> Cheeps { get; set; } = null!;

    /// <summary>
    /// Configures the mapping between C# entity properties and database columns.
    /// This ensures correct naming conventions and relationships when using SQLite.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map Author entity to "user" table
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("user_id");
            entity.Property(a => a.Name).HasColumnName("username");
            entity.Property(a => a.Email).HasColumnName("email");
            entity.Ignore(a => a.AuthorId); // just to reinforce [NotMapped]
        });

        modelBuilder.Entity<Cheep>(entity =>
        {
            entity.ToTable("message");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("message_id");
            entity.Property(c => c.Text).HasColumnName("text");
            entity.Property(c => c.TimeStamp)
                .HasColumnName("pub_date")
                .HasConversion(
                    // Store as ISO-formatted string
                    v => v.ToString("yyyy-MM-dd HH:mm:ss"),
                    // Read safely, defaulting to current UTC if invalid
                    v => ParseOrDefault(v)
                );

            entity.Property(c => c.AuthorId).HasColumnName("author_id");
            entity.Ignore(c => c.CheepId); // ignore alias

            entity.HasOne(c => c.Author)
                .WithMany(a => a.Cheeps)
                .HasForeignKey(c => c.AuthorId);
        });


    }
    private static DateTime ParseOrDefault(string value)
    {
        if (DateTime.TryParse(value, out var parsed))
            return parsed;
        return DateTime.UtcNow;
    }

}
