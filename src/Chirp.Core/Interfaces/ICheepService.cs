using Chirp.Core.DTO;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
    Task<List<CheepDTO>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task AddCheep(string authorName, string authorEmail, string text);
    Task  AddCheep(Author author, string text);
}