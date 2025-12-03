using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Service;
using Chirp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
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

builder.Services.AddSession();
/// <summary>
/// Register Entity Framework Core with SQLite backend.
/// The context manages access to Author and Cheep entities.
/// </summary>
builder.Services.AddDbContext<ChirpDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddDefaultIdentity<Author>(options =>  
        options.SignIn.RequireConfirmedAccount = true)      
    .AddEntityFrameworkStores<ChirpDbContext>(); 

builder.Services.AddAuthentication()
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication_github_clientId"]!;
        o.ClientSecret = builder.Configuration["authentication_github_clientSecret"]!;
        o.CallbackPath = "/signin-github";
        
        o.Scope.Add("user:email");
        o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    });

/// <summary>
/// Register application services for dependency injection.
/// Repository handles database queries; CheepService manages view models and paging logic.
/// </summary>
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

/// <summary>
/// Enable Razor Pages and configure application logging.
/// </summary>
builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(60));

/// <summary>
/// Build the web application and configure the HTTP request pipeline.
/// </summary>
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

    if (db.Database.IsRelational())
    {
        // Normal run
        db.Database.Migrate();
        Chirp.Infrastructure.Data.DbInitializer.SeedDatabase(db);
    }
    else
    {
        // Run for testing in memory (in future)
        db.Database.EnsureCreated();
    }
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
app.UseSession(); 
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
