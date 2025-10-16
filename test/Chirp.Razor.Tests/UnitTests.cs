using FluentAssertions;
using Chirp.Razor.DTO; 
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Database;
using Chirp.Razor.Repositories;
using Chirp.Razor.Models;

namespace Chirp.Razor.Tests;
public class UnitTests : IDisposable
{ 
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    private readonly ICheepRepository _repo;
    private readonly ICheepService _service;

    public UnitTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        
        var builder = new DbContextOptionsBuilder<ChirpDbContext>().UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated();

        Author a1 = new Author { Name = "Alice" }; 
        Author a2 = new Author { Name = "Bob" }; 
        
        _context.Authors.AddRange(
            a1, a2
        );
        
        _context.Cheeps.AddRange(
            new Cheep { Author = a1, Text = "Hello!", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = a1, Text = "Second cheep", TimeStamp = DateTime.UtcNow },
            new Cheep { Author = a2, Text = "Third cheep", TimeStamp = DateTime.UtcNow }
        );
        
        _context.SaveChanges();
        _repo = new CheepRepository(_context);
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
        var author = new Author { Name = "Alice" };
        var ts = DateTime.UtcNow; 
        var cheep = new Cheep
        {
            Author = author,
            Text = "Test",
            TimeStamp = ts
        };

        var dto = CheepRepository.CheepToDTO(cheep);

        dto.Author.Should().Be("Alice");
        dto.Message.Should().Be("Test");
        dto.Timestamp.Should().Be(cheep.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", 
                                    System.Globalization.CultureInfo.InvariantCulture));
    }
    
    
    [Fact]
    public void DTOToCheep_ShouldMapCorrectly()
    {
        var author = new Author { Name = "Bob" };
        var dto = new CheepDTO("Bob", "Test","10/15/25 14:30:00");

        var cheep = CheepRepository.DTOToCheep(dto, author);

        cheep.Author.Should().Be(author);
        cheep.Text.Should().Be("Test");
    }
    
    [Fact]
    public void DTOToCheep_ShouldUseDTOTimestamp()
    {
        var author = new Author { Name = "Alice" };
        var timestamp = "10/15/25 14:30:00";
        var dto = new CheepDTO("Alice", "Hello", timestamp);

        var cheep = CheepRepository.DTOToCheep(dto, author);
        
        cheep.TimeStamp.ToString("MM/dd/yy HH:mm:ss", 
            System.Globalization.CultureInfo.InvariantCulture).Should().Be(timestamp);
    }
    
    [Fact]
    public void CheepDTO_ShouldHaveValidProperties()
    {
        var cheep = new CheepDTO("Bob", "Hello world", "08/02/23 14:19:38");
        
        cheep.Author.Should().Be("Bob");
        cheep.Message.Should().Be("Hello world");
        cheep.Timestamp.Should().Be("08/02/23 14:19:38");
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
        Action act = () => new CheepDTO(author, message, "10/15/25 14:30:00");
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task ReadAllCheeps_ShouldReturnCheeps()
    {
        var cheeps = await _repo.ReadAllCheeps();
        cheeps.Should().NotBeNullOrEmpty();
        cheeps.Should().NotBeEmpty();
        cheeps.Should().BeOfType<List<CheepDTO>>();
        Assert.True(cheeps.Count == 3);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.Timestamp));
    }
    
    [Fact] 
    public async Task GetCheeps_ShouldReturnCheeps()
    {
        var cheeps = await _service.GetCheeps(1);
        
        cheeps.Should().NotBeNull(); 
        cheeps.Should().NotBeEmpty(); 
        cheeps.Should().BeOfType<List<CheepDTO>>();
        Assert.True(cheeps.Count == 3);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.Timestamp));
    }
    
    [Fact]
    public async Task ReadCheepsByAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = await _repo.ReadCheepsByAuthor("Alice");
        cheeps.Should().NotBeEmpty();
        Assert.True(cheeps.Count == 2);
        cheeps.Should().OnlyContain(c => c.Author == "Alice");
    }
    
    [Fact]
    public async Task  GetCheepsFromAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = await _service.GetCheepsFromAuthor("Alice", 1);
        
        cheeps.Should().NotBeEmpty();
        Assert.True(cheeps.Count == 2);
        cheeps.Should().OnlyContain(c => c.Author ==  "Alice");
    }
    
    [Fact]
    public async Task  GetCheepsFromAuthor_ShouldReturnCorrectAuthor2()
    {
        var cheeps = await _service.GetCheepsFromAuthor("Bob", 1);
        
        cheeps.Should().NotBeEmpty();
        Assert.True(cheeps.Count == 1);
        cheeps.Should().OnlyContain(c => c.Author ==  "Bob");
    }
    
    
    [Fact]
    public async Task CreateCheep_ShouldAddNewCheep()
    {
        var dto = new CheepDTO("Alice", "New message", "10/15/25 14:30:00");
        var id = await _repo.CreateCheep(dto);

        id.Should().BeGreaterThan(0);

        var all = await _repo.ReadAllCheeps();
        all.Should().Contain(c => c.Message == "New message" && c.Author ==  "Alice");
    }
    
    [Fact]
    public async Task CreateCheep_NonExistingAuthor_ShouldThrow()
    {
        var dto = new CheepDTO("Unknown", "Hello", "10/15/25 14:30:00");
        Func<Task> act = async () => await _repo.CreateCheep(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*not found*");
    }
    
    [Fact]
    public async Task GetCheeps_ShouldReturn32PageResult()
    {
        for (var i = 0; i < 30; i++)
        {
            var dto = new CheepDTO("Alice", "New message"+i, "10/15/25 14:30:00");
            await _repo.CreateCheep(dto);
        }
        
        var cheeps = await _service.GetCheeps(1);
        
        cheeps.Should().NotBeNullOrEmpty();
        Assert.True(cheeps.Count == 32);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(0)]
    public async Task GetCheeps_InvalidPage_ShouldThrow(int page)
    {
        Func<Task> act = async () => await _service.GetCheeps(page);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetCheepsFromAuthor_InvalidAuthor_ShouldThrow(string author)
    {
        Func<Task> act = async () => await _service.GetCheepsFromAuthor(author, 1);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*Author is required*");
    }
    
    [Theory]
    [InlineData("NonExisting123")]
    [InlineData("....")]
    [InlineData(".")]
    public async Task GetCheepsFromAuthor_UnknownAuthor_ShouldReturnEmpty(string author)
    {
        var cheeps = await _service.GetCheepsFromAuthor(author, 1);
        cheeps.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetCheeps_Page2_ShouldNotContainFirstPageCheeps()
    {
        for (var i = 0; i < 40; i++)
        {
            var dto = new CheepDTO("Alice", "New message " + i, "10/15/25 14:30:00");
            await _repo.CreateCheep(dto);
        }
        
        var firstPage = await _service.GetCheeps(1);
        var secondPage = await _service.GetCheeps(2);

        firstPage.Should().NotBeEmpty();
        Assert.True(firstPage.Count <= 32);

        secondPage.Should().NotBeEmpty();
        Assert.True(secondPage.Count <= 32);
        
        secondPage.Should().NotContain(c =>
            firstPage.Any(f => f.Author == c.Author && f.Message == c.Message && f.Timestamp == c.Timestamp)
        );
    }
    
    [Fact]
    public async Task GetCheeps_EmptyPage_ShouldReturnEmptyList()
    {
        var cheeps = await _service.GetCheeps(10);
        cheeps.Should().BeEmpty();
    }
}