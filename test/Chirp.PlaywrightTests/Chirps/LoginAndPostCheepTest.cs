using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Chirp.PlaywrightTests.Chirps
{
    [TestFixture]
    public class LoginAndPostCheepTest : PageTest
    {
        [Test]
        public async Task PostAndRemoveCheepFromDatabase()
        {
            const string cheepText = "This is a test";

            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
            var dbPath = Path.Combine(repoRoot, "src/Chirp.Web/App_Data/chirp.db");

            bool cheepCreated = false;

	    using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(PlaywrightTestBase.AppUrl);
                await page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
                await page.GetByRole(AriaRole.Textbox, new() { Name = "Email or Username" }).FillAsync("test");
                await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!Test123");
                await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
                await page.Locator("#Text").FillAsync(cheepText);
                await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
                await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
                await Assertions.Expect(page.GetByText("test This is a test")).ToBeVisibleAsync();

                cheepCreated = true;
            }
            finally
            {
                if (cheepCreated && File.Exists(dbPath))
                {
                    using var connection = new SqliteConnection($"Data Source={dbPath}");
                    await connection.OpenAsync();

                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText =
                        @"DELETE FROM message
                          WHERE text = $text
                            AND author_id IN (
                                SELECT Id FROM AspNetUsers
                                WHERE UserName = $username
                            );";

                    deleteCommand.Parameters.AddWithValue("$text", "This is a test");
                    deleteCommand.Parameters.AddWithValue("$username", "test");

                    await deleteCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
