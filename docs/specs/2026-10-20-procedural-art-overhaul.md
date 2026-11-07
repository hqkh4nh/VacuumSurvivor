---
title: Procedural art overhaul — "Flat dưới trăng"
status: implemented     # draft | approved | implemented
lane: fast              # chosen by the human partner; red-green per task, one gate run + one commit per acceptance criterion
---

# Procedural art overhaul - Detailed Spec

> Replace the all-circles placeholder look with one deliberate code-generated art style, "moonlit flat": a limited palette, one consistent top-light shading model, and a distinct silhouette for every entity. No imported art. The explicit goal is taste and restraint — it must not read as generic "everything glows" AI slop. Covers the player robot, the three trash enemies, the boss cat, both pickups, and a real floor to replace the empty void.

## Acceptance at a glance
- **AC-1** (MUST) - A single `Palette` static class shall be the only source of the game's art colours (one warm family for the floor, one cool "moonlight" family, an ink outline, and one accent reserved for power — suction + dock). VSBuild's art colours shall reference `Palette`, with no ad-hoc art colour literals left in the builders.
- **AC-2** (MUST) - Every generated sprite shall pass through one shared shading step: a flat base, a lighter top band, a darker bottom band, and a 1px ink outline, in hard 2-tone (no smooth gradient). The whole set shall look like one style.
- **AC-3** (MUST) - Each entity shall have a distinct, purpose-built silhouette — robot chassis, dust-bunny fuzzy blob, breadcrumb angular cluster, hair-clump tangle, cat (kept proportions + a tail), dust mote, dock pad — so no two enemy types share a shape. All three trash enemies carry small eyes; the cat keeps its proportions and gains a tail.
- **AC-4** (MUST) - The built scene shall have a tiled, low-contrast parquet floor behind all entities (strongly negative sorting), replacing the solid-colour void, with contrast low enough that entities still pop.
- **AC-5** (MUST) - `ProceduralSprite` shall select its generator via a `Kind` enum (not `bool cat`/`bool eyes`), and `VSBuild` shall wire each prefab/player child to the right `Kind` with `Palette` colours and add the floor, so a rebuilt content + scene show the new look.
- **AC-6** (SHOULD) - Glow shall be rationed to things that carry meaning: enemy/cat eyes, the robot lens, the suction ring, the dock ring, and the dust mote's accent sparkle. Nothing else glows, blooms, or uses a smooth gradient.

## Scope
- **In:** `Palette` class; shared `Shade` step + per-entity shape generators in `SpriteFactory`; `ProceduralSprite` `Kind` enum migration; parquet floor generation + scene placement; VSBuild wiring for player, 3 trash enemies, boss cat, dust mote, dock; an EditMode smoke test for the generators.
- **Out:** imported/real art and any import pipeline; sprite animation / squash-stretch; robot facing its movement direction; particles, screen shake, hit numbers and other "juice"; per-enemy colour randomisation; changing any gameplay logic, stats or balance; UI/HUD restyle.

## Requirements & design notes

The three anti-slop levers are the design, in priority order: **(1) palette discipline** — a small fixed palette from one source; **(2) one light direction** — the same `Shade` model on every sprite; **(3) distinct silhouettes** — shape, not colour or effects, tells the entities apart. Restraint is a requirement, not a preference: a gradient-mush, everything-glows result fails AC-2 and AC-6 even if it compiles.

- **`Palette` (`Assets/Scripts/Art/Palette.cs`):** a static class of named `Color`s — floor board/seam (warm), moonlight ambient (cool, used for the top-light tint), ink outline (near-black, not pure black), one accent (the existing `#F472B6` pink, reserved for suction + dock), and per-entity base colours (robot, dust-bunny, breadcrumb, hair-clump, cat, dust mote). ~8-9 colours total. VSBuild's current art colour constants (`RobotBody`, `NightBg`, enemy colours) are migrated to reference `Palette`; non-art constants are left alone (surgical).
- **Shared shading (`SpriteFactory.Shade`):** takes a filled alpha mask + base colour and returns the finished pixels: base fill, top ~40% tinted toward moonlight, bottom edge darkened, then a 1px ink outline traced from the mask. Hard bands, not a lerp across the whole height — the hard 2-tone is what reads as "drawn" rather than "rendered". Every generator ends by calling `Shade`.
- **Shape generators:** replace `CircleWithEyes`/`CatSprite` with one mask builder per `Kind`:
  - `Robot` — rounded-square chassis + a front brush/mouth bar + a round lens on top (lens is the one glow). Static facing (up); no directional rotation.
  - `DustBunny` — a circle whose radius is perturbed by low-frequency noise, plus a few stray wisp pixels; small eyes.
  - `Breadcrumb` — 3-4 overlapping angular quads/triangles, hard corners; small eyes.
  - `HairClump` — a tangled oval with several protruding curved strands; small eyes.
  - `Cat` — oval body + two triangular ears + a curved tail + larger eyes; keep current proportions (re-shaded, tail added).
  - `DustMote` — a small 4-point sparkle with a faint accent glow.
  - `Dock` — a flat rounded-rectangle mat + a subtle accent ring; sits below entities.
  - `Led` / `Ring` — the player's existing LED dot and translucent suction ring keep their current simple shapes (not re-shaded); they already carry meaning.
