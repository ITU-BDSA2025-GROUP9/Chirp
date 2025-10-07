using Microsoft.Data.Sqlite;
using Chirp.Shared;

namespace Chirp.Razor;

/// <summary>
/// Provides access to the underlying SQLite database.
/// Responsible for initializing schema, seeding data, and executing queries.
/// </summary>
public class DBFacade
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBFacade"/> class.
    /// Ensures the database file and schema exist.
    /// </summary>
    /// <param name="dbPath">Path to the SQLite database file.</param>
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

    /// <summary>
    /// Ensures that the database contains the required tables and seed data.
    /// If the schema or dump files are missing, the database remains empty.
    /// </summary>
    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='message';";
        var exists = checkCmd.ExecuteScalar() != null;

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

    /// <summary>
    /// Executes a SQL script from the given file path.
    /// </summary>
    /// <param name="conn">An open SQLite connection.</param>
    /// <param name="path">Path to a .sql file.</param>
    private static void ExecuteSqlFromFile(SqliteConnection conn, string path)
    {
        if (!File.Exists(path)) return;

        var sql = File.ReadAllText(path);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Retrieves all cheeps from the database, sorted by publication date.
    /// </summary>
    /// <returns>A list of <see cref="Cheep"/> records.</returns>
    public List<Cheep> GetCheeps()
    {
        var list = new List<Cheep>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Cheep
            {
                Author = reader.GetString(0),
                Message = reader.GetString(1),
                Timestamp = reader.GetInt64(2)
            });
        }

        return list;
    }

    /// <summary>
    /// Retrieves all cheeps posted by a specific author.
    /// </summary>
    /// <param name="author">The author’s username.</param>
    /// <returns>A list of <see cref="Cheep"/> records.</returns>
    public List<Cheep> GetCheepsFromAuthor(string author)
    {
        var list = new List<Cheep>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY m.pub_date DESC";
        cmd.Parameters.AddWithValue("@author", author);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Cheep
            {
                Author = reader.GetString(0),
                Message = reader.GetString(1),
                Timestamp = reader.GetInt64(2)
            });
        }

        return list;
    }
}
