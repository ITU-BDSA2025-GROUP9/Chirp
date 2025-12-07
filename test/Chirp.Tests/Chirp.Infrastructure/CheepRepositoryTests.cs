using Chirp.Core;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Chirp.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Tests.Chirp.Infrastructure;

public class CheepRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    
    private readonly ICheepRepository _cheepRepo;
    private readonly Author _a1; 

    public CheepRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDbContext>().UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated();

        _a1 = new Author { UserName = "Alice", Email = "alice@itu.dk" };
        var a2 = new Author { UserName = "Bob", Email = "bob@itu.dk" };

        _context.Authors.AddRange(
            _a1, a2
        );

        _context.Cheeps.AddRange(
            new Cheep { Author = _a1, Text = "Hello!", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = _a1, Text = "Second cheep", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = a2, Text = "Third cheep", TimeStamp = DateTime.UtcNow }
        );

        _context.SaveChanges();
        _cheepRepo = new CheepRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
    
    [Fact]
    public async Task GetAllCheeps_ShouldReturnCheeps()
    {
        var cheeps = await _cheepRepo.GetAllCheeps(1, 10);
    
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(3);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author.UserName) && !string.IsNullOrWhiteSpace(c.Text));
        
        var cheepsPage2 = await _cheepRepo.GetAllCheeps(2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllCheeps_ShouldReturnPageResult()
    {
        for (var i = 0; i < 10; i++)
        {
            await _cheepRepo.AddCheep(_a1, "New message" + i);
        }

        var cheeps = await _cheepRepo.GetAllCheeps(1, 10);

        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(10);
        
        cheeps = await _cheepRepo.GetAllCheeps(2, 10);
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(3);
    }
    
    [Fact]
    public async Task GetAllCheeps_Page2_ShouldNotContainFirstPageCheeps()
    {
        for (var i = 0; i < 10; i++)
        {
            await _cheepRepo.AddCheep(_a1, "New message" + i);
        }

        var firstPage = await _cheepRepo.GetAllCheeps(1, 5);
        var secondPage = await _cheepRepo.GetAllCheeps(2, 5);

        firstPage.Should().NotBeEmpty();
        firstPage.Count.Should().Be(5);

        secondPage.Should().NotBeEmpty();
        secondPage.Count.Should().Be(5);

        secondPage.Should().NotContain(c =>
            firstPage.Any(f => f.Author == c.Author && f.Text == c.Text && f.TimeStamp == c.TimeStamp));
    }
    
    [Fact]
    public async Task GetCheepsByAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 10);
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Alice");
        
        var cheepsPage2 = await _cheepRepo.GetCheepsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCheepsByAuthor_ShouldReturnPageResult()
    {
        for (var i = 0; i < 10; i++)
        {
            await _cheepRepo.AddCheep(_a1, "New message" + i);
        }

        var cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 10);
        
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(10);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Alice");
        
        cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 2, 10);
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Alice");
    }
    
    [Fact]
    public async Task GetCheepsByAuthor_Page2_ShouldNotContainFirstPageCheeps()
    {
        for (var i = 0; i < 10; i++)
        {
            await _cheepRepo.AddCheep(_a1, "New message" + i);
        }

        var firstPage = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 5);
        var secondPage = await _cheepRepo.GetCheepsByAuthor("Alice", 2, 5);

        firstPage.Should().NotBeEmpty();
        firstPage.Count.Should().Be(5);

        secondPage.Should().NotBeEmpty();
        secondPage.Count.Should().Be(5);

        secondPage.Should().NotContain(c =>
            firstPage.Any(f => f.Author == c.Author && f.Text == c.Text && f.TimeStamp == c.TimeStamp));
    }
    
    [Fact]
    public async Task GetCheepsByAuthors_ShouldReturnCorrectCheeps()
    {
        var authors = new List<string> { "Alice", "Bob" };
        var a2 = new Author { UserName = "Bob", Email = "bob@itu.dk" };
       
        await _cheepRepo.AddCheep(_a1, "Alice Cheep 1");
        await _cheepRepo.AddCheep(a2, "Bob Cheep 1");
       
        var page1 = await _cheepRepo.GetCheepsByAuthors(authors, 1, 3); 

        page1.Should().NotBeEmpty();
        page1.Should().OnlyContain(c => c.Author.UserName == "Alice" || c.Author.UserName == "Bob");
        page1.Should().HaveCount(3);
   
        var page2 = await _cheepRepo.GetCheepsByAuthors(authors, 2, 3);
        page2.Should().OnlyContain(c => c.Author.UserName == "Alice" || c.Author.UserName == "Bob");
    }
    
    [Fact]
    public async Task GetCheepsByAuthors_EmptyList_ShouldReturnEmpty()
    {
        var result = await _cheepRepo.GetCheepsByAuthors([], 1, 10);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task AddCheep_ShouldAddCheepToDB()
    {
        await _cheepRepo.AddCheep(_a1, "Alice Cheep");
        
        _context.Cheeps.Should().ContainSingle(c => c.Text == "Alice Cheep" &&  c.Author.UserName == "Alice");
        _a1.Cheeps.Should().ContainSingle(c => c.Text == "Alice Cheep" &&  c.Author.UserName == "Alice");
        
        var cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 10);
        cheeps.Should().Contain(c => c.Text == "Alice Cheep" &&  c.Author.UserName == "Alice");
    }
    
    [Fact]
    public async Task AddCheepAuthor_ShouldAddAuthor()
    {
        var author = new Author { UserName = "NewUser", Email = "newuser@itu.dk" };
        await _cheepRepo.AddCheep(author, "Test");
        
        var cheeps = await _cheepRepo.GetCheepsByAuthor("NewUser", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(1);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "NewUser" && c.Text == "Test");
        
        _context.Authors.Should().Contain(author);
    } 
    
    [Fact]
    public async Task DeleteCheep_ShouldRemoveCheep_WhenExists()
    {
        var cheepId = await _cheepRepo.AddCheep(_a1, "Delete Test");
        var result = await _cheepRepo.DeleteCheep(cheepId);
        
        result.Should().BeTrue();
        _context.Cheeps.Should().NotContain(c => c.CheepId == cheepId);
    }
    
    [Fact]
    public async Task DeleteCheep_ShouldNotRemoveOtherCheeps()
    {
        var cheepId = await _cheepRepo.AddCheep(_a1, "Delete Test");
        
        var cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 10);
        cheeps.Should().NotBeEmpty();
        cheeps.Should().Contain(c => c.Text == "Delete Test");
        
        var result = await _cheepRepo.DeleteCheep(cheepId);
        result.Should().BeTrue();
        
        cheeps = await _cheepRepo.GetCheepsByAuthor("Alice", 1, 10);
        cheeps.Should().NotBeEmpty();
        cheeps.Should().NotContain(c => c.Text == "Delete Test");
    }

    [Fact]
    public async Task DeleteCheep_ShouldReturnFalse_WhenNotExist()
    {
        var result = await _cheepRepo.DeleteCheep(999);
        
        result.Should().BeFalse(); 
        _context.Cheeps.Should().NotContain(c => c.CheepId == 999);
    }
    
    [Fact]
    public async Task GetCheepById_ShouldReturnCorrectCheep()
    {
        var cheepId = await _cheepRepo.AddCheep(_a1, "Test");

        var resultingCheep = await _cheepRepo.GetCheepById(cheepId);
        
        resultingCheep!.CheepId.Should().Be(cheepId);
        resultingCheep.Author.UserName.Should().Be("Alice");
        resultingCheep.Text.Should().Be("Test");
    }
    
    [Fact]
    public async Task GetAllCheeps_ShouldIncludeCommentsAndAuthors()
    {
        var cheepId = await _cheepRepo.AddCheep(_a1, "Test");
        var cheep = await _cheepRepo.GetCheepById(cheepId);
        
        var commentAuthor = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        cheep!.Comments = [new Comment { Author = commentAuthor, Text = "Hello Test" }];
        
        var result = await _cheepRepo.GetAllCheeps(1, 10);
        
        var cheepWithComment = result.First(c => c.CheepId == cheepId);
        cheepWithComment.Author.UserName.Should().Be("Alice");
        cheepWithComment.Text.Should().Be("Test");
        
        cheepWithComment.Comments.Should().NotBeEmpty();
        cheepWithComment.Comments.First().Author.UserName.Should().Be("Bob");
        cheepWithComment.Comments.First().Text.Should().Be("Hello Test");
    }
}