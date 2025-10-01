using SimpleDB;
using Chirp.Shared;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/// <summary>
/// Path to the CSV file used as persistent storage
/// within the Azure container.
/// </summary>
var csvPath = Path.Combine("/home", "chirp_service_db.csv");

// Ensure the file exists
if (!File.Exists(csvPath)) File.WriteAllText(csvPath, "");

/// <summary>
/// Database instance backed by the CSV file, created via DatabaseFactory.
/// </summary>
var _db = DatabaseFactory.CreateCsv(csvPath);

/// <summary>
/// POST /cheep  
/// Creates a new cheep and saves it to the database.
/// </summary>
app.MapPost("/cheep", (Cheep cheep) =>
{
    cheep.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    _db.Add(cheep);
    return Results.Created($"/cheep/{cheep.Timestamp}", cheep);
});

/// <summary>
/// GET /cheeps  
/// Retrieves all cheeps from the database.
/// </summary>
app.MapGet("/cheeps", () =>
{
    var cheeps = _db.GetAll().ToList();
    return Results.Ok(cheeps);
});

/// <summary>
/// GET /  
/// Serves a minimal HTML frontend to display all cheeps.
/// </summary>
app.MapGet("/", () =>
{
    var cheeps = _db.GetAll().OrderByDescending(c => c.Timestamp).ToList();
    var html = new StringBuilder();
    html.Append("<html><head><title>Chirp</title></head><body>");
    html.Append("<h1>Chirps</h1><ul>");
    foreach (var cheep in cheeps)
    {
        html.Append($"<li><b>{cheep.Author}</b>: {cheep.Message}</li>");
    }
    html.Append("</ul></body></html>");
    return Results.Content(html.ToString(), "text/html");
});

/// <summary>
/// Configures the service to listen on the PORT defined
/// by Azure App Service, or defaults to port 8080.
/// </summary>
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");


app.Run();