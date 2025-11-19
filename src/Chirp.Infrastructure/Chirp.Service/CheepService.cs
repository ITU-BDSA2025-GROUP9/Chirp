using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using System.Globalization;
using Chirp.Core;

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
    /// Adds a new cheep (post) to the database for the specified author.
    /// </summary>
    /// <param name="authorName">The name of the author creating the cheep.</param>
    /// <param name="authorEmail">The email of the author creating the cheep.</param>
    /// <param name="text">The text content of the cheep.</param>
    /// <remarks>
    /// This method delegates the actual data persistence to the repository layer.
    /// It assumes that the repository will handle author creation or lookup as needed.
    /// </remarks>
    public async Task AddCheep(string authorName, string authorEmail, string text)
    {
        if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("Author is required", nameof(authorName));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));
        
        await _repository.AddCheep(authorName, authorEmail, text);
    }

    public async Task AddCheep(Author author, string text)
    {
        if (author == null) throw new ArgumentNullException("Author is required " + nameof(author));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));

        await _repository.AddCheep(author, text);
    }
    
    public static CheepDTO CheepToDto(Cheep c) => new(
        c.Author.UserName!,
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.Author.Email!
    );
    
    
    public async Task<bool> FollowAuthor(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        if (followerName.Equals(followeeName))
            throw new InvalidOperationException("You cannot follow yourself.");

        return await _repository.FollowAuthor(followerName, followeeName);
    }

    public async Task<bool> UnfollowAuthor(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        if (followerName.Equals(followeeName))
            throw new InvalidOperationException("You cannot unfollow yourself.");

        return await _repository.UnfollowAuthor(followerName, followeeName);
    }

    public async Task<bool> IsFollowing(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        
        return await _repository.IsFollowing(followerName, followeeName);
    }
    
    public async Task<List<CheepDTO>> GetUserTimelineCheeps(string authorName, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("Author is required", nameof(authorName));
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");
        
        var user = await _repository.FindByName(authorName);
        if (user == null) throw new ArgumentException("User not found", nameof(authorName));

        var followees = await _repository.GetAllFollowees(authorName);
        followees.Add(authorName);
         
        var cheeps = await _repository.GetCheepsByAuthors(followees, pageNumber, pageSize);
        return cheeps.Select(CheepToDto).ToList();
    }


}
