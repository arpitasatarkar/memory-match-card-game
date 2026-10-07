using MemoryMatchArena.Core;

namespace MemoryMatchArena.Tests;

public sealed class GameEngineTests
{
    [Theory]
    [InlineData(4, 16, 8)]
    [InlineData(6, 36, 18)]
    [InlineData(8, 64, 32)]
    public void ResetBuildsPairsForEachSupportedGrid(int size, int cardCount, int pairCount)
    {
        var engine = new GameEngine(size, new Random(17));

        Assert.Equal(cardCount, engine.Cards.Count);
        Assert.Equal(pairCount, engine.PairCount);
        Assert.All(engine.Cards.GroupBy(card => card.ColorName), group => Assert.Equal(2, group.Count()));
        Assert.All(engine.Cards, card => Assert.False(card.IsFaceUp));
    }

    [Fact]
    public void TryFlipRejectsRepeatedAndThirdSelectionDuringMismatch()
    {
        var engine = new GameEngine(4, new Random(5));
        var first = engine.Cards[0];
        var second = engine.Cards.First(card => card.ColorName != first.ColorName);
        var third = engine.Cards.First(card => card.Id != first.Id && card.Id != second.Id);

        Assert.Equal(FlipResult.FirstCard, engine.TryFlip(first.Id));
        Assert.Equal(FlipResult.Rejected, engine.TryFlip(first.Id));
        Assert.Equal(FlipResult.Mismatch, engine.TryFlip(second.Id));
        Assert.Equal(FlipResult.Rejected, engine.TryFlip(third.Id));
        Assert.Equal(1, engine.Moves);

        engine.HideUnmatched();
        Assert.All(engine.Cards.Take(2), card => Assert.False(card.IsFaceUp));
        Assert.Equal(FlipResult.FirstCard, engine.TryFlip(third.Id));
    }

    [Fact]
    public void MatchingPairRemainsVisibleAndCompletingAllPairsEndsGame()
    {
        var engine = new GameEngine(4, new Random(23));
        var pairs = engine.Cards.GroupBy(card => card.ColorName).ToArray();

        foreach (var pair in pairs)
        {
            var cards = pair.ToArray();
            Assert.Equal(FlipResult.FirstCard, engine.TryFlip(cards[0].Id));
            Assert.Equal(FlipResult.Match, engine.TryFlip(cards[1].Id));
            Assert.All(cards, card =>
            {
                Assert.True(card.IsFaceUp);
                Assert.True(card.IsMatched);
            });
        }

        Assert.True(engine.IsComplete);
        Assert.Equal(engine.PairCount, engine.MatchesFound);
        Assert.Equal(engine.PairCount, engine.Moves);
        Assert.Equal(FlipResult.Rejected, engine.TryFlip(engine.Cards[0].Id));
    }

    [Fact]
    public void ResetClearsProgressAndRejectsUnsupportedGridSizes()
    {
        var engine = new GameEngine(4, new Random(31));
        var first = engine.Cards[0];
        var pair = engine.Cards.First(card => card.ColorName == first.ColorName && card.Id != first.Id);
        engine.TryFlip(first.Id);
        engine.TryFlip(pair.Id);

        engine.Reset(6);

        Assert.Equal(6, engine.GridSize);
        Assert.Equal(0, engine.MatchesFound);
        Assert.Equal(0, engine.Moves);
        Assert.False(engine.IsComplete);
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Reset(5));
    }
}