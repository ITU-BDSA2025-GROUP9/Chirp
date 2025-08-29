using System.IO;

class Program {
    private static List<string[]> messages;
    private static void Main(string[] args) {
        try{
            if(args[0] == "read"){
                ReadCsvFile("C:/Users/bruger/Desktop/Chirp/Chirp.CLI/data/chirp_cli_db.csv");
                PostMessage();
            }
        } catch (Exception e){
            throw new Exception("Missing argument."); 
        }
    }
    private static void ReadCsvFile(string filepath) {
        List<string[]> lines = new List<string[]>();
        foreach(var line in File.ReadLines(filepath)) {
            string[] parts = line.Split(',');
            if (parts[0] == "Author") continue; 
            lines.Add(parts);
        } 
        messages = lines; 
    }

    private static void PostMessage() {
        Console.WriteLine(messages[0][0]);
    }
    
    private static string ConvertTime(string date) {
        DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(date)); 
        return dateTime.UtcDateTime + "";
    }
}
