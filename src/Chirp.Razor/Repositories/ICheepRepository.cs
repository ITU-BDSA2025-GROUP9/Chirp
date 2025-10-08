using Chirp.Razor.Models;

namespace Chirp.Razor.Repositories;

public interface ICheepRepository
{
    IEnumerable<Cheep> GetAllCheeps();
    IEnumerable<Cheep> GetCheepsByAuthor(string authorName);
    void AddCheep(Cheep cheep);
}
