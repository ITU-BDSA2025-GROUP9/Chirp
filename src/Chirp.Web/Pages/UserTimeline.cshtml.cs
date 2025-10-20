using System;
using System.Collections.Generic;
using System.Linq;
using Chirp.Core.DTO;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;

    public required List<CheepDTO> Cheeps { get; set; } = new();

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        var pageno = 1;
        author = Uri.UnescapeDataString(author);
        Cheeps = _service.GetCheepsByAuthor(author, pageno, 32).ToList();
        return Page();
    }

}