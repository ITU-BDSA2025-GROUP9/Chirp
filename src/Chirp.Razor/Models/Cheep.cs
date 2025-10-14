using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Models;

public class Cheep
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}