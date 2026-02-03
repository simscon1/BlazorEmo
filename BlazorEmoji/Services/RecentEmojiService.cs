using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorEmoji.Services;

public class RecentEmojiService : IRecentEmojiService
{
    private readonly IJSRuntime _jsRuntime;
    private const int MaxRecentEmojis = 24;
    private const string StorageKey = "blazor-emoji-recents";

    public RecentEmojiService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<Models.Emoji>> GetRecentAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            
            if (string.IsNullOrEmpty(json))
                return new List<Models.Emoji>();

            var emojis = JsonSerializer.Deserialize<List<Models.Emoji>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            return emojis ?? new List<Models.Emoji>();
        }
        catch
        {
            return new List<Models.Emoji>();
        }
    }

    public async Task AddRecentAsync(Models.Emoji emoji)
    {
        var recents = await GetRecentAsync();
        
        // Remove if already exists (to avoid duplicates)
        recents.RemoveAll(e => e.Code == emoji.Code);
        
        // Add to the front of the list
        recents.Insert(0, emoji);
        
        // Keep only the maximum number of recent emojis
        if (recents.Count > MaxRecentEmojis)
        {
            recents.RemoveRange(MaxRecentEmojis, recents.Count - MaxRecentEmojis);
        }
        
        // Save to localStorage
        var json = JsonSerializer.Serialize(recents);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task ClearRecentAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}