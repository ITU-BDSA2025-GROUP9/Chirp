namespace Chirp.Razor;
using Microsoft.Data.Sqlite;
public class DBFacade
{
    private readonly string _connectionString;

    public DBFacade()
    {
        var dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            var temp = Path.GetTempPath();
            dbPath = Path.Combine(temp, "chirp.db");
        }
        _connectionString = $"Data Source={dbPath}";
    }

    public List<CheepViewModel> GetCheeps()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC";
        return ReadCheeps(cmd);
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY m.pub_date DESC";
        cmd.Parameters.AddWithValue("@author", author);
        return ReadCheeps(cmd);
    }

    private static List<CheepViewModel> ReadCheeps(SqliteCommand cmd)
    {
        var list = new List<CheepViewModel>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var author = r.GetString(0);
            var message = r.GetString(1);
            var ts = r.GetInt64(2);
            list.Add(new CheepViewModel(author, message, UnixToLocal(ts)));
        }
        return list;
    }

    private static string UnixToLocal(long unixSeconds) =>
        DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            .ToLocalTime()
            .ToString("MM/dd/yy H:mm:ss");
    
}