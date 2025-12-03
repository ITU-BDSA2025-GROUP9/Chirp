using System.Globalization;
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

    public async Task<IEnumerable<CommentDTO>> GetCommentsForCheepAsync(int cheepId)
    {
        var comments = await _repo.GetCommentsForCheepAsync(cheepId);

        return comments.Select(CommentToDto);
    }

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
    
     public static CommentDTO CommentToDto(Comment c) => new(
        AuthorToDto(c.Author),
        c.Content,
        c.CreatedAt.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.Id
    );
     
    public static AuthorDTO AuthorToDto(Author a) => new(
        a.UserName!,
        a.Email!,
        a.ProfileImage
    );

}