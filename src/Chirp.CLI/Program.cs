using System.Globalization;
using Chirp.CLI;
using CsvHelper;
using CsvHelper.Configuration;
using DocoptNet;
using SimpleDB;

public class Program
{
    const string usage = @"Chirp CLI version.
    Usage:
      Chirp read [<limit>]
      Chirp cheep <message>
      Chirp (-h | --help)
      Chirp --version

    Options:
      -h --help     Show this screen.
      --version     Show version.
    ";
    
    /// <summary>
    /// Immutable record representing a single cheep entry.
    /// </summary>
    public record Cheep
    {
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
        
    }

    private static CSVDatabase<Cheep> _db = null!;
    private static string _csvPath = "";
    private static IEnumerable<Cheep> _messages = [];

    /// <summary>
    /// Entry point for the Chirp CLI.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments. Supported commands:
    /// <list type="bullet">
    /// <item>
    /// <description> load and print all cheeps.</description>
    /// </item>
    /// <item>
    /// <description> append a new cheep and print all cheeps.</description>
    /// </item>
    /// </list>
    /// </param>
    /// <exception cref="ArgumentException">Thrown when required arguments are missing or invalid.</exception>
    public static void Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;
        
        _csvPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "chirp_cli_db.csv")
        );

        EnsureDataFile(_csvPath);
    
        _db = new CSVDatabase<Cheep>(
            filePath: _csvPath,
            fromLine: FromCsvLine,
            toLine: ToCsvLine,
            getId: c => (int)(c.Timestamp % int.MaxValue)
        );

        if (arguments["read"].IsTrue) {
            ReadCsvFile();
            var limit = !arguments["<limit>"].IsNullOrEmpty ? int.Parse(arguments["<limit>"].ToString()) : _messages.Count();
            UserInterface.PrintCheeps(_messages, limit);
            
        } else if (arguments["cheep"].IsTrue && !string.IsNullOrWhiteSpace(arguments["<message>"]?.ToString())) {
            WriteIntoCsvFile(arguments["<message>"].ToString());
            UserInterface.PrintCheeps(_messages,_messages.Count());
        } else {
            throw new ArgumentException("Missing or nonvalid argument.");
        }
    }

    /// <summary>
    /// Loads all cheeps from the CSV database into the in-memory view.
    /// </summary>
    private static void ReadCsvFile()
    {
        _messages = _db.GetAll().ToList();
    }

    /// <summary>
    /// Appends a new cheep to the CSV database and refreshes the in-memory view.
    /// </summary>
    /// <param name="message">The message text of the new cheep.</param>
    private static void WriteIntoCsvFile(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Cheep cannot be null or empty.");
        var username = Environment.UserName;
        var date = DateTimeOffset.Now.ToUnixTimeSeconds();
        
        _db.Add(new Cheep { Author = username, Message = message, Timestamp = date });
        _messages = _db.GetAll().ToList();
    }

    /// <summary>
    /// Serializes a <see cref="Cheep"/> into a single CSV line with proper escaping.
    /// </summary>
    /// <param name="c">The cheep to serialize.</param>
    /// <returns>A CSV line containing author, message, and timestamp.</returns>
    private static string ToCsvLine(Cheep c)
    {
        static string Esc(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
        return $"{Esc(c.Author)},{Esc(c.Message)},{c.Timestamp}";
    }

    /// <summary>
    /// Parses a single CSV line into a <see cref="Cheep"/> instance.
    /// </summary>
    /// <param name="line">The CSV line to parse.</param>
    /// <returns>A <see cref="Cheep"/> parsed from the line.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the timestamp field cannot be parsed as a 64-bit integer.
    /// </exception>
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
        var author = csv.GetField(0)!;
        var message = csv.GetField(1)!;
        var tsField = csv.GetField(2);

        if (!long.TryParse(tsField, out var ts))
            throw new InvalidDataException($"Invalid timestamp '{tsField}'. Remove header rows first.");

        return new Cheep { Author = author, Message = message, Timestamp = ts };
    }

    /// <summary>
    /// Ensures the CSV data file and its parent directory exist. If the file does not exist, an empty file is created.
    /// </summary>
    /// <param name="path">Absolute path to the CSV file.</param>
    private static void EnsureDataFile(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (!File.Exists(path))
            File.WriteAllText(path, "");
    }

    /// <summary>
    /// Detects and removes a single header row from the CSV file when present.
    /// A header row is assumed if the third column of the first line is not a numeric timestamp.
    /// </summary>
    /// <param name="path">Absolute path to the CSV file.</param>
    private static void RemoveHeaderIfPresent(string path)
    {
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
