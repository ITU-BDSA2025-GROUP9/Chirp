namespace Chirp.Core.DTO;
/// <summary>
/// Represents a lightweight data transfer object (DTO)
/// for sending or displaying Author information.
/// </summary>
public class AuthorDTO
{
    /// <summary>
    /// The display name of the author.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the author.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The relative URL or file path to the author's profile image.
    /// </summary>
    public string ProfileImage { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="AuthorDTO"/> instance.
    /// If the provided profile image path is null or empty, a default image is assigned.
    /// </summary>
    /// <param name="name">The display name of the author.</param>
    /// <param name="email">The email address of the author.</param>
    /// <param name="profileImage">
    /// Relative path or URL to a profile image.
    /// If null or empty, defaults to "/images/bird1-profile.png".
    /// </param>
    public AuthorDTO(string name, string email, string profileImage)
    {
        if (string.IsNullOrEmpty(profileImage))
        {
            profileImage = "/images/bird1-profile.png";
        }

        Name = name;
        Email = email;
        ProfileImage = profileImage;
    }
}