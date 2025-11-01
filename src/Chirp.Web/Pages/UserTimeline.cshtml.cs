using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    private readonly UserManager<Author> _userManager;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    public bool HasNextPage { get; set; }
    public readonly int pageSize = 32; 
    public int CurrentPage; 
    public string CurrentAuthor = string.Empty; 

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public UserTimelineModel(ICheepService service, UserManager<Author> userManager)
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
            var cheepsList = await _service.GetCheepsByAuthor(author, pageno, pageSize + 1); // Get 33 cheeps to check, if a next page exists
            Cheeps = cheepsList.Take(pageSize); 
            HasNextPage = cheepsList.Count > pageSize;
        }
        catch (ArgumentException)
        {
            Cheeps = new List<CheepDTO>();
        } 
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostPostCheepAsync(string author)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
            return await OnGetAsync(author);
        
        await _service.AddCheep(user, Input.Text);
        return RedirectToPage("/UserTimeline");
    }
}