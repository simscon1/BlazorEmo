using BlazorEmo.Components;
using BlazorEmo.Models;
using BlazorEmo.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerEventCallbackTests : TestContext
{
    private readonly Mock<IEmoService> _mockEmoService;
    private readonly Mock<IRecentEmoService> _mockRecentEmoService;

    public EmoPickerEventCallbackTests()
    {
        _mockEmoService = new Mock<IEmoService>();
        _mockRecentEmoService = new Mock<IRecentEmoService>();
        
        Services.AddSingleton(_mockEmoService.Object);
        Services.AddSingleton(_mockRecentEmoService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task OnOpened_ShouldFire_WhenIsOpenChangesFromFalseToTrue()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        bool onOpenedFired = false;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnOpened, () => { onOpenedFired = true; }));

        // Act - Re-render with IsOpen = true
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(EmoPicker.IsOpen), true }
            })));

        await Task.Delay(100);

        // Assert
        Assert.True(onOpenedFired);
    }

    [Fact]
    public async Task OnCategoryChanged_ShouldFire_WithCorrectCategoryName()
    {
        // Arrange
        var testCategory = new List<EmoCategory>
        {
            new() { Name = "Smileys & Emotion", Emojis = [] }
        };

        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync(testCategory);
        _mockEmoService.Setup(s => s.LoadCategoryAsync("Smileys & Emotion", It.IsAny<bool>()))
            .ReturnsAsync(testCategory);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        string? capturedCategory = null;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnCategoryChanged, category => { capturedCategory = category; }));

        // Act
        var tabButton = cut.Find("button[data-tab='Smileys & Emotion']");
        await tabButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.Equal("Smileys & Emotion", capturedCategory);
    }

    [Fact]
    public async Task OnSearchChanged_ShouldFire_WhenSearchQueryChanges()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);
        _mockEmoService.Setup(s => s.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync([]);

        string? capturedSearch = null;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnSearchChanged, search => { capturedSearch = search; }));

        // Act
        var searchInput = cut.Find("input#emoji-search");
        await searchInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs 
        { 
            Value = "smile" 
        });

        // Assert
        Assert.Equal("smile", capturedSearch);
    }

    [Fact]
    public async Task OnBeforeClose_ShouldPreventClosing_WhenReturnsFalse()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        bool onCloseFired = false;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnClose, () => { onCloseFired = true; })
            .Add(p => p.OnBeforeClose, () => Task.FromResult(false)));

        // Act
        var backdrop = cut.Find(".emoji-picker-backdrop");
        await backdrop.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.False(onCloseFired);
    }

    [Fact]
    public async Task OnBeforeClose_ShouldAllowClosing_WhenReturnsTrue()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        bool onCloseFired = false;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnClose, () => { onCloseFired = true; })
            .Add(p => p.OnBeforeClose, () => Task.FromResult(true)));

        // Act
        var backdrop = cut.Find(".emoji-picker-backdrop");
        await backdrop.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.True(onCloseFired);
    }

    [Fact]
    public async Task OnEmojiSelected_ShouldFire_WithCorrectEmoji()
    {
        // Arrange
        var testEmoji = new Emo { Name = "Grinning Face", Char = "😀", Code = "1f600" };
        var testCategory = new List<EmoCategory>
        {
            new() { Name = "Recent", Emojis = [testEmoji] }
        };

        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync(testCategory);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([testEmoji]);

        Emo? capturedEmoji = null;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, emoji => { capturedEmoji = emoji; }));

        // Act
        var emojiButton = cut.Find($"button[data-emoji-code='{testEmoji.Code}']");
        await emojiButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.NotNull(capturedEmoji);
        Assert.Equal("1f600", capturedEmoji.Code);
        Assert.Equal("Grinning Face", capturedEmoji.Name);
    }

    [Fact]
    public async Task OnError_ShouldFire_WhenJSInteropFails()
    {
        // Arrange
        _mockEmoService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<bool>()))
            .ReturnsAsync([]);
        _mockRecentEmoService.Setup(s => s.GetRecentAsync())
            .ReturnsAsync([]);

        Exception? capturedError = null;
        JSInterop.Mode = JSRuntimeMode.Strict;
        
        // Setup the module import to throw an exception
        JSInterop.Setup<IJSObjectReference>("import", "./_content/BlazorEmo/emoji-picker.js")
            .SetException(new JSException("Module load failed"));

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnError, error => { capturedError = error; }));

        // Wait for OnAfterRenderAsync
        await Task.Delay(200);

        // Assert
        Assert.NotNull(capturedError);
        Assert.IsType<JSException>(capturedError);
    }
}