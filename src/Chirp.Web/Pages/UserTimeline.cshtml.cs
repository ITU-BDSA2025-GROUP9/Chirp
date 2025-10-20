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
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    public readonly int pageSize = 32; 
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
            Cheeps = await _service.GetCheepsByAuthor(author, pageno, pageSize);
        }
        catch (ArgumentException)
        {
            Cheeps = new List<CheepDTO>();
        } 
        
        return Page();
    }
}