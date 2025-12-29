using Chirp.Core;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Service;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Chirp.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Tests.Chirp.Infrastructure;
public class CheepServiceTests : IDisposable
{ 
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    private readonly ICheepService _cheepService;

    public CheepServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDbContext>().UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated();

        var a1 = new Author { UserName = "Alice", Email = "alice@itu.dk" };
        var a2 = new Author { UserName = "Bob", Email = "bob@itu.dk" };

        _context.Authors.AddRange(
            a1, a2
        );

        _context.Cheeps.AddRange(
            new Cheep { Author = a1, Text = "Hello!", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = a1, Text = "Second cheep", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = a2, Text = "Third cheep", TimeStamp = DateTime.UtcNow }
        );

        _context.SaveChanges();
        var cheepRepo = new CheepRepository(_context);
        _cheepService = new CheepService(cheepRepo);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
    
    [Fact]
    public async Task GetCheeps_ShouldReturnCheeps()
    {
        var cheeps = await _cheepService.GetCheeps(1, 10);

        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(3);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author.Name) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.TimeStamp));

        var cheepsPage2 = await _cheepService.GetCheeps(2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(0)]
    public async Task GetCheeps_InvalidPage_ShouldThrow(int page)
    {
        Func<Task> act = async () => await _cheepService.GetCheeps(page, 10);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
    }
    
    [Fact]
    public async Task GetCheeps_EmptyPage_ShouldReturnEmptyList()
    {
        var cheeps = await _cheepService.GetCheeps(10, 10);
        cheeps.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCheepsByAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = await _cheepService.GetCheepsByAuthor("Alice", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author.Name == "Alice");
        
        var cheepsPage2 = await _cheepService.GetCheepsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(null)]
   public async Task GetCheepsByAuthor_InvalidAuthor_ShouldThrow(string author)
   {
       Func<Task> act = async () => await _cheepService.GetCheepsByAuthor(author, 1, 10);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");
   }
   
   [Theory]
   [InlineData(-1)]
   [InlineData(-2)]
   [InlineData(0)]
   public async Task GetCheepsByAuthor_InvalidPage_ShouldThrow(int page)
   {
       Func<Task> act = async () => await _cheepService.GetCheepsByAuthor("Alice",page, 10);

       await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
           .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
   }

   [Theory]
   [InlineData("NonExisting123")]
   [InlineData("....")]
   [InlineData(".")]
   public async Task GetCheepsFromAuthor_UnknownAuthor_ShouldReturnEmpty(string author)
   {
       var cheeps = await _cheepService.GetCheepsByAuthor(author, 1, 10);
       cheeps.Should().BeEmpty();
   }
   
   [Fact]
   public async Task AddCheep_ValidAuthorAndText()
   {
       var author = new Author { UserName = "Alice", Email = "alice@itu.dk" };
       await _cheepService.AddCheep(author, "Test");

       var cheeps = await _cheepService.GetCheepsByAuthor("Alice", 1, 10);
       cheeps.Should().NotBeEmpty();
       cheeps.Count.Should().Be(3);
       
       cheeps.Should().ContainSingle(c => c.Author.Name == "Alice" && c.Message == "Test");
   }
   
   [Fact]
   public async Task AddCheep_InvalidLengthText_ShouldThrow()
   {
       var longCheep = new string('x', 161);

       var author = new Author { UserName = "Helge", Email = "helge@itu.dk" };
       Func<Task> act = async () => await _cheepService.AddCheep(author, longCheep);

       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*cannot exceed 160 characters*");
   }
   
   [Fact]
   public async Task AddCheep_160lengthText_ShouldNotThrow()
   {
       var longCheep = new string('x', 160);
       var author = new Author { UserName = "Helge", Email = "helge@itu.dk" };
       Func<Task> act = async () => await _cheepService.AddCheep(author, longCheep);

       await act.Should().NotThrowAsync<ArgumentException>();
   }
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(null)]
   public async Task AddCheep_InvalidText_ShouldThrow(string text)
   {
       var author = new Author { UserName = "Alice", Email = "alice@itu.dk" };
       Func<Task> act = async () => await _cheepService.AddCheep(author, text);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Cheep text is required and cannot be null or empty*");
   }
   
   [Fact]
   public async Task AddCheep_NullAuthor_ShouldThrow()
   {
       Func<Task> act = () => _cheepService.AddCheep(null!, "Hello");
       await act.Should().ThrowAsync<ArgumentNullException>()
           .WithMessage("*Author is required*");
   }
   
   [Fact]
   public async Task GetCheepsByAuthors_EmptyList_ShouldReturnEmpty()
   {
       var result = await _cheepService.GetCheepsByAuthors([], 1, 10);
       result.Should().BeEmpty();
   }

   [Fact]
   public async Task GetCheepsByAuthors_ShouldReturnCheepsFromMultipleAuthors()
   {
       var result = await _cheepService.GetCheepsByAuthors(["Alice", "Bob" ], 1, 10);
       result.Should().HaveCount(3);
       result.Should().Contain(c => c.Author.Name == "Alice" || c.Author.Name == "Bob");
   }

   [Theory]
   [InlineData(0)]
   [InlineData(-1)]
   public async Task GetCheepsByAuthors_InvalidPage_ShouldThrow(int page)
   {
       Func<Task> act = () => _cheepService.GetCheepsByAuthors(["Alice"], page, 10);

       await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
           .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
   }
   
   [Fact]
   public async Task DeleteCheep_ShouldDeleteCheep()
   {
       var cheep = _context.Cheeps.First();
       var result = await _cheepService.DeleteCheep(cheep.CheepId);

       result.Should().BeTrue();
       _context.Cheeps.Should().NotContain(c => c.CheepId == cheep.CheepId);
   }

   [Fact]
   public async Task DeleteCheep_ShouldReturnFalse_WhenCheepDoesNotExist()
   {
       var result = await _cheepService.DeleteCheep(999);
       result.Should().BeFalse();
   }
}