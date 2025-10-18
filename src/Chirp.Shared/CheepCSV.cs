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
        using var reader = new StringReader(line);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim
        };
        using var csv = new CsvReader(reader, config);
        csv.Read();
        var author = csv.GetField(0)!;
        var message = csv.GetField(1)!;
        var tsField = csv.GetField(2);

        if (!long.TryParse(tsField, out var ts))
            throw new InvalidDataException($"Invalid timestamp '{tsField}'.");

        return new Cheep { Author = author, Message = message, Timestamp = ts };
    }
}