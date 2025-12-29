using Chirp.Core;
using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;
/// <summary>
/// Abstraction for retrieving and managing Comment posts.
/// </summary>
public interface ICommentService
{
    Task<List<CommentDTO>> GetCommentsForCheep(int cheepId);
    Task<List<CommentDTO>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize);
    Task<int> AddComment(int cheepId, int authorId, string text);
    Task<bool> DeleteComment(int commentId);
}