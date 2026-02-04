# BlazorEmoji 🎨

A modern, fully accessible emoji picker component for Blazor WebAssembly and Blazor Server applications.

[![NuGet Version](https://img.shields.io/nuget/v/BlazorEmoji.svg)](https://www.nuget.org/packages/BlazorEmoji/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/BlazorEmoji.svg)](https://www.nuget.org/packages/BlazorEmoji/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![WCAG 2.1 AA](https://img.shields.io/badge/WCAG%202.1-AA%20Compliant-green.svg)](https://www.w3.org/WAI/WCAG21/quickref/)

## ✨ Features

### 🎯 Core Functionality
- 🔍 **Live search** - Find emojis instantly by name or keywords
- 📁 **Category organization** - Emojis grouped by logical categories
- ⏱️ **Recent emoji tracking** - LocalStorage-based history
- 🎨 **Modern UI** - Clean, intuitive interface
- 🌐 **Platform support** - Works with Blazor WebAssembly and Server

### ♿ Accessibility (WCAG 2.1 Level AA Compliant)
- ✅ **Full keyboard navigation** - Arrow keys, Tab, Enter, Escape, Home/End
- ✅ **Screen reader support** - Complete ARIA landmarks and live regions
- ✅ **Focus management** - Visible focus indicators with 3:1 contrast ratio
- ✅ **Touch-friendly** - Minimum 44x44px touch targets (AAA)
- ✅ **Dark mode** - Automatic theme support with proper contrast ratios
- ✅ **High contrast mode** - Windows High Contrast Mode compatible
- ✅ **Reduced motion** - Respects `prefers-reduced-motion` preference
- ✅ **Responsive** - Works at 320px viewport width and 400% zoom
- ✅ **Semantic HTML** - Proper landmarks, headings, and ARIA attributes

## 🚀 Quick Start

### Installation

**Package Manager Console:**
```powershell
Install-Package BlazorEmoji
```

**.NET CLI:**
```bash
dotnet add package BlazorEmoji
```

**PackageReference:**
<PackageReference Include="BlazorEmoji" Version="1.0.0" />


### Setup

#### Blazor WebAssembly

**Program.cs:**

```csharp
using BlazorEmoji.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args); 
builder.RootComponents.Add<App>("#app"); 
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register BlazorEmoji services 
builder.Services.AddEmojiServices(builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();

```

#### Blazor Server

**Program.cs:**

``` csharp
using BlazorEmoji.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(); 
builder.Services.AddServerSideBlazor();

// Register BlazorEmoji services builder.Services.AddEmojiServices("https://yourdomain.com");
var app = builder.Build(); // ... rest of configuration
```

**_Imports.razor:**

```razor

@using BlazorEmoji.Components
@using BlazorEmoji.Models

```

### Basic Usage

```

@page "/demo" @using BlazorEmoji.Components

<h3>Emoji Picker Demo</h3>

<button @onclick="() => showPicker = true"> Open Emoji Picker </button>

<p>Selected: <strong>@selectedEmoji</strong></p>

<EmojiPicker IsOpen="@showPicker" 
             OnEmojiSelected="HandleEmojiSelected" 
             OnClose="() => showPicker = false" />

@code { 
    private bool showPicker = false; 
    private string selectedEmoji = "None";

    private void HandleEmojiSelected(Emoji emoji)
    {
        selectedEmoji = $"{emoji.Char} {emoji.Name}";
        showPicker = false;
    }
}
```


## 📚 Documentation

For detailed instructions on usage, customization, and API references, please refer to the [BlazorEmoji Documentation](https://github.com/YourUsername/BlazorEmoji/wiki).

## 🧪 Browser Support

- ✅ Chrome/Edge 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Opera 76+

## 📋 Accessibility Compliance

BlazorEmoji meets the following standards:

- ✅ **WCAG 2.1 Level AA** - All success criteria met
- ✅ **Partial WCAG 2.1 Level AAA** - Enhanced touch targets and visual presentation
- ✅ **Section 508** - U.S. Federal accessibility requirements
- ✅ **EN 301 549** - European accessibility standard
- ✅ **ARIA Authoring Practices Guide (APG)** - Dialog, Tabs, and Grid patterns

 
## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](https://github.com/YourUsername/BlazorEmoji/blob/main/.github/CONTRIBUTING.md) before submitting PRs.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏢 About

**BlazorEmoji** is created and maintained by [LoneWorx LLC](https://lonewrox.com).

- 📧 Support: support@lonewrox.com
- 🐛 Issues: [Report a bug](https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorEmoji.Solution)
- 💬 Discussions: [Ask a question](https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorEmoji.Solution)

## 🙏 Acknowledgments

- Emoji data sourced from Unicode Consortium
- Inspired by modern emoji pickers across platforms
- Built with accessibility-first principles

## 📊 Project Stats

- 🎯 .NET 10 compatible
- 📦 Zero external dependencies (except Blazor)
- 🔒 Type-safe C# API
- 🌍 Internationalization ready
- ⚡ Lightweight and performant

---

**Made with ❤️ by LoneWorx LLC** | **WCAG 2.1 AA Compliant** | **MIT Licensed**




