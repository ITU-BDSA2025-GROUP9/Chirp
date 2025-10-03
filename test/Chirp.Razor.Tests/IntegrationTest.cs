using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

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
    public async void CanSeePublicTimeline()
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
    public async void CanSeePrivateTimeline(string author)
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
    public async void CanSeePrivateTimeline_InvalidAuthor(string author)
    {
        var response = await _client.GetAsync($"/{author}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().Contain("Chirp!"); 
        content.Should().Contain("There are no cheeps so far."); 
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
    [InlineData("Helge", "?page=-1")]
    [InlineData("Helge", "?page=abc")]
    [InlineData("Adrian", "?page=0")]
    public async void ValidAuthorAndInvalidPageQuery_ShouldDefaultToPrivatePage1(string author, string query)
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
    public async Task InvalidAuthorAndPageQuery_ShouldReturnEmptyTimeline(string author, string query)
    {
        var response = await _client.GetAsync($"/{author}/{query}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain($"{author}'s Timeline");
        content.Should().Contain("There are no cheeps so far."); 
    }
    
}