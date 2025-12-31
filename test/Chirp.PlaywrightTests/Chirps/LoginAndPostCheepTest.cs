using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Chirp.PlaywrightTests.Chirps
{
    [TestFixture]
    public class LoginAndPostCheepTest : PageTest
    {
        [Test]
        public async Task PostAndRemoveCheepFromDatabase()
        {
            const string cheepText = "This is a test";
            await Page.GotoAsync(PlaywrightTestBase.AppUrl);
            await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" })
                .FillAsync("test");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
                .FillAsync("!Test123");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Page.Locator("#Text").FillAsync(cheepText);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            await Expect(
                Page.GetByText("test Delete This is a test")
            ).ToBeVisibleAsync();
        }
    }
}