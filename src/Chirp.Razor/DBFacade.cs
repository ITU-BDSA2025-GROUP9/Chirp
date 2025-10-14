using Microsoft.Data.Sqlite;
using Chirp.Razor.Models;

namespace Chirp.Razor;

/// <summary>
/// Provides low-level access to the SQLite database used by the Chirp application.
/// Responsible for database creation, schema initialization, seeding data,
/// and executing SQL queries to retrieve <see cref="Cheep"/> records.
/// </summary>
public class DBFacade
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBFacade"/> class.
    /// Ensures the database file exists and is properly initialized
    /// using schema and seed SQL scripts.
    /// </summary>
    /// <param name="dbPath">The full file path to the SQLite database file.</param>
    public DBFacade(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath}";

        // Create the database file if it doesn't exist
        if (!File.Exists(dbPath))
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
        }

        InitializeDatabase();
    }

    /// <summary>
    /// Ensures that the SQLite database contains the required tables and seed data.
    /// If the schema or dump files are missing, the database remains empty.
    /// </summary>
    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Check whether the "message" table exists
        var tableExists = conn.CreateCommand();
        tableExists.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='message';";
        var exists = tableExists.ExecuteScalar() != null;

        // If schema doesn't exist, create it
        if (!exists)
            ExecuteSqlFromFile(conn, Path.Combine("Data", "schema.sql"));

        try
        {
            // Check if there are any rows in the message table
            var countCmd = conn.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM message;";
            var count = Convert.ToInt32(countCmd.ExecuteScalar());

            // If the message table is empty, seed initial data
            if (count == 0)
                ExecuteSqlFromFile(conn, Path.Combine("Data", "dump.sql"));
        }
        catch (SqliteException)
        {
            // Recreate schema and seed data if queries fail (e.g., table missing)
            ExecuteSqlFromFile(conn, Path.Combine("Data", "schema.sql"));
            ExecuteSqlFromFile(conn, Path.Combine("Data", "dump.sql"));
        }
    }

    /// <summary>
    /// Executes SQL commands from a file against an open SQLite connection.
    /// </summary>
    /// <param name="conn">An open <see cref="SqliteConnection"/> instance.</param>
    /// <param name="path">The full path to the SQL file.</param>
    private static void ExecuteSqlFromFile(SqliteConnection conn, string path)
    {
        if (!File.Exists(path)) return;

        var sql = File.ReadAllText(path);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Retrieves all <see cref="Cheep"/> records from the database, ordered by publication time (descending).
    /// </summary>
    /// <returns>A list of <see cref="Cheep"/> entities with populated author and message data.</returns>
    public List<Cheep> GetCheeps()
    {
        const string query = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC";

        return ExecuteQuery(query);
    }

    /// <summary>
    /// Retrieves all <see cref="Cheep"/> records from a specific author.
    /// </summary>
    /// <param name="author">The author's username.</param>
    /// <returns>A list of <see cref="Cheep"/> records posted by the specified author.</returns>
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

    /// <summary>
    /// Executes a parameterized SQL query and maps each row to a <see cref="Cheep"/> object.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional named parameters for the SQL command.</param>
    /// <returns>A list of <see cref="Cheep"/> records returned by the query.</returns>
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
