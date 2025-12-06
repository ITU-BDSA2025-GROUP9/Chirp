using Chirp.Core;
using Chirp.Core.DTO;
namespace Chirp.Infrastructure.Interfaces;
public interface ICheepService
{
    Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
    Task<List<CheepDTO>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize);
    Task<Cheep?> GetCheepById(int cheepId);
    Task AddCheep(Author author, string text);
    Task<bool> DeleteCheep(int cheepId);
}