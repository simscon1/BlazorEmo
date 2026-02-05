using System.Text.Json;
using BlazorEmo.Models;

namespace BlazorEmo.Services;

public class EmoService : IEmoService
{
    // Static cache shared across all instances
    private static List<EmoCategory>? _cachedBasicCategories;
    private static List<EmoCategory>? _cachedCompleteCategories;
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

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

        // Check static cache first
        var cachedData = useCompleteDataset ? _cachedCompleteCategories : _cachedBasicCategories;
        if (cachedData != null)
        {
            return cachedData;
        }

        // Use lock to prevent multiple simultaneous loads
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            cachedData = useCompleteDataset ? _cachedCompleteCategories : _cachedBasicCategories;
            if (cachedData != null)
            {
                return cachedData;
            }

            string dataset = useCompleteDataset 
                ? "emojis_complete.json"
                : "emojis_basic.json";

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
            
            var categories = data?.Categories ?? new List<EmoCategory>();

            // Cache the result
            if (useCompleteDataset)
            {
                _cachedCompleteCategories = categories;
            }
            else
            {
                _cachedBasicCategories = categories;
            }

            _categories = categories;
            return categories;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Loads a specific category by name (lazy loading optimization).
    /// </summary>
    public async Task<List<EmoCategory>> LoadCategoryAsync(string categoryName, bool useCompleteDataset)
    {
        // Load all categories first (they're cached)
        var allCategories = await GetAllCategoriesAsync(useCompleteDataset);
        
        // Return only the requested category
        return allCategories
            .Where(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
            .ToList();
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

    public Task<List<Emo>> GetRecentAsync()
    {
        return _recentEmoService.GetRecentAsync();
    }

    public Task AddRecentAsync(Emo emoji)
    {
        return _recentEmoService.AddRecentAsync(emoji);
    }

    public Task ClearRecentAsync()
    {
        return _recentEmoService.ClearRecentAsync();
    }

    private class EmojiData
    {
        public List<EmoCategory> Categories { get; set; } = new();
    }
}