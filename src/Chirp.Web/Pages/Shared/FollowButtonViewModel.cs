namespace Chirp.Web.Pages.Shared;

public class FollowButtonViewModel
{
    public string Author { get; set; } = string.Empty;
    public bool IsFollowing;

    public FollowButtonViewModel(string author, bool isFollowing)
    {
        Author = author;
        IsFollowing = isFollowing;
    }
}
