# Vendor file modifications

This repo is code-only and does not commit the licensed packs, so any change we make to a vendor file
is recorded here for reproducibility. Re-apply these after re-importing the packs (see SETUP.md).

## PuzzleMatchKit

The playable game uses our own `SparkleBoard`, **not** the kit's runtime board, so the kit edits below
are **optional/legacy** — they are harmless and unused by the shipping game. They remain only so the
kit's own `GameScene` (if you open it) still compiles against `SparkleHooks`.

### `Assets/PuzzleMatchKit/Scripts/GameVanilla/Game/Scenes/GameScene.cs`

1. In `DestroyBlock(...)`, after `UpdateScore(score);`, one line was added:
   ```csharp
   // [SPARKLE COOKIE HOOK #1] feed the Sparkle Surge combo meter. See docs/KIT_MODIFICATIONS.md
   SparkleCookie.Core.SparkleHooks.RaiseMatchResolved(blocksToDestroy.Count, score);
   ```
2. A public method `SparkleBurstClear(List<GameObject> tiles)` was added just before `ApplyPenalty()`
   (a board-wide clear reusing `DestroyTileEntity`/`ApplyGravityAsync`). Used only by the retired
   kit-board burst path; safe to omit if you don't open the kit's GameScene.

### Block prefabs (`Assets/PuzzleMatchKit/Prefabs/Blocks/Block1..6.prefab`)
Their `SpriteRenderer.sprite` was repointed to GUI-pack animal sprites, and those animal sprites' PPU
was tuned for ~0.62 world units. **Legacy** — only affects the kit's own world-space board, which the
shipping game doesn't use. Skip unless you want the kit's GameScene reskinned too.

## GuiPackAnimalWorld

No source edits. We only **read** its demo scenes/prefabs/sprites and saved copies of the three Portrait
demo scenes into `Assets/SparkleCookie/Scenes/` (our files). The animal sprites under
`Demo/Sprites/Icons/Game/Animals/*/Eyes Open.png` may have had their PPU tuned by the step above; this
does not affect UI rendering.

## If you skip all kit edits
The shipping game (Home/Level/Game from `Assets/SparkleCookie/Scenes/`) works **without** any vendor
edits, because `SparkleBoard` is self-contained and `SparkleHooks` lives in our assembly. The only
requirement is that both packs are present so the demo scenes/prefabs/sprites resolve.
