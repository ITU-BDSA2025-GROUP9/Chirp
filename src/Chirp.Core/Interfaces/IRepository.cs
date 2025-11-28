namespace Chirp.Core.Interfaces;

/// <summary>
/// Abstraction for accessing and managing cheeps and authors.
/// </summary>
public interface IRepository
{
    Task<List<Cheep>> GetAllCheeps(int pageNumber, int pageSize);
    Task<List<Cheep>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task  AddCheep(string authorName, string authorEmail, string text);
    Task  AddCheep(Author author, string text);
    Task<Author?> FindByName(string name);
    Task<Author?> FindByEmail(string email);
    Task<Author> Create(string name, string email);
    Task<bool> FollowAuthor(string followerName, string followeeName);
    Task<bool> UnfollowAuthor(string followerName, string followeeName);
    Task<bool> IsFollowing(string followerName, string followeeName);
    Task<List<Cheep>> GetCheepsByAuthors(List<string> authors, int pageNumber, int pageSize);
    Task<List<string>> GetAllFollowees(string authorName);
    Task<bool> DeleteCheep(int cheepId);
    Task<bool> DeleteAuthor(string authorName);
    Task<bool> AuthorByNameExists(string authorName);
    Task SetProfileImage(string authorName, string profileImage);
}