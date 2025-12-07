using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Service;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Tests.Chirp.Infrastructure;

public class CommentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    private readonly ICommentService _commentService;
    private readonly Author _author;
    private readonly Cheep _cheep;

    public CommentServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDbContext>()
            .UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated();

        _author = new Author { UserName = "Alice", Email = "alice@itu.dk" };
        var author2 = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        
        _cheep = new Cheep
        {
            Author = author2, 
            Text = "Test Cheep", 
            TimeStamp = DateTime.UtcNow
        };

        _context.Authors.Add(_author);
        _context.Cheeps.Add(_cheep);
        _context.SaveChanges();

        var commentRepo = new CommentRepository(_context);
        _commentService = new CommentService(commentRepo);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetCommentsForCheep_ShouldReturnDtos()
    {
        await _commentService.AddComment(_cheep.CheepId, _author.Id, "Hello Comment1");
        await _commentService.AddComment(_cheep.CheepId, _author.Id, "Hello Comment2");

        var result = await _commentService.GetCommentsForCheep(_cheep.CheepId);

        result.Should().HaveCount(2);
        result.Should().BeOfType<List<CommentDTO>>();
        
        result.All(c => c.Author.Name == _author.UserName && c.Author.Email == _author.Email).Should().BeTrue();
        result.Should().Contain(c => c.Message == "Hello Comment1" || c.Message == "Hello Comment2" ); 
    }
    
    [Fact]
    public async Task GetCommentsForCheep_ShouldReturnEmpty_WhenNoComments()
    {
        var result = await _commentService.GetCommentsForCheep(_cheep.CheepId);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCommentsByAuthor_ShouldReturnDtos()
    {
        await _commentService.AddComment(_cheep.CheepId, _author.Id, "Hello Comment1");
        await _commentService.AddComment(_cheep.CheepId, _author.Id, "Hello Comment2");

        var result = await _commentService.GetCommentsByAuthor("Alice", 1, 10);

        result.Should().HaveCount(2);
        result.Should().BeOfType<List<CommentDTO>>();
        
        result.All(c => c.Author.Name == _author.UserName && c.Author.Email == _author.Email).Should().BeTrue();
        result.Should().Contain(c => c.Message == "Hello Comment1" || c.Message == "Hello Comment2" ); 
    }

    [Fact]
    public async Task GetCommentsByAuthor_ShouldReturnEmpty_WhenNoComments()
    {
        var result = await _commentService.GetCommentsByAuthor("Alice", 1, 10);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCommentsByAuthor_ShouldReturnPageResult()
    {
        for (var i = 0; i < 12; i++)
        {
            await _commentService.AddComment(_cheep.CheepId, _author.Id, "Comment" + i);
        }

        var page1 = await _commentService.GetCommentsByAuthor("Alice", 1, 10);
        page1.Should().HaveCount(10);

        var page2 = await _commentService.GetCommentsByAuthor("Alice", 2, 10);
        page2.Should().HaveCount(2);
        
        page1.Should().NotContain(c =>
            page2.Any(f => f.Author == c.Author && f.Message == c.Message && f.TimeStamp == c.TimeStamp));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCommentsByAuthor_InvalidAuthor_ShouldThrow(string authorName)
    {
        Func<Task> act = () => _commentService.GetCommentsByAuthor(authorName, 1, 10);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Author is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetCommentsByAuthor_InvalidPageNumber_ShouldThrow(int pageNumber)
    {
        Func<Task> act = () => _commentService.GetCommentsByAuthor("Alice", pageNumber, 10);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Pagenumber must be greater than 0*");
    }
    
    [Fact]
    public async Task AddComment_ShouldAddComment()
    {
        var commentId = await _commentService.AddComment(_cheep.CheepId, _author.Id, "Hello World");

        commentId.Should().BeGreaterThan(0);
        _context.Comments.Should().Contain(c => c.CommentId == commentId);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddComment_InvalidText_ShouldThrow(string text)
    {
        Func<Task> act = () => _commentService.AddComment(_cheep.CheepId, _author.Id, text);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Cheep text is required and cannot be null or empty*");
    }

    [Fact]
    public async Task AddComment_TextTooLong_ShouldThrow()
    {
        var longText = new string('x', 161);
        
        Func<Task> act = () => _commentService.AddComment(_cheep.CheepId, _author.Id, longText);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot exceed 160 characters*");
    }
    
    [Fact]
    public async Task AddComment_Text160Long_ShouldNotThrowArgumentException()
    {
        var longText = new string('x', 160);
        
        Func<Task> act = () => _commentService.AddComment(_cheep.CheepId, _author.Id, longText);

        await act.Should().NotThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task DeleteComment_ShouldRemoveComment()
    {
        var commentId = await _commentService.AddComment(_cheep.CheepId, _author.Id, "Delete comment");

        var deleted = await _commentService.DeleteComment(commentId);
        deleted.Should().BeTrue();

        _context.Comments.Should().NotContain(c => c.CommentId == commentId);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnFalse_WhenNotExists()
    {
        var deleted = await _commentService.DeleteComment(999);
        deleted.Should().BeFalse();
    }
}