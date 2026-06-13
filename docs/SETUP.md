# Setup & Build

## Prerequisites
- **Unity 6000.4.10f1** (Unity 6.4) or matching 6000.4.x.
- Android Build Support module (with OpenJDK, SDK & NDK) for device builds.
- The two licensed asset packs (this repo does **not** include them):
  - **PuzzleMatchKit**
  - **Animal World GUI Pack** (`GuiPackAnimalWorld`)

## First-time setup from a clone
1. Create/open the Unity 6.4 project (the repo provides `ProjectSettings/` and `Packages/`).
2. Import the two packs into `Assets/` so you have:
   - `Assets/PuzzleMatchKit/`
   - `Assets/GuiPackAnimalWorld/`
3. Let Unity import. Our code is in `Assets/SparkleCookie/`.
4. (Optional) Re-apply the legacy kit edits in [KIT_MODIFICATIONS.md](KIT_MODIFICATIONS.md). **Not
   required** — the shipping scenes work without them.
5. Open `Assets/SparkleCookie/Scenes/Home.unity` and press Play. Flow: **Home → Level → tap a level →
   Game**.

If the Game board doesn't appear, confirm the GUI pack's Tile prefabs resolve from Resources at
`Demo/Resources/Elements UI/Game Items/Tiles/Tile - …` (that's where `SparkleBoard` loads them).

## Editor preview is portrait
Set the Game view to a **portrait** resolution (e.g. 1080×1920). The build orientation is already locked
to Portrait in Player Settings; the editor Game-view aspect is separate. (A "Sparkle Portrait 1080x1920"
size is added to the Game view by our tooling.)

## Player settings already configured
- Product: **Sparkle Cookie**, id `com.sparklecookiegames.sparklecookie`
- Orientation: **Portrait** (autorotate portrait + upside-down only)
- Android: **IL2CPP**, **ARM64**, **minSdk 24**
- Build scenes: Home (0), Level (1), Game (2)

## Build for Android
1. `File ▸ Build Settings ▸ Android ▸ Switch Platform` (first switch reimports the packs — can take a
   while).
2. Confirm the scene list is Home/Level/Game.
3. `Build` (APK) or `Build` an AAB for the Play Store (IL2CPP/ARM64 already set).

## Re-running the board setup (if a scene loses it)
With the Game scene open, run **menu: `Sparkle Cookie ▸ Install Playable Board Into Open Scene`** to
(re)add the `SparkleBoard` controller. It auto-finds the Table and HUD at runtime.

## Git policy reminder
Images/audio/fonts/models and both vendor packs are git-ignored. Only our `.cs`, `.unity`, `.asset`,
docs, and `ProjectSettings/` are committed. Don't `git add -f` licensed art.
