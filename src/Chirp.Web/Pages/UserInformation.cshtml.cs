using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Chirp.Web.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class UserInformation : PageModel
{
    private readonly ICheepService _service;
    private readonly UserManager<Author> _userManager;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    public bool HasNextPage { get; set; }
    public readonly int pageSize = 5; 
    public int CurrentPage; 
    public string CurrentAuthor = string.Empty; 
    
    [BindProperty]
    public LoginModel.InputModel Input { get; set; } = new();
    
    public UserInformation(ICheepService service, UserManager<Author> userManager)
    {
        _service = service;
        _userManager = userManager;
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
        } 
        
        return Page();
    }

}