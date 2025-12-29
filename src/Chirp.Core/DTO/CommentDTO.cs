using System.Globalization;
namespace Chirp.Core.DTO;
/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Comment information.
/// </summary>
public class CommentDTO
{
    /// <summary>
    /// The author who created the comment
    /// </summary>
    public AuthorDTO Author { get; set; }
    
    /// <summary>
    /// The text of the comment
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// The time the comment was created at, formated as a string
    /// </summary>
    public string TimeStamp { get; set; }
    
    /// <summary>
    /// The unique identifier of the comment
    /// </summary>
    public int CommentId { get; set; }
    
    /// <summary>
    /// Creates a new <see cref="CommentDTO"/> intance.
    /// </summary>
    /// <param name="author">The name of the person who created this comment</param>
    /// <param name="message">The text of the comment</param>
    /// <param name="timestamp">The timestamp of when the comment was made as a string</param>
    /// <param name="commentId">The unique identifier of the comment</param>
    /// <exception cref="ArgumentException"> thrown when <paramref name="author "/> is null</exception>
    /// <exception cref="ArgumentNullException">
    ///thrown when <paramref name="message"/> or <paramref name="timestamp"/> is null or empty
    /// </exception>
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
    
    /// <summary>
    /// Maps a <see cref="Comment"/> to a <see cref="CommentDTO"/>
    /// </summary>
    /// <param name="c">The comment we which to map</param>
    /// <returns>A <see cref="CommentDTO"/> containing the mapped comment data</returns>
    public static CommentDTO ToDto(Comment c) => new(
        AuthorDTO.ToDto(c.Author),
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.CommentId
    );
    
    /// <summary>
    /// Maps a list/collection of <see cref="Comment"/> to a list of <see cref="CommentDTO"/>
    /// </summary>
    /// <param name="comments">The list of comments we wish to map</param>
    /// <returns>A list of mapped <see cref="CommentDTO"/> intances</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comments"/> are null</exception>
    public static List<CommentDTO> ToDtos(List<Comment> comments)
    {
        if (comments == null)
            throw new ArgumentNullException(nameof(comments), "List of comments cannot be null.");
        
        return comments.Select(ToDto).ToList();
    }
}