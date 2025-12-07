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
    public async Task GetCommentsForCheep_ShouldReturnCheep()
    {
        await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment1");
        await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment2");
        
        var cheeps = await _commentRepo.GetCommentsForCheep(_cheep.CheepId);
        cheeps.Should().NotBeEmpty();
        cheeps.Should().HaveCount(2);
        cheeps.Should().OnlyContain(c => c.CheepId == _cheep.CheepId);
    }
    [Fact]
    public async Task GetCommentsForCheep_ShouldReturnEmpty_WhenNoComments()
    {
        var comments = await _commentRepo.GetCommentsForCheep(_cheep.CheepId);
        comments.Should().NotBeNull();
        comments.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCommentsByAuthor_ShouldReturnCorrectAuthor()
    {
        await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment1");
        await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment2");
        
        var cheeps = await _commentRepo.GetCommentsByAuthor("Alice", 1, 10);
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Alice");
        
        var cheepsPage2 = await _commentRepo.GetCommentsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCommentsByAuthor_ShouldReturnPageResult()
    {
        for (var i = 0; i < 12; i++)
        {
            await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment" + i);
        }

        var comments = await _commentRepo.GetCommentsByAuthor("Alice", 1, 10);
        
        comments.Should().NotBeNullOrEmpty();
        comments.Count.Should().Be(10);
        comments.Should().OnlyContain(c => c.Author.UserName == "Alice");
        
        comments = await _commentRepo.GetCommentsByAuthor("Alice", 2, 10);
        comments.Should().NotBeNullOrEmpty();
        comments.Count.Should().Be(2);
        comments.Should().OnlyContain(c => c.Author.UserName == "Alice");
    }
    
    [Fact]
    public async Task GetCheepsByAuthor_Page2_ShouldNotContainFirstPageCheeps()
    {
        for (var i = 0; i < 10; i++)
        {
            await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment" + i);
        }

        var firstPage = await _commentRepo.GetCommentsByAuthor("Alice", 1, 5);
        var secondPage =  await _commentRepo.GetCommentsByAuthor("Alice", 2, 5);
        
        firstPage.Should().NotBeEmpty();
        firstPage.Count.Should().Be(5);

        secondPage.Should().NotBeEmpty();
        secondPage.Count.Should().Be(5);

        secondPage.Should().NotContain(c =>
            firstPage.Any(f => f.Author == c.Author && f.Text == c.Text && f.TimeStamp == c.TimeStamp));
    }
    
    [Fact]
    public async Task AddComment_ShouldAddCommentToCheep()
    {
        await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment");

        _cheep.Author.UserName.Should().Be("Bob"); 
        _cheep.Text.Should().Be("Test Cheep");
            
        _context.Comments.Should().ContainSingle(c => c.Text == "Hello Comment" && c.Author == _author);
        _cheep.Comments.Should().ContainSingle(c => c.Text == "Hello Comment" && c.Author == _author);
    }
    
    [Fact]
    public async Task AddComment_ShouldReturnValidId()
    {
        var commentId = await _commentRepo.AddComment(_cheep.CheepId, _author.Id, "Hello Comment");

        commentId.Should().BeGreaterThan(0);
        _context.Comments.Any(c => c.CommentId == commentId).Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteComment_ShouldRemoveComment_WhenExists()
    {
        var commentId = await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment");
        var result = await _commentRepo.DeleteComment(commentId);
        
        result.Should().BeTrue();
        _context.Comments.Should().NotContain(c => c.CommentId == commentId);
    }
    
    [Fact]
    public async Task DeleteCheep_ShouldNotRemoveOtherComments()
    {
        var commentId1 = await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment1");
        var commentId2 = await _commentRepo.AddComment(_cheep.CheepId, _author.Id,"Hello Comment2");

        var comments = await _commentRepo.GetCommentsByAuthor("Alice", 1, 10); 
        comments.Should().NotBeEmpty();
        comments.Should().HaveCount(2);
        
        var result = await _commentRepo.DeleteComment(commentId1);
        result.Should().BeTrue();
        
        comments = await _commentRepo.GetCommentsByAuthor("Alice", 1, 10); 
        comments.Should().NotBeEmpty();
        comments.Should().OnlyContain(c => c.CommentId == commentId2);
        comments.Should().NotContain(c => c.CommentId == commentId1);
    }

    [Fact]
    public async Task DeleteCheep_ShouldReturnFalse_WhenNotExist()
    {
        var result = await _commentRepo.DeleteComment(999);
        result.Should().BeFalse(); 
        _context.Comments.Should().NotContain(c => c.CommentId == 999);
    }
}