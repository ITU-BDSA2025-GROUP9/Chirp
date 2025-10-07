using System.Globalization;
using Chirp.Shared;

namespace Chirp.Razor;

/// <summary>
/// Represents a simplified view model for rendering cheeps in Razor pages.
/// </summary>
/// <param name="Author">The username of the cheep’s author.</param>
/// <param name="Message">The text content of the cheep.</param>
/// <param name="Timestamp">A formatted timestamp string.</param>
public record CheepViewModel(string Author, string Message, string Timestamp);

/// <summary>
/// Defines operations for retrieving cheep data in a paginated format.
/// </summary>
public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int page);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page);
}

/// <summary>
/// Implements logic for querying and paginating cheeps from the database.
/// </summary>
public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade;
    private const int PageSize = 32;

    /// <summary>
    /// Creates a new <see cref="CheepService"/> using the provided database facade.
    /// </summary>
    public CheepService(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
    }

    /// <summary>
    /// Retrieves a paginated list of all cheeps in descending order.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A paginated list of <see cref="CheepViewModel"/> records.</returns>
    public List<CheepViewModel> GetCheeps(int page)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));

        return _dbFacade.GetCheeps()
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Retrieves a paginated list of cheeps written by a specific author.
    /// </summary>
    /// <param name="author">The username of the author.</param>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A paginated list of <see cref="CheepViewModel"/> records.</returns>
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be null or empty.", nameof(author));
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));

        return _dbFacade.GetCheeps()
            .Where(c => string.Equals(c.Author, author, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Converts a <see cref="Cheep"/> database model to a <see cref="CheepViewModel"/>.
    /// </summary>
    private static CheepViewModel Map(Cheep c) =>
        new(c.Author, c.Message, UnixToLocal(c.Timestamp));

    /// <summary>
    /// Converts a Unix timestamp (seconds since epoch) to a local time string.
    /// </summary>
    private static string UnixToLocal(long unixSeconds) =>
        DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            .ToLocalTime()
            .ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
}
