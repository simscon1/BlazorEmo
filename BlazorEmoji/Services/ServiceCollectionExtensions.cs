using BlazorEmoji.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmoji.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmojiServices(this IServiceCollection services, string baseAddress)
    {
        services.AddScoped<IRecentEmojiService, RecentEmojiService>();

        services.AddHttpClient<EmojiService>(client => 
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        services.AddScoped<IEmojiService>(sp => sp.GetRequiredService<EmojiService>());

        return services;
    }
}