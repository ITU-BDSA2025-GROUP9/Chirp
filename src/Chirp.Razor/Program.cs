using Chirp.Razor;
using Chirp.Razor.Database;
using Chirp.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Determine database file path
string dbPath;
if (builder.Environment.IsDevelopment())
{
    var localDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
    Directory.CreateDirectory(localDir);
    dbPath = Path.Combine(localDir, "chirp.db");
}
else
{
    var azureDir = "/home/site/wwwroot/App_Data";
    Directory.CreateDirectory(azureDir);
    dbPath = Path.Combine(azureDir, "chirp.db");
}

// Initialize SQLite DB from schema.sql and dump.sql if it doesn’t exist yet
if (!File.Exists(dbPath))
{
    Console.WriteLine("Creating chirp.db from schema.sql and dump.sql...");

    var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
    var schemaPath = Path.Combine(dataDir, "schema.sql");
    var dumpPath = Path.Combine(dataDir, "dump.sql");

    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    void ExecuteSql(string path)
    {
        if (File.Exists(path))
        {
            var sql = File.ReadAllText(path);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }

    ExecuteSql(schemaPath);
    ExecuteSql(dumpPath);
    conn.Close();

    Console.WriteLine("✅ chirp.db initialized successfully.");
}
else
{
    Console.WriteLine($"Using existing database at {dbPath}");
}

// Register EF Core with SQLite
builder.Services.AddDbContext<ChirpDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register repositories and services
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

builder.Services.AddRazorPages();

var app = builder.Build();

// No EF migrations — schema comes from SQL files
// but ensure EF can read the DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.EnsureCreated(); // no schema overwrite
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.Run();
