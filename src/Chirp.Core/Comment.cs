using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class Comment
{
    public int CommentId { get; set; }

    [Required, StringLength(160)]
    public string Text { get; set; } = string.Empty;
    
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public int CheepId { get; set; }
    public Cheep Cheep { get; set; } = null!;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}

