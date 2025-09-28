using SimpleDB;
using Chirp.Shared;
using System.Text;

try
{
    Console.WriteLine("🚀 Starting Chirp CSVDBService...");
    Console.Out.Flush();

    var builder = WebApplication.CreateBuilder(args);
    var app = builder.Build();

    var csvPath = Path.Combine(AppContext.BaseDirectory, "chirp_service_db.csv");
    Console.WriteLine($"📂 Using CSV file: {csvPath}");
    Console.Out.Flush();

    if (!File.Exists(csvPath))
    {
        Console.WriteLine("⚠️ CSV file not found, creating new one...");
        File.WriteAllText(csvPath, "");
    }
    else
    {
        Console.WriteLine("✅ CSV file found.");
    }
    Console.Out.Flush();

    var _db = DatabaseFactory.Create(csvPath);
    Console.WriteLine("✅ Database initialized.");
    Console.Out.Flush();

    // POST /cheep
    app.MapPost("/cheep", (Cheep cheep) =>
    {
        try
        {
            Console.WriteLine($"➡️ POST /cheep (Author='{cheep.Author}', Message='{cheep.Message}')");
            cheep.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _db.Add(cheep);
            Console.WriteLine("✅ Cheep stored successfully.");
            return Results.Created($"/cheep/{cheep.Timestamp}", cheep);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in POST /cheep: {ex}");
            return Results.Problem(ex.Message);
        }
    });

    // GET /cheeps
    app.MapGet("/cheeps", () =>
    {
        try
        {
            Console.WriteLine("➡️ GET /cheeps");
            var cheeps = _db.GetAll().ToList();
            Console.WriteLine($"✅ Returning {cheeps.Count} cheeps.");
            return Results.Ok(cheeps);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GET /cheeps: {ex}");
            return Results.Problem(ex.Message);
        }
    });

    // Web frontend
    app.MapGet("/", () =>
    {
        Console.WriteLine("➡️ GET /");
        var cheeps = _db.GetAll().OrderByDescending(c => c.Timestamp).ToList();
        Console.WriteLine($"✅ Rendering {cheeps.Count} cheeps.");
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

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");

    Console.WriteLine($"🌍 Listening on port {port}");
    Console.Out.Flush();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"🔥 Fatal error: {ex}");
    Console.Out.Flush();
    throw;
}
