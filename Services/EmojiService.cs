using System.Text.Json;
using BlazorEmoji.Models;

namespace BlazorEmoji.Services;

public class EmojiService : IEmojiService
{
    private List<EmojiCategory>? _categories;
    private readonly HttpClient _httpClient;
    private readonly IRecentEmojiService _recentEmojiService;

    public EmojiService(HttpClient httpClient, IRecentEmojiService recentEmojiService)
    {
        _httpClient = httpClient;
        _recentEmojiService = recentEmojiService;
    }

    public async Task<List<EmojiCategory>> GetAllCategoriesAsync()
    {
        if (_categories == null)
        {
            var json = await _httpClient.GetStringAsync("_content/BlazorEmoji/data/emojis.json");
            var data = JsonSerializer.Deserialize<EmojiData>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            _categories = data?.Categories ?? new List<EmojiCategory>();
        }
        return _categories;
    }

    public async Task<List<EmojiCategory>> SearchAsync(string query)
    {
        var allCategories = await GetAllCategoriesAsync();
        
        if (string.IsNullOrWhiteSpace(query))
            return allCategories;

        var searchTerm = query.ToLower();
        var filtered = new List<EmojiCategory>();

        foreach (var category in allCategories)
        {
            var matchingEmojis = category.Emojis
                .Where(e => e.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           e.Keywords.Any(k => k.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingEmojis.Any())
            {
                filtered.Add(new EmojiCategory 
                { 
                    Name = category.Name, 
                    Emojis = matchingEmojis 
                });
            }
        }

        return filtered;
    }

    public Task<List<Models.Emoji>> GetRecentAsync()
    {
        return _recentEmojiService.GetRecentAsync();
    }

    public Task AddRecentAsync(Models.Emoji emoji)
    {
        return _recentEmojiService.AddRecentAsync(emoji);
    }

    private class EmojiData
    {
        public List<EmojiCategory> Categories { get; set; } = new();
    }
}