using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmo.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BlazorEmo services for recent emoji tracking.
    /// Note: This is OPTIONAL. EmoPicker component works standalone and creates its own services internally.
    /// Only call this if you want to use IRecentEmoService independently in your app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlazorEmo(this IServiceCollection services)
    {
        // Only register recent emoji tracking service
        // EmoPicker component creates its own EmojiProvider and RecentEmoService internally
        services.AddScoped<IRecentEmoService, RecentEmoService>();
        
        return services;
    }
}