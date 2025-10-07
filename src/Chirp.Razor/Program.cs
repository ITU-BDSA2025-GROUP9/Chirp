using Chirp.Razor;
using Chirp.Razor.Database;
using Chirp.Razor.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entry point for the Chirp Razor application.
/// Configures dependency injection, EF Core database context, and middleware.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Determine database file path depending on environment.
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

// Register EF Core with SQLite, using the same chirp.db file
builder.Services.AddDbContext<ChirpDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register repository + service using scoped lifetimes (one per web request)
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

// Register Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure middleware
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
