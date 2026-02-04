// ES6 Module for Emoji Picker
console.log('[EmojiPicker] emoji-picker.js module loading...');

export function focusById(id) {
    console.log('[EmojiPicker] Attempting to focus by ID:', id);
    const element = document.getElementById(id);
    if (element) {
        element.focus();
        console.log('[EmojiPicker] Focused element:', element);
        return true;
    }
    console.warn('[EmojiPicker] Element not found by ID:', id);
    return false;
}

export function focusTabByName(tabName) {
    console.log('[EmojiPicker] Attempting to focus tab:', tabName);
    const element = document.querySelector(`[data-tab-name="${tabName}"]`);
    if (element) {
        element.focus();
        console.log('[EmojiPicker] Focused tab:', element);
        return true;
    }
    console.warn('[EmojiPicker] Tab not found:', tabName);
    const allTabs = document.querySelectorAll('[data-tab-name]');
    console.log('[EmojiPicker] Available tabs:', 
        Array.from(allTabs).map(t => t.getAttribute('data-tab-name')));
    return false;
}

export function focusEmojiByCode(emojiCode) {
    console.log('[EmojiPicker] Attempting to focus emoji:', emojiCode);
    const element = document.querySelector(`[data-emoji-code="${emojiCode}"]`);
    if (element) {
        element.focus();
        console.log('[EmojiPicker] Focused emoji:', element);
        return true;
    }
    console.warn('[EmojiPicker] Emoji not found:', emojiCode);
    return false;
}

export function trapFocus(element) {
    const focusableElements = element.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstFocusable = focusableElements[0];
    const lastFocusable = focusableElements[focusableElements.length - 1];

    element.addEventListener('keydown', function(e) {
        if (e.key !== 'Tab') return;

        if (e.shiftKey) {
            if (document.activeElement === firstFocusable) {
                e.preventDefault();
                lastFocusable.focus();
            }
        } else {
            if (document.activeElement === lastFocusable) {
                e.preventDefault();
                firstFocusable.focus();
            }
        }
    });
}

export function setActiveDescendant(gridElement, emojiId) {
    gridElement.setAttribute('aria-activedescendant', emojiId);
}

console.log('[EmojiPicker] emoji-picker.js module loaded successfully');