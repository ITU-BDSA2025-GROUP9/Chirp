using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Models;

/// <summary>
/// Represents a "cheep" (short message) posted by an author.
/// </summary>
public class Cheep
{
    /// <summary>
    /// Primary key for the cheep.
    /// Mapped to "message_id" in the database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Legacy alias for backward compatibility with older code using CheepId.
    /// </summary>
    public int CheepId
    {
        get => Id;
        set => Id = value;
    }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}
