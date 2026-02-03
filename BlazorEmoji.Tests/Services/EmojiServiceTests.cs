using Xunit;
using BlazorEmoji.Services;

public class EmojiServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingEmojis()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new EmojiService(httpClient, new RecentEmojiService(Mock.Of<IJSRuntime>()));
        
        // Act
        var results = await service.SearchAsync("smile");
        
        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results.SelectMany(c => c.Emojis), e => e.Name.Contains("smile", StringComparison.OrdinalIgnoreCase));
    }
}