using Chirp.Core;
using Chirp.Infrastructure.Interfaces;
using Chirp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for both <see cref="Cheep"/> and <see cref="Author"/> entities.
/// Implements a set of repository methods for retrieving, creating, and persisting data.
/// </summary>
/// <remarks>
/// This class encapsulates all persistence logic for the Chirp application,
/// including author management and cheep posting/retrieval.
/// </remarks>
public class CheepRepository : ICheepRepository
{
    private readonly ChirpDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheepRepository"/> class.
    /// </summary>
    /// <param name="context">The <see cref="ChirpDbContext"/> used for database access.</param>
    public CheepRepository(ChirpDbContext context) => _context = context;
    
    public async Task<List<Cheep>> GetAllCheeps(int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .Include(c => c.Comments)
            .ThenInclude(comment => comment.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .Include(c => c.Comments)
            .ThenInclude(comment => comment.Author)
            .Where(c => c.Author.UserName == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
    public async Task AddCheep(Author author, string text)
    {
        var cheep = new Cheep
        {
            Author = author,
            Text = text,
            TimeStamp = DateTime.UtcNow.ToLocalTime()
        };

        author.Cheeps.Add(cheep);
        await _context.Cheeps.AddAsync(cheep);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<Cheep>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize)
    {
        return await _context.Cheeps
            .Include(c => c.Author)
            .Include(c => c.Comments)
            .ThenInclude(comment => comment.Author)
            .Where(c => authors.Contains(c.Author.UserName!))
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<Cheep?> GetCheepById(int cheepId)
    {
        return await _context.Cheeps
            .Include(c => c.Author)                      
            .Include(c => c.Comments)                    
            .ThenInclude(comment => comment.Author) 
            .FirstOrDefaultAsync(c => c.CheepId == cheepId);
    }
    
    public async Task<bool> DeleteCheep(int cheepId)
    {
        var cheep = await _context.Cheeps.FindAsync(cheepId);
        if (cheep == null)
            return false;

        _context.Cheeps.Remove(cheep);
        await _context.SaveChangesAsync();
        return true;
    }
}
