namespace Chirp.Web.Pages.Shared;

public class FollowButtonViewModel
{
    public string Author { get; set; } = string.Empty;
    public int CheepId { get; set; }
    public bool IsFollowing;

    public FollowButtonViewModel(int cheepId, string author, bool isFollowing)
    {
        Author = author;
        IsFollowing = isFollowing;
        CheepId=  cheepId;
    }
}