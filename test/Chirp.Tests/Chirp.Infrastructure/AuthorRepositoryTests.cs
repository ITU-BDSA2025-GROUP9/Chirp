using Chirp.Core;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Tests.Chirp.Infrastructure;

public class AuthorRepositoryTests: IDisposable
{
    private readonly IAuthorRepository _authorRepo;
    private readonly SqliteConnection _connection;
    private readonly ChirpDbContext _context;
    
    public AuthorRepositoryTests() 
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
        
        _context.SaveChanges();
        _authorRepo = new AuthorRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
    
    
    [Fact]
    public async Task CreateAuthor_ShouldAddAuthorToDB()
    {
        var author = await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");

        author.Should().NotBeNull();
        author.UserName.Should().Be("Helge");
        author.Email.Should().Be("helge@itu.dk");

        _context.Authors.Should().Contain(author); 
    }
    
    [Fact]
    public async Task FindByName_ShouldReturnAuthor()
    {
        var result = await _authorRepo.FindByName("Helge");
        result.Should().BeNull();

        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        result = await _authorRepo.FindByName("Helge");

        result.Should().NotBeNull();
        result!.Email.Should().Be("helge@itu.dk");
        result!.UserName.Should().Be("Helge");
    }
    
    [Fact]
    public async Task FindByName_UnknownUser_ShouldReturnNull()
    {
        var result = await _authorRepo.FindByName("NonExistent");
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task FindByEmail_ShouldReturnAuthor()
    {
        var result = await _authorRepo.FindByEmail("helge@itu.dk");
        result.Should().BeNull();
        
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        result = await _authorRepo.FindByEmail("helge@itu.dk");

        result.Should().NotBeNull();
        result!.Email.Should().Be("helge@itu.dk");
        result!.UserName.Should().Be("Helge");
    }
    
    [Fact]
    public async Task FindByEmail_UnknownUser_ShouldReturnNull()
    {
        var result = await _authorRepo.FindByEmail("NonExistent@itu.dk");
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAllFollowees_ShouldReturnFollowedList()
    {
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        await _authorRepo.FollowAuthor("Alice", "Bob");
        await _authorRepo.FollowAuthor("Alice", "Helge");

        var followees = await _authorRepo.GetAllFollowees("Alice");
        followees.Should().HaveCount(2); 
        followees.Should().Contain(f => f == "Bob");
        followees.Should().Contain(f => f == "Helge");
    }
    
    [Fact]
    public async Task GetAllFollowees_ShouldNotReturnUnfollowedList()
    {
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        await _authorRepo.FollowAuthor("Alice", "Bob");
        await _authorRepo.FollowAuthor("Alice", "Helge");

        var followees = await _authorRepo.GetAllFollowees("Alice");
        followees.Should().HaveCount(2); 
        followees.Should().Contain(f => f == "Bob");
        followees.Should().Contain(f => f == "Helge");
       
        await _authorRepo.UnfollowAuthor("Alice", "Bob");
        followees = await _authorRepo.GetAllFollowees("Alice");
        followees.Should().HaveCount(1); 
        followees.Should().Contain("Helge");
        followees.Should().NotContain("Bob");
    }
   
    [Fact]
    public async Task GetAllFollowees_UnknownUser_ShouldReturnEmpty()
    {
        var followees = await _authorRepo.GetAllFollowees("Unknown");
        followees.Should().BeEmpty();
    }
   
    [Fact]
    public async Task GetAllFollowees_WithNoFollowee_ShouldReturnEmpty()
    {
        var followees = await _authorRepo.GetAllFollowees("Alice");
        followees.Should().BeEmpty();
    }
   
    [Fact]
    public async Task GetAllFollowees_UserHasNoFollowees_OtherUsersFollowingDoesNotAffect()
    {
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        await _authorRepo.FollowAuthor("Bob", "Helge"); 
       
        var aliceFollowees = await _authorRepo.GetAllFollowees("Alice");
        aliceFollowees.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllFollowees_UserHasFollowersButFollowsNoOne_ShouldReturnEmpty()
    {
        await _authorRepo.FollowAuthor("Bob", "Alice");
        var aliceFollowees = await _authorRepo.GetAllFollowees("Alice");
        aliceFollowees.Should().BeEmpty(); 
    }
    
    [Fact]
    public async Task GetAllFollowees_UserHasFollowees_OtherUsersFollowingDoesNotAffect()
    {
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        
        await _authorRepo.FollowAuthor("Bob", "Helge"); 
        await  _authorRepo.FollowAuthor("Alice", "Bob");
        
        var aliceFollowees = await _authorRepo.GetAllFollowees("Alice");
        aliceFollowees.Should().HaveCount(1); 
        aliceFollowees.Should().Contain("Bob");
    }
    
    [Fact]
    public async Task GetAllFollowees_UserHasFolloweesAndFollowers_OtherUsersFollowingDoesNotAffect()
    {
        await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
        
        await _authorRepo.FollowAuthor("Bob", "Helge"); 
        await _authorRepo.FollowAuthor("Bob", "Alice"); 
        await  _authorRepo.FollowAuthor("Alice", "Bob");
        
        var aliceFollowees = await _authorRepo.GetAllFollowees("Alice");
        aliceFollowees.Should().HaveCount(1); 
        aliceFollowees.Should().Contain("Bob");
    }
    
    [Fact]
    public async Task FollowAuthor_ShouldAddFollowerAndFollowee()
    {
        var result = await _authorRepo.FollowAuthor("Alice", "Bob");
        result.Should().BeTrue();

        var alice = await _authorRepo.FindByName("Alice");
        var bob = await _authorRepo.FindByName("Bob");

        alice!.Following.Should().Contain(bob!); // Alice follows Bob
        bob!.Followers.Should().Contain(alice);
        
        alice.Followers.Should().NotContain(bob); // Bob does not follow Alice
        bob.Following.Should().NotContain(alice);
    }
    
    [Fact]
   public async Task FollowAuthor_SelfFollow_ShouldReturnFalse()
   {
       var result = await _authorRepo.FollowAuthor("Alice", "Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task FollowAuthor_DuplicateFollow_ShouldReturnFalse()
   {
       await _authorRepo.FollowAuthor("Alice", "Bob");
       var secondFollow = await _authorRepo.FollowAuthor("Alice", "Bob");
       secondFollow.Should().BeFalse();
   }

   [Fact]
   public async Task UnfollowAuthor_ShouldRemoveFollowerAndFollowee()
   {
       await _authorRepo.FollowAuthor("Alice", "Bob");
       var result = await _authorRepo.UnfollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var alice = await _authorRepo.FindByName("Alice");
       var bob = await _authorRepo.FindByName("Bob");

       alice!.Following.Should().NotContain(bob!);
       bob!.Followers.Should().NotContain(alice);
       
       alice.Followers.Should().NotContain(bob); // Bob still does not follow Alice
       bob.Following.Should().NotContain(alice);
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldNotAffectOtherUsersFollowings()
   {
       await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");

       await _authorRepo.FollowAuthor("Alice", "Bob");
       await _authorRepo.FollowAuthor("Bob", "Helge");

       var result = await _authorRepo.UnfollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var aliceFollowees = await _authorRepo.GetAllFollowees("Alice");
       aliceFollowees.Should().BeEmpty();

       var bobFollowees = await _authorRepo.GetAllFollowees("Bob");
       bobFollowees.Should().Contain("Helge");
   }
   
   [Fact]
   public async Task UnfollowAuthor_NotFollowing_ShouldReturnFalse()
   {
       var result = await _authorRepo.UnfollowAuthor("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_ShouldReturnTrue_WhenFollowing()
   {
       await _authorRepo.FollowAuthor("Alice", "Bob");
       var result = await _authorRepo.IsFollowing("Alice", "Bob");
       result.Should().BeTrue();
   }
   
   [Fact]
   public async Task IsFollowing_repoShouldReturnFalse_WhenNotFollowing()
   {
       var result = await _authorRepo.IsFollowing("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task DeleteAuthor_ShouldRemoveAuthor()
   {
       await _authorRepo.CreateAuthor("Helge", "helge@itu.dk");
       await _authorRepo.FollowAuthor("Alice", "Helge");
       await _authorRepo.FollowAuthor("Bob", "Helge");

       var result = await _authorRepo.DeleteAuthor("Helge");
       result.Should().BeTrue();

       var deletedAuthor = await _authorRepo.FindByName("Helge");
       deletedAuthor.Should().BeNull();

       var alice = await _authorRepo.FindByName("Alice");
       alice!.Following.Should().NotContain(a => a.UserName == "Helge");

       var bob = await _authorRepo.FindByName("Bob");
       bob!.Following.Should().NotContain(a => a.UserName == "Helge");
   }
   
   [Fact]
   public async Task DeleteAuthor_UnknownUser_ShouldReturnFalse()
   {
       var result = await _authorRepo.DeleteAuthor("NonExistent");
       result.Should().BeFalse();
   }

   [Fact]
   public async Task AuthorByNameExists_ShouldReturnTrue_WhenExists()
   {
       var exists = await _authorRepo.AuthorByNameExists("Alice");
       exists.Should().BeTrue();
   }

   [Theory]
   [InlineData("NonExistent")]
   [InlineData("")]
   [InlineData("  ")]
   [InlineData(null)]
   public async Task AuthorByNameExists_ShouldReturnFalse_WhenNotExistsOrInvalid(string name)
   {
       var exists = await _authorRepo.AuthorByNameExists(name);
       exists.Should().BeFalse();
   }

   [Fact]
   public async Task SetProfileImage_ShouldUpdateProfileImage()
   {
       var result = await _authorRepo.SetProfileImage("Alice", "image.png");
       result.Should().BeTrue();

       var alice = await _authorRepo.FindByName("Alice");
       alice!.ProfileImage.Should().Be("image.png");
   }

   [Fact]
   public async Task SetProfileImage_UnknownUser_ShouldReturnFalse()
   {
       var result = await _authorRepo.SetProfileImage("NonExistent", "image.png");
       result.Should().BeFalse();
   }
}