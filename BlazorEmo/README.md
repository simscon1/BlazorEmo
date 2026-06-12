# BlazorEmo - Emoji Picker for Blazor

**Keyboard Accessible • Dark Mode**

[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

## 🚀 Quick Start

```
dotnet add package BlazorEmo

```
```razor	
@page "/" 
@using BlazorEmo.Components

<button @onclick="() => isPickerOpen = true">Select Emoji</button>

<EmoPicker IsOpen="@isPickerOpen" .
		   OnEmojiSelected="HandleEmojiSelected" 
		   OnClose="() => isPickerOpen = false" 
		   UseCompleteDataset="true" 
		   UseVirtualization="true" />

<p>Selected: @selectedEmoji</p>

@code { 
	private bool isPickerOpen = false; 
	private string selectedEmoji = "";

	private void HandleEmojiSelected(BlazorEmo.Models.Emo emoji)
	{
    selectedEmoji = emoji.Char;
    isPickerOpen = false;
	}
}
```

## 🎯 Features

- **1,100+ Emojis** across 9 categories with live search (300ms debounce)
- **Virtualized Rendering** — only renders visible emojis for large lists
- **Lazy Loading** — categories loaded on-demand
- **Recent Emojis** — tracks recently used selections
- **Dark Mode** — automatic `prefers-color-scheme` detection
- **Responsive** — works on desktop, tablet, and mobile
- **Touch-Friendly** — 48×48px minimum touch targets

## 📖 Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls picker visibility |
| `OnEmojiSelected` | `EventCallback<Emo>` | — | **Required.** Invoked when emoji is selected |
| `OnClose` | `EventCallback` | — | Invoked when picker closes |
| `UseCompleteDataset` | `bool` | `false` | Full 1,100+ emojis (`true`) or basic 60 (`false`) |
| `UseVirtualization` | `bool` | `true` | Virtualized rendering for 40+ emoji lists |
| `OnOpened` | `EventCallback` | — | Invoked when picker opens |
| `OnCategoryChanged` | `EventCallback<string>` | — | Invoked on category switch |
| `OnSearchChanged` | `EventCallback<string>` | — | Invoked on search query change |
| `OnBeforeClose` | `Func<Task<bool>>?` | — | Async callback to prevent closing |
| `OnError` | `EventCallback<Exception>` | — | Invoked on errors |

## 📜 License

MIT License — see [LICENSE](LICENSE) for details.

## 📞 Support

- 🐛 Issues: [GitHub Issues](https://github.com/simscon1/BlazorEmo/issues)
- 💻 Repository: [GitHub](https://github.com/simscon1/BlazorEmo)
