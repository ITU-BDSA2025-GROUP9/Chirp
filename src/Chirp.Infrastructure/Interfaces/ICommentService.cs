using Chirp.Core;
using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;

public interface ICommentService
{
    Task<List<CommentDTO>> GetCommentsForCheep(int cheepId);
    Task<List<CommentDTO>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize);
    Task AddComment(Cheep cheep, string text);
    Task<bool> DeleteComment(int commentId);
}