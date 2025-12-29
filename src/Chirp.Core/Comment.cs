using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;
/// <summary>
/// Represents a comment (short message) that replies to a cheep and is writing by an author
/// </summary>
public class Comment
{
    /// <summary>
    /// The unique identifier of a comment
    /// </summary>
    public int CommentId { get; set; }

    /// <summary>
    /// The text that a comment contains. Maximum legnthed allowed is 160
    /// </summary>
    [Required, StringLength(160)]
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// The timestamp of when the comment was created.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// The unique identifier of the cheep this comment belong to 
    /// </summary>
    public int CheepId { get; set; }
    
    /// <summary>
    /// The cheep which this comment belongs to
    /// </summary>
    public Cheep Cheep { get; set; } = null!;
    
    /// <summary>
    /// The unique identifier of the author who wrote this cheep
    /// </summary>
    public int AuthorId { get; set; }
    
    /// <summary>
    /// The author that wrote the comment 
    /// </summary>
    public Author Author { get; set; } = null!;
}

