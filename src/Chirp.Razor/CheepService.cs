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
        if (page <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {page}");
        return _db.GetCheeps(page); 
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {page}");
        if (string.IsNullOrWhiteSpace(author)) throw new ArgumentNullException($"Author cannot be null or empty. Invalid author: {author}");
        
        return _db.GetCheepsFromAuthor(author, page); 
    }
}
