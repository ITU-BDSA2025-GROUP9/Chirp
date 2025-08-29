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
    public class MessageRecord
    {
        public string Author { get; set; } = "";
        public string Message { get; set; } = "";
        public long Timestamp { get; set; }
    }

    private static List<MessageRecord> messages = new();

    static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("Missing argument.");

        if (args[0] == "read")
        {
            var csvPath = Path.Combine(AppContext.BaseDirectory, "data", "chirp_cli_db.csv");

            if (!File.Exists(csvPath))
            {
                Console.Error.WriteLine($"CSV not found: {csvPath}");
                Console.Error.WriteLine($"BaseDir: {AppContext.BaseDirectory}");
                Environment.Exit(1);
            }

            ReadCsvFile(csvPath);
            PostMessage();
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

    private static void PostMessage()
    {
        if (messages.Count == 0)
        {
            Console.WriteLine("No messages.");
            return;
        }

        foreach (var m in messages)
        {
            Console.WriteLine($"{m.Author} @  {ConvertTime(m.Timestamp)}: {m.Message}");
        }
    }




    private static string ConvertTime(long ts)
    {
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