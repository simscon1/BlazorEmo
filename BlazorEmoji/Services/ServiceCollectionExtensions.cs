using BlazorEmoji.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmoji.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmojiServices(this IServiceCollection services)
    {
        services.AddHttpClient<EmojiService>();
        services.AddScoped<IEmojiService>(sp => sp.GetRequiredService<EmojiService>());
        services.AddScoped<IRecentEmojiService, RecentEmojiService>();

        return services;
    }
}