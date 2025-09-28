using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.Shared;

public static class CheepCsv
{
    public static string ToCsvLine(Cheep c)
    {
        static string Esc(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
        return $"{Esc(c.Author)},{Esc(c.Message)},{c.Timestamp}";
    }

    public static Cheep FromCsvLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) 
            throw new ArgumentException("Empty CSV line");

        var parts = line.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length < 3)
            throw new FormatException($"Invalid CSV line: {line}");

        return new Cheep
        {
            Author = parts[0],
            Message = parts[1],
            Timestamp = long.Parse(parts[2])
        };
    }

}