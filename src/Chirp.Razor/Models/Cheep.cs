using System;
using System.ComponentModel.DataAnnotations;
using Chirp.Razor.Models;

namespace Chirp.Razor.Models;

public class Cheep
{
    public int CheepId { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}