namespace BlazorEmo.Models;

public record EmoCategory
{
    public required string Name { get; init; }
    public required List<Emo> Emojis { get; init; }
}