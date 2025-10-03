using Microsoft.Data.Sqlite;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

namespace Chirp.Razor;

public class DBFacade
{
    private readonly string _connectionString;
    private readonly int _cheepno = 32;

    public DBFacade()
    {
        var dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            var temp = Path.GetTempPath();
            dbPath = Path.Combine(temp, "chirp.db");
        }

        _connectionString = $"Data Source={dbPath}";

		if(!File.Exists(dbPath)){
			using var conn = new SqliteConnection(_connectionString);
			conn.Open();
		}

		if(new FileInfo(dbPath).Length == 0){
			InitializeDatabase();
		}
    }

	private void InitializeDatabase() 
	{
   		var assembly = Assembly.GetExecutingAssembly(); 
		var provider = new EmbeddedFileProvider(assembly);

    	using var conn = new SqliteConnection(_connectionString);
    	conn.Open();

    	var schemaFile = provider.GetFileInfo("Data/schema.sql");
    	if (schemaFile.Exists) {
        	using var stream = schemaFile.CreateReadStream();
        	using var reader = new StreamReader(stream);
        	var schemaSql = reader.ReadToEnd();

        	using var cmd = conn.CreateCommand();
        	cmd.CommandText = schemaSql;
        	cmd.ExecuteNonQuery();
    	} else {
        	throw new FileNotFoundException("Could not find schema.sql as embedded resource.");
    	}

    	var dumpFile = provider.GetFileInfo("Data/dump.sql");
    	if (dumpFile.Exists)
    	{
        	using var stream = dumpFile.CreateReadStream();
        	using var reader = new StreamReader(stream);
        	var dumpSql = reader.ReadToEnd();

        	using var cmd = conn.CreateCommand();
	        cmd.CommandText = dumpSql;
	        cmd.ExecuteNonQuery();
    	} else {
        	throw new FileNotFoundException("Could not find dump.sql as embedded resource.");
    	}
	}


    public List<CheepViewModel> GetCheeps(int page)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            ORDER BY m.pub_date DESC
            LIMIT @_cheepno OFFSET @row_count;
            ";
        
        cmd.Parameters.AddWithValue("@row_count", (page-1) * 32);
        cmd.Parameters.AddWithValue("_cheepno", _cheepno);
        return ReadCheeps(cmd);
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT u.username, m.text, m.pub_date
            FROM message m
            JOIN user u ON m.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY m.pub_date DESC
            LIMIT @_cheepno OFFSET @row_count";
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@row_count", (page-1) * 32);
        cmd.Parameters.AddWithValue("_cheepno", _cheepno);
        
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

    public static string UnixToLocal(long unixSeconds) =>
        DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            .ToLocalTime()
            .ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    
}