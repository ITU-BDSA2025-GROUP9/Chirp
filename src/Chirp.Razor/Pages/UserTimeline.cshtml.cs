using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Razor.DTO;
using Chirp.Razor;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepViewModel> Cheeps { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        var pageQuery = Request.Query["page"];
        int pageno;
        
        if (!int.TryParse(pageQuery, out pageno) || pageno <= 0) {
            pageno = 1;
        }

        try {
            Cheeps = _service.GetCheepsFromAuthor(author, pageno);
        } catch (ArgumentException) {
            Cheeps = new List<CheepDTO>();
        } 
        
        return Page();
    }
}
