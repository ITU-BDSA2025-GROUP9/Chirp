using Chirp.Core.DTO;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    Task<List<CheepDTO>> GetCheeps(int pageNumber, int pageSize);
    Task<List<CheepDTO>> GetCheepsByAuthor(string authorName, int pageNumber, int pageSize);
    Task AddCheep(string authorName, string authorEmail, string text);
    Task  AddCheep(Author author, string text);
    Task<bool> FollowAuthor(string followerName, string followeeName);
    Task<bool> UnfollowAuthor(string followerName, string followeeName);
    Task<bool> IsFollowing(string followerName, string followeeName);
    Task<List<CheepDTO>> GetUserTimelineCheeps(string authorName, int pageNumber, int pageSize);
    Task<bool> DeleteCheep(int cheepId);
    Task<bool> DeleteAuthor(string authorName);
    Task<bool> AuthorByNameExists(string authorName);
    Task SetProfileImage(string authorName, string profileImage); 
}