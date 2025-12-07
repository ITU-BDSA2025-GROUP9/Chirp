using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using FluentAssertions;
namespace Chirp.Tests.Chirp.Core;

public class CommentDTOTests
{
    [Fact]
    public void CommentDTO_ValidProperties()
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        var comment = new CommentDTO(author, "Hello world", "08/02/23 14:19:38", 1);

        comment.Author.Should().Be(author);
        comment.Message.Should().Be("Hello world");
        comment.TimeStamp.Should().Be("08/02/23 14:19:38");
        comment.CommentId.Should().Be(1);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CommentDTO_InvalidMessage_ShouldThrowException(string message)
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        Action act = () => new CommentDTO(author, message, "10/15/25 14:30:00", 1);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be null or empty. Invalid message:*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CommentDTO_InvalidTimestamp_ShouldThrowException(string timestamp)
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        Action act = () => new CommentDTO(author, "Hello", timestamp, 1);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Timestamp cannot be null or empty. Invalid timestamp:*");
    }
    
    [Fact]
    public void CommentDTO_NullAuthor_ShouldThrowException()
    {
        Action act = () => new CommentDTO(null!, "Hello", "10/15/25 14:30:00", 1);
        
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Author cannot be null.*");
    }

    [Fact]
    public void CommentToDTO_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        var ts = DateTime.UtcNow;
        
        var comment = new Comment
        {
            Author = author,
            Text = "Test",
            TimeStamp = ts
        };

        var dto = CommentDTO.ToDto(comment);

        dto.Author.Name.Should().Be("Bob");
        dto.Author.Email.Should().Be("bob@itu.dk");
        dto.Message.Should().Be("Test");
        dto.TimeStamp.Should().Be(comment.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture));
    }
    
    [Fact]
    public void CheepsToDtos_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        var ts = DateTime.UtcNow;
        
        var comments = new List<Comment>
        {
            new Comment { Author = author, Text = "Test1", TimeStamp = ts },
            new Comment { Author = author, Text = "Test2", TimeStamp = ts }
        };
        
        var dtos = CommentDTO.ToDtos(comments);
        
        dtos.Should().HaveCount(2);
        dtos[0].Author.Name.Should().Be("Bob");
        dtos[0].Author.Email.Should().Be("bob@itu.dk");
        dtos[0].Message.Should().Be("Test1");
            
        dtos[1].Author.Name.Should().Be("Bob");
        dtos[1].Author.Email.Should().Be("bob@itu.dk");
        dtos[1].Message.Should().Be("Test2");
    }

    [Fact]
    public void CommentToDtos_EmptyComments_ShouldThrow()
    {
        List<Comment> comments = null!;

        Action act = () => CommentDTO.ToDtos(comments);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*List of comments cannot be null*");
    }
}