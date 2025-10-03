using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace DefaultNamespace;

public class DBFacade
{
    private readonly string sqlDBFilePath;
    public DBFacade()
    {
        var dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

        if (string.IsNullOrEmpty(dbPath)) {
            dbPath = Path.Combine(Path.GetTempPath(), "chirp.db");
        }
        
        sqlDBFilePath = $"Data Source={dbPath}";
    }

    public List<CheepViewModel> GetCheeps()
    {
        var cheeps = new List<CheepViewModel>();

        using var connection  = new SqliteConnection(sqlDBFilePath);
        connection.Open();
        
        var sqlQuery = @"
                SELECT u.username, m.text, m.pub_date
                FROM message m
                JOIN user u ON m.author_id = u.user_id
                ORDER BY m.pub_date DESC;";

        
        var command = connection.CreateCommand();
        command.CommandText = sqlQuery;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cheeps.Add(new CheepViewModel(
                reader.GetString(0), // Author
                reader.GetString(1), // Message 
                reader.GetString(2)  // Timestamp
            ));
        }

        return cheeps;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        var cheeps = new List<CheepViewModel>();
        
        using var connection  = new SqliteConnection(sqlDBFilePath);
        connection.Open();
        
        var sqlQuery = @"
                SELECT u.username, m.text, m.pub_date
                FROM message m
                JOIN user u ON m.author_id = u.user_id
                WHERE u.username = @author
                ORDER BY m.pub_date DESC;";
        
        var command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@author", author);

        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            cheeps.Add(new CheepViewModel(
                reader.GetString(0), // Author
                reader.GetString(1), // Message 
                reader.GetString(2)  // Timestamp
            ));
        }

        return cheeps;
    }
}