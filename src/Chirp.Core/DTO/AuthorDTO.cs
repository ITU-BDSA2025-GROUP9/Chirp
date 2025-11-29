namespace Chirp.Core.DTO;

public class AuthorDTO
{
    public string Name { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;

    public AuthorDTO(string name, string email, string profileImage)
    {
        if (string.IsNullOrEmpty(profileImage)) profileImage = "/images/bird1-profile.png";
            
        this.Name = name;
        this.Email = email;
        this.ProfileImage = profileImage;
        
    }

}