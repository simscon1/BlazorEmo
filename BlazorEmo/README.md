# BlazorEmo

A fully accessible, WCAG 2.1 AA-compliant emoji picker component for Blazor WebAssembly applications (text contrast exceeds AAA standards).

## ✨ Features

### Core Functionality
- 🎯 **1,585+ Emojis** - Complete emoji dataset with categorization
- 🔍 **Smart Search** - Real-time emoji search with instant results
- 📱 **Responsive Design** - Works seamlessly on desktop, tablet, and mobile
- ♿ **WCAG 2.1 AA Compliant** - Exceeds AAA standards for text contrast

### Keyboard Navigation
- **Tab/Shift+Tab** - Navigate between search, categories, and emojis
- **Arrow Keys** - Navigate through tabs and emoji grid
- **Home/End** - Jump to first/last item
- **Enter/Space** - Select emoji or navigate to list
- **Escape** - Close picker
- **Focus Trap** - Modal dialog pattern with contained focus

### User Experience
- **Stationary Name Display** - Shows emoji/category names in fixed header
- **Recent Emojis** - Tracks recently used emojis
- **Dark Mode** - Automatic system preference detection
- **Smooth Animations** - Respects `prefers-reduced-motion`
- **Touch-Friendly** - 48×48px minimum touch targets (exceeds AAA 44×44px requirement)

### Event Callbacks
- **OnOpened** - Triggered when picker opens (analytics, initialization)
- **OnCategoryChanged** - Triggered when user switches categories
- **OnSearchChanged** - Triggered when search query changes
- **OnEmojiSelected** - Triggered when emoji is selected (required)
- **OnClose** - Triggered when picker closes
- **OnBeforeClose** - Async callback to prevent closing (confirmations)
- **OnError** - Triggered on JS interop or other errors

### Accessibility
- ✅ WCAG 2.1 Level A compliant
- ✅ WCAG 2.1 Level AA compliant
- ✅ WCAG 2.1 Level AAA (Text Contrast only)
- ✅ ARIA 1.2 dialog, tablist, and grid patterns
- ✅ Screen reader announcements
- ✅ High contrast mode support
- ✅ Windows High Contrast Mode compatible
- ✅ Keyboard-only operation
- ✅ Focus indicators with 3px outlines

## 📦 Installation

**Package Manager Console:**

```powershell
Install-Package BlazorEmo
```

## 🚀 Quick Start

```
@page "/" @using BlazorEmo.Components

<button @onclick="() => isPickerOpen = true"> Select Emoji </button>

<EmoPicker IsOpen="@isPickerOpen" 
             OnEmojiSelected="HandleEmojiSelected" OnClose="() => isPickerOpen = false" 
             UseCompleteDataset="true" />

<p>Selected: @selectedEmoji</p>

@code { 
    private bool isPickerOpen = false; 
    private string selectedEmoji = "";

    private void HandleEmojiSelected(BlazorEmo.Models.Emoji emoji)
    {
        selectedEmoji = emoji.Char;
        isPickerOpen = false;
    }
}

```

## ⚙️ Configuration

### Parameters

| Parameter | Type | Default | Required | Description |
|-----------|------|---------|----------|-------------|
| `IsOpen` | `bool` | `false` | No | Controls picker visibility |
| `OnEmojiSelected` | `EventCallback<Emo>` | - | **Yes** | Invoked when emoji is selected |
| `OnClose` | `EventCallback` | - | No | Invoked when picker is closed |
| `UseCompleteDataset` | `bool` | `false` | No | Use full 1,585 emoji dataset (true) or basic 60 emojis (false) |
| `OnOpened` | `EventCallback` | - | No | Invoked when picker opens |
| `OnCategoryChanged` | `EventCallback<string>` | - | No | Invoked when category changes |
| `OnSearchChanged` | `EventCallback<string>` | - | No | Invoked when search query changes |
| `OnBeforeClose` | `Func<Task<bool>>?` | - | No | Async callback to prevent closing |
| `OnError` | `EventCallback<Exception>` | - | No | Invoked when errors occur |

### Emo Model

```
public class Emoji 
{ 
    public string Name { get; set; }  
    public string Char { get; set; }      
    public string Code { get; set; }      
    public string Category { get; set; }  
    public List<string> Keywords { get; set; } 
}

```

## 🔔 Event Callbacks Examples
 
```
<EmoPicker OnOpened="@(() => Analytics.Track("EmoPickerOpened"))" 
             OnCategoryChanged="@(cat => Analytics.Track("CategoryChanged", cat))" 
             OnSearchChanged="@(query => Analytics.Track("SearchQuery", query))" 
             OnEmojiSelected="@(emoji => Analytics.Track("EmojiSelected", emoji.Name))" />
             OnError="@(ex => Logger.LogError(ex, "Emoji picker error"))" />
 
@code { private string currentCategory = "";
    private void HandlePickerOpened()
    {
        Console.WriteLine("Picker opened!");
        // Initialize external state, load data, etc.
    }
    
    private void UpdateSearchSuggestions(string query)
    {
        // Update external search UI, show suggestions, etc.
    }
}

```


## 🎨 Customization

The component uses CSS scoping. Override styles in your global CSS:

```
/* Adjust picker height */ 
.emoji-picker { height: 500px !important; }

/* Custom focus color */ 
.emoji-item:focus { border-color: #your-brand-color !important; }

```

## 🌙 Dark Mode

Automatically detects `prefers-color-scheme: dark`. No configuration needed.

## 📐 Dimensions

- **Width**: ~400px (8-column grid, responsive)
- **Height**: 435px (optimized for ~9-10 rows)
- **Aspect Ratio**: 1:1.09 (taller than wide, matches industry standards)

## 🧪 Browser Support

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- Mobile browsers (iOS Safari, Chrome Mobile)

## 📊 Performance

- **Basic Dataset**: 60 emojis, ~2KB data
- **Complete Dataset**: 1,585 emojis, ~45KB data
- **First Load**: < 100ms
- **Search**: Real-time filtering

## 🔒 Accessibility Compliance

Tested against:
- WCAG 2.1 Level A ✅
- WCAG 2.1 Level AA ✅
- WCAG 2.1 Level AAA (Text Contrast) ✅
- ARIA 1.2 Authoring Practices ✅
- Section 508 ✅

**Note:** While fully AA compliant, text contrast ratios exceed AAA standards (7:1+).

## 📝 License

MIT License - see [LICENSE](LICENSE) for details

## 🤝 Contributing

Contributions welcome! Please open an issue or submit a pull request via Azure DevOps.

## 📞 Support

- 🐛 Issues: [Azure DevOps Work Items](https://dev.azure.com/LoneWorxLLC/LoneWorx/_workitems)
- 💻 Repository: [Azure DevOps](https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorEmo.Solution)
