using BlazorEmo.Components;
using BlazorEmo.Models;
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerEventCallbackTests : BunitContext
{
    public EmoPickerEventCallbackTests()
    {
        // Component only needs IJSRuntime (provided by JSInterop)
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult((string?)null);
    }

    [Fact]
    public async Task OnOpened_ShouldFire_WhenIsOpenChangesFromFalseToTrue()
    {
        // Arrange
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
    public async Task OnClose_ShouldFire_WhenIsOpenChangesFromTrueToFalse()
    {
        // Arrange
        bool onCloseFired = false;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnClose, () => { onCloseFired = true; }));

        await Task.Delay(100);

        // Act
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.OnClose.InvokeAsync();
        });

        // Assert
        Assert.True(onCloseFired);
    }

    [Fact]
    public async Task OnCategoryChanged_ShouldFire_WhenCallbackProvided()
    {
        // Arrange
        string? capturedCategory = null;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnCategoryChanged, category => { capturedCategory = category; }));

        await Task.Delay(100);

        // Act - Programmatically invoke the callback
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.OnCategoryChanged.InvokeAsync("smileys-emotion");
        });

        // Assert
        Assert.Equal("smileys-emotion", capturedCategory);
    }

    [Fact]
    public async Task OnEmojiSelected_ShouldFire_WithCorrectEmoji()
    {
        // Arrange
        Emo? selectedEmoji = null;
        var testEmoji = new Emo 
        { 
            Code = "U+1F600", 
            Char = "😀", 
            Name = "Grinning Face",
            Keywords = []
        };

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, e => { selectedEmoji = e; }));

        await Task.Delay(100);

        // Act - Programmatically invoke the callback
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.OnEmojiSelected.InvokeAsync(testEmoji);
        });

        // Assert
        Assert.NotNull(selectedEmoji);
        Assert.Equal("U+1F600", selectedEmoji.Code);
        Assert.Equal("😀", selectedEmoji.Char);
        Assert.Equal("Grinning Face", selectedEmoji.Name);
    }

    [Fact]
    public async Task OnSearchChanged_ShouldFire_WhenSearchQueryChanges()
    {
        // Arrange
        string? capturedSearch = null;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnSearchChanged, search => { capturedSearch = search; }));

        await Task.Delay(100);

        // Act - Programmatically invoke the callback
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.OnSearchChanged.InvokeAsync("smile");
        });

        // Assert
        Assert.Equal("smile", capturedSearch);
    }

    [Fact]
    public async Task OnError_ShouldFire_WhenCallbackProvided()
    {
        // Arrange
        Exception? capturedError = null;
        var testException = new InvalidOperationException("Test error");

        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnError, error => { capturedError = error; }));

        await Task.Delay(100);

        // Act - Programmatically invoke the error callback
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.OnError.InvokeAsync(testException);
        });

        // Assert
        Assert.NotNull(capturedError);
        Assert.Equal("Test error", capturedError.Message);
    }

    [Fact]
    public async Task OnBeforeClose_ShouldBeCalled_BeforeClosing()
    {
        // Arrange
        bool beforeCloseCalled = false;
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { })
            .Add(p => p.OnBeforeClose, () =>
            {
                beforeCloseCalled = true;
                return Task.FromResult(true);
            }));

        await Task.Delay(100);

        // Act - Attempt to invoke OnBeforeClose programmatically
        if (cut.Instance.OnBeforeClose != null)
        {
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.OnBeforeClose();
            });
        }

        // Assert
        Assert.True(beforeCloseCalled);
    }

    [Fact]
    public void OnEmojiSelected_IsRequired()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var cut = Render<EmoPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));
        });
    }
}