using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repository;

    public AuthorService(IAuthorRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<bool> FollowAuthor(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        if (followerName.Equals(followeeName))
            throw new InvalidOperationException("You cannot follow yourself.");

        return await _repository.FollowAuthor(followerName, followeeName);
    }

    public async Task<bool> UnfollowAuthor(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        if (followerName.Equals(followeeName))
            throw new InvalidOperationException("You cannot unfollow yourself.");

        return await _repository.UnfollowAuthor(followerName, followeeName);
    }

    public async Task<bool> IsFollowing(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        
        return await _repository.IsFollowing(followerName, followeeName);
    }
    
    public async Task<bool> DeleteAuthor(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));
        
        return await _repository.DeleteAuthor(authorName);
    }

    public async Task<bool> AuthorByNameExists(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName)) return false; 
        return await _repository.AuthorByNameExists(authorName);
    }
    
    public async Task<bool> SetProfileImage(string authorName, string profileImage)
    {
        if (string.IsNullOrWhiteSpace(authorName)) 
            throw new ArgumentException("Author is required", nameof(authorName));
        if (string.IsNullOrWhiteSpace(profileImage)) 
            throw new ArgumentException("Profile image is required", nameof(profileImage));
        
        return await _repository.SetProfileImage(authorName, profileImage);
    }
    
    public async Task<List<string>> GetAllFollowees(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));
        
        return await _repository.GetAllFollowees(authorName);
    }
    
    public async Task<List<string>> GetAllFolloweesAndSelf(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));

        var followees = await _repository.GetAllFollowees(authorName);
        followees.Add(authorName);
        return followees;
    }
}