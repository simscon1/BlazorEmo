using BlazorEmo.Models;
using BlazorEmo.Services;
using Bunit;
using Xunit;

namespace BlazorEmo.Tests.Services;

public class RecentEmoServiceTests : BunitContext
{
    public RecentEmoServiceTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetVoidResult();
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult((string?)null);
    }

    [Fact]
    public async Task AddRecentAsync_ShouldStoreInLocalStorage()
    {
        // Arrange
        var service = new RecentEmoService(JSInterop.JSRuntime);
        var emoji = new Emo { Code = "1f600", Char = "😀", Name = "Grinning Face", Keywords = [] };

        // Act
        await service.AddRecentAsync(emoji);

        // Assert - Should complete without errors
        JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public async Task GetRecentAsync_ShouldReturnEmptyList_WhenNoRecents()
    {
        // Arrange
        var service = new RecentEmoService(JSInterop.JSRuntime);

        // Act
        var result = await service.GetRecentAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddRecentAsync_ShouldAcceptMultipleEmojis()
    {
        // Arrange
        var service = new RecentEmoService(JSInterop.JSRuntime);

        // Act - Add multiple emojis
        for (int i = 0; i < 5; i++)
        {
            await service.AddRecentAsync(new Emo 
            { 
                Code = $"code{i}", 
                Char = $"😀", 
                Name = $"Emoji {i}",
                Keywords = []
            });
        }

        // Assert - Should complete without errors
        JSInterop.VerifyInvoke("localStorage.setItem", 5);
    }

    [Fact]
    public async Task AddRecentAsync_ShouldHandleDuplicates()
    {
        // Arrange
        var service = new RecentEmoService(JSInterop.JSRuntime);
        var emoji1 = new Emo { Code = "1f600", Char = "😀", Name = "Grinning Face", Keywords = [] };
        var emoji2 = new Emo { Code = "1f601", Char = "😁", Name = "Beaming Face", Keywords = [] };

        // Act - Add emoji1, then emoji2, then emoji1 again
        await service.AddRecentAsync(emoji1);
        await service.AddRecentAsync(emoji2);
        await service.AddRecentAsync(emoji1);

        // Assert - Should complete without errors
        JSInterop.VerifyInvoke("localStorage.setItem", 3);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldHandleNullFromLocalStorage()
    {
        // Arrange
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult((string?)null);
        var service = new RecentEmoService(JSInterop.JSRuntime);

        // Act
        var result = await service.GetRecentAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldHandleInvalidJson()
    {
        // Arrange
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult("invalid json");
        var service = new RecentEmoService(JSInterop.JSRuntime);

        // Act
        var result = await service.GetRecentAsync();

        // Assert - Should return empty list on error
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddRecentAsync_ShouldHandleNullEmoji()
    {
        // Arrange
        var service = new RecentEmoService(JSInterop.JSRuntime);

        // Act & Assert - Should handle null gracefully
        await Assert.ThrowsAsync<ArgumentNullException>(async () => 
            await service.AddRecentAsync(null!));
    }
}