using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class UserTimelineModel : ChirpPage
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    private readonly ICommentService _commentService;

    public string CurrentAuthor = string.Empty;
    public bool AuthorExists = true;

    public UserTimelineModel(
        ICheepService cheepService,
        IAuthorService authorService,
        UserManager<Author> userManager,
        ICommentService commentService)
        : base(cheepService, authorService, userManager)
    {
        _commentService = commentService;
    }

    public async Task<IActionResult> OnGetAsync(string author)
    {
        CurrentAuthor = author;
        CurrentPage = GetPageNumber();

        try
        {
            AuthorExists = await AuthorService.AuthorByNameExists(author);
            if (!AuthorExists)
            {
                Cheeps = new List<CheepDTO>();
                return Page();
            }

            var authors = await AuthorService.GetAllFolloweesAndSelf(author);
            var cheepsList = await CheepService.GetCheepsByAuthors(authors,CurrentPage, PageSize + 1);
            
            ApplyPagination(cheepsList);
        }
        catch (ArgumentException)
        {
            AuthorExists = false;
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
            return await OnGetAsync(CurrentAuthor);
        }
        
        await CheepService.AddCheep(user, Input.Text);
        return RedirectToPage("/UserTimeline");
    }
    
    public async Task<IActionResult> OnPostDeleteCheepAsync(int id)
    { 
        await CheepService.DeleteCheep(id);
        return RedirectToPage("/UserTimeline"); 
    }
    
    public async Task<IActionResult> OnPostAddCommentAsync(int cheepId, string content)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) 
            return RedirectToPage("/Account/Login");
        
        await _commentService.AddComment(cheepId, user.Id, content);
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
    
    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        await _commentService.DeleteComment(commentId);
        return RedirectToPage("/UserTimeline");
    }

}