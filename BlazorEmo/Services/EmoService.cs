using System.Text.Json;
using BlazorEmo.Models;

namespace BlazorEmo.Services;

public class EmoService : IEmoService
{
    private List<EmoCategory>? _categories;
    private readonly HttpClient _httpClient;
    private readonly IRecentEmoService _recentEmoService;
    private bool _useCompleteDataset;

    public EmoService(HttpClient httpClient, IRecentEmoService recentEmoService)
    {
        _httpClient = httpClient;
        _recentEmoService = recentEmoService;
    }

    public async Task<List<EmoCategory>> GetAllCategoriesAsync(bool useCompleteDataset)
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
                $"_content/BlazorEmo/data/{dataset}",
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
            _categories = data?.Categories ?? new List<EmoCategory>();
        }
        return _categories;
    }

    public async Task<List<EmoCategory>> SearchAsync(string query)
    {
        var allCategories = await GetAllCategoriesAsync(_useCompleteDataset);
        
        if (string.IsNullOrWhiteSpace(query))
            return allCategories;

        var searchTerm = query.ToLower();
        var filtered = new List<EmoCategory>();

        foreach (var category in allCategories)
        {
            var matchingEmojis = category.Emojis
                .Where(e => e.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           e.Keywords.Any(k => k.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingEmojis.Any())
            {
                filtered.Add(new EmoCategory 
                { 
                    Name = category.Name, 
                    Emojis = matchingEmojis 
                });
            }
        }

        return filtered;
    }

    public Task<List<Models.Emo>> GetRecentAsync()
    {
        return _recentEmoService.GetRecentAsync();
    }

    public Task AddRecentAsync(Models.Emo emoji)
    {
        return _recentEmoService.AddRecentAsync(emoji);
    }

    private class EmojiData
    {
        public List<EmoCategory> Categories { get; set; } = new();
    }
}