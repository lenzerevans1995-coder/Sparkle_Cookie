# ✨ Sparkle Cookie

A portrait mobile (phone + tablet) **tap-to-blast match game** for Android, built in Unity 6.

Tap any group of **2+ adjacent matching animals** to clear them; tiles fall and refill. Every clear
charges the **Sparkle Surge** meter — when it fills, the animals throw a **Sparkle Party** that bursts
across the board and clears the most abundant animal. Collect **animal companions** (2D now, 3D on the
roadmap) that grant active powers.

> Note on the name: the project/repo is named "Sparkle Cookie", but the gameplay theme is the
> **Animal World** art set (no cookies). The title can be rebranded later — the code is theme-agnostic.

---

## ⚠️ This is a CODE-ONLY repository

To respect the licenses of the asset packs this game is built on, **this repo contains only our custom
code and documentation.** It does **not** contain:

- `Assets/PuzzleMatchKit/` — licensed match-3 kit (used for project scaffolding/level data)
- `Assets/GuiPackAnimalWorld/` — licensed Animal World UI/art pack (scenes, prefabs, animal sprites)
- any images, audio, fonts, or 3D models

A fresh clone **will not run** until those two packs are re-imported locally. See **[SETUP.md](SETUP.md)**.

---

## How it's built (short version)

- **Screens are built from the GUI pack's Portrait demo scenes** (`Home - Portrait`,
  `Level - Portrait`, `Game - Portrait`), saved into `Assets/SparkleCookie/Scenes/`.
- The demo Game board is a static UI grid with no logic, so the playable match-3 is **our own custom
  controller** (`SparkleBoard`) that drives that exact grid — same cell size (130px), spacing (3px),
  frame and HUD — using the pack's animal Tile prefabs for the look.
- The **Sparkle Surge** meter is our custom mechanic, shown on the HUD's Half-Radial progress bar.

See **[ARCHITECTURE.md](ARCHITECTURE.md)** for the full picture.

## What's tracked

```
Assets/SparkleCookie/
  Scripts/Board/        SparkleBoard (the playable match-3), SparkleTileView, SparkleSession
  Scripts/SparkleMeter/ SparkleMeter (the Sparkle Surge logic)
  Scripts/Core/         SparkleHooks, SparkleLevelLoader, SaveExtensions, SafeAreaFitter
  Scripts/Companions/   AnimalCompanion, CompanionDatabase, CompanionPower (stubbed meta layer)
  Scripts/Config/       SparkleTheme
  Scripts/Editor/       SparkleSetupTools
  Scenes/               Home.unity, Level.unity, Game.unity   (our scenes; reference pack assets)
docs/                   this folder
ProjectSettings/        Android/portrait/input config (text)
.gitignore
```

## Docs index

| Doc | Contents |
|-----|----------|
| [SETUP.md](SETUP.md) | Clone setup, re-importing the packs, Android build, run on device |
| [GAME_DESIGN.md](GAME_DESIGN.md) | Pillars, Sparkle Surge, companions, roadmap |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Scenes, the custom board, navigation, save keys |
| [KIT_MODIFICATIONS.md](KIT_MODIFICATIONS.md) | Changes made to vendor files (for reproducibility) |
| [ROADMAP_3D_ANIMALS.md](ROADMAP_3D_ANIMALS.md) | Plan for collectible 3D animal characters |

## Tech
- Unity 6 (6000.4) · legacy Input + StandaloneInputModule (UI taps) · TextMeshPro HUD
- Android, portrait only, IL2CPP / ARM64, minSdk 24, app id `com.sparklecookiegames.sparklecookie`
