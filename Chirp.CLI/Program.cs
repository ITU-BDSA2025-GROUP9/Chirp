using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    private static List<string[]> messages = new();

    static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("Missing argument.");

        if (args[0] == "read")
        {
            var csvPath = Path.Combine(AppContext.BaseDirectory, "data", "chirp_cli_db.csv");
            ReadCsvFile(csvPath);
            PostMessage();
        }
    }

    private static void ReadCsvFile(string filepath)
    {
        var lines = new List<string[]>();
        foreach (var line in File.ReadLines(filepath))
        {
            var parts = line.Split(',');
            if (parts.Length == 0 || parts[0] == "Author") continue;
            lines.Add(parts);
        }
        messages = lines;
    }

    private static void PostMessage() => Console.WriteLine(messages[0][1]);

    private static string ConvertTime(string date)
    {
        var ts = long.Parse(date);
        return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime.ToString("o");
    }
}