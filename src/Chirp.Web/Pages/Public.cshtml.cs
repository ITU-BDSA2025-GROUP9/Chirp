using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; } = new();

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public IActionResult OnGet()
    {
        var pageQuery = Request.Query["page"];
        int pageno = int.TryParse(pageQuery, out var num) && num > 0 ? num : 1;

        Cheeps = _service.GetCheeps(pageno, 32).ToList();
        return Page();
    }
}