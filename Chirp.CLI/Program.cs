using System;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
class Program {
    public record Cheep{
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
    }

    private static List<Cheep> messages = [];

    static void Main(string[] args) {
        // goes out of 'bin/Debug/netX.Y/' and into the 'data' folder
        string csvPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "chirp_cli_db.csv")
        );
        
        if (args.Length == 0) throw new ArgumentException("Missing argument.");
        
        if (!File.Exists(csvPath)) {
            Console.Error.WriteLine($"CSV-file not found: {csvPath}");
            Environment.Exit(1);
        } else {
            switch (args[0]) {
                case "read":
                    
                    //RepairCsv(csvPath);
                    ReadCsvFile(csvPath);
                    PrintMessages();
                    break;
                case "cheep":
                    if(args.Length < 2) throw new ArgumentException("Missing new cheep.");
                    string message = string.Join(" ", args.Skip(1));
                    writeIntoCsvFile(message , csvPath);
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

        messages = csv.GetRecords<Cheep>().ToList();
    }
    
    static void RepairCsv(string path)
    {
        var lines = File.ReadAllLines(path).ToList();
        if (lines.Count <= 1) return; // header only or empty

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("\"") && line.EndsWith("\""))
            {
                line = line.Substring(1, line.Length - 2);
            }
            line = line.Replace("\"\"", "\"");

            lines[i] = line;
        }
        
        File.WriteAllLines(path, lines);
    }



    private static void PrintMessages() {
        if (messages.Count == 0) {
            Console.WriteLine("No messages.");
            return;
        }

        foreach (var m in messages) {
            Console.WriteLine($"{m.Author} @  {ConvertTime(m.Timestamp)}: {m.Message}");
        }
    }

    private static void writeIntoCsvFile(string cheep, string filepath) {
        var username = Environment.UserName;
        var date =  DateTimeOffset.Now.ToUnixTimeSeconds() + 7200; // Converted to danish timezone (+2h CEST)
        var post = username + ",\"" + cheep + "\"," + date;
        
        
        using (StreamWriter sw = File.AppendText(filepath)) {
            sw.WriteLine(post);
            sw.Flush();
        }
        
        ReadCsvFile(filepath);
        PrintMessages();
    }

    private static string ConvertTime(long ts) {
        string stamp = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime.ToString("o");
        stamp.Split("T");
        
        string[] parts = stamp.Split("T");
        string stampDate = parts[0];
        string stampTime = parts[1];
        stampDate = stampDate.Replace("-", "/");
        string[] fixedTimes = stampTime.Split(".");
        string returnedTime = fixedTimes[0];
        
        // correcting date output by rearranging the string
        string[] fixedDate = stampDate.Split("/");
        string shortYear = fixedDate[0].Substring(2); // remove first two chars
        string returnedDate = fixedDate[1] + "/" + fixedDate[2] + "/" + shortYear;
        
        return returnedDate + " " + returnedTime;
    }
}