using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core;

/// <summary>
/// Represents an author (user) who can post cheeps.
/// This version keeps compatibility with legacy code using AuthorId,
/// while maintaining clean EF Core mapping through Id.
/// </summary>
public class Author : IdentityUser<int>
{
    /// <summary>
    /// The list of cheeps posted by this author.
    /// </summary>
    public List<Cheep> Cheeps { get; set; } = new();
}