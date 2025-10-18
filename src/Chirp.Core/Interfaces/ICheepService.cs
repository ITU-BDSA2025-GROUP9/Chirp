using Chirp.Core.DTO;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    IEnumerable<CheepDTO> GetCheeps(int pageNumber, int pageSize);
    IEnumerable<CheepDTO> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    void AddCheep(string authorName, string authorEmail, string text);
}