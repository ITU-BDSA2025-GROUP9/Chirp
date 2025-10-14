using Chirp.Razor;
using Chirp.Razor.Database;
using Chirp.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var dataRoot = env.IsDevelopment() 
    ? Path.Combine(AppContext.BaseDirectory, "App_Data") 
    : "/home/data";

Directory.CreateDirectory(dataRoot);
var dbPath = Path.Combine(dataRoot, "chirp.db");

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

builder.Services.AddDbContext<ChirpDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddRazorPages();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

if (!env.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Chirp.Razor started using database: {Path}", dbPath);

app.Run();