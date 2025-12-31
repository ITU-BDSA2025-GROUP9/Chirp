using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.PlaywrightTests.Chirps;

[TestFixture]
public class FollowUnfollowTest : PageTest
{
    [Test]
    public async Task LoginFollowUnfollowTest()
    {
        await Page.GotoAsync(PlaywrightTestBase.AppUrl);

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        var userCard = Page
            .GetByRole(AriaRole.Listitem)
            .Filter(new() { HasText = "testMe" })
            .First;

        if (await userCard.GetByText("Unfollow").CountAsync() > 0)
        {
            await userCard.GetByText("Unfollow").ClickAsync();
        }

        await userCard.GetByText("Follow").ClickAsync();
        await Expect(userCard.GetByText("Unfollow")).ToBeVisibleAsync();

        await userCard.GetByText("Unfollow").ClickAsync();
        await Expect(userCard.GetByText("Follow")).ToBeVisibleAsync();
    }
}