namespace BlazorEmoji.Models;

public record EmojiCategory
{
    public required string Name { get; init; }
    public required List<Emoji> Emojis { get; init; }
}