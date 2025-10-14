using Chirp.Razor;
using Chirp.Razor.Database;
using Chirp.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entry point for the Chirp.Razor web application.
/// Configures the database, dependency injection, and the ASP.NET Core request pipeline.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Determine environment and set database root path.
/// Uses App_Data when running locally, and /home/data when deployed on Azure.
/// </summary>
var env = builder.Environment;
var dataRoot = env.IsDevelopment()
    ? Path.Combine(AppContext.BaseDirectory, "App_Data")
    : "/home/data";

Directory.CreateDirectory(dataRoot);
var dbPath = Path.Combine(dataRoot, "chirp.db");

/// <summary>
/// Initialize SQLite database if it does not already exist.
/// Reads schema.sql and dump.sql from the Data directory to set up tables and seed data.
/// </summary>
if (!File.Exists(dbPath))
{
    var connStr = $"Data Source={dbPath}";
    var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
    var schemaPath = Path.Combine(dataDir, "schema.sql");
    var dumpPath = Path.Combine(dataDir, "dump.sql");

    using var conn = new SqliteConnection(connStr);
    conn.Open();

    foreach (var path in new[] { schemaPath, dumpPath })
    {
        if (File.Exists(path))
        {
            var sql = File.ReadAllText(path);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}

/// <summary>
/// Register Entity Framework Core with SQLite backend.
/// The context manages access to Author and Cheep entities.
/// </summary>
builder.Services.AddDbContext<ChirpDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

/// <summary>
/// Register application services for dependency injection.
/// CheepRepository handles database queries; CheepService manages view models and paging logic.
/// </summary>
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

/// <summary>
/// Enable Razor Pages and configure application logging.
/// Azure logging integration ensures logs are visible in App Service.
/// </summary>
builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.SetMinimumLevel(LogLevel.Information);

/// <summary>
/// Build the web application and configure the HTTP request pipeline.
/// </summary>
var app = builder.Build();

/// <summary>
/// Configure error handling and security policies for production.
/// </summary>
if (!env.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

/// <summary>
/// Add middleware for HTTPS redirection, static file serving, and routing.
/// Map Razor pages as endpoints.
/// </summary>
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

/// <summary>
/// Log startup success and database path for debugging and verification.
/// </summary>
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Chirp.Razor started using database: {Path}", dbPath);

/// <summary>
/// Start the web host.
/// </summary>
app.Run();
