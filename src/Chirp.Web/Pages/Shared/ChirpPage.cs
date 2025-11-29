using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Chirp.Web.Pages.Shared;

public abstract class ChirpPage : PageModel
{
    protected readonly ICheepService CheepService;
    protected readonly IAuthorService AuthorService;
    protected readonly UserManager<Author> UserManager;
    
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    protected int PageSize = 32; 
    public bool HasNextPage; 
    public int CurrentPage;

    protected ChirpPage(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
    {
        CheepService = cheepService;
        AuthorService = authorService;
        UserManager = userManager;
    }
        
    protected int GetPageNumber()
    {
        var pageQuery = Request.Query["page"];
        if (!int.TryParse(pageQuery, out var pagenumber) || pagenumber <= 0) {
            pagenumber = 1;
        }
        return pagenumber;
    }

    protected async Task<Author?> GetCurrentUserAsync()
    {
        return await UserManager.GetUserAsync(User);
    }
    
    protected void ApplyPagination(List<CheepDTO> list)
    {
        Cheeps = list.Take(PageSize);
        HasNextPage = list.Count > PageSize;
    }
    
    public async Task<bool> IsFollowing(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return false;
        
        return await AuthorService.IsFollowing(currentUser.UserName!, author);
    }
}
