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

    public async Task<List<Comment>> GetCommentsForCheep(int cheepId) 
        => await _context.Comments
            .Where(c => c.CheepId == cheepId)
            .OrderBy(c => c.TimeStamp)
            .Include(c => c.Author)
            .ToListAsync();
    
    public async Task<List<Comment>> GetCommentsByAuthor(string authorName, int pageNumber, int pageSize)
        => await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.Author.UserName == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
    public async Task<int> AddComment(Cheep cheep, Author author, string text)
    {
        var comment = new Comment
        {
            Cheep = cheep,
            Author = author,
            Text = text,
            TimeStamp = DateTime.UtcNow.ToLocalTime()
        };
        
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment.CommentId;
    }
    
    public async Task<bool> DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;
        
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true; 
    }
}