namespace Chirp.Shared;

public record Cheep
{
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public long Timestamp { get; set; }
}
