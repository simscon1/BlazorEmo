using BlazorEmo.Components;
using BlazorEmo.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerDatasetTests : TestContext
{
    [Fact]
    public async Task UseCompleteDataset_ShouldBeFalse_ByDefault()
    {
        // Arrange
        var mockEmoService = new Mock<IEmoService>();
        var mockRecentService = new Mock<IRecentEmoService>();
        
        mockEmoService.Setup(s => s.GetAllCategoriesAsync(false))
            .ReturnsAsync([]);
        mockRecentService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        Services.AddSingleton(mockEmoService.Object);
        Services.AddSingleton(mockRecentService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        await Task.Delay(100);

        // Assert
        Assert.False(cut.Instance.UseCompleteDataset);
        mockEmoService.Verify(s => s.GetAllCategoriesAsync(false), Times.Once);
    }

    [Fact]
    public async Task OnOpened_ShouldFire_WhenIsOpenChangesFromFalseToTrue()
    {
        // Arrange
        var mockEmoService = new Mock<IEmoService>();
        var mockRecentService = new Mock<IRecentEmoService>();
        
        mockEmoService.Setup(s => s.GetAllCategoriesAsync(true))
            .ReturnsAsync([]);
        mockRecentService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        Services.AddSingleton(mockEmoService.Object);
        Services.AddSingleton(mockRecentService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.UseCompleteDataset, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        await Task.Delay(100);

        // Assert
        Assert.True(cut.Instance.UseCompleteDataset);
        mockEmoService.Verify(s => s.GetAllCategoriesAsync(true), Times.Once);
    }
}