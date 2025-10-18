using FluentAssertions;

namespace Chirp.Razor.Tests;
public class UnitTests
{
    private DBFacade _db;
    private CheepService _service;
    
    public UnitTests()
    {
        _db = new DBFacade();
        _service = new CheepService(_db);
    }
    
    
    [Theory]   
    [InlineData(1690978778, "08/02/23 14:19:38")]           
    [InlineData(1756816455, "09/02/25 14:34:15")]  
    [InlineData(1756816455.43, "09/02/25 14:34:15")]
    [InlineData(0, "01/01/70 01:00:00")] 
    [InlineData(-1, "01/01/70 00:59:59")]
    [InlineData(-3502493, "11/21/69 12:05:07")]
    [InlineData(-1690978778, "06/01/16 13:40:22")]
    public void UnixToLocalTest(long timestamp, string expOutput)
    {
        DBFacade.UnixToLocal(timestamp).Should().Be(expOutput);
    }
    
    [Fact]
    public void CheepViewModel_ShouldHaveValidProperties()
    {
        var cheep = new CheepViewModel("Bob", "Hello world", "08/02/23 14:19:38");
        
        cheep.Author.Should().Be("Bob");
        cheep.Message.Should().Be("Hello world");
        cheep.Timestamp.Should().Be("08/02/23 14:19:38");
    }
    
    
    [Fact] 
    public void GetCheeps_ShouldReturnListOfCheeps()
    {
        var cheeps = _service.GetCheeps(1);

        cheeps.Should().NotBeNull(); 
        cheeps.Should().NotBeEmpty(); 
        cheeps.Should().BeOfType<List<CheepViewModel>>();
        Assert.True(cheeps.Count <= 32);
        cheeps.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Author) && !string.IsNullOrWhiteSpace(c.Message) && !string.IsNullOrWhiteSpace(c.Timestamp));
        
    }

    [Fact]
    public void GetCheepsFromAuthor_ShouldReturnCorrectAuthor()
    {
        var cheeps = _service.GetCheeps(1); // use (first) arbitrary author 
        var firstCheep = cheeps.First();
        var author = firstCheep.Author;
        
        cheeps = _service.GetCheepsFromAuthor(author, 1);
        
        cheeps.Should().NotBeEmpty();
        Assert.True(cheeps.Count <= 32);
        cheeps.Should().OnlyContain(c => c.Author == author);
    }
    
    [Theory]
    [InlineData(-1)] 
    [InlineData(-2)]
    [InlineData(0)]
    public void GetCheeps_InvalidPageNo_ShouldThrowException(int page)
    {
        Action act = () => _service.GetCheeps(page);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.Message.Should().Contain($"Pagenumber must be greater than 0. Invalid pagenumber: {page}");  
    }

    
    [Theory]
    [InlineData("NonExistentUser123")]
    [InlineData("....")]
    [InlineData(".")]
    public void GetCheepsFromAuthor_NonExistentAuthor_ShouldReturnEmptyList(string author)
    {
        var cheeps = _service.GetCheepsFromAuthor(author, 1);
        cheeps.Should().BeEmpty();
    } 
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetCheepsFromAuthor_InvalidAuthor_ShouldThrowException(string author)
    {
        Action act = () => _service.GetCheepsFromAuthor(author, 1);
        
        act.Should().Throw<ArgumentNullException>()
            .Which.Message.Should().Contain($"Author cannot be null or empty. Invalid author: {author}");  
    }
    
    
    [Fact]
    public void GetCheeps_Page2_ShouldNotContainFirstPageCheeps()
    {
        var firstPage = _service.GetCheeps(1);
        var secondPage = _service.GetCheeps(2);

      
        firstPage.Should().NotBeEmpty();
        Assert.True(firstPage.Count <= 32);
        
        secondPage.Should().NotBeEmpty();
        Assert.True(secondPage.Count <= 32);

        secondPage.Should().NotContain(c =>
            firstPage.Any(f => f == c));
    }
}