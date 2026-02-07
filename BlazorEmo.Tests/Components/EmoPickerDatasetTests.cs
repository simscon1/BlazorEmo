using BlazorEmo.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorEmo.Tests.Components;

public class EmoPickerDatasetTests : BunitContext
{
    public EmoPickerDatasetTests()
    {
        // Component only needs IJSRuntime (provided by JSInterop)
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult((string?)null);
    }

    [Fact]
    public void UseCompleteDataset_ShouldBeFalse_ByDefault()
    {
        // Arrange & Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        // Assert
        Assert.False(cut.Instance.UseCompleteDataset);
    }

    [Fact]
    public void UseCompleteDataset_ShouldBeTrue_WhenExplicitlySet()
    {
        // Arrange & Act
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.UseCompleteDataset, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        // Assert
        Assert.True(cut.Instance.UseCompleteDataset);
    }

    [Fact]
    public async Task UseCompleteDataset_CanBeChanged_AfterInitialization()
    {
        // Arrange
        var cut = Render<EmoPicker>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnEmojiSelected, _ => { }));

        Assert.False(cut.Instance.UseCompleteDataset);

        // Act - Update parameters using SetParametersAsync
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(EmoPicker.IsOpen), true },
                { nameof(EmoPicker.UseCompleteDataset), true },
                { nameof(EmoPicker.OnEmojiSelected), EventCallback.Factory.Create<Models.Emo>(this, _ => { }) }
            })));
    }
}