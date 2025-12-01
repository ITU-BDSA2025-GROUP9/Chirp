namespace Chirp.Infrastructure.Interfaces;
/// <summary>
/// Abstraction for author follow state and basic profile management.
/// </summary>
public interface IAuthorService
{
    Task<bool> FollowAuthor(string followerName, string followeeName);
    Task<bool> UnfollowAuthor(string followerName, string followeeName);
    Task<bool> IsFollowing(string followerName, string followeeName);
    Task<bool> DeleteAuthor(string authorName);
    Task<bool> AuthorByNameExists(string authorName);
    Task SetProfileImage(string authorName, string profileImage); 
    Task<List<string>> GetAllFollowees(string authorName);
    Task<List<string>> GetAllFolloweesAndSelf(string authorName);
}