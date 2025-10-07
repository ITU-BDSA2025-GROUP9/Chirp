using System;
using System.ComponentModel.DataAnnotations;
using Chirp.Razor.Models;

namespace Chirp.Razor.Models
{
    public class Cheep
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }
}