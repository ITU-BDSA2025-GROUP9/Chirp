using Chirp.Core;
using Chirp.Infrastructure.Interfaces;
using Chirp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for <see cref="Cheep"/> entities.
/// Implements a set of repository methods for retrieving, creating, and persisting data.
/// Uses EF core and commits changes immediately in all write operations.
/// </summary>
public class CheepRepository : ICheepRepository
{
    private readonly ChirpDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheepRepository"/> class.
    /// </summary>
    /// <param name="context">The <see cref="ChirpDbContext"/> used for database access.</param>
    public CheepRepository(ChirpDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Retrieves a paginated list of all cheeps from the database.
    /// </summary>
    /// <param name="pageNumber">The page number that we are currently on.</param>
    /// <param name="pageSize">The number of cheeps to return per page (1-based index).</param>
    /// <returns>
    /// A list of <see cref="Cheep"/> objects including their associated <see cref="Author"/> data.
    /// Returns an empty list if no cheeps exist for the specified page.
    /// </returns>
    public async Task<List<Cheep>> GetAllCheeps(int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    /// <summary>
    /// Retrieves a paginated list of cheeps written by a specific author.
    /// </summary>
    /// <param name="authorName">The username of the author whose cheeps should be retrieved.</param>
    /// <param name="pageNumber">The page number that we are currently on (1-based index).</param>
    /// <param name="pageSize">The number of cheeps to return per page.</param>
    /// <returns>
    /// A list of <see cref="Cheep"/> objects including the <see cref="Author"/> data.
    /// Returns an empty list if the author has no cheeps or does not exist.
    /// </returns>
    public async Task<List<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => await _context.Cheeps
            .Include(c => c.Author)
            .Where(c => c.Author.UserName == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    /// <summary>
    /// Creates a new cheep in the database associated with an existing author.
    /// Also create a new <see cref="Cheep"/> object
    /// </summary>
    /// <param name="author">The <see cref="Author"/> posting the cheep. Must already exist in the database context.</param>
    /// <param name="text">The content of the cheep message.</param>
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

    /// <summary>
    /// Retrieves a paginated list of cheeps written by any author in the provided username list.
    /// </summary>
    /// <param name="authors">A list of author usernames to include in the result.</param>
    /// <param name="pageNumber">The page number that we are currently on (1-based index).</param>
    /// <param name="pageSize">The number of cheeps to return per page.</param>
    /// <returns>
    /// A list of <see cref="Cheep"/> objects including their <see cref="Author"/> data.
    /// Returns an empty list if none of the authors have cheeps.
    /// </returns>
    public async Task<List<Cheep>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize)
    {
        return await _context.Cheeps
            .Include(c => c.Author)
            .Where(c => authors.Contains(c.Author.UserName!))
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes a cheep by its  ID from the database.
    /// </summary>
    /// <param name="cheepId">The Cheep ID, stored in the database, of the cheep to remove.</param>
    /// <remarks>
    /// This method commits the deletion immediately in the database.
    /// </remarks>
    /// <returns>
    /// true if the cheep was found and deleted. 
    /// false if no cheep exists with the specified ID.
    /// </returns>
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