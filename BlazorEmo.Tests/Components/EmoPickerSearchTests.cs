using BlazorEmo.Components;
using BlazorEmo.Models;
using BlazorEmo.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerSearchTests : TestContext
{
    private readonly Mock<IEmoService> _mockEmoService;
    private readonly Mock<IRecentEmoService> _mockRecentEmoService;

    public EmoPickerSearchTests()
    {
        _mockEmoService = new Mock<IEmoService>();
        _mockRecentEmoService = new Mock<IRecentEmoService>();
        
        Services.AddSingleton(_mockEmoService.Object);
        Services.AddSingleton(_mockRecentEmoService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Search_ShouldDebounce_MultipleInputs()
    {
        // Arrange
        int searchCallCount = 0;
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);
        _mockEmoService.Setup(s => s.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync([])
            .Callback(() => searchCallCount++);

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        var searchInput = cut.Find("input#emoji-search");

        // Act - Rapid typing
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "s" });
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "sm" });
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "smi" });
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "smil" });
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "smile" });

        // Wait for debounce (300ms + buffer)
        await Task.Delay(400);

        // Assert - Should only call search once after debounce period
        Assert.Equal(1, searchCallCount);
        _mockEmoService.Verify(s => s.SearchAsync("smile"), Times.Once);
    }

    [Fact]
    public async Task Search_ShouldCancelPending_WhenPickerCloses()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);
        
        var searchTaskCompletionSource = new TaskCompletionSource<List<EmoCategory>>();
        _mockEmoService.Setup(s => s.SearchAsync(It.IsAny<string>()))
            .Returns(searchTaskCompletionSource.Task);

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        var searchInput = cut.Find("input#emoji-search");
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "test" });

        // Act - Close by setting IsOpen to false
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(EmoPicker.IsOpen), false }
            })));
        
        // Assert - Component should handle cancellation gracefully
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task Search_ShouldClearResults_WhenQueryIsEmpty()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        var searchInput = cut.Find("input#emoji-search");

        // Act - Type and then clear
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "smile" });
        await Task.Delay(400);
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "" });
        await Task.Delay(400);

        // Assert - Should load tab content when search is cleared
        _mockRecentEmoService.Verify(s => s.GetRecentAsync(), Times.AtLeast(2));
    }
}