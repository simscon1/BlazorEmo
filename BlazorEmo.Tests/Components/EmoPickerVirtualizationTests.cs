using BlazorEmo.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerVirtualizationTests : BunitContext
{
    public EmoPickerVirtualizationTests()
    {
        // Component only needs IJSRuntime (provided by JSInterop)
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult((string?)null);
    }

    [Fact]
    public void UseVirtualization_ShouldBeTrue_ByDefault()
    {
        // Arrange & Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        // Assert
        Assert.True(cut.Instance.UseVirtualization);
    }

    [Fact]
    public void UseVirtualization_ShouldBeFalse_WhenExplicitlyDisabled()
    {
        // Arrange & Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.UseVirtualization, false)
            .Add(p => p.OnEmojiSelected, _ => { }));

        // Assert
        Assert.False(cut.Instance.UseVirtualization);
    }

    [Fact]
    public async Task UseVirtualization_CanBeToggled_AfterInitialization()
    {
        // Arrange
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        Assert.True(cut.Instance.UseVirtualization);

        // Act - Update parameters using SetParametersAsync
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(EmoPicker.IsOpen), true },
                { nameof(EmoPicker.UseVirtualization), false },
                { nameof(EmoPicker.OnEmojiSelected), EventCallback.Factory.Create<Models.Emo>(this, _ => { }) }
            })));
    }
}