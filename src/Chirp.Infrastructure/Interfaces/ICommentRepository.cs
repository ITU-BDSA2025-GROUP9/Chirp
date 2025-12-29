using Chirp.Core;
using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;
/// <summary>
/// Abstraction for accessing and managing comments.
/// </summary>
public interface ICommentRepository
{
    Task<List<Comment>> GetCommentsForCheep(int cheepId);
    Task<List<Comment>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize);
    Task<int> AddComment(int cheepId, int authorId, string text);
    Task<bool> DeleteComment(int commentId);
}