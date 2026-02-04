namespace BlazorEmo.Models;

public record Emo
{
    public required string Code { get; init; }
    public required string Char { get; init; }
    public required string Name { get; init; }
    public string[] Keywords { get; init; } = [];
    public string? Category { get; init; }
}