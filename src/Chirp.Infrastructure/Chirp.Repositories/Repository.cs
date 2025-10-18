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

    public IEnumerable<Cheep> GetAllCheeps(int pageNumber, int pageSize)
        => _context.Cheeps
            .Include(c => c.Author)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

    public IEnumerable<Cheep> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize)
        => _context.Cheeps
            .Include(c => c.Author)
            .Where(c => c.Author.Name == authorName)
            .OrderByDescending(c => c.TimeStamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

    public void AddCheep(string authorName, string authorEmail, string text)
    {
        if (text.Length > 160)
            throw new ArgumentException("Cheep text cannot exceed 160 characters.", nameof(text));

        var author = FindByName(authorName) ?? Create(authorName, authorEmail);

        var cheep = new Cheep
        {
            Text = text,
            Author = author,
            TimeStamp = DateTime.UtcNow
        };

        _context.Cheeps.Add(cheep);
        _context.SaveChanges();
    }

    public Author? FindByName(string name)
        => _context.Authors.FirstOrDefault(a => a.Name == name);

    public Author? FindByEmail(string email)
        => _context.Authors.FirstOrDefault(a => a.Email == email);

    public Author Create(string name, string email)
    {
        var author = new Author { Name = name, Email = email };
        _context.Authors.Add(author);
        _context.SaveChanges();
        return author;
    }
}
