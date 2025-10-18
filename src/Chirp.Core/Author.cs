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
    public int Id { get; set; }

    /// <summary>
    /// Legacy alias for backward compatibility.
    /// Allows existing code referring to AuthorId to compile and function normally.
    /// This simply forwards to Id under the hood.
    /// </summary>
    [NotMapped] // ✅ prevents EF from trying to create a "AuthorId" column
    public int AuthorId
    {
        get => Id;
        set => Id = value;
    }

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