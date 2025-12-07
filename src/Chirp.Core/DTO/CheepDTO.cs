using System.Globalization;
namespace Chirp.Core.DTO;

/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Cheep information.
/// </summary>
public class CheepDTO
{
    public AuthorDTO Author { get; set; }
    public string Message { get; set; }
    public string TimeStamp { get; set; }
    public int CheepId { get; set; }
    public List<CommentDTO> Comments { get; set; }

    public CheepDTO(AuthorDTO author, string message, string timestamp, int cheepId,  List<CommentDTO> comments)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{message}'");
        if (string.IsNullOrWhiteSpace(timestamp))
            throw new ArgumentException($"Timestamp cannot be null or empty. Invalid timestamp: '{timestamp}'");
        if (author == null)
            throw new ArgumentNullException(nameof(author), "Author cannot be null.");
        
        Author = author;
        Message = message;
        TimeStamp = timestamp;
        CheepId = cheepId;
        Comments = comments;
    }
    
    public static CheepDTO ToDto(Cheep c) => new(
        AuthorDTO.ToDto(c.Author),
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.CheepId,
        CommentDTO.ToDtos(c.Comments)
    );
    
    public static List<CheepDTO> ToDtos(List<Cheep> cheeps)
    {
        if (cheeps == null)
            throw new ArgumentNullException(nameof(cheeps), "List of cheeps cannot be null.");
        
        return cheeps.Select(ToDto).ToList();
    }
}