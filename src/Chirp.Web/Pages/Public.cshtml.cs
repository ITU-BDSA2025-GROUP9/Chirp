using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Chirp.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace Chirp.Web.Pages;

/// <summary>
/// Represents the public timeline page.
/// Handles displaying cheeps, pagination, posting new cheeps, following and
/// unfollowing authors, and managing comments.
/// </summary>
public class PublicModel : ChirpPage
{
    /// <summary>
    /// Bound input model used for creating new cheeps
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    private readonly ICommentService _commentService;

    /// <summary>
    /// Intializes a new instane of the <see cref="PublicModel"/> class.
    /// </summary>
    /// <param name="cheepService"> service used for cheep-related operations</param>
    /// <param name="authorService"> service used for author-related operations</param>
    /// <param name="userManager"> user manager for accessing the current authenticated user</param>
    /// <param name="commentService">Service used for comment-related operations</param>
    public PublicModel(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager, ICommentService commentService)
        : base(cheepService, authorService, userManager)
    {
        _commentService = commentService;
    }
    
    /// <summary>
    /// Handles GET request for the public timeline page.
    /// Retrieves cheeps for the current page and applies pageination
    /// </summary>
    /// <returns> The rendered public page</returns>
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
    
    /// <summary>
    /// Handles posting a new cheep to the public timeline.
    /// </summary>
    /// <returns>
    /// Redirects to the login page if the user is not logged in
    /// otherwise redirects back to the public page.
    ///</returns>
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
    
    /// <summary>
    /// Handles deletion of a cheep
    /// </summary>
    /// <param name="id">the id of the cheep we wish to delete</param>
    /// <returns> redirecetes back to public page.</returns>
    public async Task<IActionResult> OnPostDeleteCheepAsync(int id)
    { 
        await CheepService.DeleteCheep(id);
        return RedirectToPage("/Public"); 
    }
    
    /// <summary>
    /// Handles unfollowing an author
    /// </summary>
    /// <param name="author">The name of the author to unfollow</param>
    /// <returns>
    /// Redirectes to login page if user is not logged in,
    /// otherwise redirects back to public page
    /// </returns>
    public async Task<IActionResult> OnPostUnfollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.UnfollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/Public");
    }
    
    /// <summary>
    /// Handles following an author
    /// </summary>
    /// <param name="author">the username of the author to follow</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to public page
    /// </returns>
    public async Task<IActionResult> OnPostFollowAsync(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return RedirectToPage("/Account/Login");

        await AuthorService.FollowAuthor(currentUser.UserName!, author);
        return RedirectToPage("/Public");
    }

    /// <summary>
    /// Handles adding a new comment to a cheep
    /// </summary>
    /// <param name="cheepId">The id of the cheep being commented on</param>
    /// <param name="content">The text of the comment</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to public page
    /// </returns>
    public async Task<IActionResult> OnPostAddCommentAsync(int cheepId, string content)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) 
            return RedirectToPage("/Account/Login");
        
        await _commentService.AddComment(cheepId, user.Id, content);
        return RedirectToPage("Public");
    }
    
    /// <summary>
    /// Handles deletion of a comment
    /// </summary>
    /// <param name="commentId">The id of the comment that needs to be deleted</param>
    /// <returns>
    /// Redirects to login page if user is not logged in,
    /// otherwise redirects back to public page
    /// </returns>
    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage("/Account/Login");

        await _commentService.DeleteComment(commentId);
        return RedirectToPage("/Public");
    }
}
