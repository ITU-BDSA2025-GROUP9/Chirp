using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Threading.Tasks;

namespace Chirp.PlaywrightTests.Chirps;

// This was generated awkwardly. It will possibly be replaced

[TestFixture]
public class CheepPostBoxVisibleTest : PageTest
{
    [Test]
    public async Task CheepBoxOnlyAfterLogin()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(PlaywrightTestBase.AppUrl);
        await Assertions.Expect(page.GetByText("What's on your mind test? Share")).Not.ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" })
            .FillAsync("test");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
            .FillAsync("!Test123");
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Assertions.Expect(page.GetByText("What's on your mind test? Share"))
            .ToBeVisibleAsync();
    }
}
