using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Chirp.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ChirpDbContext _context;

    public CommentRepository(ChirpDbContext context) => _context = context;

    public async Task<IEnumerable<CommentDTO>> GetCommentsForCheepAsync(int cheepId)
    {
        return await _context.Comments
            .Where(c => c.CheepId == cheepId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorName = c.Author.UserName!,
                AuthorProfileImage = c.Author.ProfileImage,
                AuthorId = c.AuthorId
            })
            .ToListAsync();
    }

    public async Task AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteCommentAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}