using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BlazorEmo.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BlazorEmo services for optimal performance and state sharing.
    /// Works in both Blazor Server and Blazor WebAssembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlazorEmo(this IServiceCollection services)
    {
        // Register recent emoji tracking service
        services.AddScoped<IRecentEmoService, RecentEmoService>();
        
        // Register emoji service with factory to handle NavigationManager in scoped context
        services.AddScoped<IEmoService>(sp =>
        {
            // Get or create HttpClient
            var httpClientFactory = sp.GetService<IHttpClientFactory>();
            HttpClient httpClient;
            
            if (httpClientFactory != null)
            {
                httpClient = httpClientFactory.CreateClient("BlazorEmo");
            }
            else
            {
                // Fallback if HttpClientFactory is not available (shouldn't happen in modern Blazor)
                httpClient = new HttpClient();
            }
            
            // Set base address from NavigationManager (available in scoped context)
            if (httpClient.BaseAddress == null)
            {
                var navManager = sp.GetService<NavigationManager>();
                if (navManager != null)
                {
                    httpClient.BaseAddress = new Uri(navManager.BaseUri);
                }
            }
            
            var recentService = sp.GetRequiredService<IRecentEmoService>();
            return new EmoService(httpClient, recentService);
        });
        
        // Register named HttpClient for BlazorEmo
        services.AddHttpClient("BlazorEmo");
        
        return services;
    }
}