using BlazorEmo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmo.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmoServices(this IServiceCollection services, string baseAddress)
    {
        services.AddScoped<IRecentEmoService, RecentEmoService>();

        services.AddHttpClient<EmoService>(client => 
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        services.AddScoped<IEmoService>(sp => sp.GetRequiredService<EmoService>());

        return services;
    }
}