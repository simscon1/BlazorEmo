using BlazorEmo.Models;
using BlazorEmo.Services;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorEmo.Tests.Services;

public class RecentEmoServiceTests
{
    [Fact]
    public async Task AddRecentAsync_ShouldStoreInLocalStorage()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(js => js.InvokeAsync<string>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var service = new RecentEmoService(jsRuntime.Object);
        var emoji = new Emo { Code = "1f600", Char = "😀", Name = "Grinning Face" };

        // Act
        await service.AddRecentAsync(emoji);

        // Assert
        jsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldReturnEmptyList_WhenNoRecents()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(js => js.InvokeAsync<string>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var service = new RecentEmoService(jsRuntime.Object);

        // Act
        var result = await service.GetRecentAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddRecentAsync_ShouldLimitTo24Emojis()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        string? storedValue = null;
        
        jsRuntime.Setup(js => js.InvokeAsync<string>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
        
        jsRuntime.Setup(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) => storedValue = args[1]?.ToString())
            .ReturnsAsync(Task.CompletedTask);

        var service = new RecentEmoService(jsRuntime.Object);

        // Act - Add 25 emojis
        for (int i = 0; i < 25; i++)
        {
            await service.AddRecentAsync(new Emo 
            { 
                Code = $"code{i}", 
                Char = "😀", 
                Name = $"Emoji {i}" 
            });
        }

        // Assert - Should only store 24
        Assert.NotNull(storedValue);
        var stored = System.Text.Json.JsonSerializer.Deserialize<List<Emo>>(storedValue);
        Assert.NotNull(stored);
        Assert.Equal(24, stored.Count);
    }
}