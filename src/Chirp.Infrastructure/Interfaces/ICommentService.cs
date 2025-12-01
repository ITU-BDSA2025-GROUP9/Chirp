using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDTO>> GetCommentsForCheepAsync(int cheepId);
    Task AddCommentAsync(int cheepId, int authorId, string content);
    Task DeleteCommentAsync(int id);

}