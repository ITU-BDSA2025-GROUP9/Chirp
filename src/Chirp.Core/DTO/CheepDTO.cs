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
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp representing when the Cheep was created or posted, stored as a string.
    /// </summary>
    public string TimeStamp { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the Cheep.
    /// Maps to the underlying database Cheep ID.
    /// </summary>
    public int CheepId { get; set; }

    /// <summary>
    /// Creates a new <see cref="CheepDTO"/> instance.
    /// Throws an <see cref="ArgumentException"/> if <paramref name="message"/> is null or whitespace.
    /// </summary>
    /// <param name="author">Author profile data for the Cheep.</param>
    /// <param name="message">The Cheep content. Cannot be null or whitespace.</param>
    /// <param name="timestamp">Formatted timestamp for creation or posting time.</param>
    /// <param name="cheepId">The unique Cheep identifier.</param>
    public CheepDTO(AuthorDTO author, string message, string timestamp, int cheepId)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException($"Message cannot be null or whitespace. Invalid message: '{message}'");
        }

        Author = author;
        Message = message;
        TimeStamp = timestamp;
        CheepId = cheepId;
    }
}