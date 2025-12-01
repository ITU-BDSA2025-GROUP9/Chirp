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

            var cheepsList = await CheepService.GetCheepsByAuthor(author, CurrentPage, PageSize + 1);
            ApplyPagination(cheepsList);
        }
        catch (ArgumentException)
        {
            AuthorExists = false;
            Cheeps = new List<CheepDTO>();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddCommentAsync(int cheepId, string content)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        await _commentService.AddCommentAsync(cheepId, user.Id, content);
        return RedirectToPage("/UserTimeline", new { author = CurrentAuthor });
    }

    public async Task<bool> IsFollowing(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return false;
        return await AuthorService.IsFollowing(currentUser.UserName!, author);
    }
    
    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        await _commentService.DeleteCommentAsync(commentId);
        return RedirectToPage("/UserTimeline", new { author = CurrentAuthor });
    }

}