namespace Chirp.Core.DTO;

public class CommentDTO
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    public string AuthorName { get; set; }
    public string? AuthorProfileImage { get; set; }
    public int AuthorId { get; set; }
}