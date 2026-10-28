# Handoff: next steps

Last updated 2026-10-27. Read this first in a new session, together with `Docs/Fool's Mate.md` (the design doc, which is the source of truth for rules).

## How to work with the user

- The user is learning Unity. **Default to walkthroughs and reviews. Don't write C# unless the user explicitly says so** ("you write it", "you finish it"). "Let's start", "let's do it", or "work on it" means walk them through doing it themselves.
- When the user writes code, check it for errors and explain each fix briefly.
- Unity edits in the Inspector or scene only land on disk after a save, so ask the user to save the scene, or use File → Save Project for assets, before checking their work.
- Commit only when asked. Work happens on a feature branch, then a PR is merged to `main`.

## Where things stand

Phase 1 (board and pieces) and Phase 2 steps M1–M7, S1, and S2 are done:

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
- **Tests** live in `Assets/_Project/Tests/EditMode` (NUnit, EditMode). 120 tests pass. Run them in Unity with Window → General → Test Runner.
- **Design change already in the doc:** the round is won by **kills** (the Tribute, 4 kills in 5 turns at Level 1), not points. Points are shop currency only.
- **Art:**
  - The Preset Manager has Aseprite import defaults: `Assets/Settings/AsepritePiece.preset` for `Art/Sprites/Pieces/**` and `AsepriteTile.preset` for the rest of `Art/**`. Both use 32 PPU, a Center pivot, and no model prefab or animation clips.
  - The board border tiles are 16 px thick. Their file names are spelled "Boarder", a typo the user may want to rename inside Unity.

Right now, everything about kills, points, turns, and the round outcome only appears in the **Console**. When a round ends, the MOVE button greys out and nothing else happens.

## Next step: S3 HUD (not started)

### Part A: the user builds the UI objects (walk them through it)

The Canvas is Screen Space - Camera, scaled to 640×360. The board takes the middle 288 px, which leaves about 160 px on each side.

1. **`LeftPanel`:**
   - Make an empty child of Canvas, anchored left/stretch, with Width 150 and Pos X 75.
   - Add a **Vertical Layout Group** with Padding Left 8 and Top 16, Spacing 8, and Upper Left alignment. Tick Control Child Size for width and height, and untick Child Force Expand.
2. **`RightPanel`:** the same, but anchored right/stretch, with Pos X -75 and Upper Right alignment.
3. **TextMeshPro texts** in `LeftPanel`: `TributeText`, `PointsText`, and `TurnText`. Use font size 16, left alignment, and wrapping disabled.
4. **`RightPanel` contents:**
   - Add `EnergyText`, then move the existing `MoveButton` and `WarningText` into the panel.
   - If MoveButton gets squashed, give it a **Layout Element** with Preferred Width 120 and Preferred Height 32.
   - Drag `EnergyText` into **Planning Input → Energy Text** on `Round`. That slot already works with no new code.
5. **`RoundOverPanel`:**
   - Make a UI Panel child of Canvas, with the Image alpha around 180.
   - Inside it, add `OutcomeText` (TMP, size 24, centered) and `PlayAgainButton` (Button - TextMeshPro).
   - Untick the panel so it starts hidden.
6. Save the scene.

### Part B: the `RoundHud` script

Only write this if the user asks; otherwise walk them through it.

- `Scripts/Game/RoundHud.cs` is a MonoBehaviour with:
  - serialized `TMP_Text` fields: tributeText, pointsText, turnText, outcomeText
  - a serialized `GameObject` roundOverPanel and a `Button` playAgainButton
- Methods:
  - `Refresh(RoundScore score)` sets `Tribute {Kills}/{Tribute}`, `Points {Points}`, and `Turn {TurnsTaken + 1}/{TurnLimit}`. Clamp the turn display to TurnLimit once the round is over.
  - `ShowOutcome(RoundOutcome outcome)` activates the panel and sets the text. `TurnController.DescribeOutcome` already has the wording; move it or share it.
  - `Hide()` deactivates the panel.
- Play Again reloads the active scene with `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`. Check that `Game.unity` is in Build Profiles / Build Settings, or load it by name.
- Wiring:
  - TurnController gets a serialized `RoundHud hud` field.
  - It calls `hud.Refresh(score)` in `Begin` and after `ApplyTurn`, and `hud.ShowOutcome(score.Outcome)` in `EndRound`.
  - Keep the hud optional with null checks, so a missing reference can't stop the round from running. That was a past pain point, when a renamed field broke piece spawning.
- Optionally, move the Console score line into the HUD only.

## After S3

- **Enemy phase:**
  - Rule-based enemy moves, some telegraphed.
  - Enemy pieces moving creates real collisions and swaps. `MoveResolver` currently only expects the player's own pieces to move; enemy collision rules (Track 1 vs Track 2) are in the doc.
  - Check rules.
- **Run loop:** lives, next round, shop, and a `LevelDefinition` asset to replace the Tribute and Turn Limit fields on `RoundSetup`.
- **Piece stats:** every piece currently has Priority, Damage, and Health of 1, so every attack captures and every friendly collision ties, meaning both pieces Bounce. The doc's stat table is still TBD.
- **Open design questions** are in the doc's TO-DO list, for example whether sliding pieces pass through enemy pieces that are moving away, and how the Tribute scales.

## Checking code outside Unity (optional)

To compile and test Core without Unity, make a scratch `net` test project, outside the repo, that globs `Assets/_Project/Scripts/Core/*.cs` and `Assets/_Project/Tests/EditMode/*.cs`. Reference NUnit 3.14 and use LangVersion 9 to match Unity. Then run `dotnet test`. Game scripts can be compile-checked by building a copy of `FoolsMate.Game.csproj` (Unity generates it at the repo root) with its Compile items replaced by `Assets/_Project/Scripts/Game/*.cs`. Delete the copy afterwards.
