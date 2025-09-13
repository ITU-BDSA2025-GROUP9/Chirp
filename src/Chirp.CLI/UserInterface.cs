using System.Globalization;
namespace Chirp.CLI;

public static class UserInterface
{
    public static void PrintCheeps(IEnumerable<Program.Cheep> cheeps, int limit)
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

    public static string ConvertTime(long ts)
    {
        var time = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();
        return time.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public static string CheepToString(Program.Cheep c)
    {
        if(string.IsNullOrEmpty(c.Author) || string.IsNullOrEmpty(c.Message)) throw new ArgumentException("Invalid cheep: Author and Message cannot be null or empty.");
        return $"{c.Author} @ {ConvertTime(c.Timestamp)}: {c.Message}";
    }
}
  