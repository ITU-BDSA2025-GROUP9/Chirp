using SimpleDB;
using Chirp.Shared;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..","..","Chirp.CLI", "data", "chirp_cli_db.csv"));
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
if (!File.Exists(dbPath)) File.WriteAllText(dbPath, "");

var _db = DatabaseFactory.Create(dbPath);
;

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

app.Run();