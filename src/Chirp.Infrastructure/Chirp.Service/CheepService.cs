using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using System.Globalization;

namespace Chirp.Infrastructure.Service;

/// <summary>
/// Provides application-level operations for managing and retrieving <c>Cheep</c> posts.
/// </summary>
/// <remarks>
/// The <see cref="CheepService"/> acts as a service layer between the application's
/// controllers and the data repository. It retrieves, transforms, and persists
/// cheep-related data by delegating persistence logic to an <see cref="IRepository"/>.
/// </remarks>
public class CheepService : ICheepService
{
    private readonly IRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheepService"/> class.
    /// </summary>
    /// <param name="repository">
    /// The repository responsible for interacting with the data store containing authors and cheeps.
    /// </param>
    public CheepService(IRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a paginated list of all cheeps (posts) from the database.
    /// </summary>
    /// <param name="pageNumber">The current page number (1-based index).</param>
    /// <param name="pageSize">The number of cheeps to retrieve per page.</param>
    /// <returns>
    /// A collection of <see cref="CheepDTO"/> objects containing the author name,
    /// email, text content, and timestamp of each cheep.
    /// </returns>
    /// <remarks>
    /// This method uses the repository to fetch raw <c>Cheep</c> entities and projects
    /// them into lightweight data transfer objects (<see cref="CheepDTO"/>) for use in the UI or API layer.
    /// </remarks>
    public IEnumerable<CheepDTO> GetCheeps(int pageNumber, int pageSize)
        => _repository.GetAllCheeps(pageNumber, pageSize)
            .Select(c => new CheepDTO
            {
                AuthorName = c.Author.Name,
                AuthorEmail = c.Author.Email,
                Text = c.Text,
                TimeStamp = c.TimeStamp
            });

    /// <summary>
    /// Retrieves a paginated list of cheeps authored by a specific user.
    /// </summary>
    /// <param name="authorName">The name of the author whose cheeps are to be retrieved.</param>
    /// <param name="pageNumber">The current page number (1-based index).</param>
    /// <param name="pageSize">The number of cheeps to retrieve per page.</param>
    /// <returns>
    /// A collection of <see cref="CheepDTO"/> objects authored by the specified user.
    /// </returns>
    /// <remarks>
    /// This method queries the repository for cheeps belonging to a given author and maps
    /// the resulting entities into <see cref="CheepDTO"/> instances for presentation.
    /// </remarks>
    public IEnumerable<CheepDTO> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => _repository.GetCheepsByAuthor(authorName, pageNumber, pageSize)
            .Select(c => new CheepDTO
            {
                AuthorName = c.Author.Name,
                AuthorEmail = c.Author.Email,
                Text = c.Text,
                TimeStamp = c.TimeStamp
            });

    /// <summary>
    /// Adds a new cheep (post) to the database for the specified author.
    /// </summary>
    /// <param name="authorName">The name of the author creating the cheep.</param>
    /// <param name="authorEmail">The email of the author creating the cheep.</param>
    /// <param name="text">The text content of the cheep.</param>
    /// <remarks>
    /// This method delegates the actual data persistence to the repository layer.
    /// It assumes that the repository will handle author creation or lookup as needed.
    /// </remarks>
    public void AddCheep(string authorName, string authorEmail, string text)
        => _repository.AddCheep(authorName, authorEmail, text);
}
