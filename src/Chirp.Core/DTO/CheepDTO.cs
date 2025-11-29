namespace Chirp.Core.DTO;

/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Cheep information.
/// </summary>
public class CheepDTO
{
    public AuthorDTO Author { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = string.Empty;
    public int CheepId { get; set; }
     
    public CheepDTO(AuthorDTO author, string message, string timestamp, int cheepId)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{message}'");
            
        this.Author = author;
        this.Message = message;
        this.TimeStamp = timestamp;
        this.CheepId = cheepId;
    }
}