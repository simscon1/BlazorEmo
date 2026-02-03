using BlazorEmoji.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEmoji.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmojiServices(this IServiceCollection services)
    {
        services.AddScoped<IEmojiService, EmojiService>(); 
        
        return services;
    }
}