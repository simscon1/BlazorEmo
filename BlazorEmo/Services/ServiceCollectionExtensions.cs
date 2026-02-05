using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmo.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BlazorEmo services for optimal performance and state sharing.
    /// OPTIONAL: Component works without this, but registration provides better performance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="baseAddress">Optional base address for HTTP client. If null, uses relative paths.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmoServices(this IServiceCollection services, string? baseAddress = null)
    {
        // Register HttpClient if not already registered (for Blazor Server compatibility)
        if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
        {
            services.AddScoped<HttpClient>(sp =>
            {
                if (!string.IsNullOrEmpty(baseAddress))
                {
                    return new HttpClient { BaseAddress = new Uri(baseAddress) };
                }
                
                // For Blazor Server, use NavigationManager to get base URI
                var navigationManager = sp.GetService<Microsoft.AspNetCore.Components.NavigationManager>();
                if (navigationManager != null)
                {
                    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
                }
                
                // Fallback for scenarios without NavigationManager
                return new HttpClient();
            });
        }
        
        // Register as Scoped for Blazor Server compatibility (IJSRuntime is scoped)
        services.AddScoped<IRecentEmoService, RecentEmoService>();

        // Configure HttpClient for EmoService
        services.AddHttpClient<EmoService>();
        
        // Register as Scoped to match RecentEmoService and IJSRuntime lifetime
        services.AddScoped<IEmoService, EmoService>();

        return services;
    }
}