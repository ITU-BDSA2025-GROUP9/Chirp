using SimpleDB;
using Chirp.Shared;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..","..","Chirp.CLI", "data", "chirp_cli_db.csv"));
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
if (!File.Exists(dbPath)) File.WriteAllText(dbPath, "");

var _db = DatabaseFactory.Create(dbPath);

app.MapPost("/cheep", (Cheep cheep) =>
{
    // override timestamp server-side
    cheep.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    _db.Add(cheep);
    return Results.Created($"/cheep/{cheep.Timestamp}", cheep);
});

app.MapGet("/cheeps", () =>
{
    var cheeps = _db.GetAll().ToList();
    return Results.Ok(cheeps);
});

app.MapGet("/", () =>
{
    var cheeps = _db.GetAll().OrderByDescending(c => c.Timestamp).ToList();
    var html = new StringBuilder();

    html.Append("<!DOCTYPE html><html><head><title>Cheeps</title></head><body>");
    html.Append("<h1>All Cheeps</h1><ul>");

    foreach (var cheep in cheeps)
    {
        html.Append($"<li>{cheep.Message} <small>({cheep.Timestamp})</small></li>");
    }

    html.Append("</ul>");

    html.Append(@"
        <h2>New Cheep</h2>
        <form method='post' action='/cheepform'>
            <input type='text' name='Message' placeholder='Write something...' />
            <input type='submit' value='Post' />
        </form>
    ");
    html.Append("</body></html>");
    return Results.Content(html.ToString(), "text/html");
});

app.MapPost("/cheepform", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var cheep = new Cheep()
    {
        Message = form["Message"]!,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };
    _db.Add(cheep);
    return Results.Redirect("/");
});

app.Run();