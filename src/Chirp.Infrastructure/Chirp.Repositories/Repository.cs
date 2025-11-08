using Chirp.Core.Interfaces;
using Chirp.Core;
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
public class Repository : IRepository
{
    private readonly ChirpDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository"/> class.
    /// </summary>
    /// <param name="context">The <see cref="ChirpDbContext"/> used for database access.</param>
    public Repository(ChirpDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cheep>> GetAllCheeps(int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .Where(c => c.Author.UserName == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task AddCheep(string authorName, string authorEmail, string text)
    {
        var author = await FindByName(authorName) ?? await Create(authorName, authorEmail);

        var cheep = new Cheep
        {
            Text = text,
            Author = author,
            TimeStamp = DateTime.UtcNow.ToLocalTime()
        };

        await _context.Cheeps.AddAsync(cheep);
        await _context.SaveChangesAsync();
    }
    
    public async Task AddCheep(Author author, string text)
    {
        var cheep = new Cheep
        {
            Author = author,
            Text = text,
            TimeStamp = DateTime.UtcNow.ToLocalTime()
        };

        await _context.Cheeps.AddAsync(cheep);
        await _context.SaveChangesAsync();
    }

    public async Task<Author?> FindByName(string name)
        => await _context.Authors.FirstOrDefaultAsync(a => a.UserName == name);

    public async Task<Author?> FindByEmail(string email)
        => await _context.Authors.FirstOrDefaultAsync(a => a.Email == email);

    public async Task<Author> Create(string name, string email)
    {
        var author = new Author { UserName = name, Email = email };
        await _context.Authors.AddAsync(author);
        await  _context.SaveChangesAsync();
        return author;
    }
}
