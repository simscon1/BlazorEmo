# 🎨 BlazorEmo

A modern, fully accessible emoji picker component for Blazor WebAssembly and Server applications.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%7C%20Server-512BD4?logo=blazor)](https://blazor.net/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![WCAG](https://img.shields.io/badge/WCAG-2.1%20AA-green.svg)](https://www.w3.org/WAI/WCAG21/quickref/)

## ✨ Features

- 🔍 **Live Search** - Filter 1,800+ emojis by name or keywords in real-time
- 📁 **9 Categories** - Smileys, People, Animals, Food, Travel, Activities, Objects, Symbols, Flags
- 🕒 **Recent Emojis** - Automatic tracking via localStorage (max 30)
- ⌨️ **Full Keyboard Navigation** - Tab, arrow keys, Enter, Escape support
- ♿ **Screen Reader Accessible** - ARIA labels, roles, and live regions
- 🌙 **Dark Mode** - Programmatic control + automatic system preference detection
- 🎯 **WCAG 2.1 AA Compliant** - Text contrast exceeds AAA standards (7:1+)
- 🔔 **Event Callbacks** - Track opens, category changes, and searches
- ⚡ **Virtualization** - Efficient rendering of large emoji datasets
- 🎨 **No Dependencies** - Works standalone without service registration

## 📦 Installation

```bash
dotnet add package BlazorEmo
```

## 🚀 Quick Start

### 1. No Registration Required

The EmoPicker component works standalone without any service registration.

### 2. Use the Component

```razor
@page "/"
@using BlazorEmo.Components
@using BlazorEmo.Models

<button @onclick="() => isPickerOpen = true">
    😀 Pick an Emoji
</button>

<EmoPicker IsOpen="@isPickerOpen"
           OnEmojiSelected="HandleEmojiSelected"
           OnClose="() => isPickerOpen = false"
           UseCompleteDataset="true"
           UseVirtualization="true"
           DarkMode="@isDarkMode" />

<p>Selected: @selectedEmoji</p>

@code {
    private bool isPickerOpen = false;
    private bool isDarkMode = false;
    private string selectedEmoji = "";

    private void HandleEmojiSelected(Emo emoji)
    {
        selectedEmoji = emoji.Char;
        isPickerOpen = false;
    }
}
```

## 🎨 Dark Mode

BlazorEmo supports both programmatic dark mode control and automatic system preference detection.

### Programmatic Control

```razor
<EmoPicker DarkMode="@isDarkMode"
           IsOpen="@isPickerOpen"
           OnEmojiSelected="HandleEmojiSelected"
           OnClose="() => isPickerOpen = false" />

<button @onclick="() => isDarkMode = !isDarkMode">
    🌙 Toggle Dark Mode
</button>
```

### System Preference Detection

The picker automatically detects and responds to the user's system color scheme preference via CSS `@media (prefers-color-scheme: dark)`.

### Dark Mode Features

- WCAG AAA contrast ratios (7:1+) in both light and dark modes
- Smooth color transitions between themes
- All interactive elements clearly visible
- Enhanced focus indicators for better accessibility
- Windows High Contrast mode support

## 📖 API Reference

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls whether the picker is visible |
| `OnEmojiSelected` | `EventCallback<Emo>` | **Required** | Fired when an emoji is selected |
| `OnClose` | `EventCallback` | - | Fired when the picker should close |
| `OnOpened` | `EventCallback` | - | Fired when the picker opens |
| `OnCategoryChanged` | `EventCallback<string>` | - | Fired when category tab changes |
| `OnSearchChanged` | `EventCallback<string>` | - | Fired when search text changes |
| `OnBeforeClose` | `Func<Task<bool>>` | - | Async validation before close (return `false` to prevent) |
| `OnError` | `EventCallback<Exception>` | - | Fired when an error occurs |
| `UseCompleteDataset` | `bool` | `false` | Use full emoji dataset vs lite version |
| `UseVirtualization` | `bool` | `true` | Enable virtualization for performance |
| `DarkMode` | `bool` | `false` | Enable dark theme styling |

### Emo Model

```csharp
public class Emo
{
    public string Code { get; set; }      // "1f600"
    public string Char { get; set; }      // "😀"
    public string Name { get; set; }      // "Grinning Face"
    public string[] Keywords { get; set; } // ["smile", "happy"]
    public string? Category { get; set; }  // "Smileys & Emotion"
}
```

## ♿ Accessibility Features

### WCAG 2.1 Level AA Compliance

- ✅ **1.4.3 Contrast (Minimum)** - 7:1+ contrast ratios (exceeds AAA)
- ✅ **1.4.11 Non-text Contrast** - UI components meet 3:1 minimum
- ✅ **2.1.1 Keyboard** - Full keyboard navigation support
- ✅ **2.4.7 Focus Visible** - Clear focus indicators
- ✅ **2.5.5 Target Size** - 44x44px minimum touch targets
- ✅ **4.1.2 Name, Role, Value** - Proper ARIA labels and roles

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Tab` | Navigate between search, tabs, and emojis |
| `Shift+Tab` | Navigate backwards |
| `Arrow Keys` | Navigate between tabs and emoji grid |
| `Enter` / `Space` | Select focused emoji or activate tab |
| `Escape` | Close picker |
| `Home` / `End` | Jump to first/last item |

### Screen Reader Support

- ARIA live regions announce search results
- ARIA labels on all interactive elements
- Screen reader-only descriptions for context
- Proper semantic roles (dialog, tablist, tab, tabpanel, searchbox)

## 🧪 Event Callbacks

Track user interactions with the emoji picker:

```razor
<EmoPicker IsOpen="@isPickerOpen"
           OnEmojiSelected="HandleEmojiSelected"
           OnClose="CloseHandler"
           OnOpened="() => pickerOpenCount++"
           OnCategoryChanged="category => currentCategory = category"
           OnSearchChanged="query => currentSearch = query" />

@code {
    private int pickerOpenCount = 0;
    private string currentCategory = "";
    private string currentSearch = "";

    private void HandleEmojiSelected(Emo emoji)
    {
        Console.WriteLine($"Selected: {emoji.Name}");
        // Analytics tracking, logging, etc.
    }
}
```

## 🎯 Performance

### Virtualization

The picker uses virtualization by default to efficiently render large emoji datasets. Only visible emojis are rendered in the DOM.

```razor
<!-- Recommended for complete dataset -->
<EmoPicker UseCompleteDataset="true"
           UseVirtualization="true" />

<!-- For smaller datasets, virtualization can be disabled -->
<EmoPicker UseCompleteDataset="false"
           UseVirtualization="false" />
```

### Recent Emojis

Recently used emojis are stored in browser localStorage (max 30) and automatically displayed in the "Recent" tab for quick access.

## 🛠️ Development

### Prerequisites

- .NET 10 SDK
- Visual Studio 2026 or VS Code with C# Dev Kit

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run Demo

```bash
cd BlazorEmo.Demo
dotnet run
```

Then navigate to the displayed URL (typically `https://localhost:5001`) to see the interactive demo.

## 🗺️ Roadmap

Future enhancements being considered:

- [ ] Custom emoji support
- [ ] Emoji skin tone variations
- [ ] Custom styling/theming API
- [ ] i18n/localization support
- [ ] Emoji search result highlighting
- [ ] Favorites/pinned emojis
- [ ] Recent emojis sync across devices
- [ ] Configurable emoji categories
- [ ] Copy to clipboard helper
- [ ] Emoji preview on hover

## 📝 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Emoji data provided by Unicode CLDR
- Icons and categories based on Unicode Emoji Standard

## 📞 Support

For issues and questions, please visit:
- **Repository**: [Azure DevOps](https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorEmo.Solution)

---

Made with ❤️ by [LoneWorx LLC](https://dev.azure.com/LoneWorxLLC)
