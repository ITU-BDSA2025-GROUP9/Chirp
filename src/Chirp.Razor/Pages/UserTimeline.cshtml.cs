using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.DTO;
using Chirp.Razor;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage; 
    public string CurrentAuthor = string.Empty; 

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
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
            Cheeps = await _service.GetCheepsFromAuthor(author, pageno);
        }
        catch (ArgumentException)
        {
            Cheeps = new List<CheepDTO>();
        } 
        
        return Page();
    }
}