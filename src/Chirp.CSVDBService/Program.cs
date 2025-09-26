using SimpleDB;
using Chirp.Shared;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Define path to the shared CSV file
var dbPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..",
    "Chirp.CLI", "data", "chirp_cli_db.csv"));

// Ensure the directory and file exist
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
if (!File.Exists(dbPath)) File.WriteAllText(dbPath, "");

// Create database instance (via DatabaseFactory)
var _db = DatabaseFactory.Create(dbPath);


// Create a new cheep
app.MapPost("/cheep", (Cheep cheep) =>
{
    cheep.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    _db.Add(cheep);
    return Results.Created($"/cheep/{cheep.Timestamp}", cheep);
});

// Get all cheeps
app.MapGet("/cheeps", () =>
{
    var cheeps = _db.GetAll().ToList();
    return Results.Ok(cheeps);
});

// Web frontend (HTML page)
app.MapGet("/", (HttpRequest request) =>
{
    var showCheeps = request.Query.ContainsKey("show");
    var html = new StringBuilder();

    html.Append("<!DOCTYPE html><html><head><title>Cheeps</title></head><body>");
    html.Append("<h1>Chirp</h1>");

    if (!showCheeps)
    {
        html.Append("<form method='get' action='/'><button type='submit' name='show' value='true'>Show Cheeps</button></form>");
    }
    else
    {
        var cheeps = _db.GetAll().OrderByDescending(c => c.Timestamp).ToList();
        html.Append("<h2>All Cheeps</h2><ul>");
        foreach (var cheep in cheeps)
        {
            html.Append($"<li><b>{cheep.Author}</b>: {cheep.Message} ({cheep.Timestamp})</li>");
        }
        html.Append("</ul>");
    }

    html.Append("<h2>New Cheep</h2>");
    html.Append("<form method='post' action='/cheep'>");
    html.Append("Author: <input type='text' name='author'><br>");
    html.Append("Message: <input type='text' name='message'><br>");
    html.Append("<input type='submit' value='Cheep'>");
    html.Append("</form>");

    html.Append("</body></html>");
    return Results.Content(html.ToString(), "text/html");
});

app.Run();
