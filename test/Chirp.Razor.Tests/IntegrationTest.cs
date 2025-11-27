using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Chirp.Web;

namespace Chirp.Razor.Tests;
public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;

    public IntegrationTest(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
    }

    [Fact]
    public async Task CanSeePublicTimeline()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("Chirp!");
        content.Should().Contain("Public Timeline");
    }
    
    [Theory]
    [InlineData("Helge")]
    [InlineData("Adrian")]
    public async Task CanSeePrivateTimeline(string author)
    {
        var response = await _client.GetAsync($"/{author}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Chirp!");
        content.Should().Contain($"{author}'s Timeline");
    }
    
    [Theory]
    [InlineData("NonExistentUser123")]
    [InlineData("Invalid!User")]
    public async Task CanSeeNotPrivateTimeline_InvalidAuthor(string author)
    {
        var response = await _client.GetAsync($"/{author}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Chirp!");
        content.Should().Contain($"User '{author}' does not exist");
        content.Should().Contain("The user you are trying to view cannot be found.");
    }

    [Theory]
    [InlineData("?page=-1")]
    [InlineData("?page=0")]
    [InlineData("?page=abc")]
    public async Task InvalidPageQuery_ShouldDefaultToPage1(string query)
    {
        var response = await _client.GetAsync($"/{query}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Chirp!");
        content.Should().Contain("Public Timeline");
    }
    
    [Theory]
    [InlineData("Roger Histand", "?page=-1")]
    [InlineData("Roger Histand", "?page=abc")]
    [InlineData("Jacqualine Gilcoine", "?page=0")]
    public async Task ValidAuthorAndInvalidPageQuery_ShouldDefaultToPrivatePage1(string author, string query)
    {
        var response = await _client.GetAsync($"/{author}/{query}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain($"{author}'s Timeline");
        content.Should().NotContain("There are no cheeps so far.");
    }
    
    [Theory]
    [InlineData("NonexistentUser123", "")]
    [InlineData("NonexistentUser123", "?page=1")]
    [InlineData("NonexistentUser123", "?page=-1")]
    [InlineData("NonexistentUser123", "?page=abc")]
    [InlineData("Invalid!User", "?page=0")]
    public async Task InvalidAuthorAndPageQuery_ShouldReturnNotExistTimeline(string author, string query)
    {
        var response = await _client.GetAsync($"/{author}/{query}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Chirp!");
        content.Should().Contain($"User '{author}' does not exist");
        content.Should().Contain("The user you are trying to view cannot be found.");
    }

    [Fact]
    public async Task PublicTimeline_PageBeyond_ShouldReturnEmpty()
    {
        var response = await _client.GetAsync("/?page=999");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("No cheeps on current pagenumber:");
    }
    
    [Fact]
    public async Task PrivateTimeline_ShouldOnlyContainUserCheeps()
    {
        var response = await _client.GetAsync("/Helge");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("Helge's Timeline");
        content.Should().NotContain("Bob");
    }

    [Fact]
    public async Task PublicTimeline_Page2_ShouldNotContainPage1Cheeps()
    {
        var response1 = await _client.GetAsync("/?page=1");
        var response2 = await _client.GetAsync("/?page=2");

        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var page1 = await response1.Content.ReadAsStringAsync();
        var page2 = await response2.Content.ReadAsStringAsync();

        page1.Should().NotBeNullOrEmpty();
        page2.Should().NotBeNullOrEmpty();
        page2.Should().NotContain(page1);
    }
    
    [Fact]
    public async Task PrivateTimeline_Page2_ShouldNotContainPage1Cheeps()
    {
        var response1 = await _client.GetAsync("/Helge?page=1");
        var response2 = await _client.GetAsync("/Helge?page=2");

        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var page1 = await response1.Content.ReadAsStringAsync();
        var page2 = await response2.Content.ReadAsStringAsync();

        page1.Should().NotBeNullOrEmpty();
        page2.Should().NotBeNullOrEmpty();
        page2.Should().NotContain(page1);
    }
    
    [Fact]
    public async Task PublicTimeline_ShouldContainNextButton()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<button> Next </button>");
    }
    
    [Fact]
    public async Task PublicTimeline_Page2_ShouldContainPreviousButton()
    {
        var response = await _client.GetAsync("/?page=2");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<button> Previous </button>");
    }
}