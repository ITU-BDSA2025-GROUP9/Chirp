using Chirp.Core;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Service;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Tests.Chirp.Infrastructure;

public class AuthorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    
    private readonly IAuthorService _authorService;
    
    public AuthorServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDbContext>().UseSqlite(_connection);
        _context = new ChirpDbContext(builder.Options);
        _context.Database.EnsureCreated(); 
        
        var a1 = new Author { UserName = "Alice", Email = "alice@itu.dk" };
        var a2 = new Author { UserName = "Bob", Email = "bob@itu.dk" };
        var a3 = new Author { UserName = "Helge", Email = "helge@itu.dk" };

        _context.Authors.AddRange(
            a1, a2, a3
        );
        
        _context.SaveChanges();
        
        var authorRepo = new AuthorRepository(_context);
        _authorService = new AuthorService(authorRepo);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
    
   [Fact]
   public async Task FollowAuthor_ShouldAllowValidFollow()
   {
       var result = await _authorService.FollowAuthor("Alice", "Bob");
       result.Should().BeTrue();
       
       var isFollowing = await _authorService.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeTrue();
   }
   
   [Fact]
   public async Task FollowAuthor_ShouldNotAllowSelfFollow()
   {
       Func<Task> act = async () => await _authorService.FollowAuthor("Alice", "Alice");

       await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("You cannot follow yourself.");
   }
   
   [Theory]
   [InlineData(null, "Bob")]
   [InlineData("", "Bob")]
   [InlineData("   ", "Bob")]
   [InlineData("Alice", null)]
   [InlineData("Alice", "")]
   [InlineData("Alice", "   ")]
   public async Task FollowAuthor_InvalidFollowerFollowee_ShouldThrow(string follower, string followee)
   {
       Func<Task> act = async () => await _authorService.FollowAuthor(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task FollowAuthor_Twice_ShouldReturnFalse()
   {
       var first = await _authorService.FollowAuthor("Alice", "Bob");
       var second = await _authorService.FollowAuthor("Alice", "Bob");

       first.Should().BeTrue();
       second.Should().BeFalse();

       var isFollowing = await _authorService.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeTrue();
   }
   
   [Fact]
   public async Task FollowAuthor_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.FollowAuthor("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task FollowAuthor_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.FollowAuthor("Unknown","Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldAllowValidUnfollow()
   {
       await _authorService.FollowAuthor("Alice", "Bob");

       var result = await _authorService.UnfollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var isFollowing = await _authorService.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldNotAllowSelfUnfollow()
   {
       Func<Task> act = async () => await _authorService.UnfollowAuthor("Alice", "Alice");
       await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("You cannot unfollow yourself.");
   }
   
   [Fact]
   public async Task UnfollowAuthor_WhenNotFollowing_ShouldReturnFalse()
   {
       var result = await _authorService.UnfollowAuthor("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldNotAffectOtherFollowers()
   {
       await _authorService.FollowAuthor("Alice", "Bob");
       await _authorService.FollowAuthor("Helge", "Bob");
       
       await _authorService.UnfollowAuthor("Alice", "Bob");

       var result =  await _authorService.IsFollowing("Helge", "Bob");
       result.Should().BeTrue();
   }
   
   [Fact]
   public async Task UnfollowAuthor_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.UnfollowAuthor("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.UnfollowAuthor("Unknown", "Alice");
       result.Should().BeFalse();
   }
   
   [Theory]
   [InlineData(null, "Bob")]
   [InlineData("", "Bob")]
   [InlineData("   ", "Bob")]
   [InlineData("Alice", null)]
   [InlineData("Alice", "")]
   [InlineData("Alice", "   ")]
   public async Task UnfollowAuthor_InvalidFollowerFollowee_ShouldThrow(string follower, string followee)
   {
       Func<Task> act = async () => await _authorService.UnfollowAuthor(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }

   [Fact]
   public async Task IsFollowing_ShouldReturnFalse_WhenNotFollowing()
   {
       var result = await _authorService.IsFollowing("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_ShouldReturnTrue_WhenFollowing()
   {
       await _authorService.FollowAuthor("Alice", "Bob");
       var result = await _authorService.IsFollowing("Alice", "Bob");

       result.Should().BeTrue();
   }
   
   [Theory]
   [InlineData(null, "Bob")]
   [InlineData("", "Bob")]
   [InlineData("   ", "Bob")]
   [InlineData("Alice", null)]
   [InlineData("Alice", "")]
   [InlineData("Alice", "   ")]
   public async Task IsFollowing_InvalidFollowerFollowee_ShouldThrow(string follower, string followee)
   {
       Func<Task> act = async () => await _authorService.IsFollowing(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task IsFollowing_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.IsFollowing("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _authorService.IsFollowing("Unknown", "Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task DeleteAuthor_ShouldDelete_WhenAuthorExists()
   {
       var result = await _authorService.DeleteAuthor("Alice");
       result.Should().BeTrue();
       
       _context.Authors.Should().NotContain(a => a.UserName == "Alice");
   }

   [Fact]
   public async Task DeleteAuthor_ShouldReturnFalse_WhenAuthorDoesNotExist()
   {
       var result = await _authorService.DeleteAuthor("Unknown");
       result.Should().BeFalse();
       
       _context.Authors.Should().NotContain(a => a.UserName == "Unknown");
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public async Task DeleteAuthor_InvalidName_ShouldThrow(string name)
   {
       Func<Task> act = () => _authorService.DeleteAuthor(name);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");
   }
   
   [Fact]
   public async Task AuthorByNameExists_ShouldReturnTrue_WhenAuthorExists()
   {
       var result = await _authorService.AuthorByNameExists("Alice");
       result.Should().BeTrue();
   }
   
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("Unknown")]
   public async Task AuthorByNameExists_ShouldReturnFalse(string name)
   {
       var result = await _authorService.AuthorByNameExists(name);
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task SetProfileImage_ShouldReturnTrue_WhenValid()
   {
       _context.Authors.Should().Contain(a => a.UserName == "Alice" && a.ProfileImage != "image.png");
       
       var result = await _authorService.SetProfileImage("Alice", "image.png");
       result.Should().BeTrue();
       _context.Authors.Should().Contain(a => a.UserName == "Alice" && a.ProfileImage == "image.png");
   }

   [Theory]
   [InlineData(null, "image.png")]
   [InlineData("", "image.png")]
   [InlineData("   ", "image.png")]
   [InlineData("Alice", null)]
   [InlineData("Alice", "")]
   [InlineData("Alice", "   ")]
   public async Task SetProfileImage_InvalidArguments_ShouldThrow(string author, string img)
   {
       Func<Task> act = () => _authorService.SetProfileImage(author, img);
       await act.Should().ThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task GetAllFollowees_ShouldReturnFollowees()
   {
       await _authorService.FollowAuthor("Alice", "Bob");
       await _authorService.FollowAuthor("Alice", "Helge");

       var result = await _authorService.GetAllFollowees("Alice");
       result.Should().Contain(["Bob", "Helge"]);
   }
   
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public async Task GetAllFollowees_ShouldThrow(string name)
   {
       Func<Task> act = () => _authorService.GetAllFollowees(name);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");
   }
   
   [Fact]
   public async Task GetAllFolloweesAndSelf_ShouldIncludeSelf()
   {
       await _authorService.FollowAuthor("Alice", "Bob");
       await _authorService.FollowAuthor("Alice", "Helge");

       var result = await _authorService.GetAllFolloweesAndSelf("Alice");
       result.Should().Contain(["Bob", "Helge", "Alice"]);
   }
   
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public async Task GetAllFolloweesAndSelf_ShouldThrow(string name)
   {
       Func<Task> act = () => _authorService.GetAllFolloweesAndSelf(name);
       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");;
   }
}