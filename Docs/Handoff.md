# Handoff: next steps

Last updated 2026-10-28. Read this first in a new session, together with `Docs/Fool's Mate.md` (the design doc, which is the source of truth for rules).

## How to work with the user

- The user is learning Unity. **Default to walkthroughs and reviews. Don't write C# unless the user explicitly says so** ("you write it", "you finish it"). "Let's start", "let's do it", or "work on it" means walk them through doing it themselves.
- When the user writes code, check it for errors and explain each fix briefly.
- Unity edits in the Inspector or scene only land on disk after a save, so ask the user to save the scene, or use File → Save Project for assets, before checking their work.
- Commit only when asked. Work happens on a feature branch, then a PR is merged to `main`.

## Where things stand

Phase 1 (board and pieces) and Phase 2 steps M1–M7 and S1–S3 are done:

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

## Next step: the enemy phase (not started)

Start with a design pass before any code. Read the doc's enemy rules and Track 1 vs Track 2 collisions, then agree with the user on:

- how the enemy picks its moves (simple rules first, some telegraphed)
- how many moves it makes per turn
- whether its moves resolve together with the player's in `MoveResolver`

Enemy pieces moving creates real collisions and swaps, and `MoveResolver` currently only expects the player's own pieces to move. Check rules come after that.

## After the enemy phase

- **Run loop:** lives, next round, shop, and a `LevelDefinition` asset to replace the Tribute and Turn Limit fields on `RoundSetup`.
- **Piece stats:** every piece currently has Priority, Damage, and Health of 1, so every attack captures and every friendly collision ties, meaning both pieces Bounce. The doc's stat table is still TBD.
- **Open design questions** are in the doc's TO-DO list, for example whether sliding pieces pass through enemy pieces that are moving away, and how the Tribute scales.

## Checking code outside Unity (optional)

To compile and test Core without Unity, make a scratch `net` test project, outside the repo, that globs `Assets/_Project/Scripts/Core/*.cs` and `Assets/_Project/Tests/EditMode/*.cs`. Reference NUnit 3.14 and use LangVersion 9 to match Unity. Then run `dotnet test`. Game scripts can be compile-checked by building a copy of `FoolsMate.Game.csproj` (Unity generates it at the repo root) with its Compile items replaced by `Assets/_Project/Scripts/Game/*.cs`. Delete the copy afterwards.
