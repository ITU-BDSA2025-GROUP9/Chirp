using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace Chirp.Core;

/// <summary>
/// Represents an author (user) who can post cheeps.
/// This version keeps compatibility with legacy code using AuthorId,
/// while maintaining clean EF Core mapping through Id.
/// </summary>
public class Author : IdentityUser<int>
{
    /// <summary>
    /// The list of cheeps posted by this author.
    /// </summary>
    public List<Cheep> Cheeps { get; set; } = [];

    /// <summary>
    /// The list of Author followed by this author. 
    /// </summary>
    public List<Author> Following { get; set; } = [];

    /// <summary>
    /// The list of Author following this author. 
    /// </summary>
    public List<Author> Followers { get; set; } = [];

    /// <summary>
    ///  A string representing the URL or file path to the profile image.
    /// </summary>
    [StringLength(160)]
    public string ProfileImage { get; set; } = string.Empty;
    
     /// <summary>
    ///  Shared random number generator used to assign a default profile image.
    /// </summary>
    private static readonly Random Random = new();

    /// <summary>
    ///  Sets random profile picture, when initializing new Author
    /// </summary>
    public Author()
    {
        if (!string.IsNullOrWhiteSpace(ProfileImage)) return;
        var n = Random.Next(1, 6);
        ProfileImage = $"/images/bird{n}-profile.png";
    }
}