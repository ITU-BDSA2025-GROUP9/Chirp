namespace Chirp.Core.Interfaces;

/// <summary>
/// Abstraction for accessing and managing cheeps and authors.
/// </summary>
public interface IRepository
{
    Task<IEnumerable<Cheep>> GetAllCheeps(int pageNumber, int pageSize);
    Task<IEnumerable<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task  AddCheep(string authorName, string authorEmail, string text);
    Task<Author?> FindByName(string name);
    Task<Author?> FindByEmail(string email);
    Task<Author> Create(string name, string email);
}