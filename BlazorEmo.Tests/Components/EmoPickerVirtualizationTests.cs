using BlazorEmo.Components;
using BlazorEmo.Models;
using BlazorEmo.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerVirtualizationTests : TestContext
{
    [Fact]
    public void UseVirtualization_ShouldBeTrue_ByDefault()
    {
        // Arrange
        var mockEmoService = new Mock<IEmoService>();
        var mockRecentService = new Mock<IRecentEmoService>();
        
        mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
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

        // Assert
        Assert.True(cut.Instance.UseVirtualization);
    }

    [Fact]
    public void UseVirtualization_ShouldBeFalse_WhenExplicitlyDisabled()
    {
        // Arrange
        var mockEmoService = new Mock<IEmoService>();
        var mockRecentService = new Mock<IRecentEmoService>();
        
        mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        mockRecentService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        Services.AddSingleton(mockEmoService.Object);
        Services.AddSingleton(mockRecentService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.UseVirtualization, false)
            .Add(p => p.OnEmojiSelected, _ => { }));

        // Assert
        Assert.False(cut.Instance.UseVirtualization);
    }
}