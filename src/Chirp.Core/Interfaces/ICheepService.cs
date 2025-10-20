using Chirp.Core.DTO;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    Task<IEnumerable<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
    Task<IEnumerable<CheepDTO>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task AddCheep(string authorName, string authorEmail, string text);
}