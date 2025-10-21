using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

/// <summary>
/// Represents a "cheep" (short message) posted by an author.
/// </summary>
public class Cheep
{
    /// <summary>
    /// Primary key for the cheep.
    /// Mapped to "message_id" in the database.
    /// </summary>
    public int CheepId { get; set; }

    /// <summary>
    /// The text content of the cheep.
    /// </summary>
    [Required, StringLength(160)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The time when the cheep was created (UTC).
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key referencing the author who posted the cheep.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Navigation property linking this cheep to its author.
    /// </summary>
    public Author Author { get; set; } = null!;
}