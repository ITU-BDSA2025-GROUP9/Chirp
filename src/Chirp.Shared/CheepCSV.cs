using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.Shared;

/// <summary>
/// Provides helper methods for converting <see cref="Cheep"/> objects
/// to and from CSV-formatted lines.
/// </summary>
public static class CheepCsv
{
    /// <summary>
    /// Converts a <see cref="Cheep"/> instance into a single CSV line.
    /// </summary>
    /// <param name="c">The <see cref="Cheep"/> object. </param>
    /// <returns>
    /// A CSV-formatted string containing the Author, Message, and Timestamp fields.
    /// </returns>
    public static string ToCsvLine(Cheep c)
    {
        static string Esc(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
        return $"{Esc(c.Author)},{Esc(c.Message)},{c.Timestamp}";
    }

    /// <summary>
    /// Parses a CSV line into a <see cref="Cheep"/> instance.
    /// </summary>
    /// <param name="line"> A CSV-formatted string containing the Author, Message, and Timestamp fields. </param>
    /// <returns>A new <see cref="Cheep"/> object based on the parsed fields.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown if the timestamp field cannot be parsed as a valid <see cref="long"/>.
    /// </exception>
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