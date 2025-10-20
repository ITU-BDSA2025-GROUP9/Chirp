namespace Chirp.Core.Interfaces;

/// <summary>
/// Abstraction for accessing and managing cheeps and authors.
/// </summary>
public interface IRepository
{
    IEnumerable<Cheep> GetAllCheeps(int pageNumber, int pageSize);
    IEnumerable<Cheep> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    void AddCheep(string authorName, string authorEmail, string text);
    Author? FindByName(string name);
    Author? FindByEmail(string email);
    Author Create(string name, string email);
}