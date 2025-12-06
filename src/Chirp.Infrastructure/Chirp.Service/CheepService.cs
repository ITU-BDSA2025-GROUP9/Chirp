using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

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
    private readonly ICheepRepository _repository;
    /// <summary>
    /// Initializes a new instance of the <see cref="CheepService"/> class.
    /// </summary>
    /// <param name="repository">
    /// The repository responsible for interacting with the data store containing authors and cheeps.
    /// </param>
    public CheepService(ICheepRepository repository, ICommentService commentService)
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
    public async Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        var cheeps = await _repository.GetAllCheeps(pageNumber, pageSize);
       
        return CheepDTO.ToDtos(cheeps);
    }

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
    public async Task<List<CheepDTO>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("Author is required", nameof(authorName));
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        
        var cheeps = await _repository.GetCheepsByAuthor(authorName, pageNumber, pageSize);
        return CheepDTO.ToDtos(cheeps);
    }

    public async Task<int> AddCheep(Author author, string text)
    {
        if (author == null) throw new ArgumentNullException("Author is required " + nameof(author));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));

        return await _repository.AddCheep(author, text);
    }
    
    public async Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize)
    {
        if (authors.Count == 0) return [];
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        
        var cheeps = await _repository.GetCheepsByAuthors(authors, pageNumber, pageSize);
        return CheepDTO.ToDtos(cheeps);
    }

    public async Task<Cheep?> GetCheepById(int cheepId)
    {
        return await _repository.GetCheepById(cheepId);
    }
    
    public async Task<bool> DeleteCheep(int cheepId)
    {
        return await _repository.DeleteCheep(cheepId);
    }
}