- **Floor:** generate one parquet tile texture (warm board, slightly darker seams, deliberately low contrast) and tile it across a large background `SpriteRenderer` (wrap/repeat) at a strongly negative `sortingOrder`, sized to cover the camera's roam area. The dark camera clear stays visible past the floor edge as the night. One `BuildFloor()` step added to `BuildGameScene()`.
- **`ProceduralSprite` migration:** replace the `bool cat` + `bool eyes` fields with `enum Kind { Robot, DustBunny, Breadcrumb, HairClump, Cat, DustMote, Dock, Led, Ring }` and a `Kind` field; `Apply()` switches on it. This changes the component's serialized fields — the scene and prefabs are regenerated by `VSBuild`, so there is no hand migration, but the old serialized values are abandoned by design (VSBuild sets `Kind` explicitly on every renderer).

- **Global constraints:**
  - Namespace `VacuumSurvivor`; runtime asmdef `VacuumSurvivor`, tests asmdef `VacuumSurvivor.Tests.EditMode`.
  - No gameplay/stat/balance change: colliders, layers, speeds, health and all ScriptableObject values stay as they are. Sprite pixel sizes may change only as needed for the new shapes to read, without changing collider radii.
  - Textures are generated at runtime in `Awake`/`Apply`, same as today (no asset files written for sprites).
  - Build/verify by driving the open Editor via unity-cli (see the build-workflow memory): `BuildContent()` + `BuildGameScene()`, `recompile`, `run_tests` (no `--format` param), then a play-mode screenshot with `timeScale=1`.

## Tasks

Build order keeps every commit compiling: the generators are added **additively** in AC-3 (the old `CircleWithEyes`/`CatSprite` stay), and are removed only in AC-5 where their last callers migrate to the `Kind` enum.

### AC-1 · Palette
- [x] **T1** [S] The art palette
  - files: `Assets/Scripts/Art/Palette.cs`
  - contract: `public static class Palette` exposing `public static readonly Color` fields: `FloorBoard`, `FloorSeam`, `Moonlight`, `Ink`, `Accent` (`#F472B6`), `Robot`, `DustBunny`, `Breadcrumb`, `HairClump`, `Cat`, `DustMote`, `NightBg`. Low-saturation values: one warm family (floor / breadcrumb / hair-clump / cat), one cool family (moonlight / robot), `Ink` near-black (not pure), `Accent` the only vivid colour. No behaviour.
  - done when: `unity recompile` reports 0 errors
- [x] **T2** [S] Migrate VSBuild's art colours to Palette (depends on T1)
  - files: `Assets/Editor/VSBuild.cs`
  - contract: replace the art colour constants currently in VSBuild (`RobotBody`, `NightBg`, the enemy body colours) with references to `Palette`, and set the translucent suction-ring tint from `Palette.Accent` at its existing alpha. Leave non-art constants untouched. No wiring change yet — colours only.
  - done when: `unity recompile` 0 errors; `run_tests -- --mode EditMode --timeout 180000` stays green

### AC-2 · Shared shading model
- [x] **T3** [M] `Shade` step + mask helpers + first smoke tests (depends on T1)
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: `static Color[] Shade(float[] mask, int px, Color body)` — `mask` is row-major coverage 0..1. Fill `body` scaled by coverage; rows in the top 40% lerp toward `Palette.Moonlight` (~0.25); rows in the bottom ~12% darken (~0.35); any covered pixel with an uncovered 4-neighbour or texture edge becomes `Palette.Ink` (1px inner outline). Uncovered pixels stay transparent. Bands are stepped by row region, **not** a per-row lerp. Add private mask builders reused by AC-3 (filled circle, rounded rect, triangle fill).
  - tests: `shade outlines the mask edge with ink` · `shade lightens the top band and darkens the bottom band` · `shade leaves uncovered pixels transparent`
  - done when: `run_tests -- --mode EditMode --timeout 180000` green

### AC-3 · Distinct silhouettes
- [x] **T4** [M] `SpriteKind` enum + `Build` dispatch + creature generators (depends on T3)
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: `public enum SpriteKind { Robot, DustBunny, Breadcrumb, HairClump, Cat, DustMote, Dock, Led, Ring }`; `public static Sprite Build(SpriteKind kind, Color body, int px)` at `PixelsPerUnit = 64`. Creature masks → `Shade`, then overlay bright extras at full brightness (not shaded): Robot = rounded-square chassis + front mouth bar, lens on top; DustBunny = noise-perturbed circle + stray wisps + small eyes; Breadcrumb = union of 3-4 angular quads + small eyes; HairClump = oval + protruding strands + small eyes; Cat = oval body + two ear triangles + a curved tail + larger eyes. **Add additively — leave `CircleWithEyes`/`CatSprite` in place.**
  - tests: `each creature kind returns a non-null sprite of the requested size` · `each creature kind covers pixels and stays within bounds` · `the creature kinds are not all the same silhouette`
  - done when: `run_tests -- --mode EditMode --timeout 180000` green
