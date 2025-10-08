using Chirp.Razor.Models;

namespace Chirp.Razor.Repositories;

public class CheepRepository : ICheepRepository
{
    private readonly DBFacade _db;

    public CheepRepository(IWebHostEnvironment env)
    {
        var dbPath = Path.Combine(env.ContentRootPath, "App_Data", "chirp.db");
        _db = new DBFacade(dbPath);
    }

    public IEnumerable<Cheep> GetAllCheeps() => _db.GetCheeps();

    public IEnumerable<Cheep> GetCheepsByAuthor(string authorName) => _db.GetCheepsFromAuthor(authorName);

    public void AddCheep(Cheep cheep)
    {
        throw new NotImplementedException("Posting new cheeps not implemented yet.");
    }
}

