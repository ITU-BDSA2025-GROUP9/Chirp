using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

/// <summary>
/// Represents the user timeline page.
/// Displays a timeline of cheeps authored by a specific user and the
/// users they follow, and allows interaction trough posting cheeps,
/// commenting, and following or unfollowing authors.
/// </summary>
public class UserTimelineModel : ChirpPage
{
    /// <summary>
    /// Bound input model used for creating new cheeps and comments.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    private readonly ICommentService _commentService;

    /// <summary>
    /// The username of the author timeline currently being viewed.
    /// </summary>
    public string CurrentAuthor = string.Empty;
    
    /// <summary>
    /// Indicated wether the requested author exist in the system
    /// </summary>
    public bool AuthorExists = true;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserTimelineModel"/> class.
    /// </summary>
    /// <param name="cheepService">Service used for cheep-related operations</param>
    /// <param name="authorService">Service used for author-related operations</param>
    /// <param name="userManager">User manager used to retrieve the current user</param>
    /// <param name="commentService">Service used for comment-related operations</param>
    public UserTimelineModel(
        ICheepService cheepService,
        IAuthorService authorService,
        UserManager<Author> userManager,
        ICommentService commentService)
        : base(cheepService, authorService, userManager)
    {
        _commentService = commentService;
    }
    
    /// <summary>
    /// Handles GET requests for the user timeline page.
    /// Retrieves cheeps from the specified author nad their followees and applies pagination.
    /// </summary>
    /// <param name="author">The name of the timeline owner</param>
    /// <returns>The rendered user timeline page</returns>
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
    
    /// <summary>
    /// Handles posting a new cheep to the user timeline
    /// </summary>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to user timeline
    /// </returns>
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
    
    /// <summary>
    /// Handles deletion of a cheep from the timeline.
    /// </summary>
    /// <param name="id">The id of the cheep to delete</param>
    /// <returns>Redirects back to user timeline</returns>
    public async Task<IActionResult> OnPostDeleteCheepAsync(int id)
    { 
        await CheepService.DeleteCheep(id);
        return RedirectToPage("/UserTimeline"); 
    }
    
    /// <summary>
    /// Handles adding a new comment to a cheep on the timeline.
    /// </summary>
    /// <param name="cheepId">The id of the cheep being commented on</param>
    /// <param name="content">The content of the comment</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to user timeline
    /// </returns>
    public async Task<IActionResult> OnPostAddCommentAsync(int cheepId, string content)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) 
            return RedirectToPage("/Account/Login");
        
        await _commentService.AddComment(cheepId, user.Id, content);
        return RedirectToPage("/UserTimeline");
    }
    
    /// <summary>
    /// Handles unfollowing an author from the timeline.
    /// </summary>
    /// <param name="author">The name of the author to unfollow</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to user timeline.
    /// </returns>
    public async Task<IActionResult> OnPostUnfollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.UnfollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/UserTimeline");
    }
    
    /// <summary>
    /// Handles following an author from the timeline.
    /// </summary>
    /// <param name="author">The username of the author to follow</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to user timeline
    /// </returns>
    public async Task<IActionResult> OnPostFollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.FollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/UserTimeline");
    }
    
    /// <summary>
    /// Handles deletion of a comment from the timeline
    /// </summary>
    /// <param name="commentId">The id of the comment to delete</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to user timeline.
    /// </returns>
    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        await _commentService.DeleteComment(commentId);
        return RedirectToPage("/UserTimeline");
    }

}