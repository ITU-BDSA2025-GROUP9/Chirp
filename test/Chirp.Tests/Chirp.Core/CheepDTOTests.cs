using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using FluentAssertions;
namespace Chirp.Tests.Chirp.Core;

public class CheepDTOTests
{
    [Fact]
    public void CheepDTO_ValidProperties()
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        var cheep = new CheepDTO(author, "Hello world", "08/02/23 14:19:38", 1, []);

        cheep.Author.Should().Be(author);
        cheep.Message.Should().Be("Hello world");
        cheep.TimeStamp.Should().Be("08/02/23 14:19:38");
        cheep.CheepId.Should().Be(1);
        
        cheep.Comments.Should().NotBeNull();
        cheep.Comments.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CheepDTO_InvalidMessage_ShouldThrowException(string message)
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        Action act = () => new CheepDTO(author, message, "10/15/25 14:30:00", 1, []);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be null or empty. Invalid message:*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CheepDTO_InvalidTimestamp_ShouldThrowException(string timestamp)
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        Action act = () => new CheepDTO(author, "Hello", timestamp, 1, []);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Timestamp cannot be null or empty. Invalid timestamp:*");
    }
    
    [Fact]
    public void CheepDTO_NullAuthor_ShouldThrowException()
    {
        Action act = () => new CheepDTO(null!, "Hello", "10/15/25 14:30:00", 1, []);
        
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Author cannot be null.*");
    }

    [Fact]
    public void CheepToDTO_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        var ts = DateTime.UtcNow;
        
        var cheep = new Cheep
        {
            Author = author,
            Text = "Test",
            TimeStamp = ts
        };

        var dto = CheepDTO.ToDto(cheep);

        dto.Author.Name.Should().Be("Bob");
        dto.Author.Email.Should().Be("bob@itu.dk");
        dto.Message.Should().Be("Test");
        dto.TimeStamp.Should().Be(cheep.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture));
        
        dto.Comments.Should().NotBeNull();
        dto.Comments.Should().BeEmpty();
    }
    
    [Fact]
    public void CheepsToDtos_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        var ts = DateTime.UtcNow;
        
        var cheeps = new List<Cheep>
        {
            new Cheep { Author = author, Text = "Test1", TimeStamp = ts },
            new Cheep { Author = author, Text = "Test2", TimeStamp = ts }
        };
        
        var dtos = CheepDTO.ToDtos(cheeps);
        
        dtos.Should().HaveCount(2);
        dtos[0].Author.Name.Should().Be("Bob");
        dtos[0].Author.Email.Should().Be("bob@itu.dk");
        dtos[0].Message.Should().Be("Test1");
            
        dtos[1].Author.Name.Should().Be("Bob");
        dtos[1].Author.Email.Should().Be("bob@itu.dk");
        dtos[1].Message.Should().Be("Test2");
    }

    [Fact]
    public void CheepsToDtos_EmptyCheeps_ShouldThrow()
    {
        List<Cheep> cheeps = null!;

        Action act = () => CheepDTO.ToDtos(cheeps);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*List of cheeps cannot be null*");
    }
}