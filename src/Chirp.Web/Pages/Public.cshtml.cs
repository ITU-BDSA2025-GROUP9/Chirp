using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    private readonly int pageSize = 32; 
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
            Cheeps = await _service.GetCheeps(pageno, pageSize);
        }
        catch (ArgumentOutOfRangeException)
        {
            Cheeps = new List<CheepDTO>();
        }
        
        return Page();
    }
}