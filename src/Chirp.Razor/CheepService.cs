using System.Globalization;
using Chirp.Razor.Models;
using Chirp.Razor.Repositories;

namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int page);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page);
}

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;
    private const int PageSize = 32;

    public CheepService(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public List<CheepViewModel> GetCheeps(int page)
    {
        return _cheepRepository.GetAllCheeps()
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        return _cheepRepository.GetCheepsByAuthor(author)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    private static CheepViewModel Map(Cheep c) =>
        new(
            c.Author.Name,
            c.Text,
            c.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
        );
}
