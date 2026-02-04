using BlazorEmo.Models;

namespace BlazorEmo.Services;

public interface IEmoService
{
    Task<List<EmoCategory>> GetAllCategoriesAsync(bool useCompleteDataset);
    Task<List<EmoCategory>> SearchAsync(string query);
    Task<List<Models.Emo>> GetRecentAsync();
    Task AddRecentAsync(Models.Emo emoji);
}