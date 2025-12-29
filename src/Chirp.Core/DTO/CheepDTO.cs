using System.Globalization;
namespace Chirp.Core.DTO;

/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Cheep information.
/// </summary>
public class CheepDTO
{
    /// <summary>
    /// Public author profile information associated with this Cheep.
    /// </summary>
    public AuthorDTO Author { get; set; }
    /// <summary>
    /// The textual content of the Cheep.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Timestamp representing when the Cheep was created or posted, stored as a string.
    /// </summary>
    public string TimeStamp { get; set; }

    /// <summary>
    /// The unique identifier of the Cheep.
    /// Maps to the underlying database Cheep ID.
    /// </summary>
    public int CheepId { get; set; }

    /// <summary>
    /// A list of comments to the cheep, gotten from the <see cref="CheepDTO"/>>
    /// </summary>
    public List<CommentDTO> Comments { get; set; }
    
    /// <summary>
    /// Creates a new <see cref="CheepDTO"/> instance.
    /// Throws an <see cref="ArgumentException"/> if <paramref name="message"/> is null or whitespace.
    /// </summary>
    /// <param name="author">Author profile data for the Cheep.</param>
    /// <param name="message">The Cheep content. Cannot be null or whitespace.</param>
    /// <param name="timestamp">Formatted timestamp for creation or posting time.</param>
    /// <param name="cheepId">The unique Cheep identifier.</param>
    /// <param name="comment">A list of comments for this cheep.</param>
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
    
    /// <summary>
    /// Maps a single <see cref="Cheep"/> to the <see cref="CheepDTO"/>
    /// </summary>
    /// <param name="c">The cheep (a message) we are wish to map to the DTO</param>
    public static CheepDTO ToDto(Cheep c) => new(
        AuthorDTO.ToDto(c.Author),
        c.Text,
        c.TimeStamp.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture),
        c.CheepId,
        CommentDTO.ToDtos(c.Comments)
    );
    
    /// <summary>
    /// Maps a list <see cref="Cheep"/> to <see cref="CheepDTO"/>
    /// </summary>
    /// <param name="cheeps">A list of cheeps, that we wish to map</param>
    public static List<CheepDTO> ToDtos(List<Cheep> cheeps)
    {
        if (cheeps == null)
            throw new ArgumentNullException(nameof(cheeps), "List of cheeps cannot be null.");
        
        return cheeps.Select(ToDto).ToList();
    }
}