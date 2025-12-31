using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.PlaywrightTests.Chirps;

[TestFixture]
public class PaginationButtonsTest : PageTest
{
    [Test]
    public async Task CanNavigateNextAndPreviousPages()
    {
        await Page.GotoAsync(PlaywrightTestBase.AppUrl);

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" })
            .FillAsync("testMe");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
            .FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        for (int i = 0; i < 50; i++)
        {
            await Page.Locator("#Text").FillAsync($"Cheep {i}");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        }

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GotoAsync(PlaywrightTestBase.AppUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Previous" }))
            .ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Previous" }).ClickAsync();
    }
}