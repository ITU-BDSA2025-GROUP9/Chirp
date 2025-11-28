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

        dto.Author.Name.Should().Be("Alice");
        dto.Message.Should().Be("Test");
        dto.TimeStamp.Should().Be(cheep.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture));
    }
 
    [Fact]
    public void CheepDTO_ShouldHaveValidProperties()
    {
        var author = new AuthorDTO("Bob", "bob@itu.dk", "image.png");
        var cheep = new CheepDTO(author, "Hello world", "08/02/23 14:19:38", 1);

        cheep.Author.Name.Should().Be("Bob");
        cheep.Message.Should().Be("Hello world");
        cheep.TimeStamp.Should().Be("08/02/23 14:19:38");
        cheep.CheepId.Should().Be(1);
    }
   
    /*
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
        Action act = () => new CheepDTO(author, message, "10/15/25 14:30:00", 1);
        act.Should().Throw<ArgumentException>();
    }*/
    
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
        cheeps.Should().OnlyContain(c => c.Author.Name == "Alice");
        
        var cheepsPage2 = await _service.GetCheepsByAuthor("Alice", 2, 10);
        cheepsPage2.Should().BeEmpty();
    }
    
   [Fact]
   public async Task GetAllCheeps_ShouldReturnCheeps2()
   {
       var cheeps = await _service.GetCheeps(1, 10);

       cheeps.Should().NotBeEmpty();
       cheeps.Count.Should().Be(3);
       cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author.Name) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.TimeStamp));

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
       
       cheeps.Should().ContainSingle(c => c.Author.Name == "Alice" && c.Message == "Test");
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
   
   
   [Fact]
   public async Task FollowAuthor_ShouldAllowValidFollow()
   {
       var result = await _service.FollowAuthor("Alice", "Bob");
       
       result.Should().BeTrue();
       
       var isFollowing = await _service.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeTrue();
   }
   
   [Fact]
   public async Task FollowAuthor_ShouldNotAllowSelfFollow()
   {
       Func<Task> act = async () => await _service.FollowAuthor("Alice", "Alice");

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
       Func<Task> act = async () => await _service.FollowAuthor(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task FollowAuthor_Twice_ShouldReturnFalse()
   {
       var first = await _service.FollowAuthor("Alice", "Bob");
       var second = await _service.FollowAuthor("Alice", "Bob");

       first.Should().BeTrue();
       second.Should().BeFalse();

       var isFollowing = await _service.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeTrue();
   }
   
   [Fact]
   public async Task FollowAuthor_ShouldUpdateFollowersAndFollowing()
   {
       await _service.FollowAuthor("Alice", "Bob");

       var alice = await _repo.FindByName("Alice");
       var bob = await _repo.FindByName("Bob");
       
       alice!.Following.Should().Contain(bob!);
       bob!.Followers.Should().Contain(alice);
   }

   
   [Fact]
   public async Task FollowAuthor_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.FollowAuthor("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task FollowAuthor_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.FollowAuthor("Unknown","Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldAllowValidUnfollow()
   {
       await _service.FollowAuthor("Alice", "Bob");

       var result = await _service.UnfollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var isFollowing = await _service.IsFollowing("Alice", "Bob");
       isFollowing.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldNotAllowSelfUnfollow()
   {
       Func<Task> act = async () => await _service.UnfollowAuthor("Alice", "Alice");
       await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("You cannot unfollow yourself.");
   }
   
   [Fact]
   public async Task UnfollowAuthor_WhenNotFollowing_ShouldReturnFalse()
   {
       var result = await _service.UnfollowAuthor("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_ShouldNotAffectOtherFollowers()
   {
       await _repo.Create("Helge", "helge@itu.dk");
       await _service.FollowAuthor("Alice", "Bob");
       await _service.FollowAuthor("Helge", "Bob");

       await _service.UnfollowAuthor("Alice", "Bob");

       var result =  await _service.IsFollowing("Helge", "Bob");
       result.Should().BeTrue();
   }
   
   [Fact]
   public async Task UnfollowAuthor_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.UnfollowAuthor("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task UnfollowAuthor_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.UnfollowAuthor("Unknown", "Alice");
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
       Func<Task> act = async () => await _service.UnfollowAuthor(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }

   [Fact]
   public async Task IsFollowing_ShouldReturnFalse_WhenNotFollowing()
   {
       var result = await _service.IsFollowing("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_ShouldReturnTrue_WhenFollowing()
   {
       await _service.FollowAuthor("Alice", "Bob");
       var result = await _service.IsFollowing("Alice", "Bob");

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
       Func<Task> act = async () => await _service.IsFollowing(follower, followee);
       await act.Should().ThrowAsync<ArgumentException>();
   }
   
   [Fact]
   public async Task IsFollowing_FolloweeDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.IsFollowing("Alice", "Unknown");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_FollowerDoesNotExist_ShouldReturnFalse()
   {
       var result = await _service.IsFollowing("Unknown", "Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task GetUserTimelineCheeps_ShouldReturnOwnAndFolloweeCheeps()
   {
       await _service.FollowAuthor("Alice", "Bob");
       
       var result = await _service.GetUserTimelineCheeps("Alice", 1, 10);

       result.Should().NotBeEmpty();
       result.Should().HaveCount(3); // 2 Alice + 1 Bob = 3

       result.Should().OnlyContain(c => c.Author.Name== "Alice" || c.Author.Name == "Bob");
   }
   
   [Fact]
   public async Task GetUserTimelineCheeps_UnknownUser_ShouldThrow()
   {
       Func<Task> act = async () => await _service.GetUserTimelineCheeps("Unknown", 1, 10);

       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*User not found*");
   }
   
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public async Task GetUserTimelineCheeps_InvalidAuthor_ShouldThrow(string author)
   {
       Func<Task> act = async () => await _service.GetUserTimelineCheeps(author, 1, 10);

       await act.Should().ThrowAsync<ArgumentException>()
           .WithMessage("*Author is required*");
   }
   
   [Theory]
   [InlineData(0)]
   [InlineData(-1)]
   public async Task GetUserTimelineCheeps_InvalidPageNumber_ShouldThrow(int page)
   {
       Func<Task> act = async () => await _service.GetUserTimelineCheeps("Alice", page, 10);

       await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
           .WithMessage($"*Pagenumber must be greater than 0. Invalid pagenumber: {page}*");
   }
   
   [Fact]
   public async Task GetUserTimelineCheeps_Page2_ShouldNotContainFirstPageCheeps()
   {
       for (int i = 0; i < 10; i++)
           await _repo.AddCheep("Bob", "bob@itu.dk", "New message " + i);

       await _service.FollowAuthor("Alice", "Bob");

       var page1 = await _service.GetUserTimelineCheeps("Alice", 1, 5);
       var page2 = await _service.GetUserTimelineCheeps("Alice", 2, 5);

       page1.Should().NotBeEmpty();
       page1.Should().HaveCount(5);
       page2.Should().NotBeEmpty();

       page2.Should().NotContain(c =>
           page1.Any(f => f.Author == c.Author && f.Message == c.Message));
   }
   
   [Fact]
   public async Task FollowAuthor_ShouldAddFollowerAndFollowee()
   {
       var result = await _repo.FollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var alice = await _repo.FindByName("Alice");
       var bob = await _repo.FindByName("Bob");

       alice!.Following.Should().Contain(bob!);
       bob!.Followers.Should().Contain(alice);
   }
   
   [Fact]
   public async Task FollowAuthor_SelfFollow_ShouldReturnFalse()
   {
       var result = await _repo.FollowAuthor("Alice", "Alice");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task FollowAuthor_DuplicateFollow_ShouldReturnFalse()
   {
       await _repo.FollowAuthor("Alice", "Bob");
       var secondFollow = await _repo.FollowAuthor("Alice", "Bob");
       secondFollow.Should().BeFalse();
   }

   [Fact]
   public async Task UnfollowAuthor_ShouldRemoveFollowerAndFollowee()
   {
       await _repo.FollowAuthor("Alice", "Bob");
       var result = await _repo.UnfollowAuthor("Alice", "Bob");
       result.Should().BeTrue();

       var alice = await _repo.FindByName("Alice");
       var bob = await _repo.FindByName("Bob");

       alice!.Following.Should().NotContain(bob!);
       bob!.Followers.Should().NotContain(alice);
   }
   
   [Fact]
   public async Task UnfollowAuthor_NotFollowing_ShouldReturnFalse()
   {
       var result = await _repo.UnfollowAuthor("Alice", "Bob");
       result.Should().BeFalse();
   }
   
   [Fact]
   public async Task IsFollowing_repoShouldReturnTrue_WhenFollowing()
   {
       await _repo.FollowAuthor("Alice", "Bob");
       var result = await _repo.IsFollowing("Alice", "Bob");
       result.Should().BeTrue();
   }
   
   [Fact]
   public async Task IsFollowing_repoShouldReturnFalse_WhenNotFollowing()
   {
       var result = await _repo.IsFollowing("Alice", "Bob");
       result.Should().BeFalse();
   }

   [Fact]
   public async Task GetAllFollowees_ShouldReturnCorrectList()
   {
       await _repo.Create("Helge", "helge@itu.dk");
       await _repo.FollowAuthor("Alice", "Bob");
       await _repo.FollowAuthor("Alice", "Helge");

       var followees = await _repo.GetAllFollowees("Alice");
       followees.Should().HaveCount(2); 
       followees.Should().Contain(f => f == "Bob");
       followees.Should().Contain(f => f == "Helge");
       
       await _repo.UnfollowAuthor("Alice", "Bob");
       followees = await _repo.GetAllFollowees("Alice");
       followees.Should().HaveCount(1); 
       followees.Should().Contain("Helge");
   }
   
   [Fact]
   public async Task GetAllFollowees_UnknownUser_ShouldReturnEmpty()
   {
       var followees = await _repo.GetAllFollowees("Unknown");
       followees.Should().BeEmpty();
   }
   
   [Fact]
   public async Task GetAllFollowees_WithNoFollowee_ShouldReturnEmpty()
   {
       var followees = await _repo.GetAllFollowees("Alice");
       followees.Should().BeEmpty();
   }
   
   [Fact]
   public async Task GetAllFollowees_UserHasNoFollowees_OtherUsersFollowingDoesNotAffect()
   {
       await _repo.Create("Helge", "helge@itu.dk");
       await _repo.FollowAuthor("Bob", "Helge");
       
       var aliceFollowees = await _repo.GetAllFollowees("Alice");
       aliceFollowees.Should().BeEmpty();
   }
   
   [Fact]
   public async Task GetAllFollowees_UserHasFollowersButFollowsNoOne_ShouldReturnEmpty()
   {
       await _repo.FollowAuthor("Bob", "Alice");
       var aliceFollowees = await _repo.GetAllFollowees("Alice");
       aliceFollowees.Should().BeEmpty(); 
   }
   
   [Fact]
   public async Task GetCheepsByAuthors_ShouldReturnCorrectCheeps()
   {
       var authors = new List<string> { "Alice", "Bob" };
       await _repo.AddCheep("Alice", "alice@itu.dk", "Alice Cheep 1");
       await _repo.AddCheep("Bob", "bob@itu.dk", "Bob Cheep 1");
       
       var page1 = await _repo.GetCheepsByAuthors(authors, 1, 3); 

       page1.Should().NotBeEmpty();
       page1.Should().OnlyContain(c => c.Author.UserName == "Alice" || c.Author.UserName == "Bob");
       page1.Should().HaveCount(3);
   
       var page2 = await _repo.GetCheepsByAuthors(authors, 2, 3);
       page2.Should().OnlyContain(c => c.Author.UserName == "Alice" || c.Author.UserName == "Bob");
   }

}