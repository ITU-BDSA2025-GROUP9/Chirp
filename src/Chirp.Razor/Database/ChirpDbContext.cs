using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Models;

namespace Chirp.Razor.Database
{
    public class ChirpDbContext : DbContext
    {
        public ChirpDbContext(DbContextOptions<ChirpDbContext> options)
            : base(options) { }

        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Cheep> Cheeps { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Author -> "user" table
            modelBuilder.Entity<Author>().ToTable("user");
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(a => a.AuthorId);
                entity.Property(a => a.AuthorId).HasColumnName("user_id");
                entity.Property(a => a.Name).HasColumnName("username");
                entity.Property(a => a.Email).HasColumnName("email");
            });

            // Map Cheep -> "message" table
            modelBuilder.Entity<Cheep>().ToTable("message");
            modelBuilder.Entity<Cheep>(entity =>
            {
                entity.HasKey(c => c.CheepId);
                entity.Property(c => c.CheepId).HasColumnName("message_id");
                entity.Property(c => c.Text).HasColumnName("text");
                entity.Property(c => c.TimeStamp).HasColumnName("pub_date");
                entity.Property(c => c.AuthorId).HasColumnName("author_id");

                entity.HasOne(c => c.Author)
                    .WithMany(a => a.Cheeps)
                    .HasForeignKey(c => c.AuthorId);
            });
        }
    }
}