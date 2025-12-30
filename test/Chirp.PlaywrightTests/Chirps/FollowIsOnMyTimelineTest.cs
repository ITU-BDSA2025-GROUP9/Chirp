using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.PlaywrightTests.Chirps;

[TestFixture]
public class FollowOnMyTimelineTests : PageTest
{
    [Test]
    public async Task FollowUserAndSeeTheirCheepsOnMyTimeline()
    {
        await Page.GotoAsync(PlaywrightTestBase.AppUrl);

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        var cheep = Page
            .GetByRole(AriaRole.Listitem)
            .Filter(new() { HasText = "testMe" })
            .First;

        if (await cheep.GetByText("Unfollow").CountAsync() > 0)
        {
            await cheep.GetByText("Unfollow").ClickAsync();
        }

        await cheep.GetByText("Follow").ClickAsync();
        await Expect(cheep.GetByText("Unfollow")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();

        var timelineCheeps = Page
            .GetByRole(AriaRole.Listitem)
            .Filter(new() { HasText = "testMe" });

        Assert.That(await timelineCheeps.CountAsync(), Is.GreaterThan(0));

        await Page.GetByRole(AriaRole.Link, new() { Name = "public" }).ClickAsync();

        var cleanupCheep = Page
            .GetByRole(AriaRole.Listitem)
            .Filter(new() { HasText = "testMe" })
            .First;

        await cleanupCheep.GetByText("Unfollow").ClickAsync();
    }
}