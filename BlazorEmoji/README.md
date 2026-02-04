# BlazorEmoji

A fully accessible, WCAG 2.1 AAA-compliant emoji picker component for Blazor WebAssembly applications.

## ✨ Features

### Core Functionality
- 🎯 **1,585+ Emojis** - Complete emoji dataset with categorization
- 🔍 **Smart Search** - Real-time emoji search with instant results
- 📱 **Responsive Design** - Works seamlessly on desktop, tablet, and mobile
- ♿ **WCAG 2.1 AAA Compliant** - Industry-leading accessibility

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
- **Touch-Friendly** - 48×48px minimum touch targets (AAA)

### Accessibility
- ✅ WCAG 2.1 Level A, AA, AAA compliant
- ✅ ARIA 1.2 dialog, tablist, and grid patterns
- ✅ Screen reader announcements
- ✅ High contrast mode support
- ✅ Windows High Contrast Mode compatible
- ✅ Keyboard-only operation
- ✅ Focus indicators with 3px outlines

## 📦 Installation

**Package Manager Console:**

```powershell
Install-Package BlazorEmoji
```

## 🚀 Quick Start

```
@page "/" @using BlazorEmoji.Components

<button @onclick="() => isPickerOpen = true"> Select Emoji </button>

<EmojiPicker IsOpen="@isPickerOpen" 
             OnEmojiSelected="HandleEmojiSelected" OnClose="() => isPickerOpen = false" 
             UseCompleteDataset="true" />

<p>Selected: @selectedEmoji</p>

@code { 
    private bool isPickerOpen = false; 
    private string selectedEmoji = "";

    private void HandleEmojiSelected(BlazorEmoji.Models.Emoji emoji)
    {
        selectedEmoji = emoji.Char;
        isPickerOpen = false;
    }
}

```

## ⚙️ Configuration

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls picker visibility |
| `OnEmojiSelected` | `EventCallback<Emoji>` | - | **Required.** Invoked when emoji is selected |
| `OnClose` | `EventCallback` | - | Invoked when picker is closed |
| `UseCompleteDataset` | `bool` | `false` | Use full 1,585 emoji dataset (true) or basic 60 emojis (false) |

### Emoji Model

```
public class Emoji 
{ 
    public string Name { get; set; }  
    public string Char { get; set; }      
    public string Code { get; set; }      
    public string Category { get; set; }  
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
- WCAG 2.1 Level AAA ✅
- ARIA 1.2 Authoring Practices ✅
- Section 508 ✅

## 📝 License

MIT License - see [LICENSE](LICENSE) for details

## 🤝 Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

## 📞 Support

- 📧 Email: support@example.com
- 🐛 Issues: [GitHub Issues](https://github.com/yourorg/blazoremoji/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/yourorg/blazoremoji/discussions)
