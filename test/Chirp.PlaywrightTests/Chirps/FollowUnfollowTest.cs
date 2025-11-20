using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.PlaywrightTests.Chirps;

[TestFixture]
public class FollowUnfollowTest : PageTest
{
    [Test]
    public async Task CanFollowAndUnfollow()
    {
        await Page.GotoAsync(PlaywrightTestBase.AppUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        var followButton = Page.GetByRole(AriaRole.Button, new() { Name = "Follow" }).First;
        await followButton.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).First).ToBeVisibleAsync();
        var unfollowButton = Page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).First;
        await unfollowButton.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Follow" }).First).ToBeVisibleAsync();
    }
}
