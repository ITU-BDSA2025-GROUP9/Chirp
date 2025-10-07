namespace Chirp.Razor.DTO
{
    /// <summary>
    /// does not expose database/internal structure.
    /// </summary>
    public record CheepDTO(
        string Author,
        string Message,
        string Timestamp
    );
}