using Chirp.Core;
using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetCommentsForCheep(int cheepId);
    Task<List<Comment>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize);
    Task AddComment(Cheep cheep, string text);
    Task<bool> DeleteComment(int commentId);
}