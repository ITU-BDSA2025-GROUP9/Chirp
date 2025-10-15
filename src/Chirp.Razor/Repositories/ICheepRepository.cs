using Chirp.Razor.Models;
using Chirp.Razor.DTO;

namespace Chirp.Razor.Repositories;
public interface ICheepRepository
{
    public Task<int> CreateCheep(CheepDTO newCheep);
    public Task<List<CheepDTO>> ReadAllCheeps();
    public Task<List<CheepDTO>> ReadCheepsByAuthor(string authorName);
}
