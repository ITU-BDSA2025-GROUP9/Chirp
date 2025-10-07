using Chirp.Razor;

/// <summary>
/// Entry point for the Chirp Razor application.
/// Configures dependency injection, database path resolution, and middleware.
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

// Register services and dependencies.
builder.Services.AddSingleton(_ => new DBFacade(dbPath));
builder.Services.AddSingleton<ICheepService, CheepService>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure middleware.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// Run the web application.
app.Run();
