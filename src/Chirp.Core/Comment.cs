namespace Chirp.Core;

public class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CheepId { get; set; }
    public Cheep Cheep { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; }
}

