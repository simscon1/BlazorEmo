using BlazorEmoji.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace BlazorEmoji.Components;

/// <summary>
/// A fully accessible emoji picker component with keyboard navigation and screen reader support.
/// </summary>
public partial class EmojiPicker : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets whether the emoji picker is visible.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Event callback invoked when an emoji is selected.
    /// </summary>
    [Parameter] public EventCallback<Models.Emoji> OnEmojiSelected { get; set; }
    
    /// <summary>
    /// Event callback invoked when the picker is closed.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }
    
    private IJSObjectReference? _jsModule;
    
    private string searchQuery = "";
    private string activeTab = "recent";
    private List<EmojiCategory>? _categories;
    private List<EmojiCategory>? _allCategories;
    private string _screenReaderAnnouncement = "";
    private ElementReference _searchInput;
    private ElementReference _pickerElement;
    private int _focusedEmojiIndex = -1;

    protected override async Task OnInitializedAsync()
    {
        _allCategories = await EmojiService.GetAllCategoriesAsync();
        await LoadTabContent();
        Debug.WriteLine($"[EmojiPicker] Initialized with {_allCategories?.Count ?? 0} categories");
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !string.IsNullOrWhiteSpace(searchQuery))
        {
            await UpdateSearch(); // ✅ Called on every keystroke
        }
        else if (IsOpen && _categories == null)
        {
            await LoadTabContent();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Load module on first render OR when picker opens
        if (IsOpen && _jsModule == null)
        {
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", 
                    "./_content/BlazorEmoji/emoji-picker.js");
                Debug.WriteLine("[EmojiPicker] JS module loaded");
                
                await _jsModule.InvokeVoidAsync("focusById", "emoji-search");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmojiPicker] Module load error: {ex.Message}");
            }
        }
    }
    
    private async Task SelectTab(string tabName)
    {
        Debug.WriteLine($"[EmojiPicker] Selecting tab: {tabName}");
        activeTab = tabName;
        searchQuery = "";
        _focusedEmojiIndex = -1;
        await LoadTabContent();
        await AnnounceToScreenReader($"{tabName} category selected. {GetEmojiCount()} emojis available.");
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
        Debug.WriteLine($"[EmojiPicker] Loaded {_categories?.Count ?? 0} categories for tab '{activeTab}'");
    }
    
    // This should be called automatically when you type
    private async Task UpdateSearch()
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await LoadTabContent(); // Clear search, show current tab
        }
        else
        {
            _categories = await EmojiService.SearchAsync(searchQuery); // ✅ Filter emojis
            var count = GetEmojiCount();
            await AnnounceToScreenReader($"{count} emoji{(count != 1 ? "s" : "")} found for {searchQuery}.");
            Debug.WriteLine($"[EmojiPicker] Search '{searchQuery}' found {count} emojis");
        }
    }
    
    private async Task SelectEmoji(Models.Emoji emoji)
    {
        Debug.WriteLine($"[EmojiPicker] Emoji selected: {emoji.Name} ({emoji.Char})");
        await OnEmojiSelected.InvokeAsync(emoji);
        await EmojiService.AddRecentAsync(emoji);
        await AnnounceToScreenReader($"{emoji.Name} emoji selected.");
        searchQuery = "";
        
        if (activeTab == "recent")
        {
            await LoadTabContent();
        }
    }

    private async Task HandleBackdropClick()
    {
        await OnClose.InvokeAsync();
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmojiPicker] Container key: {e.Key}");
        if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmojiPicker] Search key: {e.Key}");
        if (e.Key == "ArrowDown")
        {
            _focusedEmojiIndex = 0;
            await FocusEmoji(0);
        }
        else if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(searchQuery))
        {
            var firstEmoji = _categories?.FirstOrDefault()?.Emojis.FirstOrDefault();
            if (firstEmoji != null)
            {
                await SelectEmoji(firstEmoji);
            }
        }
        else if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }

    private async Task HandleTabsContainerKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmojiPicker] Tabs container key: {e.Key}");
        var currentTabIndex = GetCurrentTabIndex();
        var allTabs = GetAllTabNames();
        
        switch (e.Key)
        {
            case "ArrowRight":
                if (currentTabIndex < allTabs.Count - 1)
                {
                    var nextTab = allTabs[currentTabIndex + 1];
                    await SelectTab(nextTab);
                    StateHasChanged();
                    await Task.Delay(10); // Allow re-render
                    await FocusTab(currentTabIndex + 1);
                }
                break;
                
            case "ArrowLeft":
                if (currentTabIndex > 0)
                {
                    var prevTab = allTabs[currentTabIndex - 1];
                    await SelectTab(prevTab);
                    StateHasChanged();
                    await Task.Delay(10);
                    await FocusTab(currentTabIndex - 1);
                }
                break;
                
            case "Home":
                var firstTab = allTabs[0];
                await SelectTab(firstTab);
                StateHasChanged();
                await Task.Delay(10);
                await FocusTab(0);
                break;
                
            case "End":
                var lastTab = allTabs[^1];
                await SelectTab(lastTab);
                StateHasChanged();
                await Task.Delay(10);
                await FocusTab(allTabs.Count - 1);
                break;
                
            case "ArrowDown":
            case "Enter":
            case " ":
                await FocusEmoji(0);
                break;

            case "Escape":
                await OnClose.InvokeAsync();
                break;
        }
    }

    private async Task HandleEmojiContainerKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmojiPicker] Emoji container key: {e.Key}");
        
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList() ?? new List<Models.Emoji>();
        if (allEmojis.Count == 0) return;

        var currentIndex = _focusedEmojiIndex >= 0 ? _focusedEmojiIndex : 0;

        switch (e.Key)
        {
            case "ArrowRight":
                if (currentIndex < allEmojis.Count - 1)
                {
                    await FocusEmoji(currentIndex + 1);
                }
                break;
                
            case "ArrowLeft":
                if (currentIndex > 0)
                {
                    await FocusEmoji(currentIndex - 1);
                }
                break;
                
            case "ArrowDown":
                var nextRowIndex = currentIndex + 8;
                if (nextRowIndex < allEmojis.Count)
                {
                    await FocusEmoji(nextRowIndex);
                }
                break;
                
            case "ArrowUp":
                if (currentIndex >= 8)
                {
                    await FocusEmoji(currentIndex - 8);
                }
                else
                {
                    var tabIndex = GetCurrentTabIndex();
                    await FocusTab(tabIndex);
                }
                break;
                
            case "Home":
                await FocusEmoji(0);
                break;
                
            case "End":
                await FocusEmoji(allEmojis.Count - 1);
                break;

            case "Enter":
            case " ":
                if (currentIndex >= 0 && currentIndex < allEmojis.Count)
                {
                    await SelectEmoji(allEmojis[currentIndex]);
                }
                break;

            case "Escape":
                await OnClose.InvokeAsync();
                break;
        }
    }

    private async Task FocusSearch()
    {
        if (_jsModule == null) return;
        
        try
        {
            await _jsModule.InvokeVoidAsync("focusById", "emoji-search");
            Debug.WriteLine("[EmojiPicker] Focused search");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmojiPicker] Focus search error: {ex.Message}");
        }
    }

    private async Task FocusTab(int index)
    {
        var allTabs = GetAllTabNames();
        if (index < 0 || index >= allTabs.Count || _jsModule == null) return;

        var tabName = allTabs[index];
        Debug.WriteLine($"[EmojiPicker] Focusing tab {index}: {tabName}");
        
        try
        {
            await _jsModule.InvokeVoidAsync("focusTabByName", tabName);
            Debug.WriteLine($"[EmojiPicker] Focused tab: {tabName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmojiPicker] Focus tab error: {ex.Message}");
        }
    }

    private async Task FocusEmoji(int index)
    {
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList();
        if (allEmojis == null || allEmojis.Count == 0 || index < 0 || index >= allEmojis.Count || _jsModule == null)
        {
            Debug.WriteLine($"[EmojiPicker] Cannot focus emoji at index {index} (total: {allEmojis?.Count ?? 0})");
            return;
        }

        _focusedEmojiIndex = index;
        var emojiCode = allEmojis[index].Code;
        Debug.WriteLine($"[EmojiPicker] Focusing emoji {index}: {emojiCode}");
        
        try
        {
            await _jsModule.InvokeVoidAsync("focusEmojiByCode", emojiCode);
            Debug.WriteLine($"[EmojiPicker] Focused emoji: {emojiCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmojiPicker] Focus emoji error: {ex.Message}");
        }
    }

    private async Task AnnounceToScreenReader(string message)
    {
        _screenReaderAnnouncement = message;
        StateHasChanged();
        
        await Task.Delay(100);
        _screenReaderAnnouncement = "";
        StateHasChanged();
    }

    private int GetEmojiCount()
    {
        return _categories?.Sum(c => c.Emojis.Count) ?? 0;
    }

    private List<string> GetAllTabNames()
    {
        var tabs = new List<string> { "recent" };
        if (_allCategories != null)
        {
            tabs.AddRange(_allCategories.Select(c => c.Name));
        }
        return tabs;
    }

    private int GetCurrentTabIndex()
    {
        var allTabs = GetAllTabNames();
        return allTabs.IndexOf(activeTab);
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

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
    }

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";
        Debug.WriteLine($"[EmojiPicker] Search input changed to: '{searchQuery}'");
        await UpdateSearch();
    }
}
