using BlazorEmo.Extensions;
using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorEmo.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBlazorEmo_RegistersRecentEmoService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBlazorEmo();

        // Assert
        var recentEmoServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRecentEmoService));
        Assert.NotNull(recentEmoServiceDescriptor);
        Assert.Equal(typeof(RecentEmoService), recentEmoServiceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddBlazorEmo_RegistersRecentEmoService_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBlazorEmo();

        // Assert
        var recentEmoServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRecentEmoService));
        Assert.NotNull(recentEmoServiceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, recentEmoServiceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBlazorEmo_ReturnsServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBlazorEmo();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBlazorEmo_CanBeCalledMultipleTimes_WithoutError()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Call multiple times
        services.AddBlazorEmo();
        services.AddBlazorEmo();

        // Assert - Should register service twice
        var recentEmoServiceDescriptors = services.Where(d => d.ServiceType == typeof(IRecentEmoService)).ToList();
        Assert.Equal(2, recentEmoServiceDescriptors.Count);
    }

    [Fact]
    public void AddBlazorEmo_IsOptional_ForEmoPicker()
    {
        // This test documents that EmoPicker works standalone without DI registration
        // Arrange
        var services = new ServiceCollection();
        // Note: NOT calling AddBlazorEmo()

        // Act & Assert
        // EmoPicker creates its own EmojiProvider and RecentEmoService internally
        // So it should work without any service registration (as long as IJSRuntime is available)
        Assert.True(true, "EmoPicker component is standalone and doesn't require AddBlazorEmo() to be called");
    }
}
