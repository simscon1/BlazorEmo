using BlazorEmoji.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Threading;

namespace BlazorEmoji.Components;

/// <summary>
/// A fully accessible emoji picker component with keyboard navigation and screen reader support.
/// </summary>
public partial class EmojiPicker : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets whether to use the complete emoji dataset (1,585 emojis) or basic dataset (60 emojis).
    /// Default is false (Basic dataset for faster loading).
    /// </summary>
    [Parameter] public bool UseCompleteDataset { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether the emoji picker is visible.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Event callback invoked when an emoji is selected.
    /// Passes the selected emoji object containing Name, Char, and Code properties.
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

    // Better
    /// <summary>
    /// The name of the currently hovered or keyboard-focused emoji/tab.
    /// Displayed in the stationary label for user feedback.
    /// </summary>
    private string _hoveredEmojiName = string.Empty;

    /// <summary>
    /// The zero-based index of the currently focused emoji in the flattened emoji list.
    /// -1 indicates no emoji is focused.
    /// </summary>
    private int _focusedEmojiIndex = -1;

    // Add constants
    /// <summary>
    /// Number of columns in the emoji grid layout.
    /// </summary>
    private const int GRID_COLUMN_COUNT = 8;
    private const int RENDER_DELAY_MS = 10;
    private const int SCREEN_READER_DELAY_MS = 100;

    protected override async Task OnInitializedAsync()
    {
        // Load either Basic or Complete dataset based on parameter
        _allCategories = await EmojiService.GetAllCategoriesAsync(UseCompleteDataset);
        await LoadTabContent();
        Debug.WriteLine($"[EmojiPicker] Initialized with {_allCategories?.Count ?? 0} categories ({(UseCompleteDataset ? "Complete" : "Basic")} dataset)");
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

    protected override void OnParametersSet()
    {
        if (!OnEmojiSelected.HasDelegate)
        {
            throw new InvalidOperationException(
                "EmojiPicker requires the OnEmojiSelected callback to be set. " +
                "Please provide a handler for emoji selection events.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && _jsModule == null)
        {
            var cts = new CancellationTokenSource();
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", 
                    cts.Token, // Add cancellation support
                    "./_content/BlazorEmoji/emoji-picker.js");
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
    
    private async Task SelectTab(string tabName)
    {
        Debug.WriteLine($"[EmojiPicker] Selecting tab: {tabName}");
        activeTab = tabName;
        searchQuery = "";
        _focusedEmojiIndex = -1;
        
        // Update the label to show tab name
        _hoveredEmojiName = FormatTabName(tabName);
        
        await LoadTabContent();
        await AnnounceToScreenReader($"Switched to {FormatTabName(tabName)} category. {GetEmojiCount()} emojis available. Press Tab to browse emojis.");
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
        
        // Refresh recent tab if it's active
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
        Debug.WriteLine($"[EmojiPicker] Tabs container key: {e.Key}, Shift: {e.ShiftKey}");
        var currentTabIndex = GetCurrentTabIndex();
        var allTabs = GetAllTabNames();
        
        // Handle Shift+Tab to go back to search
        if (e.Key == "Tab" && e.ShiftKey)
        {
            await FocusSearch();
            return;
        }
        
        switch (e.Key)
        {
            case "ArrowRight":
                if (currentTabIndex < allTabs.Count - 1)
                {
                    var nextTab = allTabs[currentTabIndex + 1];
                    await SelectTab(nextTab);
                    StateHasChanged();
                    await Task.Delay(RENDER_DELAY_MS);
                    await FocusTab(currentTabIndex + 1);
                }
                break;
                
            case "ArrowLeft":
                if (currentTabIndex > 0)
                {
                    var prevTab = allTabs[currentTabIndex - 1];
                    await SelectTab(prevTab);
                    StateHasChanged();
                    await Task.Delay(RENDER_DELAY_MS);
                    await FocusTab(currentTabIndex - 1);
                }
                break;
                
            case "Home":
                var firstTab = allTabs[0];
                await SelectTab(firstTab);
                StateHasChanged();
                await Task.Delay(RENDER_DELAY_MS);
                await FocusTab(0);
                break;
                
            case "End":
                var lastTab = allTabs[^1];
                await SelectTab(lastTab);
                StateHasChanged();
                await Task.Delay(RENDER_DELAY_MS);
                await FocusTab(allTabs.Count - 1);
                break;
                
            case "Tab":  // ✅ Tab key moves to emoji list (this was blocked!)
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
        Debug.WriteLine($"[EmojiPicker] Emoji container key: {e.Key}, Shift: {e.ShiftKey}");
        
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList() ?? new List<Models.Emoji>();
        if (allEmojis.Count == 0) return;

        var currentIndex = _focusedEmojiIndex >= 0 ? _focusedEmojiIndex : 0;

        // Handle Shift+Tab to go back to tabs
        if (e.Key == "Tab" && e.ShiftKey)
        {
            var tabIndex = GetCurrentTabIndex();
            await FocusTab(tabIndex);
            return;
        }

        // ✅ ADD THIS - Handle Tab (forward) to wrap back to search
        if (e.Key == "Tab" && !e.ShiftKey)
        {
            await FocusSearch(); // Complete the focus cycle
            return;
        }

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
                var nextRowIndex = currentIndex + GRID_COLUMN_COUNT;
                if (nextRowIndex < allEmojis.Count)
                {
                    await FocusEmoji(nextRowIndex);
                }
                break;
                
            case "ArrowUp":
                if (currentIndex >= GRID_COLUMN_COUNT)
                {
                    await FocusEmoji(currentIndex - GRID_COLUMN_COUNT);
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
        
        // Update the label to show tab name when navigating with keyboard
        _hoveredEmojiName = FormatTabName(tabName);
        StateHasChanged();
        
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

    private async ValueTask FocusEmoji(int index)
    {
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList();
        if (allEmojis == null || allEmojis.Count == 0 || index < 0 || index >= allEmojis.Count || _jsModule == null)
        {
            Debug.WriteLine($"[EmojiPicker] Cannot focus emoji at index {index} (total: {allEmojis?.Count ?? 0})");
            return;
        }

        _focusedEmojiIndex = index;
        var emoji = allEmojis[index];
        
        // Update the label to show emoji name when navigating with keyboard
        _hoveredEmojiName = emoji.Name;
        StateHasChanged();
        
        var emojiCode = emoji.Code;
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
        
        await Task.Delay(SCREEN_READER_DELAY_MS);
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

    private void OnEmojiMouseEnter(Emoji emoji)
    {
        _hoveredEmojiName = emoji.Name;
    }

    private void OnEmojiMouseLeave()
    {
        _hoveredEmojiName = string.Empty;
    }

    private void OnTabMouseEnter(string tabName)
    {
        _hoveredEmojiName = tabName;
    }

    private void OnTabMouseLeave()
    {
        _hoveredEmojiName = string.Empty;
    }

    // Helper method to format tab names
    private string FormatTabName(string tabName)
    {
        return tabName switch
        {
            "recent" => "Recently Used",
            _ => tabName
        };
    }

    private string GetAriaLabelForContent()
    {
        var count = GetEmojiCount();
        return $"{FormatTabName(activeTab)}, {count} emoji{(count != 1 ? "s" : "")} available";
    }

    private string GetActiveFocusedEmojiId()
    {
        if (_focusedEmojiIndex < 0) return "";
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList();
        if (allEmojis == null || _focusedEmojiIndex >= allEmojis.Count) return "";
        return $"emoji-{allEmojis[_focusedEmojiIndex].Code}";
    }

    private async ValueTask SafeJsInvoke(string method, params object[] args)
    {
        if (_jsModule == null) return;
    
        try
        {
            await _jsModule.InvokeVoidAsync(method, args);
        }
        catch (JSException jsEx)
        {
            Debug.WriteLine($"[EmojiPicker] JS Error in {method}: {jsEx.Message}");
            // Optionally show user-friendly error
        }
        catch (ObjectDisposedException)
        {
            Debug.WriteLine($"[EmojiPicker] Component disposed during {method}");
        }
    }

    /// <summary>
    /// Calculates the total number of rows needed for the emoji grid.
    /// </summary>
    /// <param name="emojiCount">Total number of emojis in the category.</param>
    /// <returns>The number of rows required (1-based).</returns>
    private int GetRowCount(int emojiCount)
    {
        return (int)Math.Ceiling(emojiCount / (double)GRID_COLUMN_COUNT);
    }

    /// <summary>
    /// Calculates the row index for an emoji in the grid (1-based for ARIA).
    /// </summary>
    /// <param name="emojiIndex">Zero-based index of the emoji.</param>
    /// <returns>1-based row index.</returns>
    private int GetRowIndex(int emojiIndex)
    {
        return (emojiIndex / GRID_COLUMN_COUNT) + 1; // ARIA uses 1-based indexing
    }

    /// <summary>
    /// Calculates the column index for an emoji in the grid (1-based for ARIA).
    /// </summary>
    /// <param name="emojiIndex">Zero-based index of the emoji.</param>
    /// <returns>1-based column index.</returns>
    private int GetColIndex(int emojiIndex)
    {
        return (emojiIndex % GRID_COLUMN_COUNT) + 1; // ARIA uses 1-based indexing
    }
}
