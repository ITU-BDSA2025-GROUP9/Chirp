using Chirp.Core;
namespace Chirp.Infrastructure.Interfaces;

/// <summary>
/// Abstraction for accessing and managing cheeps and authors.
/// </summary>
public interface ICheepRepository
{
    Task<List<Cheep>> GetAllCheeps(int pageNumber, int pageSize);
    Task<List<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task<List<Cheep>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize);
    Task AddCheep(Author author, string text);
    Task<bool> DeleteCheep(int cheepId);
}