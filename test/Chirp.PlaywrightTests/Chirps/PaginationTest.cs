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
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Previous" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Previous" }).ClickAsync();
    }
}

