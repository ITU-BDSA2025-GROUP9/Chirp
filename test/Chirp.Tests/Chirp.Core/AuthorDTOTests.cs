using Chirp.Core;
using Chirp.Core.DTO;
using FluentAssertions;

namespace Chirp.Tests.Chirp.Core;

public class AuthorDTOTests
{
    [Fact]
    public void AuthorDTO_ValidProperties()
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        
        author.Name.Should().Be("Bob");
        author.Email.Should().Be("bob@itu.dk");
        author.ProfileImage.Should().Be("image.png");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AuthorDTO_InvalidProfileImage_ShouldDefault(string profileImage)
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", profileImage);
        
        author.Name.Should().Be("Bob");
        author.Email.Should().Be("bob@itu.dk");
        author.ProfileImage.Should().Be("/images/bird1-profile.png");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AuthorDTO_InvalidName_ShouldThrowException(string name)
    {
        Action act = () => new AuthorDTO(name, "bob@itu.dk", "image.png");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be null or empty. Invalid author name:*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AuthorDTO_InvalidEmail_ShouldThrowException(string email)
    {
        Action act = () => new AuthorDTO("Bob", email, "image.png");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be null or empty. Invalid author email:*");
    }
    
    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData(null, " ")]
    public void AuthorDTO_InvalidArguments_ShouldThrowException(string name, string email)
    {
        Action act = () => new AuthorDTO(name, email, "image.png");
        
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void AuthorToDTO_ShouldMapCorrectly()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk", ProfileImage = "image.png"};
        var dto = AuthorDTO.ToDto(author);

        dto.Name.Should().Be("Bob");
        dto.Email.Should().Be("bob@itu.dk");
        dto.ProfileImage.Should().Be("image.png");
    }
    
    [Fact]
    public void AuthorToDTO_ShouldMapCorrectly_RandomProfileImage()
    {
        var author = new Author { UserName = "Bob", Email = "bob@itu.dk"};
        var dto = AuthorDTO.ToDto(author);

        dto.Name.Should().Be("Bob");
        dto.Email.Should().Be("bob@itu.dk");
        dto.ProfileImage.Should().MatchRegex(@"^/images/bird[1-5]-profile\.png$");
    }
}