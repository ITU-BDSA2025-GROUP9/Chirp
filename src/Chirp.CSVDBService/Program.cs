using SimpleDB;
using Chirp.Shared;
using System.Text;
using Chirp.CLI;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/// <summary>
/// Path to the shared CSV database file (same as used by CLI).
/// Ensures the directory and file exist before initializing the database.
/// </summary>
var dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Chirp.CLI", "data", "chirp_cli_db.csv"));
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
if (!File.Exists(dbPath)) File.WriteAllText(dbPath, "");

var _db = DatabaseFactory.Create(dbPath);

/// <summary>
/// API endpoint: Create a new cheep via JSON POST request.
/// Author is provided by client, timestamp is overridden server-side.
/// </summary>
app.MapPost("/cheep", (Cheep cheep) =>
{
    cheep.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    _db.Add(cheep);
    return Results.Created($"/cheep/{cheep.Timestamp}", cheep);
});

/// <summary>
/// API endpoint: Get all cheeps as JSON.
/// </summary>
app.MapGet("/cheeps", () =>
{
    var cheeps = _db.GetAll().ToList();
    return Results.Ok(cheeps);
});

/// <summary>
/// Web frontend: Displays the Chirp homepage.
/// - Shows a "Show Cheeps" button initially.
/// - When ?show=true is present, displays all cheeps formatted using CLI logic.
/// - Provides a form to create a new cheep.
/// </summary>
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
            var formatted = UserInterface.CheepToString(cheep);
            html.Append($"<li>{formatted}</li>");
        }
        html.Append("</ul>");
        html.Append("<a href='/'>Hide Cheeps</a>");
    }

    html.Append(@"
        <h2>New Cheep</h2>
        <form method='post' action='/cheepform'>
            <input type='text' name='Author' placeholder='Your name' required />
            <br/>
            <input type='text' name='Message' placeholder='Write something...' required />
            <br/>
            <input type='submit' value='Post' />
        </form>
    ");

    html.Append("</body></html>");
    return Results.Content(html.ToString(), "text/html");
});

/// <summary>
/// Web frontend: Handles HTML form submission for creating a cheep.
/// - Validates input using CLI UserInterface logic.
/// - On success: saves and redirects back to homepage with cheeps visible.
/// - On failure: displays error message.
/// </summary>
app.MapPost("/cheepform", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var cheep = new Cheep()
    {
        Author = form["Author"]!,
        Message = form["Message"]!,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };

    try
    {
        var _ = UserInterface.CheepToString(cheep);
        _db.Add(cheep);
        return Results.Redirect("/?show=true");
    }
    catch (Exception ex)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><title>Error</title></head><body>");
        html.Append($"<h1>Error</h1><p>{ex.Message}</p>");
        html.Append("<a href='/'>Back</a>");
        html.Append("</body></html>");
        return Results.Content(html.ToString(), "text/html");
    }
});

app.Run();
