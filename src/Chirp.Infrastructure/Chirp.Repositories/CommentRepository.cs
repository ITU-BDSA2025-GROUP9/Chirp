using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Chirp.Repositories;

/// <summary>
/// Provides data access methods for <see cref="Comment"/> entities.
/// Implements a set of repository methods for retrieving, creating, and persisting data.
/// Uses EF core and commits changes immediately in all write operations.
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly ChirpDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CommentRepository"/> class.
    /// </summary>
    /// <param name="context">used for database access.</param>
    public CommentRepository(ChirpDbContext context) => _context = context;

    /// <summary>
    /// Retrieves all comments associated with a specific cheep
    /// </summary>
    /// <param name="cheepId">The id of the cheep which we want the comments of</param>
    /// <returns>
    /// A list of <see cref="Comment"/> ordered by creation time in ascending order,
    /// including also their <see cref="Author"/> data.
    /// Returns an empty list if the cheep has no comments.
    /// </returns>
    public async Task<List<Comment>> GetCommentsForCheep(int cheepId) 
        => await _context.Comments
            .Where(c => c.CheepId == cheepId)
            .OrderBy(c => c.TimeStamp)
            .Include(c => c.Author)
            .ToListAsync();
    
    /// <summary>
    /// Get a paginated list of comments written by a specific author.
    /// </summary>
    /// <param name="authorName">The name of the author whose comments we are trying to get</param>
    /// <param name="pageNumber">The page number to retrieve (1-based index)</param>
    /// <param name="pageSize">The number of comments allowed on 1 page</param>
    /// <returns>
    /// A list of <see cref="Comment"/> ordered by creation time in descending order,
    /// including also their <see cref="Author"/> data.
    /// Returns an empty list if the author has not made any comments or do not exist
    /// </returns>
    public async Task<List<Comment>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize)
        => await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.Author.UserName == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
    /// <summary>
    /// Creates a new comment that are associated with a specific cheep and author
    /// </summary>
    /// <param name="cheepId"> the id of the cheep we are making comment to</param>
    /// <param name="authorId">the id of the author making the comment</param>
    /// <param name="text">the text the comment contains, limited to a length of 160</param>
    /// <returns>
    ///Returns the id of the comment that just have been made
    /// </returns>
    public async Task<int> AddComment(int cheepId, int authorId, string text)
    {
        var comment = new Comment
        {
            CheepId = cheepId,
            AuthorId = authorId,
            Text = text,
            TimeStamp = DateTime.UtcNow.ToLocalTime()
        };
        
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment.CommentId;
    }
    
    /// <summary>
    /// Deletes a comment from the database by its unique id
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;
        
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true; 
    }
}