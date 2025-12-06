using Chirp.Core;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Tests.Chirp.Infrastructure;

public class CommentRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    private readonly ICommentRepository _commentRepo;
    private readonly Author _author;
    private readonly Cheep _cheep;

    public CommentRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDbContext>().UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated();

        _author = new Author { UserName = "Alice", Email = "alice@itu.dk"};
        var author2 = new Author { UserName = "Bob", Email = "bob@itu.dk"};
        
        _cheep = new Cheep {
            Author = author2, 
            Text = "Test Cheep", 
            TimeStamp = DateTime.UtcNow, 
        };
        
        _context.Authors.Add(_author);
        _context.Cheeps.Add(_cheep);
        _context.SaveChanges();

        _commentRepo = new CommentRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddComment_ShouldAddCommentToCheep()
    {
        await _commentRepo.AddComment(_cheep, _author,"Hello Comment");

        _cheep.Author.UserName.Should().Be("Bob"); 
        _cheep.Text.Should().Be("Test Cheep");
            
        _context.Comments.Should().ContainSingle(c => c.Text == "Hello Comment" && c.Author.UserName == "Alice");
        _cheep.Comments.Should().ContainSingle(c => c.Text == "Hello Comment" && c.Author.UserName == "Alice");
    }
}