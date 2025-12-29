using Chirp.Core;
namespace Chirp.Infrastructure.Interfaces;
/// <summary>
/// Abstraction for accessing and managing authors.
/// </summary>
public interface IAuthorRepository
{
    Task<Author> CreateAuthor(string name, string email);
    Task<Author?> FindByName(string name);
    Task<Author?> FindByEmail(string email);
    Task<List<string>> GetAllFollowees(string authorName);
    Task<bool> FollowAuthor(string followerName, string followeeName);
    Task<bool> UnfollowAuthor(string followerName, string followeeName);
    Task<bool> IsFollowing(string followerName, string followeeName);
    Task<bool> DeleteAuthor(string authorName);
    Task<bool> AuthorByNameExists(string authorName);
    Task<bool> SetProfileImage(string authorName, string profileImage);
}