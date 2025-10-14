using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Models;

public class Author
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public List<Cheep> Cheeps { get; set; } = new();
}