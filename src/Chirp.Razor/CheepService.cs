using Microsoft.Data.Sqlite;

namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int page);
    List<CheepViewModel> GetCheepsFromAuthor(string author);
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
        return _db.GetCheeps(1); 
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        return _db.GetCheepsFromAuthor(author); 
    }
}
