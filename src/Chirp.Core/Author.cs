using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

/// <summary>
/// Represents an author (user) who can post cheeps.
/// This version keeps compatibility with legacy code using AuthorId,
/// while maintaining clean EF Core mapping through Id.
/// </summary>
public class Author
{
    /// <summary>
    /// Primary key for the author entity.
    /// Mapped to "user_id" in the database.
    /// </summary>
    public int AuthorId { get; set; }
    
    /// <summary>
    /// The username of the author.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the author.
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The list of cheeps posted by this author.
    /// </summary>
    public List<Cheep> Cheeps { get; set; } = new();
}