using BlazorEmoji.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorEmoji.Components;

public partial class EmojiPicker : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<Models.Emoji> OnEmojiSelected { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    
    private string searchQuery = "";
    private string activeTab = "recent";
    private List<EmojiCategory>? _categories;
    private List<EmojiCategory>? _allCategories;

    protected override async Task OnInitializedAsync()
    {
        _allCategories = await EmojiService.GetAllCategoriesAsync();
        await LoadTabContent();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !string.IsNullOrWhiteSpace(searchQuery))
        {
            await UpdateSearch();
        }
        else if (IsOpen && _categories == null)
        {
            await LoadTabContent();
        }
    }
    
    private async Task SelectTab(string tabName)
    {
        activeTab = tabName;
        searchQuery = ""; // Clear search when switching tabs
        await LoadTabContent();
    }
    
    private async Task LoadTabContent()
    {
        if (activeTab == "recent")
        {
            var recentEmojis = await EmojiService.GetRecentAsync();
            _categories = recentEmojis.Any() 
                ? new List<EmojiCategory> { new EmojiCategory { Name = "Recent", Emojis = recentEmojis } }
                : new List<EmojiCategory>();
        }
        else
        {
            _categories = _allCategories?
                .Where(c => c.Name == activeTab)
                .ToList() ?? new List<EmojiCategory>();
        }
    }
    
    private async Task UpdateSearch()
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await LoadTabContent();
        }
        else
        {
            _categories = await EmojiService.SearchAsync(searchQuery);
        }
    }
    
    private async Task SelectEmoji(Models.Emoji emoji)
    {
        await OnEmojiSelected.InvokeAsync(emoji);
        await EmojiService.AddRecentAsync(emoji);
        searchQuery = "";
        
        // Refresh recent tab if it's active
        if (activeTab == "recent")
        {
            await LoadTabContent();
        }
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }
    
    private string GetCategoryIcon(string categoryName)
    {
        return categoryName switch
        {
            "Smileys & Emotion" => "😀",
            "People & Body" => "👋",
            "Animals & Nature" => "🐶",
            "Food & Drink" => "🍔",
            "Travel & Places" => "✈️",
            "Activities" => "⚽",
            "Objects" => "💡",
            "Symbols" => "❤️",
            "Flags" => "🏁",
            _ => "📁"
        };
    }
}
