# Game Design

## Pillars
1. **Cute & instant** — tap a cluster of animals, watch them pop and tumble. One-handed portrait play.
2. **Sparkle Surge** — a skill/combo layer that turns good chains into a board-wide payoff.
3. **Collect & equip** — animal companions give the meta-game a reason to keep playing (roadmap).

## Core loop
Tap a group of **2+ connected same animals** → they clear → columns collapse → new animals fall in
from the top. Score rewards bigger groups (`group² × 10`). A move limit bounds each level.

## Sparkle Surge (headline mechanic)
- Every clear adds **charge** to the Sparkle meter, scaled by group size **and** the current combo
  chain (back-to-back clears within a short window compound).
- Idle time **drains** the meter, so the reward is for sustained chaining, not slow play.
- At full charge the animals throw a **Sparkle Party**: a burst clears **all of the most abundant
  animal** on the board and awards bonus score, usually triggering a big cascade.
- Shown on the HUD's Half-Radial progress bar.

Tuning lives on `SparkleBoard` (capacity, combo window, drain) and `SparkleMeter`.

## Animal companions (meta layer — stubbed)
Collectible animals the player unlocks and equips. Each can carry a **power** that plugs into the
Sparkle meter, e.g.:
- **Panda — Precharge:** starts each level with the meter partly charged.
- **Cat — Chain:** longer combo window so chains are easier to sustain.

Today companions are data + 2D portraits, disabled behind `CompanionPowers.Enabled`. The 3D collectible
characters are the end-goal — see [ROADMAP_3D_ANIMALS.md](ROADMAP_3D_ANIMALS.md).

## Levels
Difficulty knobs (per level, future): board size, number of animal types, move count, goals (collect N
of an animal), and which companion powers are allowed. `SparkleSession.SelectedLevel` carries the choice
from the Level screen; the Level map exposes 90 buttons.

## What makes it not an asset rip
- A **custom match engine** (`SparkleBoard`) — not the kit's runtime — driving the GUI pack's exact
  board.
- The **Sparkle Surge** meter + Sparkle Party, a mechanic neither pack ships.
- The **companion-power** seam tying a collection meta-game into the core loop.

## Roadmap (near-term)
- Win/Lose popups (reuse GUI pack popups) instead of the MVP "out of moves → Home".
- Real goals + star scoring wired to the HUD's Goals and Stars.
- Lives/coins economy (kit systems are bootstrapped and available).
- Collection screen + companion unlock flow, then 3D models.
