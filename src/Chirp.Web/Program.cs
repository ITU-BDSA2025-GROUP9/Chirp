using System.Security.Claims;
using Chirp.Core;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Service;
using Chirp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entry point for the Chirp.Razor web application.
/// Configures the database, dependency injection, and the ASP.NET Core request pipeline.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

var isPlaywright =
    Environment.GetEnvironmentVariable("CHIRP_PLAYWRIGHT") == "true";

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
if (isPlaywright)
{
    var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
    connection.Open();

    builder.Services.AddSingleton<Microsoft.Data.Sqlite.SqliteConnection>(connection);
    builder.Services.AddDbContext<ChirpDbContext>(options =>
    {
        options.UseSqlite(connection);
    });
}
else
{
    builder.Services.AddDbContext<ChirpDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}


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

    if (isPlaywright)
    {
        var cs = db.Database.GetDbConnection().ConnectionString;
        if (!cs.Contains(":memory:"))
            throw new InvalidOperationException("Playwright tests are using the real database!");

        db.Database.EnsureCreated();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();
        SeedPlaywrightUsers(db, userManager);
    }
    else
    {
        db.Database.Migrate();
        Chirp.Infrastructure.Data.DbInitializer.SeedDatabase(db);
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

static void SeedPlaywrightUsers(
    ChirpDbContext db,
    UserManager<Author> userManager)
{
    if (db.Users.Any())
        return;

    var test = new Author
    {
        UserName = "test",
        Email = "test@test.test",
        EmailConfirmed = true
    };

    var testMe = new Author
    {
        UserName = "testMe",
        Email = "testme@test.test",
        EmailConfirmed = true
    };

    userManager.CreateAsync(test, "!Test123").GetAwaiter().GetResult();
    userManager.CreateAsync(testMe, "!Test123").GetAwaiter().GetResult();

    db.Cheeps.AddRange(
        new Cheep
        {
            AuthorId = test.Id,
            Text = "Cheep from test",
            TimeStamp = DateTime.UtcNow.AddMinutes(-5)
        },
        new Cheep
        {
            AuthorId = testMe.Id,
            Text = "Hello from testMe",
            TimeStamp = DateTime.UtcNow.AddMinutes(-1)
        }
    );
    db.SaveChanges();
}


/// <summary>
/// Start the web host.
/// </summary>
app.Run();
public partial class Program { }
