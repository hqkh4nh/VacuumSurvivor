---
title: Completable spine
status: implemented      # draft | approved | implemented
lane: fast               # user directive ("làm nhanh, test những thứ cần test") + boss-cat/weapons precedent; the four pure kernels are still EditMode-tested, scene wiring is proven by the build
---

# Completable spine - Detailed Spec

> Turn the one-room slice into a game you can play from start to finish: boot into a main menu, play through three rooms (Phòng khách → Phòng ngủ → Nhà bếp) that keep your weapons and recharge your battery between them, beat the final boss (Máy hút bụi cũ) to win, and see a score saved to a top-5 high-score table. Reuses the existing enemies, weapons and art; new enemy types, the other weapons and audio are deliberately left for later specs.

## Acceptance at a glance
- **AC-1** (MUST) - The campaign shall run three levels inside the single `Game` scene: beating a non-final boss advances to the next level with the battery fully recharged and weapons/upgrades kept; the advance-vs-win decision lives in a pure, EditMode-tested `Campaign` kernel.
- **AC-2** (MUST) - The final boss Máy hút bụi cũ shall cycle Hút → Thổi → Lao with rests and telegraphs (timing in a pure, EditMode-tested `BossVacuumBrain`); in the scene it pulls the robot, emits a ring of Bụi bông, telegraphs then dashes, takes damage like other trash, is not knocked back or recycled, and killing it wins the whole game.
- **AC-3** (MUST) - End of run (win or loss) shall compute a score `dust×10 + kills×5 + seconds×2 (+5000 if won)` via a pure kernel, record it into a top-5 high-score table (pure kernel) persisted as JSON, and both pure kernels shall be EditMode-tested.
- **AC-4** (MUST) - A `MainMenu` scene shall offer Chơi / Hướng dẫn / Điểm cao / Thoát; the end screen shall show the run stats + score and offer Chơi lại and Về menu; the HUD shall show the current room name and a brief "đã dọn sạch [phòng]" banner appears between levels.
- **AC-5** (MUST) - `VSBuild` shall create the three level `WaveData`, the vacuum-boss `EnemyData`/prefab/sprite, the `MainMenu` scene and the level banner, and set the build-settings scene order, so a fresh `BuildContent` + build of both scenes boots to the menu and is playable through to the win screen.

## Scope
- **In:** `Campaign` kernel + GameManager campaign flow (level index, boss-defeated → advance/full-recharge/keep-upgrades or win, level reset, `LevelChanged` event); EnemySpawner reconfigure between levels (swap WaveData + boss prefab, clear live trash) and boss-per-level; `BossVacuumBrain` + `BossVacuum` behaviour; `SpriteKind.Vacuum`; three level `WaveData` (reusing the two existing enemy behaviours, differing by intensity/boss/floor tint); `RunStats` score kernel + a tracker; `HighScoreTable` kernel + a JSON `SaveSystem`; end-screen stats/score + "Về menu"; `MainMenu` scene with instructions + high-score panel; HUD room label + between-level banner; all `VSBuild` wiring; EditMode tests for the four pure kernels.
- **Out:** New enemy types (Vụn bánh mì already exists as data; Tất lẻ / Cục LEGO / Dây sạc / Vũng nước are future specs); the other three weapons (Vệt lau, Tia LIDAR, Còi báo động); audio / AudioManager; floor-drop power-ups (giẻ lau, chổi vàng, túi bụi); hand-built obstacle maps / Tilemaps per room; WebGL build; score breakdown by enemy type; pause menu rework.

## Requirements & design notes

The whole run stays in one `Game` scene (no per-room scene loads). A level is just data swapped into the live objects; the enemy pools are shared because all three rooms draw from the same two behaviours.

**Campaign (AC-1).** A pure `Campaign` kernel holds the level count and current index and answers one question: on a boss defeat, does the run advance to the next level or win? `GameManager` owns a `Campaign` and a per-level config array (`LevelConfig[]`, a plain serializable struct: display name, `WaveData`, `bossPrefab`, `floorTint`, `duration`). On boss defeat `GameManager.BossDefeated()` asks the campaign; if advancing it: resets `Elapsed` to 0, clears `bossPhase`, fully recharges the battery, reconfigures the spawner with the next level's wave + boss and clears live trash, retints the floor, raises `GameEvents.LevelChanged(index, name)`; if the last level, calls `Win()`. Weapons and upgrades are untouched, so they carry over for free. `BossCat.Die()` changes from calling `GameManager.Win()` directly to calling `GameManager.BossDefeated()`.

