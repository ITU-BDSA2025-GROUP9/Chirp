using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

/// <summary>
/// Provides application-level operations for managing and retrieving Cheep posts.
/// </summary>
/// <remarks>
/// The <see cref="CheepService"/> acts as a service layer between the application's
/// controllers and the data repository. It retrieves, transforms, and persists
/// cheep-related data by delegating persistence logic to an <see cref="ICheepRepository"/>.
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
    public CheepService(ICheepRepository repository)
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
    /// This method uses the repository to fetch raw Cheep entities and projects
    /// them into lightweight data transfer objects (<see cref="CheepDTO"/>) for use in the UI or API layer.
    /// </remarks>
    public async Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        var cheeps = await _repository.GetAllCheeps(pageNumber, pageSize);
        
        return cheeps.Select(CheepToDto).ToList();
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
        
        return cheeps.Select(CheepToDto).ToList();
    }
    
    /// <summary>
    /// Creates and persists a new Cheep post created by the specified <see cref="Author"/>.
    /// </summary>
    /// <param name="author">The author posting the cheep. Must be a valid, non-null <see cref="Author"/>.</param>
    /// <param name="text">The text content of the cheep. Must not be null/whitespace and max 160 characters.</param>
    /// <remarks>
    /// The timestamp is set automatically at creation time inside the repository.
    /// Changes are committed immediately.
    /// </remarks>
    public async Task AddCheep(Author author, string text)
    {
        if (author == null) throw new ArgumentNullException("Author is required " + nameof(author));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));

        await _repository.AddCheep(author, text);
    }
    
    /// <summary>
    /// Maps a <see cref="Cheep"/> into UI friendly<see cref="CheepDTO"/> object.
    /// </summary>
    /// <param name="c">The <see cref="Cheep"/> to transform.</param>
    /// <returns>A populated <see cref="CheepDTO"/> containing author data, text, formatted timestamp, and cheep ID.</returns>
    /// <remarks>
    /// This is a lightweight projection used to avoid exposing EF-tracked entities outside the infrastructure layer.
    /// </remarks>
    public static CheepDTO CheepToDto(Cheep c) => new(
        AuthorToDto(c.Author),
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.CheepId
    );
    
    /// <summary>
    /// Maps an <see cref="Author"/> entity into a UI friendly <see cref="AuthorDTO"/> object.
    /// </summary>
    /// <param name="a">The <see cref="Author"/> entity to transform.</param>
    /// <returns>A populated <see cref="AuthorDTO"/> containing username, email, and profile image reference.</returns>
    /// <remarks>
    /// This transformation prevents leaking persistence concerns into higher layers of the application.
    /// </remarks>
    public static AuthorDTO AuthorToDto(Author a) => new(
        a.UserName!,
        a.Email!,
        a.ProfileImage
    );
    
    /// <summary>
    /// Retrieves cheeps authored by any username in the provided list.
    /// Results are paginated.
    /// </summary>
    /// <param name="authors">A list of author usernames to include in the query. If empty, returns an empty result.</param>
    /// <param name="pageNumber">The current page number (1-based index). Must be greater than 0.</param>
    /// <param name="pageSize">Number of cheeps to return per page.</param>
    /// <returns>A list of <see cref="CheepDTO"/> posts matching the provided authors.</returns>
    public async Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize)
    {
        if (authors.Count == 0) return [];
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        
        var cheeps = await _repository.GetCheepsByAuthors(authors, pageNumber, pageSize);
        return cheeps.Select(CheepToDto).ToList();
    }
    
    /// <summary>
    /// Deletes a Cheep post from the database by its ID.
    /// </summary>
    /// <param name="cheepId">The cheep's database ID that needs to be deleted.</param>
    /// <returns>
    /// true if the cheep existed and was deleted, otherwise false.
    /// </returns>
    /// <remarks>
    /// This method executes the deletion at the repository level and commits immediately.
    /// </remarks>
    public async Task<bool> DeleteCheep(int cheepId)
    {
        return await _repository.DeleteCheep(cheepId);
    }
}
