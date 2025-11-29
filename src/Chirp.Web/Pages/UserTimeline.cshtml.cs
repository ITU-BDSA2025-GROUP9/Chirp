using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Chirp.Web.Pages;

public class UserTimelineModel : ChirpPage
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string CurrentAuthor = string.Empty;
    public bool AuthorExists; 
    
    public UserTimelineModel(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
        : base(cheepService, authorService, userManager) {}
    
    public async Task<IActionResult> OnGetAsync(string author)
    {
        AuthorExists  = await AuthorService.AuthorByNameExists(author);
        if(!AuthorExists)  return Page();
        
        CurrentPage = GetPageNumber();
        CurrentAuthor = author;
        
        try
        {
            var user = await GetCurrentUserAsync();
            List<CheepDTO> cheepsList;
            if (user != null && user.UserName == author)
            {
                var authors = await AuthorService.GetAllFolloweesAndSelf(author);
                cheepsList = await CheepService.GetCheepsByAuthors(authors,CurrentPage, PageSize + 1);
            }
            else
            {
                cheepsList = await CheepService.GetCheepsByAuthor(author, CurrentPage, PageSize + 1);
            }

            ApplyPagination(cheepsList);
        }
        catch (ArgumentException)
        {
            Cheeps = new List<CheepDTO>();
        } 
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostPostCheepAsync(string author)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
            return await OnGetAsync(author);
        
        await CheepService.AddCheep(user, Input.Text);
        return RedirectToPage("/UserTimeline");
    }
    
    public async Task<IActionResult> OnPostDeleteCheepAsync(int id)
    { 
        await CheepService.DeleteCheep(id);
        return RedirectToPage("/UserTimeline"); 
    }
    
    public async Task<IActionResult> OnPostUnfollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.UnfollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/UserTimeline");
    }
    
    public async Task<IActionResult> OnPostFollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.FollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/UserTimeline");
    }
}