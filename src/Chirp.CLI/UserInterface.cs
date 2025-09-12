using System.Globalization;
namespace Chirp.CLI;

public static class UserInterface
{
    public static void PrintCheeps(IEnumerable<Program.Cheep> cheeps, int limit)
    {
        var list = cheeps.ToList();
        if (list.Count == 0) throw new ArgumentException("No cheeps found.");

        if (limit < list.Count) {
            for (var i = 0; i < limit; i++) {
                CheepToString(list[i]);
            }
        } else {
            foreach (var m in list)
                CheepToString(m);
        }
    }

    private static string ConvertTime(long ts)
    {
        var time = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();
        return time.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static void CheepToString(Program.Cheep c)
    {
        Console.WriteLine($"{c.Author} @ {ConvertTime(c.Timestamp)}: {c.Message}");
    }
}
  