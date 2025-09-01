using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

class Program
{
    // Define a type for your CSV rows
    public class MessageRecord {
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
    }

    private static List<MessageRecord> messages = new();

    static void Main(string[] args)
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "data", "chirp_cli_db.csv");
        if (args.Length == 0) throw new ArgumentException("Missing argument.");
        
        if (!File.Exists(csvPath)) {
            Console.Error.WriteLine($"CSV-file not found: {csvPath}");
            Environment.Exit(1);
        } else {
            switch (args[0]) {
                case "read":
                    ReadCsvFile(csvPath);
                    PrintMessages();
                    break;
                case "cheep":
                    if(args.Length < 2) throw new ArgumentException("Missing new cheep.");
                    writeIntoCsvFile(args[1] , csvPath);
                    break;
                default:
                    Console.Error.WriteLine("Not a valid argument: " + args[0]);
                    break;
            }
        }
    }

    private static void ReadCsvFile(string filepath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filepath);
        using var csv = new CsvReader(reader, config);

        messages = csv.GetRecords<MessageRecord>().ToList();
    }

    private static void writeIntoCsvFile(string cheep, string filepath) {
        var username = Environment.UserName;
        var date =  DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var post = username + ",\"" + cheep + "\"," + date;
        
        using (FileStream fs = new FileStream(filepath, FileMode.Append, FileAccess.Write))
        using (BufferedStream bs = new BufferedStream(fs)) 
        using (StreamWriter sw = new StreamWriter(bs)) {
            sw.WriteLine(post);
            //File.AppendAllText(filepath, post);
            sw.Flush();
            bs.Flush();
            fs.Flush(true);
            
            Console.WriteLine(post);
        }
        
        using (StreamReader sr = File.OpenText(filepath))
        {
            string s = "";
            while ((s = sr.ReadLine()) != null)
            {
                Console.WriteLine(s);
            }
        }

        //ReadCsvFile(filepath);
        //PrintMessages();
    }
    
    private static void PrintMessages() {
        if (messages.Count == 0) {
            Console.WriteLine("No messages.");
            return;
        }

        foreach (var m in messages)
        {
            Console.WriteLine($"{m.Author} @  {ConvertTime(m.Timestamp)}: {m.Message}");
        }
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