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
    private readonly ICheepRepository _repository;
    private const int PageSize = 32;

    public CheepService(ICheepRepository repository)
    {
        _repository = repository;
    }

    public List<CheepViewModel> GetCheeps(int page) =>
        _repository.GetAllCheeps()
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page) =>
        _repository.GetCheepsByAuthor(author)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();

    private static CheepViewModel Map(Cheep c) => new(
        c.Author.Name,
        c.Text,
        c.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
    );
}