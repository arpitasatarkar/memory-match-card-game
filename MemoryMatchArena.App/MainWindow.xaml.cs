using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MemoryMatchArena.Core;

namespace MemoryMatchArena.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private GameEngine _engine;
    private CancellationTokenSource? _mismatchDelay;
    private int _elapsedSeconds;

    public MainWindow()
    {
        InitializeComponent();
        _engine = new GameEngine();
        _timer.Tick += (_, _) =>
        {
            _elapsedSeconds++;
            UpdateSessionDisplay();
        };
        GridSizeSelector.SelectionChanged += (_, _) => StartNewGame();
        StartNewGame();
    }

    private void StartNewGame()
    {
        CancelMismatchDelay();
        var selectedSize = (GridSizeSelector.SelectedItem as ComboBoxItem)?.Tag as string;
        var gridSize = int.TryParse(selectedSize, out var parsedSize) ? parsedSize : 4;
        _engine.Reset(gridSize);
        _elapsedSeconds = 0;
        _timer.Stop();
        GameStatusTextBlock.Text = "Ready when you are.";
        BoardHintTextBlock.Text = "A fresh palette is on the table.";
        CardGrid.Rows = gridSize;
        CardGrid.Columns = gridSize;
        RenderBoard();
        UpdateSessionDisplay();
    }

    private async void CardClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int cardId })
            return;

        var result = _engine.TryFlip(cardId);
        if (result == FlipResult.Rejected)
            return;

        if (!_timer.IsEnabled)
            _timer.Start();

        RenderBoard();
        switch (result)
        {
            case FlipResult.FirstCard:
                GameStatusTextBlock.Text = "One color revealed. Choose another.";
                BoardHintTextBlock.Text = "Hold that color in your memory.";
                break;
            case FlipResult.Match:
                GameStatusTextBlock.Text = $"Pair found: {_engine.Cards[cardId].ColorName}.";
                BoardHintTextBlock.Text = "Two colors, one match.";
                if (_engine.IsComplete)
                {
                    _timer.Stop();
                    GameStatusTextBlock.Text = "Palette complete. Excellent memory.";
                    BoardHintTextBlock.Text = "Every color found its partner.";
                }
                break;
            case FlipResult.Mismatch:
                GameStatusTextBlock.Text = "Not a match. Take another look.";
                BoardHintTextBlock.Text = "Those two colors will turn over shortly.";
                await HideMismatchAfterDelay();
                break;
        }

        UpdateSessionDisplay();
        RenderBoard();
    }

    private async Task HideMismatchAfterDelay()
    {
        var delay = new CancellationTokenSource();
        _mismatchDelay = delay;
        try
        {
            await Task.Delay(850, delay.Token);
            _engine.HideUnmatched();
            GameStatusTextBlock.Text = "Keep searching.";
            BoardHintTextBlock.Text = "A fresh palette is on the table.";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_mismatchDelay, delay))
                _mismatchDelay = null;
            delay.Dispose();
        }
    }

    private void CancelMismatchDelay()
    {
        _mismatchDelay?.Cancel();
    }

    private void RenderBoard()
    {
        CardGrid.Children.Clear();
        foreach (var card in _engine.Cards)
        {
            var button = new Button
            {
                Tag = card.Id,
                Style = (Style)FindResource("CardButton"),
                IsEnabled = !card.IsFaceUp && !card.IsMatched && !_engine.IsInputLocked,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(card.IsFaceUp ? "#FFFFFF" : "#24574E")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(card.IsFaceUp ? "#183B35" : "#B8D0C7")),
                ToolTip = card.IsFaceUp ? card.ColorName : "Hidden color"
            };
            AutomationProperties.SetName(button, card.IsFaceUp ? $"{card.ColorName} color" : "Hidden color");

            if (card.IsFaceUp || card.IsMatched)
            {
                var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                content.Children.Add(new Ellipse
                {
                    Width = 25,
                    Height = 25,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(card.ColorHex)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 6)
                });
                content.Children.Add(new TextBlock
                {
                    Text = card.ColorName.ToUpperInvariant(),
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36564E"))
                });
                button.Content = content;
                if (card.IsMatched)
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F0E8"));
            }
            else
            {
                button.Content = new TextBlock
                {
                    Text = "MM",
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            button.Click += CardClicked;
            CardGrid.Children.Add(button);
        }
    }

    private void UpdateSessionDisplay()
    {
        MatchesTextBlock.Text = $"{_engine.MatchesFound:00} / {_engine.PairCount:00}";
        MovesTextBlock.Text = $"{_engine.Moves:00}";
        TimerTextBlock.Text = TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"mm\:ss");
        BoardProgressTextBlock.Text = $"{_engine.PairCount:00} PAIRS";
        PlayerDisplayTextBlock.Text = string.IsNullOrWhiteSpace(PlayerNameTextBox.Text)
            ? "PLAYER ONE"
            : PlayerNameTextBox.Text.Trim().ToUpperInvariant();
    }

    private void NewGameClicked(object sender, RoutedEventArgs e) => StartNewGame();

    private void ExitGameClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void PlayerNameTextChanged(object sender, TextChangedEventArgs e)
    {
        if (PlayerDisplayTextBlock is not null)
            UpdateSessionDisplay();
    }
}