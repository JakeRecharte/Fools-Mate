# Handoff: next steps

Last updated 2026-10-29. Read this first in a new session, together with `Docs/Fool's Mate.md` (the design doc, which is the source of truth for rules).

## How to work with the user

- The user is learning Unity. **Default to walkthroughs and reviews. Don't write C# unless the user explicitly says so** ("you write it", "you finish it"). "Let's start", "let's do it", or "work on it" means walk them through doing it themselves.
- When the user writes code, check it for errors and explain each fix briefly.
- Unity edits in the Inspector or scene only land on disk after a save, so ask the user to save the scene, or use File → Save Project for assets, before checking their work.
- Commit only when asked. Work happens on a feature branch, then a PR is merged to `main`.

## Where things stand

Phase 1 (board and pieces), Phase 2 steps M1–M7 and S1–S3, and the enemy phase are done:

- **Core** (`Assets/_Project/Scripts/Core`, pure C# with `noEngineReferences`, so no Unity APIs):
  - `BoardState` holds the pieces.
  - `MoveTargets` and `TurnPlan` handle planning.
  - `MoveCheck` validates the plan when MOVE is pressed.
  - `MoveResolver` resolves the turn, using `Bounce`, and returns a list of `ResolutionEvent`s.
  - `RoundScore` counts kills toward the Tribute, points for the shop, and turns, and decides the `RoundOutcome`.
- **Game** (`Assets/_Project/Scripts/Game`, MonoBehaviours on the `Round` object in `Scenes/Game.unity`):
  - `RoundSetup` builds the round and holds the Energy, Tribute, Turn Limit, and Random Seed fields.
  - `PlanningInput` handles clicks.
  - `BoardView` draws the board.
  - `TurnController` runs MOVE, then resolve, then score, then either the next turn or the end of the round.
  - `RoundHud` shows Tribute, Points, and Turn, and the Round Over panel. Play Again reloads the scene. TurnController's `hud` slot is optional.
- **Tests** live in `Assets/_Project/Tests/EditMode` (NUnit, EditMode). 120 tests pass. Run them in Unity with Window → General → Test Runner.
- **Design change already in the doc:** the round is won by **kills** (the Tribute, 4 kills in 5 turns at Level 1), not points. Points are shop currency only.
- **Art:**
  - The Preset Manager has Aseprite import defaults: `Assets/Settings/AsepritePiece.preset` for `Art/Sprites/Pieces/**` and `AsepriteTile.preset` for the rest of `Art/**`. Both use 32 PPU, a Center pivot, and no model prefab or animation clips.
  - The board border tiles are 16 px thick. Their file names are spelled "Boarder", a typo the user may want to rename inside Unity.

## S3 HUD (done)

- **UI layout** (Canvas is Screen Space - Camera at 640×360):
  - `LeftPanel` is anchored left/stretch and holds TributeText, PointsText, and TurnText.
  - `RightPanel` is anchored right/stretch and holds Energy Text, MoveButton (with a Layout Element), and WarningText.
  - `RoundOverPanel` is a black overlay at about 70% alpha, hidden at start, holding OutcomeText and Replay Button.
- **Wiring:** `RoundHud` is on `Round`, and Turn Controller → Hud points at it.
- `TurnController.DescribeOutcome` is public, so the HUD and the Console share the outcome wording.
- **Pitfall:** Inspector changes made in Play mode are lost, so check that edits are saved with Play off.

## Enemy phase (done, PR #4)

- `EnemyPlanner` (Core) picks the enemy's moves each turn: greedy captures, plus moving pieces to safety. They're telegraphed as arrows to their destinations.
- `MoveResolver` plays both sides' plans at once, including collisions between enemy and player pieces.
- Sliding pieces can't overtake your own planned movers, and you can't move onto an enemy that's telegraphed to leave.
- Removed pieces are kept, so the Box and the captured row can show them.
- The design doc has the enemy rules and the Box.
- Check rules still aren't done.

## Since PR #4 (on branch `main-menu`, not merged yet)

- **Move arrows (done):** `MoveArrow` builds each arrow from one 32×32 sprite per cell, from the art in `Art/Sprites/Markers`.
  - **Straight moves** use `StartAndEndPoints` and `Middle`, with 4 slots each: Up, Right, Down, Left. Those files still contain old diagonal frames that nothing uses; they can be deleted, but if frames move, check the prefab's slots afterwards.
  - **Knights** use the straight ends and middles, plus `Bends` at the corner. A knight-only end set was tried and then removed, once the regular ends lined up.
  - **Diagonals** zig so the band stays straight. Each step puts a Bottom piece (`DiagonalMiddle`) on the lower of the two cells beside it, and every cell the move passes gets a Top piece. So the band leaves the start, and enters the destination, through a side or bottom edge rather than a corner. The ends come from `DiagonalStartAndEndPoint`, picked by direction like the straight ends: the move's direction at the start, the opposite one at the destination. UpRight/DownLeft share one Top/Bottom pair and UpLeft/DownRight the other. The 1px offset between the Top and Bottom pieces is intentional; it keeps the line straight.
  - The marker sprites are named after their Aseprite tags (`End_Up`, `Diagonal_TopUpRight`, `DiagonalEnd_DownLeft`, ...). The importer keeps a sprite's existing name when it reimports, so a file that started with one frame keeps that frame's old name until it's renamed in the `.meta`.
- **Move markers:** `BoardView` no longer draws ghosts on empty destinations. The player marker goes on an enemy piece you're moving onto, the enemy marker on your piece a telegraphed enemy moves onto, and the clash marker on any cell both sides move into.
- **`ElementNames` attribute:** names the elements of the `MoveArrow` sprite arrays in the Inspector ("Up", "TopUpRight", ...) so it's clear which frame goes in which slot.
- **Main menu (in progress):**
  - `Scenes/MainMenu.unity` is first in the build list, ahead of `Game`.
  - `MainMenu` wires the Start Run button (loads `Game` by name) and the Quit button.
  - `MenuSelection` and `SelectOnHover` let the keyboard and the mouse share one highlight.
- `.idea/` is now in `.gitignore`.

## Next steps

- **Run loop:** lives, next round, shop, and a `LevelDefinition` asset to replace the Tribute and Turn Limit fields on `RoundSetup`.
- **Piece stats:** every piece currently has Priority, Damage, and Health of 1, so every attack captures and every friendly collision ties, meaning both pieces Bounce. The doc's stat table is still TBD.
- **Open design questions** are in the doc's TO-DO list, for example whether sliding pieces pass through enemy pieces that are moving away, and how the Tribute scales.

## Checking code outside Unity (optional)

To compile and test Core without Unity, make a scratch `net` test project, outside the repo, that globs `Assets/_Project/Scripts/Core/*.cs` and `Assets/_Project/Tests/EditMode/*.cs`. Reference NUnit 3.14 and use LangVersion 9 to match Unity. Then run `dotnet test`. Game scripts can be compile-checked by building a copy of `FoolsMate.Game.csproj` (Unity generates it at the repo root) with its Compile items replaced by `Assets/_Project/Scripts/Game/*.cs`. Delete the copy afterwards.
