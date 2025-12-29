using Chirp.Core;
using FluentAssertions;
namespace Chirp.Tests.Chirp.Core;

public class AuthorTests
{
    [Fact]
    public void Author_EmptyDefaultConstructor_RandomProfileImage()
    {
        var author = new Author();
        
        author.ProfileImage.Should().NotBeNullOrWhiteSpace();
        author.ProfileImage.Should().MatchRegex(@"^/images/bird[1-5]-profile\.png$");
    }
    
    [Fact]
    public void Author_DefaultConstructor_RandomProfileImage()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk"};
        
        author.ProfileImage.Should().NotBeNullOrWhiteSpace();
        author.ProfileImage.Should().MatchRegex(@"^/images/bird[1-5]-profile\.png$");
    }
    
    [Fact]
    public void Author_DefaultConstructor_WithProfileImage()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk", ProfileImage = "image.png"};
        
        author.ProfileImage.Should().NotMatchRegex(@"^/images/bird[1-5]-profile\.png$");
        author.ProfileImage.Should().Be("image.png");
    }
}