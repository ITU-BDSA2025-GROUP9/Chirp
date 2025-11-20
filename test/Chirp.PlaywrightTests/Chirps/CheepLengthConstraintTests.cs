using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Threading.Tasks;
using Chirp.PlaywrightTests;

namespace Chirp.PlaywrightTests.Chirps;

// this is a version that will be replaced. It works but was generated awkwardly.

[TestFixture]
public class CheepLengthConstraintTest : PageTest
{
    [Test]
    public async Task CannotPostTooLongCheep()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();
        await page.GotoAsync(PlaywrightTestBase.AppUrl);
        await page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await page.Locator("#Text").ClickAsync();
        await page.Locator("#Text").ClickAsync();
        await page.Locator("#Text").FillAsync("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Assertions.Expect(page.GetByText("The maximum length is 160")).ToBeVisibleAsync();
    }
}
