using System.Globalization;
namespace Chirp.Core.DTO;

public class CommentDTO
{
    public AuthorDTO Author { get; set; }
    public string Message { get; set; }
    public string TimeStamp { get; set; }
    public int CommentId { get; set; }
    
    public CommentDTO(AuthorDTO author, string message, string timestamp, int commentId)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{message}'");
        if (string.IsNullOrWhiteSpace(timestamp))
            throw new ArgumentException($"Timestamp cannot be null or empty. Invalid timestamp: '{timestamp}'");
        if (author == null)
            throw new ArgumentNullException(nameof(author), "Author cannot be null.");
        
        CommentId = commentId;
        Message = message;
        TimeStamp = timestamp;
        Author = author;
    }
    
    public static CommentDTO ToDto(Comment c) => new(
        AuthorDTO.ToDto(c.Author),
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.CommentId
    );

    public static List<CommentDTO> ToDtos(List<Comment> comments)
    {
        if (comments == null)
            throw new ArgumentNullException(nameof(comments), "List of comments cannot be null.");
        
        return comments.Select(ToDto).ToList();
    }
}