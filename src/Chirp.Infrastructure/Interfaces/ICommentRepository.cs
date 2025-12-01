using Chirp.Core;
using Chirp.Core.DTO;

namespace Chirp.Infrastructure.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<CommentDTO>> GetCommentsForCheepAsync(int cheepId);
    Task AddCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);

}