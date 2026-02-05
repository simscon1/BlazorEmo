# Changelog

All notable changes to BlazorEmo will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-03

### 🎉 Initial Release

A fully accessible, WCAG 2.1 AA-compliant emoji picker component for Blazor (text contrast exceeds AAA standards).
		

#### Event Callbacks
- **OnOpened** - Event callback invoked when picker is opened
  - Useful for analytics tracking and initialization
  - Fires every time `IsOpen` transitions from `false` to `true`
- **OnCategoryChanged** - Event callback with category name
  - Tracks when user switches between emoji categories
  - Provides category name for external state synchronization
- **OnSearchChanged** - Event callback with search query
  - Fires on every keystroke in search input
  - Enables external search suggestions and analytics
- **OnBeforeClose** - Async callback to prevent closing
  - Return `false` to cancel close action
  - Perfect for confirmation dialogs
- **OnError** - Event callback for error handling
  - Receives exception details from JS interop failures
  - Enables centralized error logging

#### Demo Enhancements
- Added interactive event callback demonstration page
- Event tracking display with real-time updates
- Category and search history with gradient pill UI
- Clear history button to reset event tracking
- Console logging for all event callbacks

 ---

#### Core Features
- **1,585+ Emojis**: Complete emoji dataset with 9 categories
- **Basic Dataset Option**: Lightweight 60-emoji set for faster loading
- **Real-time Search**: Instant filtering as you type
- **Recent Emojis**: LocalStorage-based tracking of recently used emojis
- **Category Navigation**: Browse emojis by organized categories

#### Accessibility (WCAG 2.1 AA - Exceeding AAA for Contrast)
- **Complete Keyboard Navigation**: Tab, Shift+Tab, arrows, Home/End, Escape
- **Focus Trap**: ARIA modal dialog pattern with proper focus management
- **Screen Reader Support**: Full ARIA 1.2 implementation
  - `role="dialog"`, `role="tablist"`, `role="grid"` patterns
  - Dynamic screen reader announcements
  - Descriptive labels and instructions
- **Touch Targets**: 48×48px minimum (exceeds Level AAA 44×44px requirement)
- **Focus Indicators**: 3px visible focus outlines with 3:1 contrast
- **High Contrast Mode**: Windows High Contrast and `prefers-contrast` support
- **Color Contrast**: Text contrast ratios of 7:1+ (exceeds AAA standard of 4.5:1)

#### User Experience
- **Stationary Name Display**: Fixed header shows emoji/tab names on hover/focus
- **Dark Mode**: Automatic `prefers-color-scheme: dark` detection
- **Reduced Motion**: Respects `prefers-reduced-motion` preference
- **Responsive Design**: Works on 320px to desktop widths
- **Smooth Animations**: Elegant transitions and interactions

#### Technical
- **.NET 10 Support**: Built for latest framework
- **Blazor WebAssembly**: Optimized for client-side rendering
- **Blazor Server**: Full compatibility
- **Scoped CSS**: Component-isolated styles
- **ValueTask**: Performance optimization for frequently-called methods
- **Error Handling**: Safe JS interop with graceful fallbacks
- **XML Documentation**: Full API documentation

### Keyboard Shortcuts
- Tab/Shift+Tab: Navigate between search, tabs, and emojis
- Arrow keys: Navigate tabs and emoji grid
- Home/End: Jump to first/last item
- Enter/Space: Select emoji
- Escape: Close picker

### Dimensions
- Width: ~400px (8-column grid, responsive)
- Height: 435px (optimized for visibility)
- Aspect Ratio: 1:1.09 (taller than wide)

### Performance
- Basic Dataset: 60 emojis, ~2KB
- Complete Dataset: 1,585 emojis, ~45KB
- First Load: < 100ms
- Search: Real-time filtering

### Browser Support
- Chrome/Edge 90+

*** Not tested
- Firefox 88+
- Safari 14+
- Mobile browsers (iOS Safari, Chrome Mobile)
 

### Dependencies
- .NET 10.0+
- Blazor WebAssembly or Server

--- 

[1.0.0]: https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorEmo.Solution?version=GBv1.0.0