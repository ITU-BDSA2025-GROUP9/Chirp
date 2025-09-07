using System.Globalization;
namespace Chirp.CLI;

public static class UserInterface {
    public static void PrintCheeps(IEnumerable<Program.Cheep> cheeps) {
        var cheepList = cheeps.ToList(); 
        
        if (cheepList.Count == 0) {
            throw new ArgumentException("No cheeps found.");
        }

        foreach (var m in cheepList) {
            Console.WriteLine($"{m.Author} @ {ConvertTime(m.Timestamp)}: {m.Message}");
        }
    }
    
    private static string ConvertTime(long ts) {
        var time = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();
        return time.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    }
}