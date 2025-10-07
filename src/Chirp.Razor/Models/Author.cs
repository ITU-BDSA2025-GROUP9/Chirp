using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Models
{
    public class Author
    {
        public int Id { get; set; } // EF Core will recognize this as the primary key by convention

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // One author can have many cheeps that should know its own cheeps
        public List<Cheep> Cheeps { get; set; } = new();
    }
}