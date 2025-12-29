namespace Chirp.Web.Pages.Shared;
/// <summary>
/// Represents the view model used for rendering the follow and unfollow button
/// associaed with a specific author and cheep
/// </summary>
public class FollowButtonViewModel
{
    /// <summary>
    /// The username of the author associated with the follow button
    /// </summary>
    public string Author { get; set; } = string.Empty;
    /// <summary>
    /// the id of the cheep the follow button is related to (the cheep where the follow button is by)
    /// </summary>
    public int CheepId { get; set; }
    
    /// <summary>
    /// The indicator of if a person is already following this author or not
    /// </summary>
    public bool IsFollowing;

    /// <summary>
    /// Initializes a new instance of the <see cref="FollowButtonViewModel"/> class
    /// </summary>
    /// <param name="cheepId">The id of the cheep where the follow button is placed</param>
    /// <param name="author">The author name to follow or unfollow</param>
    /// <param name="isFollowing">The indicator of whether the current user is already following current author</param>
    public FollowButtonViewModel(int cheepId, string author, bool isFollowing)
    {
        Author = author;
        IsFollowing = isFollowing;
        CheepId=  cheepId;
    }
}