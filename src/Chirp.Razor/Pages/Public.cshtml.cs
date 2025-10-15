using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.DTO;
using Chirp.Razor;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage; 

    public PublicModel(ICheepService service)
    {
        _service = service;
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
            Cheeps = await _service.GetCheeps(pageno);
        }
        catch (ArgumentOutOfRangeException)
        {
            Cheeps = new List<CheepDTO>();
        }

        return Page();
    }
}
