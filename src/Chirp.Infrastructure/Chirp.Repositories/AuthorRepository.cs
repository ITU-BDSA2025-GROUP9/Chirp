using Chirp.Core;
using Chirp.Infrastructure.Database;
using Chirp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Infrastructure.Chirp.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDbContext _context;
    public AuthorRepository(ChirpDbContext context) => _context = context;
    
    public async Task<Author> CreateAuthor(string name, string email)
    {
        var author = new Author { UserName = name, Email = email };
        await _context.Authors.AddAsync(author);
        await  _context.SaveChangesAsync();
        return author;
    }
    
    public async Task<Author?> FindByName(string name)
        => await _context.Authors.FirstOrDefaultAsync(a => a.UserName == name);

    public async Task<Author?> FindByEmail(string email)
        => await _context.Authors.FirstOrDefaultAsync(a => a.Email == email);

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

    public async Task<bool> IsFollowing(string followerName, string followeeName)
    {
        return await _context.Authors
            .AnyAsync(a => a.UserName == followerName && 
                           a.Following.Any(f => f.UserName == followeeName));
    }
    
    public async Task<bool> DeleteAuthor(string authorName)
    {
        var author = await _context.Authors
            .Include(a => a.Cheeps)
            .Include(a => a.Followers)
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (author == null) return false;
        
        _context.Cheeps.RemoveRange(author.Cheeps);
        
        foreach (var followers in author.Followers.ToList())
            followers.Following.Remove(author);
        
        foreach (var followee in author.Following.ToList())
            followee.Followers.Remove(author);
        
        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> AuthorByNameExists(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            return false;

        return await _context.Authors
            .AnyAsync(a => a.UserName == authorName);
    }
    
    public async Task<bool> SetProfileImage(string authorName, string profileImage)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (author == null)
            return false;

        author.ProfileImage = profileImage;
        await _context.SaveChangesAsync();
        return true;
    }
}