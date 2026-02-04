# Changelog

All notable changes to BlazorEmoji will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-03

### 🎉 Major Release: Enhanced Accessibility & UX

### Added
- **Stationary Name Display**: Fixed header shows emoji/tab names on hover and keyboard navigation
- **Complete Keyboard Navigation**: Full Tab/Shift+Tab support for bidirectional navigation
- **Focus Trap**: ARIA modal dialog pattern with proper focus containment
- **WCAG 2.1 AAA Compliance**: Enhanced to meet highest accessibility standards
- **ARIA Grid Pattern**: Proper `aria-rowindex`, `aria-colindex`, and `aria-activedescendant` attributes
- **Screen Reader Enhancements**: Improved announcements with contextual information
- **Dark Mode Support**: Automatic detection of `prefers-color-scheme: dark`
- **Touch Target Compliance**: All interactive elements meet 48×48px AAA standard
- **Reduced Motion Support**: Respects `prefers-reduced-motion` preference
- **High Contrast Mode**: Full support for Windows High Contrast and `prefers-contrast: high`

### Changed
- **UI Redesign**: Cleaner, more compact layout (435px height)
- **Tab Spacing**: Reduced padding between tabs and emoji content (saves 7px)
- **Focus Indicators**: Single elegant border (removed double-border effect)
- **Category Header**: More compact with better visual hierarchy
- **Grid Layout**: Optimized for responsive breakpoints (8/6/5 columns)

### Improved
- **Keyboard Navigation**: Arrow keys work in all directions with grid-aware movement
- **Tab Order**: Natural flow: Search → Tabs → Emojis → Search (loops)
- **Performance**: Optimized rendering with `ValueTask` for frequently-called methods
- **Code Quality**: Added XML documentation, constants for magic numbers, parameter validation
- **Error Handling**: Safe JS interop with graceful fallbacks

### Fixed
- **Focus Visibility**: Consistent focus styles across all interactive elements
- **Color Contrast**: Updated colors to meet WCAG AAA ratios
- **Z-Index Issues**: Proper stacking context for tabs, content, and modals
- **Search Highlighting**: Matches emoji button focus appearance

### Removed
- **Tooltips**: Replaced with stationary name display for better UX
- **Commented Code**: Cleaned up unused tooltip CSS

### Technical
- **.NET 10 Support**: Updated to latest framework
- **Blazor WebAssembly**: Optimized for client-side rendering
- **CSS Scoping**: All styles properly scoped to component
- **ARIA 1.2**: Implements latest accessibility patterns

### Breaking Changes
- `OnEmojiSelected` parameter is now required (throws `InvalidOperationException` if not set)
- Removed `data-tooltip` attribute (use `@onmouseenter`/`@onmouseleave` handlers instead)

## [1.0.0] - 2025-01-15

### Added
- Initial release
- Basic emoji picker functionality
- Search capability
- Category tabs
- Recent emojis tracking

---

[2.0.0]: https://github.com/yourorg/blazoremoji/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/yourorg/blazoremoji/releases/tag/v1.0.0