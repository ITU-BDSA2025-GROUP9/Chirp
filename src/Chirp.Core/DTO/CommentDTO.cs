namespace Chirp.Core.DTO;

public class CommentDTO
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string CreatedAt { get; set; }
    public AuthorDTO Author { get; set; }
    
    public CommentDTO(AuthorDTO author, string message, string timestamp, int id)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{message}'");
            
        this.Id = id;
        this.Content = message;
        this.CreatedAt = timestamp;
        this.Author = author;
        
    }
}