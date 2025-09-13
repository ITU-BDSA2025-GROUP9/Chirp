using Chirp.CLI;
namespace Chirp.Cli.Tests;

public class UserInterfaceTests
{
    [Theory]   
    [InlineData(1690978778, "08/02/23 14:19:38")]           
    [InlineData(1756816455, "09/02/25 14:34:15")]  
    [InlineData(1756816455.43, "09/02/25 14:34:15")]
    [InlineData(0, "01/01/70 01:00:00")] 
    [InlineData(-1, "01/01/70 00:59:59")]
    [InlineData(-3502493, "11/21/69 12:05:07")]
    [InlineData(-1690978778, "06/01/16 13:40:22")]
    public void ConvertTimeTest(long timestamp, string expOutput)
    {
        Assert.Equal(expOutput, UserInterface.ConvertTime(timestamp));
    }
    
    
    [Theory]   
    [InlineData("Bob", 1690978778, "Hello", "Bob @ 08/02/23 14:19:38: Hello")]
    [InlineData("Bob2", -3502493, "Hello2", "Bob2 @ 11/21/69 12:05:07: Hello2")]    
    [InlineData("Lea", 0, "Hi", "Lea @ 01/01/70 01:00:00: Hi")]
    public void CheepToStringTest_ReturnsExpectedString(string author, long timestamp, string message, string expOutput)
    {
        var cheep = new Program.Cheep {
            Author = author,
            Timestamp = timestamp,
            Message = message
        };
        
        Assert.Equal(expOutput, UserInterface.CheepToString(cheep));
    }
    
    
    [Theory]   
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null,"")]
    [InlineData(null, null)]
    [InlineData("Bob", null)]
    [InlineData("Bob","")]
    [InlineData(null,"Hello")]
    [InlineData("","Hello")]
    public void CheepToStringTest_ShouldThrowException(string author, string message)
    {
        var cheep = new Program.Cheep {
            Author = author,
            Timestamp = 0,
            Message = message
        };
        
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.CheepToString(cheep));
        Assert.Equal("Invalid cheep: Author and Message cannot be null or empty.", ex.Message);
    }
    
    
    [Fact]
    public void PrintCheepsTest_EmptyList()
    {
        var emptyCheeps = new List<Program.Cheep>();
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(emptyCheeps, emptyCheeps.Count));
        Assert.Equal("No cheeps found.", ex.Message);
    }
    
    [Fact]
    public void PrintCheepsTest_NullList()
    {
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(null!, 10));
        Assert.Equal("List of cheeps cannot be null", ex.Message);
    }
    

    [Fact]
    public void PrintCheepsTest_InvalidCheep()
    {
        var cheeps = new List<Program.Cheep> {
            new Program.Cheep { Author = "Bob", Timestamp = 0, Message = "Hello" },
            new Program.Cheep { Author = "", Timestamp = 0 , Message = "Hi" }
        };
        
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(cheeps, cheeps.Count));
        Assert.Equal("Invalid cheep: Author and Message cannot be null or empty.", ex.Message);
    }
    
    [Fact]   
    public void PrintCheeps_Limit1()
    {
        var cheeps = new List<Program.Cheep> {
            new Program.Cheep { Author = "Bob", Timestamp = 1690978778, Message = "Hello" },
            new Program.Cheep { Author = "Lea", Timestamp = 1690978778, Message = "Hi" }
        };

        using var sw = new StringWriter();
        Console.SetOut(sw);
        
        UserInterface.PrintCheeps(cheeps, 1);
        
        var output = sw.ToString();
        Assert.Contains("Hello", output);
        Assert.Contains("Bob @", output);
        Assert.Contains("08/02/23 14:19:38", output);
        
        Assert.DoesNotContain("Lea", output);
        Assert.DoesNotContain("Hi", output);
    }
    
    [Fact]
    public void PrintCheeps_Limit2()
    {
        var cheeps = new List<Program.Cheep>
        {
            new Program.Cheep { Author = "Bob", Timestamp = 1690978778, Message = "Hello" },
            new Program.Cheep { Author = "Lea", Timestamp = 1690978778, Message = "Hi" }
        };

        using var sw = new StringWriter();
        Console.SetOut(sw);
        
        UserInterface.PrintCheeps(cheeps, cheeps.Count);
        
        var output = sw.ToString();
        Assert.Contains("Hello", output);
        Assert.Contains("Bob @", output);
        Assert.Contains("08/02/23 14:19:38", output);
        Assert.Contains("Lea @", output);
        Assert.Contains("Hi", output);
    }
    
    
    [Fact]
    public void PrintCheeps_NegativeLimit()
    {
        var cheeps = new List<Program.Cheep> {
            new Program.Cheep { Author = "Bob", Timestamp = 1690978778, Message = "Hello" },
            new Program.Cheep { Author = "Lea", Timestamp = 1690978778, Message = "Hi" }
        };
        
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(cheeps, -1));
        Assert.Equal("Limit cannot be negative: -1", ex.Message);
    }
}
    
    
   
