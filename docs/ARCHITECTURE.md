# Architecture

## Overview

Sparkle Cookie is assembled from two licensed packs plus our custom code:

- **GuiPackAnimalWorld** (`Ricimi` namespace) — provides the look: portrait demo scenes, the animal
  Tile prefabs, UI prefabs, animal sprites, fonts, sounds. This is the visual foundation.
- **PuzzleMatchKit** (`GameVanilla.*`) — the licensed match-3 kit. Used for project scaffolding and as
  a source of level data; **its runtime board is not used** for the playable game (see below).
- **SparkleCookie** (`SparkleCookie.*`) — all of our custom code.

## Why a custom board

The GUI pack's `Game - Portrait` demo is gorgeous but its board (`Canvas/Table`, a `GridLayoutGroup`
of 64 animal Images) is **static — no game logic**. The PuzzleMatchKit board *has* logic but is a
world-space sprite board with a different look. The product decision was to keep the **exact demo board
layout/spacing** and make it playable, so we wrote our own UI-based match controller that drives the
demo's `Table` directly.

## The three scenes (`Assets/SparkleCookie/Scenes/`)

All three are saved from the GUI pack's **Portrait** demo scenes.

| Scene | Built from | Our additions |
|-------|-----------|----------------|
| `Home.unity` | `Home - Portrait` | Kit `GameManager` + `SoundManager` bootstrap (DontDestroyOnLoad); Play button → `Level` |
| `Level.unity` | `Level - Portrait` | `SparkleLevelLoader` on all 90 level buttons (sets level, loads `Game`) |
| `Game.unity` | `Game - Portrait` | `SparkleBoard` controller on a `SparkleBoard` GameObject |

Build order in Player Settings: **Home (0) → Level (1) → Game (2)**.

## The playable board — `SparkleCookie.Board.SparkleBoard`

`Scripts/Board/SparkleBoard.cs`. Self-contained; no PuzzleMatchKit runtime dependency.

- **Auto-wires by hierarchy path** in `Awake`: finds `Canvas/Table` (+ its `GridLayoutGroup` for the
  column count), `Canvas/Top Bar/Score/Text - Score Amount` (TMP), and
  `Canvas/Top Bar/Progress Bar (Half Radial)/Bar (Animation)` (the Sparkle meter fill Image).
- **Loads animal Tile prefabs from Resources** (`Elements UI/Game Items/Tiles/Tile - …`) and extracts
  each animal's sprite from the prefab's `Button/Item` Image.
- **Builds cells once** (`BuildCells`): clears the demo's placeholder tiles and instantiates one
  persistent cell per grid slot from a single template prefab, in row-major sibling order so the
  `GridLayoutGroup` positions them at the exact demo spacing. On each move it only **swaps each cell's
  animal sprite** (`RefreshSprites`) — no destroy/instantiate, no flicker.
  - Each cell's `Button/Item` has an idle **Animator that keyframes the sprite**; `SparkleTileView`
    disables it so our per-type sprite assignment sticks.
- **Gameplay loop** (`OnTileClicked`): flood-fill the connected same-animal group (4-neighbour); if
  `>= minMatch` (2), clear it, add score (`n²·10`), charge the meter, decrement moves, collapse
  columns, refill from the top, refresh sprites, update HUD, check end.
- **Sparkle Surge** (`Scripts/SparkleMeter/SparkleMeter.cs`): pure logic. Each clear adds charge scaled
  by group size × combo chain; idle time drains it; at capacity it fires `Burst` → `SparkleBoard`
  clears the most abundant animal (the "Sparkle Party"). The HUD Half-Radial bar shows `Normalized`.

`SparkleTileView` (`Scripts/Board/SparkleTileView.cs`) is the per-cell view: holds row/col, relays the
button tap to the board, and swaps its animal sprite on demand.

## Navigation & session

- `SparkleCookie.Core.SparkleLevelLoader` (on each demo level button) records the chosen level into
  `SparkleSession.SelectedLevel` (a tiny static) and loads `Game`. It also sets the kit's
  `lastSelectedLevel` if that manager is present, but the board does not require it.
- `Home → Level` uses the GUI pack's own `Ricimi.SceneTransition` (Play button `scene = "Level"`).

## Custom event hook — `SparkleCookie.Core.SparkleHooks`

A static event bus. `SparkleBoard` raises `MatchResolved(count, score)` on every clear — the seam for
analytics or companion powers. (It originally also bridged the kit board; that path is retired.)

## Companions (stubbed meta layer) — `Scripts/Companions/`

`AnimalCompanion` (ScriptableObject, with a reserved `modelPrefab` slot for future 3D), a
`CompanionDatabase`, and `CompanionPower` implementations that act on the `SparkleMeter`. Disabled
behind `CompanionPowers.Enabled`. Owned/equipped state persists via `SaveExtensions` (`sc_` PlayerPrefs
keys). See [ROADMAP_3D_ANIMALS.md](ROADMAP_3D_ANIMALS.md).

## Input & mobile

Active Input Handling stays **legacy** so the GUI pack's `StandaloneInputModule` UI taps work; the board
is UI-button driven, so taps are resolution-independent. Portrait orientation is locked in Player
Settings. `SafeAreaFitter` is available for notch-safe HUD placement.

## Save keys
- Ours: `sc_owned_companions`, `sc_equipped_companion` (see `SaveExtensions`).
- Kit (if bootstrapped from Home): `num_coins`, `num_lives`, `next_level`, etc.
