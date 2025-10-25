using Chirp.Core;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Service;
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
    ? Path.Combine(env.ContentRootPath, "App_Data") //ensures path under project root, not bin/
    : "/home/data";

Directory.CreateDirectory(dataRoot);
var dbPath = Path.Combine(dataRoot, "chirp.db");

/// <summary>
/// Register Entity Framework Core with SQLite backend.
/// The context manages access to Author and Cheep entities.
/// </summary>
builder.Services.AddDbContext<ChirpDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddDefaultIdentity<Author>(options =>  
        options.SignIn.RequireConfirmedAccount = true)      
    .AddEntityFrameworkStores<ChirpDbContext>(); 

/// <summary>
/// Register application services for dependency injection.
/// Repository handles database queries; CheepService manages view models and paging logic.
/// </summary>
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ICheepService, CheepService>();

/// <summary>
/// Enable Razor Pages and configure application logging.
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.EnsureCreated();
    Chirp.Infrastructure.Data.DbInitializer.SeedDatabase(db);
}

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
app.UseAuthentication();
app.UseAuthorization();
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
public partial class Program { }