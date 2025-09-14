using Chirp.CLI;
using FluentAssertions;

namespace Chirp.Cli.Tests;

public class UnitTests
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
        UserInterface.ConvertTime(timestamp).Should().Be(expOutput);
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
        
        UserInterface.CheepToString(cheep).Should().Be(expOutput);
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
        ex.Message.Should().Be("Invalid cheep: Author and Message cannot be null or empty.");
    }
    
    
    [Fact]
    public void PrintCheepsTest_EmptyList()
    {
        var emptyCheeps = new List<Program.Cheep>();
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(emptyCheeps, emptyCheeps.Count));
        ex.Message.Should().Be("No cheeps found.");
    }
    
    [Fact]
    public void PrintCheepsTest_NullList()
    {
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(null!, 10));
        ex.Message.Should().Be("List of cheeps cannot be null");
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
        output.Should().Contain("Hello");
        output.Should().Contain("Bob @");
        output.Should().Contain("08/02/23 14:19:38");
        
        output.Should().NotContain("Lea @");
        output.Should().NotContain("Hi");
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
        output.Should().Contain("Hello");
        output.Should().Contain("Bob @");
        output.Should().Contain("08/02/23 14:19:38");
        
        output.Should().Contain("Lea @");
        output.Should().Contain("Hi");
    }
    
    
    [Fact]
    public void PrintCheeps_NegativeLimit()
    {
        var cheeps = new List<Program.Cheep> {
            new Program.Cheep { Author = "Bob", Timestamp = 1690978778, Message = "Hello" },
            new Program.Cheep { Author = "Lea", Timestamp = 1690978778, Message = "Hi" }
        };
        
        var ex = Assert.Throws<ArgumentException>(() => UserInterface.PrintCheeps(cheeps, -1));
        ex.Message.Should().Be("Limit cannot be negative: -1");
    }
}
    