using Chirp.Core;
using Chirp.Core.DTO;
using Chirp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Chirp.Web.Pages.Shared;

/// <summary>
/// Base PageModel for Chirp pages providing Cheep access,
/// author follow state, user context, and pagination.
/// </summary>
public abstract class ChirpPage : PageModel
{
    protected readonly ICheepService CheepService;
    protected readonly IAuthorService AuthorService;
    protected readonly UserManager<Author> UserManager;
    
    public required IEnumerable<CheepDTO> Cheeps { get; set; }
    protected int PageSize = 32; 
    public bool HasNextPage; 
    public int CurrentPage;
    
    /// <summary>
    /// Creates a new instance of <see cref="ChirpPage"/>.
    /// </summary>
    /// <param name="cheepService">Service for fetching and mutating Cheep post.</param>
    /// <param name="authorService">Service for follow state and author profile actions.</param>
    /// <param name="userManager">Identity user manager dependency.</param>
    protected ChirpPage(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
    {
        CheepService = cheepService;
        AuthorService = authorService;
        UserManager = userManager;
    }
    
    /// <summary>
    /// Retrieves page number of current page,
    /// defaults to 1 if invalid or missing.
    /// </summary>
    /// <returns>A valid 1-based indexed page number.</returns>
    protected int GetPageNumber()
    {
        var pageQuery = Request.Query["page"];
        if (!int.TryParse(pageQuery, out var pagenumber) || pagenumber <= 0) {
            pagenumber = 1;
        }
        return pagenumber;
    }
    
    /// <summary>
    /// Retrieves the current authenticated author.
    /// </summary>
    /// <returns>The current <see cref="Author"/> or null if not signed in.</returns>
    protected async Task<Author?> GetCurrentUserAsync()
    {
        return await UserManager.GetUserAsync(User);
    }
    
    /// <summary>
    /// Applies the pagination from <see cref="CheepDTO"/>.
    /// </summary>
    /// <param name="list">A list of display information of cheeps.</param>
    protected void ApplyPagination(List<CheepDTO> list)
    {
        Cheeps = list.Take(PageSize);
        HasNextPage = list.Count > PageSize;
    }
    
    /// <summary>
    /// Checks whether the current signed-in Author follows the specified Author name.
    /// </summary>
    /// <param name="author">Target Author username to check follow state for.</param>
    /// <returns>True if following, false otherwise.</returns>
    public async Task<bool> IsFollowing(string author)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return false;
        
        return await AuthorService.IsFollowing(currentUser.UserName!, author);
    }
}
