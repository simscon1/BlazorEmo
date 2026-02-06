using BlazorEmo.Models;
using BlazorEmo.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace BlazorEmo.Components;

/// <summary>
/// A fully accessible emoji picker component with keyboard navigation and screen reader support.
/// Works standalone - NO service registration required!
/// </summary>
public partial class EmoPicker : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ✅ Simple providers - no DI, no HttpClient, no complexity!
    private readonly EmojiProvider _emojiProvider = new();
    private RecentEmoService? _recentService;

    /// <summary>
    /// Gets or sets a value indicating whether the complete dataset is used for processing.
    /// </summary>
    [Parameter] public bool UseCompleteDataset { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently open.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when an emoji is selected.
    /// </summary>
    [Parameter] public EventCallback<Models.Emo> OnEmojiSelected { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the component is closed.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the component is opened.
    /// </summary>
    [Parameter] public EventCallback OnOpened { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected category changes.
    /// </summary>
    [Parameter] public EventCallback<string> OnCategoryChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the search text changes.
    /// </summary>
    [Parameter] public EventCallback<string> OnSearchChanged { get; set; }

    /// <summary>
    /// Gets or sets a callback function that is invoked before the component is closed.
    /// </summary>
    [Parameter] public Func<Task<bool>>? OnBeforeClose { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when an error occurs.
    /// </summary>
    [Parameter] public EventCallback<Exception> OnError { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether virtualization is used for emoji rendering.
    /// Defaults to true for optimal performance with large datasets.
    /// </summary>
    [Parameter] public bool UseVirtualization { get; set; } = true;

    private IJSObjectReference? _jsModule;
    private string searchQuery = "";
    private string activeTab = "recent";
    private List<EmoCategory>? _categories;
    private List<EmoCategory>? _allCategories;
    private string _screenReaderAnnouncement = "";
    private ElementReference _searchInput;
    private ElementReference _pickerElement;
    private string _hoveredEmojiName = string.Empty;
    private int _focusedEmojiIndex = -1;
    private const int GRID_COLUMN_COUNT = 8;
    private const int RENDER_DELAY_MS = 10;
    private const int SCREEN_READER_DELAY_MS = 100;
    private const int SEARCH_DEBOUNCE_MS = 300;
    private bool _wasOpen = false;
    private ElementReference _scrollContainer;
    private List<Models.Emo>? _allEmojisCache;

    private CancellationTokenSource? _searchCts;
    private System.Timers.Timer? _debounceTimer;
    private bool _isSearching = false;

    private Dictionary<string, List<EmoCategory>> _categoryCache = new Dictionary<string, List<EmoCategory>>();

    protected override async Task OnInitializedAsync()
    {
        // Initialize RecentEmoService (only needs IJSRuntime)
        _recentService = new RecentEmoService(JS);
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        // Use EmojiProvider - no HTTP calls, no services!
        _allCategories = await _emojiProvider.GetAllCategoriesAsync();
        
        await LoadTabContent();
        
        Debug.WriteLine($"[EmoPicker] Initialized with {_allCategories?.Count ?? 0} categories");
    }

    // Update OnParametersSetAsync to track when picker opens
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !_wasOpen)
        {
            await OnOpened.InvokeAsync();
            _wasOpen = true;
        }
        else if (!IsOpen && _wasOpen)
        {
            _wasOpen = false;
            // ✅ NEW: Cancel pending operations when closing
            CancelPendingSearch();
        }
        
        if (IsOpen && !string.IsNullOrWhiteSpace(searchQuery))
        {
            await UpdateSearchImmediate(); // Already typed, no debounce
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
                "EmoPicker requires the OnEmojiSelected callback to be set. " +
                "Please provide a handler for emoji selection events.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && _jsModule == null)
        {
            try
            {
                _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                    "import", 
                    "./_content/BlazorEmo/emoji-picker.js");
                
                Debug.WriteLine("[EmoPicker] JS module loaded");
                
                // Focus search input
                await _jsModule.InvokeVoidAsync("focusById", "emoji-search");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmoPicker] Module load error: {ex.Message}");
                await OnError.InvokeAsync(ex);
            }
        }

        if (IsOpen && firstRender)
        {
            await Task.Delay(50); // Give time for animations
            await FocusSearch();
        }
    }

    private async Task SelectTab(string tabName)
    {
        Debug.WriteLine($"[EmoPicker] Selecting tab: {tabName}");
        activeTab = tabName;
        searchQuery = "";
        _focusedEmojiIndex = -1;
        _hoveredEmojiName = FormatTabName(tabName);

        // ✅ Lazy load category on demand
        if (tabName == "recent")
        {
            await LoadTabContent(); // Special handling for recent
        }
        else if (_categoryCache.TryGetValue(tabName, out var cached))
        {
            _categories = cached;
        }
        else
        {
            // Use EmojiProvider.GetCategoryAsync
            var category = await _emojiProvider.GetCategoryAsync(tabName);
            _categories = category != null ? new List<EmoCategory> { category } : new List<EmoCategory>();
            _categoryCache[tabName] = _categories;
            _allEmojisCache = null; // Invalidate emoji cache
        }

        await AnnounceToScreenReader($"Switched to {FormatTabName(tabName)} category. {GetEmojiCount()} emojis available. Press Tab to browse emojis.");
        await OnCategoryChanged.InvokeAsync(tabName);
    }

    private async Task LoadTabContent()
    {
        if (_recentService == null) return;

        if (activeTab == "recent")
        {
            var recentEmojis = await _recentService.GetRecentAsync();
            _categories = recentEmojis.Any()
                ? new List<EmoCategory> { new EmoCategory { Name = "Recent", Emojis = recentEmojis } }
                : new List<EmoCategory>();
        }
        else
        {
            _categories = _allCategories?
                .Where(c => c.Name == activeTab)
                .ToList() ?? new List<EmoCategory>();
        }
        
        // ✅ ADD THIS LINE
        _allEmojisCache = null;
        
        Debug.WriteLine($"[EmoPicker] Loaded {_categories?.Count ?? 0} categories for tab '{activeTab}'");
    }

    // ✅ NEW: Immediate search (no debounce)
    private async Task UpdateSearchImmediate()
    {
        CancelPendingSearch();
        await PerformSearch();
    }

    // ✅ NEW: Debounced search
    private void UpdateSearchDebounced()
    {
        CancelPendingSearch();
        
        _debounceTimer = new System.Timers.Timer(SEARCH_DEBOUNCE_MS);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += async (s, e) =>
        {
            await InvokeAsync(async () =>
            {
                await PerformSearch();
            });
        };
        _debounceTimer.Start();
    }

    // ✅ NEW: Actual search logic
    private async Task PerformSearch()
    {
        if (_isSearching) return;
        
        _isSearching = true;
        _searchCts = new CancellationTokenSource();
        
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await LoadTabContent();
            }
            else
            {
                // Use EmojiProvider.SearchAsync
                var results = await _emojiProvider.SearchAsync(searchQuery);
                _categories = results.Any() 
                    ? new List<EmoCategory> { new EmoCategory { Name = "Search Results", Emojis = results } }
                    : new List<EmoCategory>();
                
                // ✅ ADD THIS LINE
                _allEmojisCache = null;
                
                var count = GetEmojiCount();
                await AnnounceToScreenReader($"{count} emoji{(count != 1 ? "s" : "")} found for {searchQuery}.");
                Debug.WriteLine($"[EmoPicker] Search '{searchQuery}' found {count} emojis");
            }
            
            StateHasChanged();
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("[EmoPicker] Search cancelled");
        }
        finally
        {
            _isSearching = false;
        }
    }

    // ✅ NEW: Cancel helper
    private void CancelPendingSearch()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = null;
        
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = null;
    }

    private async Task SelectEmoji(Models.Emo emoji)
    {
        Debug.WriteLine($"[EmoPicker] Emoji selected: {emoji.Name} ({emoji.Char})");
        await OnEmojiSelected.InvokeAsync(emoji);
        
        if (_recentService != null)
        {
            await _recentService.AddRecentAsync(emoji);
        }
        
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
        await ClosePickerAsync();
    }

    private async Task ClosePickerAsync()
    {
        // Check if close is allowed
        if (OnBeforeClose != null)
        {
            bool canClose = await OnBeforeClose.Invoke();
            if (!canClose)
            {
                Debug.WriteLine("[EmoPicker] Close cancelled by OnBeforeClose");
                return;
            }
        }

        await OnClose.InvokeAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmoPicker] Container key: {e.Key}");
        if (e.Key == "Escape")
        {
            await ClosePickerAsync(); // Use new method
        }
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmoPicker] Search key: {e.Key}");
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
        Debug.WriteLine($"[EmoPicker] Tabs container key: {e.Key}, Shift: {e.ShiftKey}");
        var currentTabIndex = GetCurrentTabIndex();
        var allTabs = GetAllTabNames();

        // Handle Shift+Tab to go back to search
        if (e.Key == "Tab" && e.ShiftKey)
        {
            await FocusSearch();
            return;
        }

        // ✅ OPTIMIZED: Batch state updates
        bool needsRender = false;
        string? newTab = null;
        int? newTabIndex = null;

        switch (e.Key)
        {
            case "ArrowRight":
                if (currentTabIndex < allTabs.Count - 1)
                {
                    newTab = allTabs[currentTabIndex + 1];
                    newTabIndex = currentTabIndex + 1;
                    needsRender = true;
                }
                break;

            case "ArrowLeft":
                if (currentTabIndex > 0)
                {
                    newTab = allTabs[currentTabIndex - 1];
                    newTabIndex = currentTabIndex - 1;
                    needsRender = true;
                }
                break;

            case "Home":
                newTab = allTabs[0];
                newTabIndex = 0;
                needsRender = true;
                break;

            case "End":
                newTab = allTabs[^1];
                newTabIndex = allTabs.Count - 1;
                needsRender = true;
                break;

            case "Tab":
            case "ArrowDown":
            case "Enter":
            case " ":
                await FocusEmoji(0);
                break;

            case "Escape":
                await OnClose.InvokeAsync();
                break;
        }

        // ✅ OPTIMIZED: Single render + focus
        if (needsRender && newTab != null && newTabIndex.HasValue)
        {
            await SelectTab(newTab);
            StateHasChanged();
            await Task.Delay(RENDER_DELAY_MS);
            await FocusTab(newTabIndex.Value);
        }
    }

    private async Task HandleEmojiContainerKeyDown(KeyboardEventArgs e)
    {
        Debug.WriteLine($"[EmoPicker] Emoji container key: {e.Key}, Shift: {e.ShiftKey}");

        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList() ?? new List<Models.Emo>();
        if (allEmojis.Count == 0) return;

        var currentIndex = _focusedEmojiIndex >= 0 ? _focusedEmojiIndex : 0;

        // Handle Shift+Tab to go back to tabs
        if (e.Key == "Tab" && e.ShiftKey)
        {
            var tabIndex = GetCurrentTabIndex();
            await FocusTab(tabIndex);
            return;
        }

        // ✅ FIX: Handle Tab (forward) to return to search
        if (e.Key == "Tab" && !e.ShiftKey)
        {
            await FocusSearch();
            return;
        }

        // ✅ OPTIMIZED: Calculate new index without multiple focus calls
        int? newIndex = e.Key switch
        {
            "ArrowRight" when currentIndex < allEmojis.Count - 1 => currentIndex + 1,
            "ArrowLeft" when currentIndex > 0 => currentIndex - 1,
            "ArrowDown" when currentIndex + GRID_COLUMN_COUNT < allEmojis.Count => currentIndex + GRID_COLUMN_COUNT,
            "Home" => 0,
            "End" => allEmojis.Count - 1,
            _ => null
        };

        if (newIndex.HasValue)
        {
            await FocusEmoji(newIndex.Value);
        }
        else if (e.Key == "ArrowUp")
        {
            if (currentIndex >= GRID_COLUMN_COUNT)
            {
                await FocusEmoji(currentIndex - GRID_COLUMN_COUNT);
            }
            else
            {
                var tabIndex = GetCurrentTabIndex();
                await FocusTab(tabIndex);
            }
        }
        else if (e.Key is "Enter" or " ")
        {
            if (currentIndex >= 0 && currentIndex < allEmojis.Count)
            {
                await SelectEmoji(allEmojis[currentIndex]);
            }
        }
        else if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }

    private async Task FocusSearch()
    {
        if (_jsModule == null) return;

        try
        {
            await _jsModule.InvokeVoidAsync("focusById", "emoji-search");
            Debug.WriteLine("[EmoPicker] Focused search");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmoPicker] Focus search error: {ex.Message}");
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

        Debug.WriteLine($"[EmoPicker] Focusing tab {index}: {tabName}");

        try
        {
            await _jsModule.InvokeVoidAsync("focusTabByName", tabName);
            Debug.WriteLine($"[EmoPicker] Focused tab: {tabName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmoPicker] Focus tab error: {ex.Message}");
        }
    }

    private async ValueTask FocusEmoji(int index)
    {
        var allEmojis = _categories?.SelectMany(c => c.Emojis).ToList();
        if (allEmojis == null || allEmojis.Count == 0 || index < 0 || index >= allEmojis.Count || _jsModule == null)
        {
            Debug.WriteLine($"[EmoPicker] Cannot focus emoji at index {index} (total: {allEmojis?.Count ?? 0})");
            return;
        }

        _focusedEmojiIndex = index;
        var emoji = allEmojis[index];

        // Update the label to show emoji name when navigating with keyboard
        _hoveredEmojiName = emoji.Name;
        StateHasChanged();

        var emojiCode = emoji.Code;
        Debug.WriteLine($"[EmoPicker] Focusing emoji {index}: {emojiCode}");

        try
        {
            await _jsModule.InvokeVoidAsync("focusEmojiByCode", emojiCode);
            Debug.WriteLine($"[EmoPicker] Focused emoji: {emojiCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmoPicker] Focus emoji error: {ex.Message}");
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
        // ✅ NEW: Clean up resources
        CancelPendingSearch();
        
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
    }

    // ✅ OPTIMIZED: Debounced search on input
    private async Task OnSearchInput(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";
        Debug.WriteLine($"[EmoPicker] Search input changed to: '{searchQuery}'");
        
        UpdateSearchDebounced(); // ✅ Use debouncing
        await OnSearchChanged.InvokeAsync(searchQuery);
    }

    private void OnEmojiMouseEnter(Emo emoji)
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

    private int GetRowCount(int emojiCount)
    {
        return (int)Math.Ceiling(emojiCount / (double)GRID_COLUMN_COUNT);
    }

    private int GetRowIndex(int emojiIndex)
    {
        return (emojiIndex / GRID_COLUMN_COUNT) + 1;
    }

    private int GetColIndex(int emojiIndex)
    {
        return (emojiIndex % GRID_COLUMN_COUNT) + 1;
    }

    /// <summary>
    /// Gets all emojis from current categories with caching for performance.
    /// </summary>
    private List<Models.Emo> GetAllEmojis()
    {
        if (_allEmojisCache != null)
        {
            return _allEmojisCache;
        }

        _allEmojisCache = _categories?.SelectMany(c => c.Emojis).ToList() ?? new List<Models.Emo>();
        return _allEmojisCache;
    }

    /// <summary>
    /// Splits emojis into rows of 8 for virtualized rendering.
    /// </summary>
    private List<List<Models.Emo>> GetEmojiRows(List<Models.Emo> emojis)
    {
        var rows = new List<List<Models.Emo>>();
        for (int i = 0; i < emojis.Count; i += GRID_COLUMN_COUNT)
        {
            rows.Add(emojis.Skip(i).Take(GRID_COLUMN_COUNT).ToList());
        }
        return rows;
    }
}
