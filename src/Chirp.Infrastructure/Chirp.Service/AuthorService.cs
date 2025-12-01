using Chirp.Infrastructure.Interfaces;

namespace Chirp.Infrastructure.Chirp.Service;
/// <summary>
/// Provides application-level operations for managing and retrieving Authors.
/// </summary>
/// <remarks>
/// The <see cref="AuthorService"/> acts as a service layer between the application's
/// controllers and the data repository. It retrieves, transforms, and persists
/// cheep-related data by delegating persistence logic to an <see cref="IAuthorRepository"/>.
/// </remarks>
public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorService"/> class.
    /// </summary>
    /// <param name="repository">The repository handling author follow relationships.</param>
    public AuthorService(IAuthorRepository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// Creates a follow relationship between two authors.
    /// Authors username is not allowed to be null or white space. An author is not allowed to try and follow itself.
    /// </summary>
    /// <param name="followerName">Username of the author who wants to follow.</param>
    /// <param name="followeeName">Username of the author to follow.</param>
    /// <returns>true if the follow relationship was created successfully, otherwise false.</returns>
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
    
    /// <summary>
    /// Removes a follow relationship between two authors.
    /// Authors username is not allowed to be null or white space. An author is not allowed to try and unfollow itself.
    /// </summary>
    /// <param name="followerName">Username of the author who wants to unfollow.</param>
    /// <param name="followeeName">Username of the author to unfollow.</param>
    /// <returns>true if the follow relationship was removed, otherwise false.</returns>
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
    
    /// <summary>
    /// Determine whether two authors are following each-other.
    /// Authors username are not allowed to be null or whitespace
    /// </summary>
    /// <param name="followerName">Username of the potential follower.</param>
    /// <param name="followeeName">Username of the potential followee.</param>
    /// <returns>true if follower follows followee, otherwise false.</returns>
    public async Task<bool> IsFollowing(string followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName))
            throw new ArgumentException("Follower name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(followeeName))
            throw new ArgumentException("Followee name cannot be null or empty");
        
        return await _repository.IsFollowing(followerName, followeeName);
    }
    
    /// <summary>
    /// Deletes an author and removes all associated data and follow relationships.
    /// Author name is not allowed to be null or whitespace
    /// </summary>
    /// <param name="authorName">Username of the author to delete.</param>
    /// <returns>true if the author existed and was deleted, otherwise false.</returns>
    public async Task<bool> DeleteAuthor(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));
        
        return await _repository.DeleteAuthor(authorName);
    }
    
    /// <summary>
    /// Checks if an author exists in the database by username.
    /// </summary>
    /// <param name="authorName">The username to check for existence.</param>
    /// <returns>
    /// True if the author exists, otherwise false.
    /// Returns false immediately if input is null or whitespace.
    /// </returns>
    public async Task<bool> AuthorByNameExists(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName)) return false; 
        return await _repository.AuthorByNameExists(authorName);
    }
    
    /// <summary>
    /// Sets or updates the profile image reference for an existing author.
    /// Author name and profile image is not allowed to be null or whitespace
    /// </summary>
    /// <param name="authorName">Username of the author to update.</param>
    /// <param name="profileImage">The new profile image path.</param>
    public async Task SetProfileImage(string authorName, string profileImage)
    {
        if (string.IsNullOrWhiteSpace(authorName)) 
            throw new ArgumentException("Author is required", nameof(authorName));
        if (string.IsNullOrWhiteSpace(profileImage)) 
            throw new ArgumentException("Profile image is required", nameof(profileImage));
        
        await _repository.SetProfileImage(authorName, profileImage);
    }
    
    /// <summary>
    /// Gets all authors that the specified author is following.
    /// Author name are not allowed to be null or whitespace.
    /// </summary>
    /// <param name="authorName">The author whose followees should be retrieved.</param>
    /// <returns>
    /// A list of followee usernames. Empty list if the author exists but follows nobody.
    /// </returns>
    public async Task<List<string>> GetAllFollowees(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));
        
        return await _repository.GetAllFollowees(authorName);
    }
    
    /// <summary>
    /// Gets all authors that the specified author follows, 
    /// and add the author’s own username to the result in the end.
    /// Author name are not allowed to be null or whitespace.
    /// </summary>
    /// <param name="authorName">The author whose followees (and self) should be retrieved.</param>
    /// <returns>
    /// A list of usernames the author follows including their own username.
    /// </returns>
    public async Task<List<string>> GetAllFolloweesAndSelf(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author is required", nameof(authorName));

        var followees = await _repository.GetAllFollowees(authorName);
        followees.Add(authorName);
        return followees;
    }
}