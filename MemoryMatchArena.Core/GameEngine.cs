namespace MemoryMatchArena.Core;

public sealed class GameEngine
{
    private static readonly (string Name, string Hex)[] Colors =
    [
        ("Coral", "#E76F51"), ("Ocean", "#277DA1"), ("Lime", "#84A98C"), ("Sun", "#F4A261"),
        ("Berry", "#C44569"), ("Mint", "#2A9D8F"), ("Iris", "#6D597A"), ("Saffron", "#E9C46A"),
        ("Cherry", "#D1495B"), ("Lagoon", "#168AAD"), ("Leaf", "#588157"), ("Apricot", "#FA8B62"),
        ("Plum", "#9B5DE5"), ("Cyan", "#00A6A6"), ("Rose", "#F15BB5"), ("Olive", "#6A994E"),
        ("Cobalt", "#4361EE"), ("Tangerine", "#F3722C"), ("Jade", "#43AA8B"), ("Lilac", "#A06CD5"),
        ("Ruby", "#B23A48"), ("Sky", "#4CC9F0"), ("Moss", "#718355"), ("Peach", "#FF9F68"),
        ("Grape", "#7209B7"), ("Teal", "#008C95"), ("Butter", "#F9C74F"), ("Pine", "#386641"),
        ("Salmon", "#E8897B"), ("Denim", "#577590"), ("Pistachio", "#90BE6D"), ("Magenta", "#D0008F")
    ];

    private readonly Random _random;
    private readonly List<MemoryCard> _cards = [];
    private int? _firstCardId;

    public GameEngine(int gridSize = 4, Random? random = null)
    {
        _random = random ?? Random.Shared;
        Reset(gridSize);
    }

    public int GridSize { get; private set; }
    public int PairCount => GridSize * GridSize / 2;
    public int MatchesFound { get; private set; }
    public int Moves { get; private set; }
    public bool IsComplete => MatchesFound == PairCount;
    public bool IsInputLocked { get; private set; }
    public IReadOnlyList<MemoryCard> Cards => _cards;

    public FlipResult TryFlip(int cardId)
    {
        if (IsComplete || IsInputLocked)
            return FlipResult.Rejected;

        var card = _cards.FirstOrDefault(candidate => candidate.Id == cardId);
        if (card is null || card.IsFaceUp || card.IsMatched)
            return FlipResult.Rejected;

        card.IsFaceUp = true;
        if (_firstCardId is null)
        {
            _firstCardId = card.Id;
            return FlipResult.FirstCard;
        }

        Moves++;
        var firstCard = _cards.Single(candidate => candidate.Id == _firstCardId.Value);
        _firstCardId = null;
        if (firstCard.ColorName == card.ColorName)
        {
            firstCard.IsMatched = true;
            card.IsMatched = true;
            MatchesFound++;
            return FlipResult.Match;
        }

        IsInputLocked = true;
        return FlipResult.Mismatch;
    }

    public void HideUnmatched()
    {
        if (!IsInputLocked)
            return;

        foreach (var card in _cards.Where(card => card.IsFaceUp && !card.IsMatched))
            card.IsFaceUp = false;

        IsInputLocked = false;
    }

    public void Reset(int gridSize = 4)
    {
        if (gridSize is not (4 or 6 or 8))
            throw new ArgumentOutOfRangeException(nameof(gridSize), "Grid size must be 4, 6, or 8.");

        GridSize = gridSize;
        MatchesFound = 0;
        Moves = 0;
        IsInputLocked = false;
        _firstCardId = null;
        _cards.Clear();

        var pairs = gridSize * gridSize / 2;
        var selectedColors = Colors.Take(pairs).ToArray();
        var shuffled = selectedColors
            .SelectMany(color => new[] { color, color })
            .ToArray();

        for (var index = shuffled.Length - 1; index > 0; index--)
        {
            var otherIndex = _random.Next(index + 1);
            (shuffled[index], shuffled[otherIndex]) = (shuffled[otherIndex], shuffled[index]);
        }

        for (var index = 0; index < shuffled.Length; index++)
            _cards.Add(new MemoryCard(index, shuffled[index].Name, shuffled[index].Hex));
    }
}