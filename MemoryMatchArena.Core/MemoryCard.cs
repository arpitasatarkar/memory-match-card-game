namespace MemoryMatchArena.Core;

public sealed class MemoryCard
{
    internal MemoryCard(int id, string colorName, string colorHex)
    {
        Id = id;
        ColorName = colorName;
        ColorHex = colorHex;
    }

    public int Id { get; }
    public string ColorName { get; }
    public string ColorHex { get; }
    public bool IsFaceUp { get; internal set; }
    public bool IsMatched { get; internal set; }
}