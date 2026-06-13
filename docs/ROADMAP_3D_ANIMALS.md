# Roadmap: Collectible 3D Animal Companions

The end-goal is a collection of **3D animal characters** the player unlocks and equips, each granting a
gameplay power. The data layer already supports this; only art + UI remain.

## Already in place
- `SparkleCookie.Companions.AnimalCompanion` (ScriptableObject) has a reserved
  **`GameObject modelPrefab`** slot for a 3D model alongside the 2D `portrait2D` used today.
- `CompanionDatabase` registers all companions.
- `CompanionPower` implementations act on the live `SparkleMeter` (the gameplay seam), gated by
  `CompanionPowers.Enabled`.
- Ownership/equip persists via `SaveExtensions` (`sc_owned_companions`, `sc_equipped_companion`).

## Steps to add 3D animals
1. **Import/author FBX models** (one per animal) into a local, git-ignored art folder (e.g.
   `Assets/SparkleCookie/Art/Models/` — `.fbx` stays out of git per policy).
2. **Make a prefab per animal** with the model + an idle Animator; assign it to the companion's
   `modelPrefab`.
3. **Create `AnimalCompanion` assets** (Assets ▸ Create ▸ SparkleCookie ▸ Animal Companion), fill id,
   rarity, cost, `powerId`, `portrait2D`, `modelPrefab`; add them to a `CompanionDatabase`.
4. **Collection screen** (build from GUI-pack popups/avatars): a grid of owned/locked companions with a
   rotating 3D preview (a small `RenderTexture` + camera showing `modelPrefab`).
5. **Unlock flow**: spend coins/stars (kit `CoinsSystem` is bootstrapped) → `SaveExtensions.AddOwned`.
6. **Equip → power**: on level start, resolve the equipped companion's `powerId` via
   `CompanionPowers.Resolve` and call `OnLevelStart(board.Meter)`; flip `CompanionPowers.Enabled = true`.

## Notes
- 3D models render fine over the 2D UI board via a dedicated preview camera / RenderTexture; the board
  itself stays 2D UI.
- Keep powers readable and meter-centric so the collection meaningfully changes how Sparkle Surge plays.
