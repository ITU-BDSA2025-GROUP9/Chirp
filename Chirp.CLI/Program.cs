using System.Globalization;
using Chirp.CLI;
using CsvHelper;
using CsvHelper.Configuration;

public class Program {
    public record Cheep{
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
    }

    private static IEnumerable<Cheep> _messages = [];
    static void Main(string[] args) {
        if (args.Length == 0) throw new ArgumentException("Missing argument.");
        
        // goes out of 'bin/Debug/netX.Y/' and into the 'data' folder
        var csvPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "chirp_cli_db.csv")
        );
        
        if (!File.Exists(csvPath)) {
            Console.Error.WriteLine($"CSV-file not found: {csvPath}");
            Environment.Exit(1);
        } else {
            switch (args[0]) {
                case "read":
                    try {
                        ReadCsvFile(csvPath);
                        UserInterface.PrintCheeps(_messages);
                    } catch (Exception e){
                        Console.Error.WriteLine(e.Message);
                    }
                    break;
                case "cheep":
                    if(args.Length < 2) throw new ArgumentException("Missing new cheep.");
                    var message = string.Join(" ", args.Skip(1));
                    WriteIntoCsvFile(message , csvPath);
                    break;
                default:
                    Console.Error.WriteLine("Not a valid argument: " + args[0]);
                    break;
            }
        }
    }

    private static void ReadCsvFile(string filepath) {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filepath);
        using var csv = new CsvReader(reader, config);

        _messages = csv.GetRecords<Cheep>().ToList();
    }
    
    static void RepairCsv(string path) {
        var lines = File.ReadAllLines(path).ToList();
        if (lines.Count <= 1) return; // header only or empty

        for (int i = 1; i < lines.Count; i++) {
            var line = lines[i].Trim();
            if (line.StartsWith("\"") && line.EndsWith("\"")) {
                line = line.Substring(1, line.Length - 2);
            }
            line = line.Replace("\"\"", "\"");

            lines[i] = line;
        }
        File.WriteAllLines(path, lines);
    }
    
    private static void WriteIntoCsvFile(string cheep, string filepath) {
        var username = Environment.UserName;
        var date = DateTimeOffset.Now.ToUnixTimeSeconds();
        var post = username + ",\"" + cheep + "\"," + date;
        
        
        using (StreamWriter sw = File.AppendText(filepath)) {
            sw.WriteLine(post);
            sw.Flush();
        }
        
        ReadCsvFile(filepath);
        UserInterface.PrintCheeps(_messages);
    }
}