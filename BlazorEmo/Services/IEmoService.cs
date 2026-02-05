using BlazorEmo.Models;

namespace BlazorEmo.Services;

public interface IEmoService
{
    Task<List<EmoCategory>> GetAllCategoriesAsync(bool useCompleteDataset);
    
    /// <summary>
    /// Loads a specific category by name (lazy loading).
    /// </summary>
    Task<List<EmoCategory>> LoadCategoryAsync(string categoryName, bool useCompleteDataset);
    
    Task<List<EmoCategory>> SearchAsync(string query);
    Task<List<Emo>> GetRecentAsync();
    Task AddRecentAsync(Emo emoji);
    Task ClearRecentAsync();
}