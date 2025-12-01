using Chirp.Core;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Infrastructure.Chirp.Repositories;
/// <summary>
/// Repository responsible for handling all database operations related to <see cref="Author"/> entities.
/// Manages author creation, deletion, profile updates, and follow/unfollow relationships.
/// Uses EF core and commits changes immediately in all write operations.
/// </summary>
public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDbContext _context;
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorRepository"/> class.
    /// </summary>
    /// <param name="context">used for database access.</param>
    public AuthorRepository(ChirpDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Creates a new author object and adds the author to the database.
    /// </summary>
    /// <param name="name">The display name of the author.</param>
    /// <param name="email">The email address of the author.</param>
    /// <returns>The created and database-tracked <see cref="Author"/> instance.</returns>
    public async Task<Author> CreateAuthor(string name, string email)
    {
        var author = new Author { UserName = name, Email = email };
        await _context.Authors.AddAsync(author);
        await  _context.SaveChangesAsync();
        return author;
    }
    
    /// <summary>
    /// Retrieves an author by their username.
    /// </summary>
    /// <param name="name">The username that needs to be searched for.</param>
    /// <returns>The matching <see cref="Author"/> if found, else null</returns>
    public async Task<Author?> FindByName(string name)
        => await _context.Authors.FirstOrDefaultAsync(a => a.UserName == name);

    /// <summary>
    /// Retrieves an authors by their email.
    /// </summary>
    /// <param name="email">The email that needs to be searched for.</param>
    /// <returns>The matching <see cref="Author"/> if found, else null</returns>
    public async Task<Author?> FindByEmail(string email)
        => await _context.Authors.FirstOrDefaultAsync(a => a.Email == email);

    /// <summary>
    /// Gets a list of usernames that the specified author is following 
    /// </summary>
    /// <param name="authorName">The username of the author whose followees should be retrieved.</param>
    /// <returns>A list of usernames the author follows. Empty list if the author does not exist or follows nobody.</returns>
    public async Task<List<string>> GetAllFollowees(string authorName)
    {
        var user = await _context.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (user == null) return new List<string>();

        return user.Following
            .OrderBy(a => a.UserName)
            .Select(a => a.UserName!)
            .ToList();
    }
    
    /// <summary>
    /// Creates a follow realtionship between two authors.
    /// </summary>
    /// <param name="followerName">Username of author who wants to follow an author.</param>
    /// <param name="followeeName">Username of author to be followed.</param>
    /// <returns>true if the follow succeeded, otherwise false.</returns>
    public async Task<bool> FollowAuthor(string followerName, string followeeName)
    {
        if (followerName == followeeName) return false;
        var follower = await _context.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);
        
        var followee = await _context.Authors
            .Include(a => a.Followers)
            .FirstOrDefaultAsync(a => a.UserName == followeeName);

        if (follower == null || followee == null)
            return false;

        if (follower.Following.Contains(followee))
            return false;
        
        follower.Following.Add(followee);
        followee.Followers.Add(follower);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Removes a follow realtionship between two authors.
    /// </summary>
    /// <param name="followerName">Username of author who wants to follow an author.</param>
    /// <param name="followeeName">Username of author to be followed.</param>
    /// <returns>true if the unfollow succeeded, otherwise false.</returns>
    public async Task<bool> UnfollowAuthor(string followerName, string followeeName)
    {
        if (followerName == followeeName) return false;
        var follower = await _context.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);
        var followee = await _context.Authors
            .Include(a => a.Followers)
            .FirstOrDefaultAsync(a => a.UserName == followeeName);

        if (follower == null || followee == null)
            return false;

        if (!follower.Following.Contains(followee))
            return false;

        follower.Following.Remove(followee);
        followee.Followers.Remove(follower);
        await _context.SaveChangesAsync();
        return true;
    }

      /// <summary>
    /// Checks whether a follow relationship exists between two authors.
    /// Executes the check at the database level.
    /// </summary>
    /// <param name="followerName">Username of the potential follower.</param>
    /// <param name="followeeName">Username of the potential followee.</param>
    /// <returns>true if <paramref name="followerName"/> follows <paramref name="followeeName"/>, otherwise false.</returns>
    public async Task<bool> IsFollowing(string followerName, string followeeName)
    {
        return await _context.Authors.AnyAsync(a =>
            a.UserName == followerName &&
            a.Following.Any(f => f.UserName == followeeName));
    }

    /// <summary>
    /// Deletes an author and removes all associated data and relationships.
    /// Ensures follow references and authored cheeps are cleaned up before removal.
    /// </summary>
    /// <param name="authorName">Username of the author to delete.</param>
    /// <returns>true if the author was deleted, otherwise false if the author did not exist.</returns>
    public async Task<bool> DeleteAuthor(string authorName)
    {
        var author = await _context.Authors
            .Include(a => a.Cheeps)
            .Include(a => a.Followers)
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (author == null) return false;

        _context.Cheeps.RemoveRange(author.Cheeps);

        foreach (var follower in author.Followers.ToList())
            follower.Following.Remove(author);

        foreach (var followee in author.Following.ToList())
            followee.Followers.Remove(author);

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if an author exists in the database by username.
    /// Returns false immediately if the input is null, empty, or whitespace.
    /// </summary>
    /// <param name="authorName">The username to check if excites.</param>
    /// <returns>true if the author exists, otherwise false.</returns>
    public async Task<bool> AuthorByNameExists(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName)) return false;
        return await _context.Authors.AnyAsync(a => a.UserName == authorName);
    }

    /// <summary>
    /// Sets the profile image path of an existing author.
    /// return immediately if author does not exist.
    /// </summary>
    /// <param name="authorName">Username of the author to set profile picture.</param>
    /// <param name="profileImage">The new profile image path to assign.</param>
    public async Task SetProfileImage(string authorName, string profileImage)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserName == authorName);
        if (author == null) return;

        author.ProfileImage = profileImage;
        await _context.SaveChangesAsync();
    }
}