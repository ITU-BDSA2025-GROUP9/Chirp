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
    // ✅ Use persistent storage under /home/data for Azure
    var azureDir = "/home/data";
    Directory.CreateDirectory(azureDir);
    dbPath = Path.Combine(azureDir, "chirp.db");
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
    DbInitializer.SeedDatabase(db);
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
public partial class Program { }
