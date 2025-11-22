using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Chirp.Web.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace Chirp.Web.Pages;

public class UserInformation : PageModel
{
    private readonly ICheepService _service;
    private readonly UserManager<Author> _userManager;
    private readonly IRepository _repository;   // NEW

    
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    public bool HasNextPage { get; set; }
    public readonly int pageSize = 5; 
    public int CurrentPage; 
    public string CurrentAuthor = string.Empty; 
    
    
    public IEnumerable<string> Followees { get; set; } = new List<string>(); // NEW
    public string? Email { get; set; }                                        // NEW
    
    [BindProperty]
    public LoginModel.InputModel Input { get; set; } = new();
    
    public UserInformation(ICheepService service, UserManager<Author> userManager,  IRepository repository)
    {
        _service = service;
        _userManager = userManager;
        _repository = repository;
    }
    
    public async Task<IActionResult> OnGetAsync(string author)
    {
        var pageQuery = Request.Query["page"];
        int pageno;
        
        if (!int.TryParse(pageQuery, out pageno) || pageno <= 0) {
            pageno = 1;
        }
        
        CurrentPage = pageno;
        CurrentAuthor = author;
        
        try
        {
            var user = await _userManager.GetUserAsync(User);
            List<CheepDTO> cheepsList;

            Email = user?.Email; // Email of the current user
            Followees = await _repository.GetAllFollowees(author); // All authors this user follows
            
            if (user != null && user.UserName == author)
            {
                cheepsList = await _service.GetUserTimelineCheeps(author, pageno, pageSize + 1);
            }
            else
            {
                cheepsList = await _service.GetCheepsByAuthor(author, pageno, pageSize + 1);
            }

            Cheeps = cheepsList.Take(pageSize);
            HasNextPage = cheepsList.Count > pageSize;
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
        var user = await _userManager.GetUserAsync(User);
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
        var followees = await _repository.GetAllFollowees(user.UserName!);
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
            // Get cheeps page by page until we run out
            var cheepsPage = await _service.GetCheepsByAuthor(user.UserName!, page, exportPageSize);
            
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
}