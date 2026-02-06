using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmo.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds BlazorEmo services to the service collection.
        /// This MUST be called in Program.cs for BlazorEmo components to work.
        /// </summary>
        public static IServiceCollection AddBlazorEmo(this IServiceCollection services)
        {
            // Register services in the correct order to satisfy dependencies
            services.AddScoped<IRecentEmoService, RecentEmoService>();
            services.AddScoped<IEmoService, EmoService>();
            
            return services;
        }
    }
}