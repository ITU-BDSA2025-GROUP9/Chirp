using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

/// <summary>
/// Provides application-level operations for managing and retrieving comment posts.
/// </summary>
/// <remarks>
/// The <see cref="CommentService"/> acts as a service layer between the application's
/// controllers and the data repository. It retrieves, transforms, and persists
/// cheep-related data by delegating persistence logic to an <see cref="ICommentRepository"/>.
/// </remarks>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;
    
    /// <summary>
    /// Initializes a new intance of the <see cref="CommentService"/> class
    /// </summary>
    /// <param name="repo">The comment repository used for data access opreations</param>
    public CommentService(ICommentRepository repo) => _repo = repo;
    
    /// <summary>
    /// Retrieves all comments associated with a specific cheep
    /// </summary>
    /// <param name="cheepId">The id of the cheep we want the comments from</param>
    /// <returns>
    ///A list of <see cref="CommentDTO"/> objects ordered by creation time
    /// Returns an empty list if the cheep has no comments
    /// </returns>
    public async Task<List<CommentDTO>> GetCommentsForCheep(int cheepId)
    {
        var comments = await _repo.GetCommentsForCheep(cheepId);
        return CommentDTO.ToDtos(comments);
    }
    
    /// <summary>
    /// Retrieves a paginated list of comments written by a specific author
    /// and maps them to the <see cref="CommentDTO"/>
    /// </summary>
    /// <param name="authorName">The author of the comments we are trying to retrieve</param>
    /// <param name="pageNumber"> The page number to retrieve (1-based index)</param>
    /// <param name="pageSize">The number of comments per page</param>
    /// <returns>
    /// A list of <see cref="CommentDTO"/> objects roder by creation time in decending order
    /// </returns>
    /// <exception cref="ArgumentException">thrown when <paramref name="authorName"/> is null or empty </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// thrown when <paramref name="pageNumber"/> is less or equal to zero
    /// </exception>
    public async Task<List<CommentDTO>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(authorName)) throw new ArgumentException("Author is required", nameof(authorName));
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException($"Pagenumber must be greater than 0. Invalid pagenumber: {pageNumber}");

        var comments = await _repo.GetCommentsByAuthor(authorName, pageNumber, pageSize);
        return CommentDTO.ToDtos(comments);
    }
    
    /// <summary>
    /// Creates a new comment associated with a specific cheep and author
    /// </summary>
    /// <param name="cheepId">The id of the cheep we are creating a comment to</param>
    /// <param name="authorId">The id of the author that are creating a comment</param>
    /// <param name="text">the text the comment contains. Length allowed to max be 160</param>
    /// <returns>The id of the newly created comment</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the <paramref name="text"/> is null, empty or
    /// exceeds the maximum allowed length
    /// </exception>
    public async Task<int> AddComment(int cheepId, int authorId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Cheep text is required and cannot be null or empty", nameof(text));
        if (text.Length > 160) throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));
        
        return await _repo.AddComment(cheepId, authorId, text);
    }
    
    /// <summary>
    /// Deletes a comment by using its id
    /// </summary>
    /// <param name="commentId">The id of the comment we are trying to delete</param>
    /// <returns>
    /// True if the comment was found and deleted
    /// False if the comment does not exist with this specific id
    /// </returns>
    public Task<bool> DeleteComment(int commentId)
        => _repo.DeleteComment(commentId);
  
}