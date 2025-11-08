using FluentAssertions;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure.Database; 
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Service;
using Chirp.Core.Interfaces; 
using Chirp.Core.DTO;
using Chirp.Core;

namespace Chirp.Razor.Tests;
public class UnitTests : IDisposable
{ 
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    private readonly IRepository _repo;
    private readonly ICheepService _service;

    public UnitTests()
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
        _repo = new Repository(_context);
        _service = new CheepService(_repo);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
    
    [Fact]
    public void CheepToDTO_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Alice", Email = "alice@itu.dk" };
        var ts = DateTime.UtcNow;
        var cheep = new Cheep
        {
            Author = author,
            Text = "Test",
            TimeStamp = ts
        };

        var dto = CheepService.CheepToDto(cheep);

        dto.Author.Should().Be("Alice");
        dto.Message.Should().Be("Test");
        dto.AuthorEmail.Should().Be("alice@itu.dk");
        dto.TimeStamp.Should().Be(cheep.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture));
    }
 
    [Fact]
    public void CheepDTO_ShouldHaveValidProperties()
    {
        var cheep = new CheepDTO("Bob", "Hello world", "08/02/23 14:19:38", "bob@itu.dk");

        cheep.Author.Should().Be("Bob");
        cheep.Message.Should().Be("Hello world");
        cheep.TimeStamp.Should().Be("08/02/23 14:19:38");
        cheep.AuthorEmail.Should().Be("bob@itu.dk");
    }
   
    [Theory]
    [InlineData("", "Hello")]
    [InlineData("   ", "Hello")]
    [InlineData(null, "Hello")]
    [InlineData("Alice", "")]
    [InlineData("Alice", "   ")]
    [InlineData("Alice", null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData(null, null)]
    public void CheepDTO_InvalidArguments_ShouldThrowException(string author, string message)
    {
        Action act = () => new CheepDTO(author, message, "10/15/25 14:30:00", $"{author}@itu.dk");
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task GetAllCheeps_ShouldReturnCheeps()
    {
        var cheeps = await _repo.GetAllCheeps(1, 10);
    
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Count.Should().Be(3);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author.UserName) && !string.IsNullOrWhiteSpace(c.Text));
        
        var cheepsPage2 = await _repo.GetAllCheeps(2, 10);
        cheepsPage2.Should().BeEmpty();
    }
 
    [Fact]
    public async Task GetCheepsByAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = await _repo.GetCheepsByAuthor("Alice", 1, 10);
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Alice");
        
        var cheepsPage2 = await _repo.GetCheepsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Create_ShouldAddAuthorToDatabase()
    {
        var author = await _repo.Create("Helge", "helge@itu.dk");

        author.Should().NotBeNull();
        author.UserName.Should().Be("Helge");
        author.Email.Should().Be("helge@itu.dk");

        await _repo.AddCheep(author.UserName, author.Email, "Test");
        var cheeps = await _repo.GetCheepsByAuthor("Helge", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(1);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "Helge");
    }

    [Fact]
    public async Task AddCheep_ShouldCreateAuthor()
    {
        await _repo.AddCheep("NewUser", "newuser@itu.dk", "Test");

        var author = await _repo.FindByName("NewUser");
        author.Should().NotBeNull();
        
        var cheeps = await _repo.GetCheepsByAuthor("NewUser", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(1);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "NewUser");
    }
    
    [Fact]
    public async Task AddCheepAuthor_ShouldAddAuthor()
    {
        var author = new Author { UserName = "NewUser", Email = "newuser@itu.dk" };
        await _repo.AddCheep(author, "Test");
        
        var cheeps = await _repo.GetCheepsByAuthor("NewUser", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(1);
        cheeps.Should().OnlyContain(c => c.Author.UserName == "NewUser" && c.Text == "Test");
    }
    
    [Fact]
    public async Task FindByName_ShouldReturnAuthor()
    {
        var result = await _repo.FindByName("Helge");
        result.Should().BeNull();

        await _repo.Create("Helge", "helge@itu.dk");
        result = await _repo.FindByName("Helge");

        result.Should().NotBeNull();
        result!.Email.Should().Be("helge@itu.dk");
        result!.UserName.Should().Be("Helge");
    }
    
    [Fact]
    public async Task FindByName_ShouldReturnNull()
    {
        var result = await _repo.FindByName("NonExistent");
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task FindByEmail_ShouldReturnAuthor()
    {
        var result = await _repo.FindByEmail("helge@itu.dk");
        result.Should().BeNull();
        
        await _repo.Create("Helge", "helge@itu.dk");
        result = await _repo.FindByEmail("helge@itu.dk");

        result.Should().NotBeNull();
        result!.Email.Should().Be("helge@itu.dk");
        result!.UserName.Should().Be("Helge");
    }
    
    [Fact]
    public async Task FindByEmail_ShouldReturnNull()
    {
        var result = await _repo.FindByEmail("NonExistent@itu.dk");
        result.Should().BeNull();
    }
    
   
    [Fact]
    public async Task GetCheepsByAuthor_ShouldReturnCorrectAuthor2()
    {
        var cheeps = await _service.GetCheepsByAuthor("Alice", 1, 10);
        
        cheeps.Should().NotBeEmpty();
        cheeps.Count.Should().Be(2);
        cheeps.Should().OnlyContain(c => c.Author == "Alice");
        
        var cheepsPage2 = await _service.GetCheepsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
   [Fact]
   public async Task GetAllCheeps_ShouldReturnCheeps2()
   {
       var cheeps = await _service.GetCheeps(1, 10);

       cheeps.Should().NotBeEmpty();
       cheeps.Count.Should().Be(3);
       cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.TimeStamp) && !string.IsNullOrWhiteSpace(c.AuthorEmail));

       var cheepsPage2 = await _service.GetCheeps(2, 10);
       cheepsPage2.Should().BeEmpty();
   }

   [Fact]
   public async Task GetCheeps_ShouldReturnPageResult()
   {
       for (var i = 0; i < 10; i++)
       {
           await _repo.AddCheep("Alice", "alice@itu.dk", "New message" + i);
       }

       var cheeps = await _service.GetCheeps(1, 10);

       cheeps.Should().NotBeNullOrEmpty();
       cheeps.Count.Should().Be(10);


       cheeps = await _service.GetCheeps(2, 10);
       cheeps.Should().NotBeNullOrEmpty();
       cheeps.Count.Should().Be(3);
   }

   [Theory]
   [InlineData(-1)]
   [InlineData(-2)]
   [InlineData(0)]
   public async Task GetCheeps_InvalidPage_ShouldThrow(int page)
   {
       Func<Task> act = async () => await _service.GetCheeps(page, 10);

       await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
           .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(null)]
   public async Task GetCheepsByAuthor_InvalidAuthor_ShouldThrow(string author)
   {
       Func<Task> act = async () => await _service.GetCheepsByAuthor(author, 1, 10);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage($"*Author is required*");
   }

   [Theory]
   [InlineData("NonExisting123")]
   [InlineData("....")]
   [InlineData(".")]
   public async Task GetCheepsFromAuthor_UnknownAuthor_ShouldReturnEmpty(string author)
   {
       var cheeps = await _service.GetCheepsByAuthor(author, 1, 10);
       cheeps.Should().BeEmpty();
   }

   [Fact]
   public async Task GetCheeps_Page2_ShouldNotContainFirstPageCheeps()
   {
       for (var i = 0; i < 10; i++)
       {
           await _repo.AddCheep("Alice", "alice@itu.dk", "New message" + i);
       }

       var firstPage = await _service.GetCheeps(1, 5);
       var secondPage = await _service.GetCheeps(2, 5);

       firstPage.Should().NotBeEmpty();
       firstPage.Count.Should().Be(5);

       secondPage.Should().NotBeEmpty();
       secondPage.Count.Should().Be(5);

       secondPage.Should().NotContain(c =>
           firstPage.Any(f => f.Author == c.Author && f.Message == c.Message && f.TimeStamp == c.TimeStamp));
    }

   [Fact]
   public async Task GetCheeps_EmptyPage_ShouldReturnEmptyList()
   {
       var cheeps = await _service.GetCheeps(10, 10);
       cheeps.Should().BeEmpty();
   }
    
   [Fact]
   public async Task AddCheep_ValidAuthorAndText()
   {
       await _service.AddCheep("Alice", "alice@itu.dk", "Test");

       var cheeps = await _service.GetCheepsByAuthor("Alice", 1, 10);
       cheeps.Should().NotBeEmpty();
       cheeps.Count.Should().Be(3);
       
       cheeps.Should().ContainSingle(c => c.Author == "Alice" && c.Message == "Test" && c.AuthorEmail == "alice@itu.dk");
   }
   
   [Fact]
   public async Task AddCheep_InvalidText_ShouldThrowException()
   {
       var longCheep = new string('x', 161);

       Func<Task> act = async () => await _service.AddCheep("Helge", "helge@itu.dk", longCheep);

       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*cannot exceed 160 characters*");
   }
   
   [Fact]
   public async Task AddCheep_160lengthText_ShouldNotThrowException()
   {
       var longCheep = new string('x', 160);

       Func<Task> act = async () => await _service.AddCheep("Helge", "helge@itu.dk", longCheep);

       await act.Should().NotThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task AddCheepAuthor_InvalidText_ShouldThrowException()
   {
       var longCheep = new string('x', 161);
       var author = new Author { UserName = "Helge", Email = "helge@itu.dk" };

       Func<Task> act = async () => await _service.AddCheep(author, longCheep);

       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*cannot exceed 160 characters*");
   }
   
   [Fact]
   public async Task AddCheepAuthor_160lengthText_ShouldNotThrowException()
   {
       var longCheep = new string('x', 160);
       var author = new Author { UserName = "Helge", Email = "helge@itu.dk" };

       Func<Task> act = async () => await _service.AddCheep(author, longCheep);

       await act.Should().NotThrowAsync<ArgumentException>();
   }
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(null)]
   public async Task AddCheep_InvalidText_ShouldThrowException2(string text)
   {
       Func<Task> act = async () => await _service.AddCheep("Alice", "alice@itu.dk", text);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Cheep text is required and cannot be null or empty*");
   }
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(null)]
   public async Task AddCheep_NullAuthor_ShouldThrow(string author)
   {
       Func<Task> act = async () => await _service.AddCheep(author, "test@itu.dk", "Hello");
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");
   }
   
   [Fact]
   public async Task AddCheep_ExistingAuthor_ShouldNotCreateDuplicateAuthor()
   {
       await _service.AddCheep("Alice", "alice@itu.dk", "New message");

       var authors = _context.Authors.Where(a => a.UserName == "Alice").ToList();
       authors.Count.Should().Be(1);
   }
}