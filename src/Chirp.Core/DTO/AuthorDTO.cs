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
    public string Name { get; set; }

    /// <summary>
    /// The email address of the author.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The relative URL or file path to the author's profile image.
    /// </summary>
    public string ProfileImage { get; set; }

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
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Name cannot be null or empty. Invalid author name: '{name}'");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException($"Email cannot be null or empty. Invalid author email: '{email}'");
        if (string.IsNullOrWhiteSpace(profileImage)) 
            profileImage = "/images/bird1-profile.png"; 
            
        Name = name;
        Email = email;
        ProfileImage = profileImage;
    }
    
    /// <summary>
    /// Maps a <see cref="Author"/> to the <see cref="AuthorDTO"/>
    /// </summary>
    /// <param name="a">The author name we which to map.</param>
    public static AuthorDTO ToDto(Author a) => new(
        a.UserName!,
        a.Email!,
        a.ProfileImage
    );
    
    /// <summary>
    /// Maps a list of <see cref="Author"/> to the <see cref="AuthorDTO"/>
    /// It throws and exeception if author list is empty
    /// </summary>
    /// <param name="authors">A list of authors, that we wish to map</param>
    public static List<AuthorDTO> ToDtos(List<Author> authors)
    {
        if (authors == null)
            throw new ArgumentNullException(nameof(authors), "List of authors cannot be null.");
        
        return authors.Select(ToDto).ToList();
    }
}