namespace Chirp.Core.DTO;

/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Cheep information.
/// </summary>
public class CheepDTO
{
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = string.Empty;
    public int CheepId { get; set; }
     
    public CheepDTO(string author, string message, string timestamp, int cheepId)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException($"Author cannot be null or empty. Invalid author: '{author}'");

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{message}'");
            
        this.Author = author;
        this.Message = message;
        this.TimeStamp = timestamp;
        this.CheepId = cheepId;
    }
}