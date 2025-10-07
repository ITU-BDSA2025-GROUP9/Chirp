using System.Globalization;
using Chirp.Razor.Models;
using Chirp.Razor.Repositories;
using Chirp.Razor.DTO;

namespace Chirp.Razor;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page);
}

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;
    private const int PageSize = 32;

    public CheepService(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public List<CheepDTO> GetCheeps(int page)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        
        return _cheepRepository.GetAllCheeps()
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page)
    {
        if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Author is required", nameof(author));
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        
        return _cheepRepository.GetCheepsByAuthor(author)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    private static CheepDTO Map(Cheep c) =>
        new(
            Author: c.Author.Name,
            Message: c.Text,
            Timestamp: c.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
        );
}
