using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Models;

namespace Chirp.Razor.Database;

public class ChirpDbContext : DbContext
{
    public ChirpDbContext(DbContextOptions<ChirpDbContext> options) : base(options) { }

    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Cheep> Cheeps { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("user_id");
            entity.Property(a => a.Name).HasColumnName("username");
            entity.Property(a => a.Email).HasColumnName("email");
            entity.Ignore(a => a.Cheeps);
        });

        modelBuilder.Entity<Cheep>(entity =>
        {
            entity.ToTable("message");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("message_id");
            entity.Property(c => c.Text).HasColumnName("text");
            entity.Property(c => c.TimeStamp).HasColumnName("pub_date");
            entity.Property(c => c.AuthorId).HasColumnName("author_id");

            entity.HasOne(c => c.Author)
                .WithMany(a => a.Cheeps)
                .HasForeignKey(c => c.AuthorId);
        });
    }
}