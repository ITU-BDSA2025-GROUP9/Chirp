using System.Globalization;
using Chirp.Razor.Models;
using Chirp.Razor.Repositories;

namespace Chirp.Razor;

/// <summary>
/// Represents a lightweight view model used to display cheeps (posts)
/// with formatted timestamp and author name.
/// </summary>
/// <param name="Author">The name of the author who posted the cheep.</param>
/// <param name="Message">The content of the cheep message.</param>
/// <param name="Timestamp">A formatted local timestamp for display.</param>
public record CheepViewModel(string Author, string Message, string Timestamp);

/// <summary>
/// Defines the contract for a service that provides access to cheep data.
/// Responsible for retrieving and paginating cheeps for display.
/// </summary>
public interface ICheepService
{
    /// <summary>
    /// Retrieves a paginated list of all cheeps in the system.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A list of <see cref="CheepViewModel"/> instances representing the cheeps.</returns>
    List<CheepViewModel> GetCheeps(int page);

    /// <summary>
    /// Retrieves a paginated list of cheeps posted by a specific author.
    /// </summary>
    /// <param name="author">The username of the author.</param>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A list of <see cref="CheepViewModel"/> instances filtered by author.</returns>
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page);
}

/// <summary>
/// Service layer responsible for fetching and formatting cheep data.
/// Converts database entities (<see cref="Cheep"/>) into view models
/// suitable for display in the user interface.
/// </summary>
public class CheepService : ICheepService
{
    private readonly ICheepRepository _repository;
    private const int PageSize = 32;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheepService"/> class.
    /// </summary>
    /// <param name="repository">The repository used to access cheep data from the database.</param>
    public CheepService(ICheepRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a paginated list of all cheeps in descending chronological order.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A list of formatted <see cref="CheepViewModel"/> objects.</returns>
    public List<CheepViewModel> GetCheeps(int page) =>
        _repository.GetAllCheeps()
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();

    /// <summary>
    /// Retrieves a paginated list of cheeps created by a specific author.
    /// </summary>
    /// <param name="author">The username of the author.</param>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <returns>A list of formatted <see cref="CheepViewModel"/> objects filtered by author.</returns>
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page) =>
        _repository.GetCheepsByAuthor(author)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(Map)
            .ToList();

    /// <summary>
    /// Maps a <see cref="Cheep"/> entity to a <see cref="CheepViewModel"/> for UI display.
    /// Formats timestamps in a human-readable local time format.
    /// </summary>
    /// <param name="c">The cheep entity to map.</param>
    /// <returns>A formatted <see cref="CheepViewModel"/> instance.</returns>
    private static CheepViewModel Map(Cheep c) => new(
        c.Author.Name,
        c.Text,
        c.TimeStamp.ToLocalTime().ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture)
    );
}
