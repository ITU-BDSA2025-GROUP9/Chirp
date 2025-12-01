using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;

    public CommentService(ICommentRepository repo) => _repo = repo;
    
    public Task DeleteCommentAsync(int id)
        => _repo.DeleteCommentAsync(id);

    public Task<IEnumerable<CommentDTO>> GetCommentsForCheepAsync(int cheepId)
        => _repo.GetCommentsForCheepAsync(cheepId);

    public async Task AddCommentAsync(int cheepId, int authorId, string content)
    {
        var comment = new Comment
        {
            CheepId = cheepId,
            AuthorId = authorId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddCommentAsync(comment);
    }

}