using Microsoft.Data.Sqlite;
using Chirp.Razor.Models;

namespace Chirp.Razor;

public class DBFacade
{
    private readonly string _connectionString;

    public DBFacade(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath}";

        if (!File.Exists(dbPath))
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
        }

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var tableExists = conn.CreateCommand();
        tableExists.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='message';";
        var exists = tableExists.ExecuteScalar() != null;

        if (!exists)
            ExecuteSqlFromFile(conn, Path.Combine("Data", "schema.sql"));

        try
        {
            var countCmd = conn.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM message;";
            var count = Convert.ToInt32(countCmd.ExecuteScalar());

            if (count == 0)
                ExecuteSqlFromFile(conn, Path.Combine("Data", "dump.sql"));
        }
        catch (SqliteException)
        {
            ExecuteSqlFromFile(conn, Path.Combine("Data", "schema.sql"));
            ExecuteSqlFromFile(conn, Path.Combine("Data", "dump.sql"));
        }
    }

    private static void ExecuteSqlFromFile(SqliteConnection conn, string path)
    {
        if (!File.Exists(path)) return;
        var sql = File.ReadAllText(path);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public List<Cheep> GetCheeps()
    {
        const string query = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC";

        return ExecuteQuery(query);
    }

    public List<Cheep> GetCheepsFromAuthor(string author)
    {
        const string query = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY m.pub_date DESC";

        return ExecuteQuery(query, ("@author", author));
    }

    private List<Cheep> ExecuteQuery(string query, params (string Name, object Value)[] parameters)
    {
        var list = new List<Cheep>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = query;

        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Cheep
            {
                Author = new Author { Name = reader.GetString(0) },
                Text = reader.GetString(1),
                TimeStamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime
            });
        }

        return list;
    }
}
