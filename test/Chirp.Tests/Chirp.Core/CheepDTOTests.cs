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
        var cheep = new CheepDTO(author, "Hello world", "08/02/23 14:19:38", 1);

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
        Action act = () => new CheepDTO(author, message, "10/15/25 14:30:00", 1);
        
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
        Action act = () => new CheepDTO(author, "Hello", timestamp, 1);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Timestamp cannot be null or empty. Invalid timestamp:*");
    }
    
    [Fact]
    public void CheepDTO_NullAuthor_ShouldThrowException()
    {
        Action act = () => new CheepDTO(null!, "Hello", "10/15/25 14:30:00", 1);
        
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
}