- [x] **T5** [S] Pickup and player-child generators (depends on T4)
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: extend `Build` for `DustMote` = 4-point star mask → `Shade` + a faint `Palette.Accent` glow overlay; `Dock` = rounded-rectangle mat → `Shade` + a subtle `Palette.Accent` ring overlay; `Led` and `Ring` = plain filled circle, **not** shaded (the suction ring keeps its translucent SpriteRenderer tint).
  - tests: `dock and dust mote return non-null in-bounds sprites` · `led and ring are plain filled circles`
  - done when: `run_tests -- --mode EditMode --timeout 180000` green

### AC-4 · Parquet floor
- [x] **T6** [M] Floor tile + floor object in the scene (depends on T1)
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Scripts/Art/ProceduralFloor.cs`, `Assets/Editor/VSBuild.cs`
  - contract: `SpriteFactory.FloorTile(int px = 512)` → a fully-opaque parquet `Sprite`: `Palette.FloorBoard` planks (row-offset brick layout) with thin `Palette.FloorSeam` seams, deliberately low contrast. `ProceduralFloor : MonoBehaviour` (`[RequireComponent(SpriteRenderer)]`): on `Awake` build the tile and assign it; serialized `int pixels = 512`. `VSBuild.BuildGameScene()` gains a `BuildFloor()` step creating a floor GameObject with `ProceduralFloor`, scaled to cover the roam area (~60×60 units) at `sortingOrder = -100`.
  - tests: `floor tile is non-null, the requested size, and fully opaque`
  - done when: `unity recompile` 0 errors; `BuildGameScene()` runs clean and the built scene shows the floor behind entities (E2E screenshot)

### AC-5 · ProceduralSprite Kind migration + wiring
- [x] **T7** [L] `ProceduralSprite` → `Kind`, VSBuild wiring, remove orphaned generators (depends on T4, T5, T6)
  - files: `Assets/Scripts/Art/ProceduralSprite.cs`, `Assets/Editor/VSBuild.cs`, `Assets/Scripts/Art/SpriteFactory.cs`
  - contract: `ProceduralSprite` — replace `bool cat` / `bool eyes` with `[SerializeField] SpriteKind kind` (+ `Kind` property); keep `color`/`pixels`; `Apply()` → `sprite = SpriteFactory.Build(kind, color, pixels)`. VSBuild — set `.Kind` + `.Color` (from `Palette`) on every `ProceduralSprite`: player = `Robot`, LED = `Led`, SuctionRing = `Ring`, BuiBong = `DustBunny`, VunBanhMi = `Breadcrumb`, CucTocRoi(+Nho) = `HairClump`, ConMeo = `Cat`, Dust = `DustMote`, ChargingDock = `Dock`; set each enemy `EnemyData.bodyColor` from `Palette` to match. Now **remove the orphaned `CircleWithEyes` / `CatSprite`** from SpriteFactory.
  - done when: `unity recompile` 0 errors; `BuildContent()` + `BuildGameScene()` run clean; `run_tests -- --mode EditMode --timeout 180000` all green (new smoke tests + the existing 43)

### AC-6 · (no task — satisfied by AC-2 / AC-3; glow is only overlaid where those tasks specify, and the rationing is confirmed in the E2E screenshot)

## End-to-end verification
1. `unity command eval … 'return VacuumSurvivor.EditorTools.VSBuild.BuildContent();'` then `… BuildGameScene();` — both return success.
2. `unity recompile` → 0 errors; `unity status` → `ready`.
3. `unity command run_tests -- --mode EditMode --timeout 180000` → all green (new `SpriteFactoryTests` + the existing suite).
4. Play-mode screenshot: `editor_play`, `eval 'UnityEngine.Application.runInBackground=true;'`, `wait_for Time.frameCount greaterThan …`, force `Time.timeScale=1`, then `capture_game_view`. Confirm by eye: a low-contrast parquet floor sits behind everything (no empty void); the robot reads as a chassis with a lens, not a circle; the three trash types have visibly different silhouettes each with small eyes; the boss cat shows ears + a tail; dust motes and the dock read as themselves; glow appears only on eyes / lens / suction ring / dock ring / dust motes — no gradient-mush or everything-glows look; the whole screen reads as one deliberate "moonlit flat" style. Save one frame as evidence, show it, then delete it (it lands under `Assets/Screenshots/`).

## Open questions
<!-- none — eyes-on-all-trash and keep-cat-proportions decided in brainstorming -->
