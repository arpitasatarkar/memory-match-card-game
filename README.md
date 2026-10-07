# Memory Match Arena

A native Windows desktop memory game built around a reusable game engine and a color-swatch theme. Find matching pairs on 4×4, 6×6, or 8×8 boards.

## Features

- Three board sizes, with a freshly shuffled deck for each game.
- Paired colors, one pair per color.
- Two-card turns, matched pairs stay visible, and mismatches turn down after a short delay.
- Player name, move count, pairs found, elapsed time, and game-completion status.
- New Game control and board-size changes reset the session.
- Invalid selections and extra clicks during a mismatch delay are rejected by the game engine.
- Keyboard-focusable cards with accessible names.

## Technology

- C# and .NET 8
- WPF for the Windows desktop interface
- xUnit for game-engine tests
- No external runtime packages

## Setup

Install the .NET 8 SDK on Windows. A Visual Studio installation with the .NET desktop development workload is optional; the .NET CLI is sufficient.

From the repository root, restore packages and build:

```powershell
dotnet restore MemoryMatchArena.sln
dotnet build MemoryMatchArena.sln
```

## Run

Launch the desktop application from the repository root:

```powershell
dotnet run --project MemoryMatchArena.App/MemoryMatchArena.App.csproj
```

## How to Play

Choose a 4×4, 6×6, or 8×8 board, then reveal two cards per move. Matching colors stay face up. If the colors differ, they remain visible briefly and then turn face down. Find every pair to complete the board. Your player name, matches, moves, and time are shown in the session panel. Select **New Game** to restart.

## Architecture

- `MemoryMatchArena.Core` contains `GameEngine`, `MemoryCard`, and `FlipResult`. It creates and shuffles boards, validates flips, tracks matches and moves, locks input during mismatches, and detects completion. The engine has no WPF dependency.
- `MemoryMatchArena.App` renders the state exposed by Core. Its card click handler delegates each selection to `GameEngine.TryFlip`; it schedules the display delay and then calls `HideUnmatched`, without implementing matching rules.
- `MemoryMatchArena.Tests` exercises the Core project independently of the desktop interface.

## Tests

```powershell
dotnet test MemoryMatchArena.Tests/MemoryMatchArena.Tests.csproj
```

## Screenshots

Screenshots are not bundled yet. Capture the running window on Windows and add the image here when preparing a submission.

## Bonus Features

The app includes a live session timer, editable player name, keyboard-focusable cards, and a three-size board selector. No multiplayer, leaderboard, or external card API is included.