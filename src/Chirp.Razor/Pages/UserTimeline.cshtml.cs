﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
		var pageQuery = Request.Query["page"];

        int pageno;
        if (!string.IsNullOrWhiteSpace(pageQuery))
        {
            pageno = Int32.Parse(pageQuery);
        } else {
            pageno = 1; 
        }
        Cheeps = _service.GetCheepsFromAuthor(author, pageno);
        return Page();
    }
}
