using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Chirp.Web.Pages;

public class PublicModel : ChirpPage
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public PublicModel(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
        : base(cheepService, authorService, userManager) {}

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentPage = GetPageNumber();
        
        try
        {
            var cheepsList = await CheepService.GetCheeps(CurrentPage,PageSize + 1); // Get 33 cheeps to check, if a next page exists
            ApplyPagination(cheepsList);
        }
        catch (ArgumentOutOfRangeException)
        {
            Cheeps = new List<CheepDTO>();
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostPostCheepAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid){
            return await OnGetAsync();
        }
        
        await CheepService.AddCheep(user, Input.Text);
        return RedirectToPage("/Public");
    }
    
    public async Task<IActionResult> OnPostDeleteCheepAsync(int id)
    { 
        await CheepService.DeleteCheep(id);
        return RedirectToPage("/Public"); 
    }
    
    public async Task<IActionResult> OnPostUnfollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.UnfollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/Public");
    }
    
    public async Task<IActionResult> OnPostFollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.FollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/Public");
    }
}