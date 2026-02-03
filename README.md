# BlazorEmoji

A fully accessible, keyboard-navigable emoji picker for Blazor WebAssembly.

## Features
- ✅ WCAG 2.1 AA/AAA compliant
- ✅ Full keyboard navigation
- ✅ Screen reader support
- ✅ Recent emoji tracking (LocalStorage)
- ✅ Live search
- ✅ Dark mode support

## Installation

```
dotnet add package BlazorEmoji

```

## Usage

```
// Program.cs builder.Services.AddEmojiServices();

@using BlazorEmoji.Components @using BlazorEmoji.Models
<EmojiPicker IsOpen="@showPicker" OnEmojiSelected="HandleEmojiSelected" OnClose="() => showPicker = false" />
@code { bool showPicker = false;

void HandleEmojiSelected(Emoji emoji) {
    Console.WriteLine($"Selected: {emoji.Char}");
    showPicker = false;
}

```
## License
MIT



