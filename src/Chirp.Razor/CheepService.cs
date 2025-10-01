using Microsoft.Data.Sqlite;

namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int page);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _db; 

    public CheepService(DBFacade db)
    {
        _db = db; 
    }

    public List<CheepViewModel> GetCheeps(int page)
    {
        return _db.GetCheeps(page); 
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        return _db.GetCheepsFromAuthor(author, page); 
    }
}
