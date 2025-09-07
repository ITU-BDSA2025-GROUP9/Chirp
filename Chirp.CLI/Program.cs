using System.Globalization;
using Chirp.CLI;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;

public class Program
{
    public record Cheep
    {
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
    }

    private static CSVDatabase<Cheep> db = null!;
    private static string csvPath = "";
    private static IEnumerable<Cheep> _messages = [];

    static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("Missing argument.");

        csvPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "chirp_cli_db.csv")
        );

        EnsureDataFile(csvPath);
        RemoveHeaderIfPresent(csvPath);

        db = new CSVDatabase<Cheep>(
            filePath: csvPath,
            fromLine: FromCsvLine,
            toLine: ToCsvLine,
            getId: c => (int)(c.Timestamp % int.MaxValue)
        );

        switch (args[0])
        {
            case "read":
                ReadCsvFile();
                UserInterface.PrintCheeps(_messages);
                break;

            case "cheep":
                if (args.Length < 2) throw new ArgumentException("Missing new cheep.");
                var message = string.Join(" ", args.Skip(1));
                WriteIntoCsvFile(message);
                UserInterface.PrintCheeps(_messages);
                break;

            default:
                Console.Error.WriteLine("Not a valid argument: " + args[0]);
                break;
        }
    }
    
    private static void ReadCsvFile()
    {
        _messages = db.GetAll().ToList();
    }

    private static void WriteIntoCsvFile(string message)
    {
        var username = Environment.UserName;
        var date = DateTimeOffset.Now.ToUnixTimeSeconds();

        db.Add(new Cheep { Author = username, Message = message, Timestamp = date });

        // refresh the in-memory view
        _messages = db.GetAll().ToList();
    }

    private static string ToCsvLine(Cheep c)
    {
        static string Esc(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
        return $"{Esc(c.Author)},{Esc(c.Message)},{c.Timestamp}";
    }

    private static Cheep FromCsvLine(string line)
    {
        using var reader = new StringReader(line);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim
        };
        using var csv = new CsvReader(reader, config);
        csv.Read();
        var author = csv.GetField(0);
        var message = csv.GetField(1);
        var tsField = csv.GetField(2);

        if (!long.TryParse(tsField, out var ts))
            throw new InvalidDataException($"Invalid timestamp '{tsField}'. Remove header rows first.");

        return new Cheep { Author = author, Message = message, Timestamp = ts };
    }

    private static void EnsureDataFile(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (!File.Exists(path))
            File.WriteAllText(path, ""); // create empty file (no header)
    }

    private static void RemoveHeaderIfPresent(string path)
    {
        // If first line’s third column isn’t a number, assume it’s a header and drop it.
        var lines = File.ReadAllLines(path).ToList();
        if (lines.Count == 0) return;

        var first = lines[0];
        var cols = first.Split(',');
        if (cols.Length >= 3 && !long.TryParse(cols[2].Trim().Trim('"'), out _))
        {
            lines.RemoveAt(0);
            File.WriteAllLines(path, lines);
        }
    }
}
