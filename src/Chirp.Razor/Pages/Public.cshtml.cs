using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public PublicModel(ICheepService service)
    {
        _service = service;
    }
    
    public ActionResult OnGet()
    {
        var pageQuery = Request.Query["page"];

        int pageno;
        if (!string.IsNullOrWhiteSpace(pageQuery))
        {
            pageno = Int32.Parse(pageQuery);
        } else {
            pageno = 1; 
        }
        
        Cheeps = _service.GetCheeps(pageno);
        return Page();
    }
}
