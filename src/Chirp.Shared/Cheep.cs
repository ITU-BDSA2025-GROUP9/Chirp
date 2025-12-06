namespace Chirp.Shared;

/// <summary>
/// Represents a message, i.e. a cheep, posted by a user.
/// </summary>
public record Cheep
{
    /// <summary>
    /// The name of the author who posted the cheep.
    /// </summary>
    public string Author { get; set; } = "";
    
    /// <summary>
    /// The text content of the cheep.
    /// </summary>
    public string Message { get; set; } = "";
    
    /// <summary>
    /// The time when the cheep was created.
    /// </summary>
    public long Timestamp { get; set; }
}
