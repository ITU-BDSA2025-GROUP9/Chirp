using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly UserManager<Author> _userManager;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    public bool HasNextPage { get; set; }
    private readonly int pageSize = 32; 
    public int CurrentPage;
    
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public PublicModel(ICheepService service, UserManager<Author> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var pageQuery = Request.Query["page"];
        int pageno;
    
        if (!int.TryParse(pageQuery, out pageno) || pageno <= 0) {
            pageno = 1;
        }
        CurrentPage = pageno;
        try
        {
            var cheepsList = await _service.GetCheeps(pageno, pageSize + 1); // Get 33 cheeps to check, if a next page exists
            Cheeps = cheepsList.Take(pageSize); 
            HasNextPage = cheepsList.Count > pageSize;
        }
        catch (ArgumentOutOfRangeException)
        {
            Cheeps = new List<CheepDTO>();
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostPostCheepAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid){
            return await OnGetAsync();
        }
        
        await _service.AddCheep(user, Input.Text);
        return RedirectToPage("/Public");
    }
}