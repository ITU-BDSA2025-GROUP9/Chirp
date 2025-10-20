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
     
    public CheepDTO(string Author, string Message, string Timestamp, string AuthorEmail)
    {
        if (string.IsNullOrWhiteSpace(Author))
            throw new ArgumentException($"Author cannot be null or empty. Invalid author: '{Author}'");

        if (string.IsNullOrWhiteSpace(Message))
            throw new ArgumentException($"Message cannot be null or empty. Invalid message: '{Message}'");
            
        this.Author = Author;
        this.Message = Message;
        this.TimeStamp = Timestamp;
        this.AuthorEmail = AuthorEmail;
    }
}
