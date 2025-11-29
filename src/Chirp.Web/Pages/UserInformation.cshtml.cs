using Chirp.Core;
using Chirp.Core.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
namespace Chirp.Web.Pages;

public class UserInformation : ChirpPage
{
    private readonly SignInManager<Author> _signInManager;
    public required IEnumerable<string> Followees { get; set; }
    public string CurrentProfileImage = string.Empty;
    public string CurrentAuthor = string.Empty;
    public string? Email;

    public UserInformation(ICheepService cheepService, IAuthorService authorService, SignInManager<Author> signInManager, UserManager<Author> userManager)
        : base(cheepService, authorService, userManager)
    {
        _signInManager = signInManager;
        PageSize = 5; 
    }
    
    public async Task<IActionResult> OnGetAsync(string author)
    {
        var user = await GetCurrentUserAsync();
        CurrentProfileImage = user?.ProfileImage ?? "/images/bird1-profile.png";
        Email = user?.Email;
        CurrentPage = GetPageNumber();
        CurrentAuthor = author;
        
        try
        {
            if (user != null && user.UserName == author)
            {
                Followees = await AuthorService.GetAllFollowees(author);
                var cheepsList = await CheepService.GetCheepsByAuthor(author, CurrentPage, PageSize + 1);
                ApplyPagination(cheepsList);
            }
        }
        catch (ArgumentException)
        {
            Cheeps = new List<CheepDTO>();
            Followees = new List<string>();
        } 
        
        return Page();
    }
    
    // For the Download
    public async Task<IActionResult> OnPostDownloadAsync(string author)
    {
        var user = await GetCurrentUserAsync();
        if (user == null || !string.Equals(user.UserName, author, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var sb = new StringBuilder();

        // User info
        sb.AppendLine(user.UserName + "'s Information");
        sb.AppendLine($"Name;{user.UserName}");
        sb.AppendLine($"Email;{user.Email}");
        if (user.PasswordHash != null)
        {
            sb.AppendLine($"PasswordHash;{user.PasswordHash}");
        }
        sb.AppendLine();

        // Followees
        var followees = await AuthorService.GetAllFollowees(user.UserName!);
        sb.AppendLine("Users you follow");

        foreach (var followee in followees)
        {
            sb.AppendLine($"Username;\"{followee}\"");
        }
        sb.AppendLine();

        // Cheeps
        sb.AppendLine("Your cheeps");

        int page = 1;
        const int exportPageSize = 100;

        while (true)
        {
            // Get cheeps page by page
            var cheepsPage = await CheepService.GetCheepsByAuthor(user.UserName!, page, exportPageSize);
            
            if (cheepsPage.Count == 0) { break; }

            foreach (var cheep in cheepsPage)
            {
                // Quote wrap
                var CheepMessage = cheep.Message?.Replace("\"", "\"\"") ?? string.Empty;
                var timestamp = cheep.TimeStamp;

                sb.AppendLine($"{timestamp};\"{CheepMessage}\"");
            }

            // Last Page
            if (cheepsPage.Count < exportPageSize) { break; }
            page++;
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"chirp-export-{user.UserName}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(string author)
    {
        await _signInManager.SignOutAsync();
        await AuthorService.DeleteAuthor(author);
        return RedirectToPage("/Public");
    }
      
    [BindProperty]
    public string? SelectedImage { get; set; }
    public async Task<IActionResult> OnPostImageProfileAsync(string author)
    {
        if (SelectedImage == null) return RedirectToPage();

        await AuthorService.SetProfileImage(author, SelectedImage);
        return RedirectToPage();
    }
}