**Between-level transition.** Beating a boss opens a short banner ("Đã dọn sạch [phòng]!") before the next level's spawning starts. Model it as a new `GameManager` state `Transition` (a timer, ~2s, driven by unscaled time so it is deterministic); while in `Transition` the spawner is idle and `Elapsed` does not advance. The robot stays controllable (no `timeScale=0`, so FixedUpdate keeps running and the banner shows over live play). After the timer the state returns to `Playing` and the next level begins.

**Final boss (AC-2).** Mirror the `BossCat` split exactly: a pure `BossVacuumBrain` (no Unity types, under the `VacuumSurvivor` runtime asmdef) owns the attack cycle and all timing; `BossVacuum : EnemyBase` applies the brain's per-frame decision to the world and overrides only `Move()`/`Die()`. Cycle, each separated by a `rest` (1.5s): **Hút** (3s, brain flags `Pulling`; MonoBehaviour pulls the player's Rigidbody toward the boss ~3 u/s), **Thổi** (one-shot `BlowBurst`; MonoBehaviour spawns 8 Bụi bông on a ring via `EnemyBase.SpawnRequest`), **Lao** (brain flags `DashTelegraphing` for 1s then `DashActive` for a fixed duration; MonoBehaviour picks the dash direction toward the player at telegraph start and drives a fast straight dash, tinting the sprite during the telegraph like the cat's slap). `RecycleWhenFar => false`; prefab `knockback = 0`. `Die()` calls `GameManager.BossDefeated()` (which, being the final level, wins). It raises the same `BossSpawned`/`BossHealthChanged` events so the existing boss HP bar works; label copy via the level name.

**Score & high scores (AC-3).** A pure `RunStats` holds `dust`, `kills`, `seconds` (ints) and `Score(bool won) => dust*10 + kills*5 + seconds*2 + (won ? 5000 : 0)`. A small `RunTracker` MonoBehaviour accumulates during the run: `kills` from `EnemyBase.EnemyDied`, `dust` from dust actually collected (player pickup), `seconds` from total `Elapsed` across levels; on `Won`/`Lost` it finalises, hands the entry to `SaveSystem` and lets the end screen read it. A pure `HighScoreTable` inserts an entry and keeps the top 5 sorted by score descending (ties keep existing order). `SaveSystem` is the thin JSON layer: load/save the table as JSON in `Application.persistentDataPath` via `JsonUtility`, tolerating a missing/corrupt file by starting empty. The table kernel is tested; the file I/O is not.

**Frame screens (AC-4).** `MainMenu` is its own scene built by `VSBuild`, scene index 0 (Game is index 1). Buttons: **Chơi** loads `Game`; **Hướng dẫn** toggles a static panel explaining WASD / auto-attack / battery / charging dock; **Điểm cao** shows the top-5 table from `SaveSystem`; **Thoát** calls `Application.Quit()`. The end screen (win and loss) gains the run stats (time survived, kills, dust, level reached) + score, records the score on show, and adds a **Về menu** button beside **Chơi lại** (loads `MainMenu`). The HUD gains the current room name (updated on `LevelChanged`); a `LevelBanner` UI shows the "đã dọn sạch" line during the `Transition` state.

**Build wiring (AC-5).** `VSBuild.BuildContent` additionally creates: `WaveData_PhongKhach` (reuse/retune), `WaveData_PhongNgu`, `WaveData_NhaBep` (progressively higher spawn rate / cap / duration, drawing from the existing enemy data), the vacuum boss `EnemyData` + prefab (`SpriteKind.Vacuum`, `RecycleWhenFar=false`, `knockback=0`, Enemy layer + collider) and a level-2 cat variant at 600 HP. `BuildGameScene` wires the `LevelConfig[]` into `GameManager`, adds the `LevelBanner` to the canvas, and leaves the spawner to be driven by the campaign. A new `BuildMainMenu` builds the menu scene. Build-settings order is set so index 0 is `MainMenu`. `GameManager.Restart` keeps reloading the `Game` scene; "Về menu" loads `MainMenu`.

- **Global constraints:**
  - Namespace `VacuumSurvivor`; runtime asmdef `VacuumSurvivor`, tests asmdef `VacuumSurvivor.Tests.EditMode`; new pure kernels live under the runtime asmdef and are tested in EditMode.
  - Vietnamese UI uses legacy `UnityEngine.UI.Text` + `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`. Room names: `Phòng khách`, `Phòng ngủ`, `Nhà bếp`. Final win copy stays `NHÀ SẠCH BONG!`.
  - Vacuum boss stats (GDD Màn 3): maxHealth `800`, moveSpeed `1.5`, dustValue `60`. Hút pull `3` u/s for `3`s; Thổi `8` Bụi bông on a ring; Lao telegraph `1`s then dash; rest `1.5`s between attacks. Cat at level 2: maxHealth `600`.
  - New enemies/pickups go on the `Enemy` layer with a collider so `SuctionWeapon` and contact damage work; the boss is instantiated (not pooled) so `Die()` fires its dust + the win flow.
  - Score formula and the +5000 win bonus are exactly as in GDD §1.8.3.

## Tasks

### AC-1 · Three-level campaign
- [ ] **T1** [S][P] `Campaign` decision kernel
  - files: `Assets/Scripts/Core/Campaign.cs`, `Assets/Tests/EditMode/CampaignTests.cs`
  - contract: `Campaign(int levelCount)`; `int Index` (0-based, starts at 0); `bool IsFinalLevel => Index >= levelCount - 1`; `bool BossDefeated()` advances `Index` by one when not final and returns `true` (play next), or leaves `Index` at the last level and returns `false` (win) when already final. `Index` never exceeds `levelCount - 1`.
  - tests: `starts at level zero` · `defeating a non-final boss advances and returns true` · `defeating the final boss returns false and index stays` · `index never exceeds last`
  - done when: `unity command run_tests -- --mode EditMode` green for `CampaignTests`
- [ ] **T2** [M] GameManager campaign flow + level config (depends on T1)
  - files: `Assets/Scripts/Core/GameManager.cs`, `Assets/Scripts/Core/GameEvents.cs`, `Assets/Scripts/Player/PlayerStats.cs`, `Assets/Scripts/Player/BatteryModel.cs`, `Assets/Scripts/Enemies/EnemySpawner.cs`
  - contract: `struct LevelConfig { string name; WaveData waves; GameObject bossPrefab; Color floorTint; float duration; }`. `GameManager` holds `LevelConfig[] levels` + a `Campaign`; `State` gains `Transition`. `BossDefeated()`: if `Campaign.BossDefeated()` returns true → `Elapsed=0`, `bossPhase=false`, `PlayerStats.RechargeFull()`, `EnemySpawner.Reconfigure(next.waves, next.bossPrefab)` + `ClearLive()`, retint floor, `GameEvents.RaiseLevelChanged(index, name)`, enter `Transition` for 2s (unscaled) then back to `Playing`; else `Win()`. `sliceDuration` is set from the active level's `duration`. `GameEvents.LevelChanged(int, string)`. `PlayerStats.RechargeFull()`. `EnemySpawner.Reconfigure(WaveData, GameObject)` + `ClearLive()` (return/deactivate all active pooled trash, reset `activeCount`/`bossPhase`).
  - tests: none - MonoBehaviour wiring; the decision is covered by T1
  - done when: `unity recompile --format json` reports 0 errors
- [ ] **T3** [S] Boss death routes through the campaign
  - files: `Assets/Scripts/Enemies/BossCat.cs`
  - contract: `BossCat.Die()` calls `GameManager.Instance.BossDefeated()` instead of `Win()`; no other behaviour change.
  - done when: `unity recompile --format json` reports 0 errors

### AC-2 · Final boss (Máy hút bụi cũ)
- [ ] **T4** [M][P] `BossVacuumBrain` attack-cycle kernel
  - files: `Assets/Scripts/Enemies/BossVacuumBrain.cs`, `Assets/Tests/EditMode/BossVacuumBrainTests.cs`
  - contract: pure kernel, no Unity types. `enum Phase { Suck, RestAfterSuck, Blow, RestAfterBlow, DashTelegraph, Dash, RestAfterDash }`. `struct Decision { Phase Phase; bool Pulling; bool BlowBurst; bool DashTelegraphing; bool DashActive; }`. `Decision Tick(float dt)` cycles Suck(3s) → rest(1.5s) → Blow(one-shot, single frame) → rest(1.5s) → DashTelegraph(1s) → Dash(fixed dur, default 0.6s) → rest(1.5s) → Suck…. `Pulling` true throughout Suck; `BlowBurst` true on exactly one tick per cycle; `DashTelegraphing`/`DashActive` match their phases. Durations are constructor args with those defaults.
  - tests: `starts in suck with pulling` · `blow burst fires exactly once per cycle` · `telegraph precedes dash` · `rests separate every attack` · `cycle repeats in order`
  - done when: `unity command run_tests -- --mode EditMode` green for `BossVacuumBrainTests`
- [ ] **T5** [M] `BossVacuum` behaviour (depends on T4)
  - files: `Assets/Scripts/Enemies/BossVacuum.cs`
  - contract: `BossVacuum : EnemyBase`, `RecycleWhenFar => false`. Serialized: `EnemyData blowData`, pull speed `3`, blow count `8`, dash speed, telegraph tint. Applies the brain each `FixedUpdate`: `Pulling` → move the player's Rigidbody toward the boss at pull speed; `BlowBurst` → spawn 8 `blowData` on a ring via `EnemyBase.SpawnRequest`; `DashTelegraphing` → tint sprite + lock in the dash direction (toward player at telegraph start); `DashActive` → drive a fast straight dash in that direction. Raises `BossSpawned`/`BossHealthChanged` like `BossCat`; `Die()` calls `GameManager.Instance.BossDefeated()`.
  - done when: `unity recompile --format json` reports 0 errors

### AC-3 · Score & high scores
- [ ] **T6** [S][P] `RunStats` score kernel
  - files: `Assets/Scripts/Core/RunStats.cs`, `Assets/Tests/EditMode/RunStatsTests.cs`
  - contract: fields `int dust, kills, seconds`; `int Score(bool won) => dust*10 + kills*5 + seconds*2 + (won ? 5000 : 0)`.
  - tests: `score without win bonus` · `score with win bonus adds 5000` · `all-zero loss scores zero`
  - done when: `unity command run_tests -- --mode EditMode` green for `RunStatsTests`
- [ ] **T7** [S][P] `HighScoreTable` top-5 kernel
  - files: `Assets/Scripts/Core/HighScoreTable.cs`, `Assets/Tests/EditMode/HighScoreTableTests.cs`
  - contract: `[Serializable] struct HighScoreEntry { int score, seconds, level; string date; }`. `static List<HighScoreEntry> Insert(List<HighScoreEntry> table, HighScoreEntry e)` returns a new list sorted by `score` descending, trimmed to 5; a tie keeps the already-present entry ahead of the new one.
  - tests: `insert into empty list` · `sorts descending by score` · `trims to five keeping the highest` · `tie keeps existing ahead of new`
  - done when: `unity command run_tests -- --mode EditMode` green for `HighScoreTableTests`
- [ ] **T8** [S] `SaveSystem` JSON + `RunTracker` (depends on T6, T7)
  - files: `Assets/Scripts/Core/SaveSystem.cs`, `Assets/Scripts/Core/RunTracker.cs`
  - contract: `SaveSystem.Load() : List<HighScoreEntry>` (empty on missing/corrupt file), `SaveSystem.Save(List<HighScoreEntry>)` via `JsonUtility` to `Application.persistentDataPath/highscores.json`. `RunTracker` MonoBehaviour accumulates `kills` and `dust` from `EnemyBase.EnemyDied` (count and summed drop) and `seconds` from its own `Update` while `GameManager` is `Playing` (so it survives per-level `Elapsed` resets); on `GameEvents.Won`/`Lost` it finalises a `RunStats`, inserts the entry through `HighScoreTable`, saves, and exposes the last `RunStats` + `won` + reached level for the end screen.
  - done when: `unity recompile --format json` reports 0 errors

### AC-4 · Frame screens
- [ ] **T9** [M] End screen stats, score and "Về menu" (depends on T8)
  - files: `Assets/Scripts/UI/EndScreen.cs`
  - contract: on win/loss show time survived, kills, dust and level reached + the score from `RunTracker`; keep **Chơi lại** (`GameManager.Restart`) and add **Về menu** → `SceneManager.LoadScene("MainMenu")`. Win copy stays `NHÀ SẠCH BONG!`.
  - done when: `unity recompile --format json` reports 0 errors
- [ ] **T10** [M] Main-menu screen (depends on T8)
  - files: `Assets/Scripts/UI/MainMenuScreen.cs`
  - contract: builds the menu UI (legacy `Text` + `LegacyRuntime.ttf`): title `VACUUM SURVIVOR`, buttons **Chơi** (`LoadScene("Game")`), **Hướng dẫn** (toggle a static panel: WASD di chuyển, vũ khí tự đánh, pin tụt dần, đứng ụ sạc để hồi), **Điểm cao** (list top-5 from `SaveSystem.Load()`), **Thoát** (`Application.Quit()`).
  - done when: `unity recompile --format json` reports 0 errors
- [ ] **T11** [S] HUD room label + between-level banner (depends on T2)
  - files: `Assets/Scripts/UI/HUD.cs`, `Assets/Scripts/UI/LevelBanner.cs`
  - contract: HUD shows the current room name, updated on `GameEvents.LevelChanged`; `LevelBanner` shows `Đã dọn sạch [phòng]!` while `GameManager.Current == State.Transition` and hides otherwise.
  - done when: `unity recompile --format json` reports 0 errors

### AC-5 · Build wiring
- [ ] **T12** [S][P] `SpriteKind.Vacuum` silhouette
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: add `Vacuum` to `SpriteKind`, a `case` in `Build`, and `Vacuum(Color, int)` drawing a tall rounded-rect body + a nozzle + one eye, shaded through `Shade` like the other kinds.
  - tests: extend the existing "every kind builds a non-null sprite" coverage to include `Vacuum`
  - done when: `unity command run_tests -- --mode EditMode` green for `SpriteFactoryTests`
- [ ] **T13** [L] `VSBuild` wiring for levels, bosses, menu (depends on T2, T5, T11, T12)
  - files: `Assets/Editor/VSBuild.cs`
  - contract: `BuildContent` additionally creates `WaveData_PhongNgu` and `WaveData_NhaBep` (higher spawn rate / cap than Phòng khách, drawing from the existing enemy data), the `MayHutBui` `EnemyData` (HP 800, speed 1.5, dust 60) + boss prefab (`SpriteKind.Vacuum`, `BossVacuum`, Enemy layer + collider, `knockback=0`), and a 600-HP cat variant data + prefab for level 2. `BuildGameScene` wires a three-element `LevelConfig[]` (Phòng khách/cat → Phòng ngủ/cat600 → Nhà bếp/vacuum) into `GameManager`, adds `LevelBanner` to the canvas and a `RunTracker` object. New `BuildMainMenu` builds `Assets/Scenes/MainMenu.unity` with a `MainMenuScreen`. Build-settings order: `MainMenu.unity` index 0, `Game.unity` index 1.
  - done when: `VSBuild.BuildContent()`, `BuildMainMenu()` and `BuildGameScene()` each return without error and `unity recompile --format json` reports 0 errors

## End-to-end verification
1. `unity command run_tests -- --mode EditMode` — all EditMode tests green, including the new `CampaignTests`, `BossVacuumBrainTests`, `RunStatsTests`, `HighScoreTableTests` and the extended `SpriteFactoryTests`.
2. From the CLI run `VSBuild.BuildContent()`, `VSBuild.BuildMainMenu()`, `VSBuild.BuildGameScene()`; `unity recompile --format json` reports 0 errors and `unity status` is `ready`.
3. Play `Game` (set each level's `duration` low to demo) with `Application.runInBackground=true`: confirm beating the cat advances to Phòng ngủ with a full battery and kept weapons, the banner shows, Nhà bếp spawns the vacuum boss, the boss pulls/blows/dashes, and killing it shows the win screen with a non-zero score.
4. Confirm `MainMenu` boots at index 0 with working Chơi / Hướng dẫn / Điểm cao / Thoát, and that a finished run's score appears in Điểm cao.

## Open questions
- None.

