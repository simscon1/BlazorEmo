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

console.log('[EmojiPicker] emoji-picker.js module loaded successfully');