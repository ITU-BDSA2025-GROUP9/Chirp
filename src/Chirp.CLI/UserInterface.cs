using System.Globalization;
using Chirp.Shared;   // for Cheep
using SimpleDB;       // for DatabaseFactory
namespace Chirp.CLI;

/// <summary>
/// Provides static helper methods for displaying and formatting <see cref="Cheep"/> objects in the CLI.
/// </summary>
public static class UserInterface
{
    
    /// <summary>
    /// Prints up to the limit cheeps to the console.
    /// </summary>
    /// <param name="cheeps">The collection of cheeps to display.</param>
    /// <param name="limit">The maximum number of cheeps to print.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the limit is negative, or if cheeps is null (i.e. the collection contains no cheeps).
    /// </exception>
    public static void PrintCheeps(IEnumerable<Cheep> cheeps, int limit)
    {
        if (limit < 0) throw new ArgumentException("Limit cannot be negative: " + limit);
        if (cheeps == null) throw new ArgumentException("List of cheeps cannot be null");
        
        var list = cheeps.ToList();
        if (list.Count == 0) throw new ArgumentException("No cheeps found.");
        
        var cheepCount = Math.Min(limit, list.Count);
        for (var i = 0; i < cheepCount; i++) {
            Console.WriteLine(CheepToString(list[i]));
        }
    }

    /// <summary>
    /// Converts a unix timestamp into a local date/time string.
    /// </summary>
    /// <param name="ts">The timestamp to convert.</param>
    /// <returns>A string formatted as "MM/dd/yy HH:mm:ss" in local time.</returns>
    public static string ConvertTime(long ts)
    {
        var time = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();
        return time.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a <see cref="Cheep"/> object into a formatted string. 
    /// </summary>
    /// <param name="c">The cheep to format.</param>
    /// <returns>A string in the format: "Author @ MM/dd/yy HH:mm:ss: Message".</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the cheep has a null or empty Author or Message.
    /// </exception>
    public static string CheepToString(Cheep c)
    {
        if (string.IsNullOrEmpty(c.Author) || string.IsNullOrEmpty(c.Message)) 
            throw new ArgumentException("Invalid cheep: Author and Message cannot be null or empty.");
        return $"{c.Author} @ {ConvertTime(c.Timestamp)}: {c.Message}";
    }

}
  