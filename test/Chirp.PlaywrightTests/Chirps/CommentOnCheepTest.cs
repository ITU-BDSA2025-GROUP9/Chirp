using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.PlaywrightTests.Chirps;

[TestFixture]
public class CommentOnCheepTest : PageTest
{
    [Test]
    public async Task CanCommentAndDeleteComment()
    {
        await Page.GotoAsync(PlaywrightTestBase.AppUrl);

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("testMe");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Page.Locator("#Text").FillAsync("new cheep");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "logout [testMe]" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Click here to Logout" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Follow" }).First.ClickAsync();

        var cheep = Page.GetByRole(AriaRole.Listitem)
            .Filter(new() { HasText = "testMe" })
            .First;

        await cheep.GetByPlaceholder("Write a comment...").FillAsync("Hello");
        await cheep.GetByRole(AriaRole.Button, new() { Name = "Comment" }).ClickAsync();

        await cheep.GetByText("Comments (1)").ClickAsync();
        await Expect(cheep.GetByText("test Hello")).ToBeVisibleAsync();

        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        await cheep.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        await Expect(cheep.GetByText("Comments (0)")).ToBeVisibleAsync();
    }
}
