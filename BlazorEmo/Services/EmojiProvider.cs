using BlazorEmo.Data;
using BlazorEmo.Models;

namespace BlazorEmo.Services
{
    public class EmojiProvider
    {
        public Task<List<EmoCategory>> GetAllCategoriesAsync()
        {
            return Task.FromResult(EmojiData.Categories);
        }

        public Task<EmoCategory?> GetCategoryAsync(string categoryName)
        {
            var category = EmojiData.Categories.FirstOrDefault(c => 
                c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(category);
        }

        public Task<List<Emo>> SearchAsync(string query)
        {
            var results = EmojiData.Categories
                .SelectMany(c => c.Emojis)
                .Where(e => 
                    e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            return Task.FromResult(results);
        }
    }
}