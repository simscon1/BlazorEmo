using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorEmo.Services;

public class RecentEmoService : IRecentEmoService
{
    private readonly IJSRuntime _jsRuntime;
    private const int MaxRecentEmojis = 30;
    private const string StorageKey = "blazoremo_recent";

    public RecentEmoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<Models.Emo>> GetRecentAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            
            if (string.IsNullOrEmpty(json))
                return new List<Models.Emo>();

            var emojis = JsonSerializer.Deserialize<List<Models.Emo>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            return emojis ?? new List<Models.Emo>();
        }
        catch
        {
            return new List<Models.Emo>();
        }
    }

    public async Task AddRecentAsync(Models.Emo emoji)
    {
        ArgumentNullException.ThrowIfNull(emoji);

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