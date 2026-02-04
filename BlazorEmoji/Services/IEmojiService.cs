using BlazorEmoji.Models;

namespace BlazorEmoji.Services;

public interface IEmojiService
{
    Task<List<EmojiCategory>> GetAllCategoriesAsync(bool useCompleteDataset);
    Task<List<EmojiCategory>> SearchAsync(string query);
    Task<List<Models.Emoji>> GetRecentAsync();
    Task AddRecentAsync(Models.Emoji emoji);
}