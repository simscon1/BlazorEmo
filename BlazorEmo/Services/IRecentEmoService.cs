namespace BlazorEmo.Services;

public interface IRecentEmoService
{
    Task<List<Models.Emo>> GetRecentAsync();
    Task AddRecentAsync(Models.Emo emoji);
    Task ClearRecentAsync();
}
