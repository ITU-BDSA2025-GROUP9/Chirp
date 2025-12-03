using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
public class ChirpDbContext : IdentityDbContext<Author, IdentityRole<int>, int>
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

    public DbSet<Comment> Comments { get; set; } = null!;
    
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
        base.OnModelCreating(modelBuilder);
        // --- Cheep entity mapping ---
        modelBuilder.Entity<Cheep>(entity =>
        {
            // Table name
            entity.ToTable("message");

            // Primary key
            entity.HasKey(c => c.CheepId);

            // Column mappings
            entity.Property(c => c.CheepId).HasColumnName("message_id");
            entity.Property(c => c.Text).HasColumnName("text");

            // Map and convert the timestamp column between DateTime and string
            entity.Property(c => c.TimeStamp)
                .HasColumnName("pub_date");

            entity.Property(c => c.AuthorId).HasColumnName("author_id");

            // Define one-to-many relationship: Author → Cheeps
            entity.HasOne(c => c.Author)
                .WithMany(a => a.Cheeps)
                .HasForeignKey(c => c.AuthorId);
        });
        
        // --- Comment entity mapping ---
        modelBuilder.Entity<Comment>(entity =>
        {
            // Table name
            entity.ToTable("comment");

            // Primary key
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id).HasColumnName("comment_id");
            entity.Property(c => c.Content).HasColumnName("text");
            entity.Property(c => c.CreatedAt).HasColumnName("pub_date");

            // Foreign keys
            entity.Property(c => c.CheepId).HasColumnName("message_id");
            entity.Property(c => c.AuthorId).HasColumnName("author_id");

            // Comment → Cheep relationship
            entity.HasOne(c => c.Cheep)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.CheepId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Author relationship
            entity.HasOne(c => c.Author)
                .WithMany() // Add a list to the author class maybe
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
