namespace BlazorEmoji.Services;

public interface IRecentEmojiService
{
    Task<List<Models.Emoji>> GetRecentAsync();
    Task AddRecentAsync(Models.Emoji emoji);
    Task ClearRecentAsync();
}
