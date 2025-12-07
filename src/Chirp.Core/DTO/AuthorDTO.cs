namespace Chirp.Core.DTO;

public class AuthorDTO
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfileImage { get; set; }

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
    
    public static AuthorDTO ToDto(Author a) => new(
        a.UserName!,
        a.Email!,
        a.ProfileImage
    );
    
    public static List<AuthorDTO> ToDtos(List<Author> authors)
    {
        if (authors == null)
            throw new ArgumentNullException(nameof(authors), "List of authors cannot be null.");
        
        return authors.Select(ToDto).ToList();
    }
}