using System.Diagnostics;
using FluentAssertions;

namespace Chirp.Cli.Tests;
public class End2EndTests
{
    /* Tests, that require the original csv database. 
    [Fact]
    public void E2ETest_ReadCommand()
    {

        var expectedOutput = "ropf @ 08/01/23 14:09:20: Hello, BDSA students!\r\n" +
                       "adho @ 08/02/23 14:19:38: Welcome to the course!\r\n" +
                       "adho @ 08/02/23 14:37:38: I hope you had a good summer.\r\n" +
                       "ropf @ 08/02/23 15:04:47: Cheeping cheeps on Chirp :)\r\n";
        
        var output = "";
        using (var process = new Process())
        {
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "./src/Chirp.CLI/bin/Debug/net8.0/Chirp.CLI.dll read 10";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = "../../../../../";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
          
            StreamReader reader = process.StandardOutput;
            output = reader.ReadToEnd();
            process.WaitForExit();
        }
        
        
        var outputLines = output.Split("\n");
        var exp = expectedOutput.Split("\n");
        for(var i = 0; i < exp.Length-1; i++) {
            outputLines[i].Should().Be(exp[i]);
        }
    }
    
    
    [Fact]
    public void E2ETest_CheepCommand()
    {
        var output = "";
        using (var process = new Process())
        {
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "./src/Chirp.CLI/bin/Debug/net8.0/Chirp.CLI.dll cheep \"Test: Hello!!!\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = "../../../../../";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
          
            StreamReader reader = process.StandardOutput;
            output = reader.ReadToEnd();
            process.WaitForExit();
        }
        output.Should().EndWith("Test: Hello!!!\r\n");
    }*/
}