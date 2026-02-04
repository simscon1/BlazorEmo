using System.Text.Json;
using BlazorEmoji.Models;

namespace BlazorEmoji.Services;

public class EmojiService : IEmojiService
{
    private List<EmojiCategory>? _categories;
    private readonly HttpClient _httpClient;
    private readonly IRecentEmojiService _recentEmojiService;
    private bool _useCompleteDataset;

    public EmojiService(HttpClient httpClient, IRecentEmojiService recentEmojiService)
    {
        _httpClient = httpClient;
        _recentEmojiService = recentEmojiService;
    }

    public async Task<List<EmojiCategory>> GetAllCategoriesAsync(bool useCompleteDataset)
    {
        _useCompleteDataset = useCompleteDataset;

        // ✅ FIXED: Match the actual file names (lowercase e, underscores)
        string dataset = useCompleteDataset 
            ? "emojis_complete.json"  // Changed from Emojis.Complete.json
            : "emojis_basic.json";     // Changed from Emojis.Basic.json

        if (_categories == null)
        {
            // Try RCL path first, fallback to local path
            string[] possiblePaths = 
            [
                $"_content/BlazorEmoji/data/{dataset}",
                $"data/{dataset}"
            ];

            string? json = null;
            foreach (var path in possiblePaths)
            {
                try
                {
                    json = await _httpClient.GetStringAsync(path);
                    break;
                }
                catch (HttpRequestException)
                {
                    continue;
                }
            }

            if (json == null)
            {
                throw new FileNotFoundException($"Could not find {dataset} in any expected location");
            }

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
        var allCategories = await GetAllCategoriesAsync(_useCompleteDataset);
        
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