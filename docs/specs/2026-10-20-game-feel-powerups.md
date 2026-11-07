---
title: game-feel-powerups
status: implemented
lane: fast   # chỉ thêm thành phần song song pattern có sẵn (pickup/pool/layer, weapon-hook, SpriteKind, VSBuild), không interface liên-module mới, không schema/bảo mật/đồng thời; 6 task; người dùng đã chọn fast và treo máy
---

# Âm thanh + Power-up rơi sàn - Detailed Spec

> Thêm "game feel": âm thanh sinh hoàn toàn bằng code (đối xứng với `SpriteFactory` - không asset ngoài) và vật phẩm power-up rơi trên sàn khi diệt rác (Pin hồi năng lượng, Nam châm hút sạch bụi, Bom nổ diện rộng), cộng tinh chỉnh số liệu wave cho 3 phòng. Thuần code + prefab + tune số liệu trong `VSBuild`.

## Acceptance at a glance
- **AC-1** (MUST) - Khi các sự kiện chính xảy ra lúc chơi (bắn, nhặt bụi, trúng đòn, lên cấp, boss, thắng, thua), game phát hiệu ứng âm thanh sinh bằng code; một bản nhạc nền lặp chạy ở âm lượng thấp và đổi tông nhẹ theo phòng. Không thêm file audio nào vào project.
- **AC-2** (MUST) - Khi một con rác chết, có xác suất nhỏ rơi một power-up trên sàn; robot chạm vào sẽ kích hoạt đúng loại: **Pin** hồi một phần năng lượng, **Nam châm** thu toàn bộ bụi đang rơi trên sàn, **Bom** gây sát thương diện rộng cho rác quanh robot. Power-up không được nhặt sẽ tự biến mất sau một thời gian.
- **AC-3** (MUST) - Toàn bộ nội dung mới được dựng qua `VSBuild` (AudioListener + object `Sfx`, prefab power-up + pool + `PowerUpSpawner`, 3 SpriteKind power-up), số liệu 3 `WaveData` được tinh chỉnh để dễ sống sót hơn ở đầu màn, và lỗi nhấp nháy buff khi hai dock chồng nhau được sửa; cả hai scene build lại không lỗi.
- **AC-4** (SHOULD) - `unity recompile` 0 lỗi, `run_tests` EditMode xanh (gồm test mới của `ToneSynth` và `PowerUpRoll`), và một lượt chơi thực cho thấy có âm thanh phát (`AudioSource.isPlaying`), power-up rơi và ba hiệu ứng power-up hoạt động, 0 lỗi console.

## Scope
- **In:** `ToneSynth` thuần (sinh mẫu sóng) + test; `SoundFactory` (gói mẫu thành `AudioClip`); `Sfx` (MonoBehaviour phát SFX + nhạc nền, nghe `GameEvents`); vài sự kiện mới trên `GameEvents` + hook phát âm (bắn, trúng đòn, power-up); `PowerUpRoll` thuần (xác suất rơi + chọn loại) + test; `PowerUp` (pickup pooled 3 loại) + `PowerUpSpawner`; một hook `PlayerStats.AddBattery`; 3 `SpriteKind` mới (Battery/Magnet/Bomb) + generator; dựng mọi thứ trong `VSBuild`; tune số liệu `WaveData`; sửa nhấp nháy buff dock.
- **Out:** nhạc nền nhiều bài/chuyển cảnh phức tạp (chỉ một loop đổi cao độ theo phòng); cân chỉnh âm lượng theo từng SFX bằng tai (dùng hằng số hợp lý); hệ thống mixer/âm lượng tùy chỉnh trong menu; power-up loại thứ tư; vật cản tĩnh từng phòng; build WebGL; màn Tạm dừng/Hướng dẫn.

## Requirements & design notes

Âm thanh theo đúng triết lý "sinh bằng code, không asset ngoài" của dự án (như `SpriteFactory`/`Palette`). Lớp **sinh mẫu sóng** (`ToneSynth`) là thuần, đặt dưới asmdef runtime `VacuumSurvivor`, test được trong EditMode; lớp gói sóng thành `AudioClip` (`SoundFactory`) và lớp phát (`Sfx`) dùng kiểu Unity nên **không** test đơn vị - chứng bởi cổng compile + lượt chơi (`AudioSource.isPlaying`). Theo yêu cầu "giảm test không cần thiết", chỉ test hai lớp có nhánh/toán thật: `ToneSynth`, `PowerUpRoll`.

**Âm thanh (AC-1):**
- `ToneSynth` (thuần): `enum Wave { Sine, Square, Noise }`; `Tone(float freq, float seconds, int rate, Wave wave, float attack, float decay) → float[]` (độ dài = `round(seconds*rate)`, biên độ trong `[-1,1]`, bao hình attack-rồi-decay tuyến tính); `Arp(float[] freqs, float noteSeconds, int rate, Wave wave) → float[]` nối các nốt thành một chuỗi (cho nhạc). `seconds<=0` trả mảng rỗng.
- `SoundFactory` (static, runtime asmdef): `Clip(string name, float[] samples, int rate) → AudioClip` (bọc `AudioClip.Create` + `SetData`); các preset trả `AudioClip`: `Shoot/Pickup/Hit/LevelUp/Boss/Win/Lose/PowerUp` (SFX ngắn qua `Tone`) và `MusicLoop` (qua `Arp`, dài vài giây, dùng làm loop).
- `Sfx` (MonoBehaviour, một thực thể): `Awake` dựng mọi clip; giữ một `AudioSource` cho SFX (`PlayOneShot`) và một cho nhạc (`loop=true`, volume thấp). Nghe `GameEvents`: `DustChanged`→Pickup (có khoá chống spam theo thời gian tối thiểu mỗi clip), `PlayerLevelChanged`→LevelUp, `PlayerHurt`→Hit, `Shot`→Shoot, `PowerUp`→PowerUp, `BossPhase`→Boss, `Won`→Win, `Lost`→Lose. `LevelChanged(index,_)`→đặt `musicSource.pitch` theo phòng (ví dụ 1.0 / 1.08 / 0.94). Huỷ đăng ký trong `OnDisable`.
- `GameEvents` thêm: `PlayerHurt` (Action), `Shot` (Action), `PowerUp` (Action<PowerUpKind>) + ba hàm `Raise…`. Hook: `PlayerStats.TakeContactDamage` gọi `RaisePlayerHurt()` sau khi trừ máu (chỉ khi thực sự ăn đòn, ngoài i-frame); `ProjectileWeapon` gọi `RaiseShot()` mỗi khi bắn một viên (sự kiện bắn rời rạc; các vũ khí liên tục không phát để tránh rè); `PowerUp.Apply` gọi `RaisePowerUp(kind)`.

**Power-up rơi sàn (AC-2)** - theo đúng khuôn `Dust`/pool/layer "Pickup":
- `enum PowerUpKind { Battery, Magnet, Bomb }` (đặt cạnh `PowerUp`).
- `PowerUpRoll` (thuần): `ShouldDrop(double roll01, float chance) → roll01 < chance`; `Pick(double roll01) → PowerUpKind` chia `[0,1)` thành 3 khoảng đều (`<1/3` Battery, `<2/3` Magnet, ngược lại Bomb).
- `PowerUp` (MonoBehaviour, pooled, collider trigger, layer "Pickup"): `Init(PowerUpKind kind, float batteryAmount, float bombRadius, float bombDamage, float lifetime)` - đặt sprite theo kind qua `SpriteFactory.Build(kindSprite, …)`, hẹn giờ `lifetime` rồi về pool. `OnTriggerEnter2D` gặp `PlayerStats`: gọi `Apply`, `RaisePowerUp(kind)`, rồi về pool. `Apply`:
  - Battery → `stats.AddBattery(batteryAmount)`.
  - Magnet → `Object.FindObjectsByType<Dust>()`; với mỗi cái đang bật: `stats.AddDust(d.Value)` + `d.ReturnToPool()`.
  - Bomb → `Physics2D.OverlapCircleAll(pos, bombRadius, enemyMask)`; mỗi `IDamageable.TakeDamage(bombDamage)`.
- `PlayerStats.AddBattery(float amount)` (hook mới): cộng pin hiện tại, kẹp theo Max (thêm `BatteryModel.Add(float)` kẹp 0..Max), rồi `RaiseBattery`.
- `PowerUpSpawner` (MonoBehaviour trong scene): `Awake` dựng một `GameObjectPool` cho prefab power-up; nghe `EnemyBase.EnemyDied`; mỗi lần chết `PowerUpRoll.ShouldDrop(rng, dropChance)` → nếu rơi, `PowerUpRoll.Pick(rng)` chọn loại, `pool.Get(pos)`, `pu.Init(kind, …)` + gán `Pool`. Huỷ đăng ký trong `OnDisable`.
- `SpriteKind` thêm `Battery, Magnet, Bomb`; mỗi kind một generator kết thúc bằng `Shade` (vật phẩm: không mắt phát sáng, dùng điểm nhấn `Palette.Accent`). Battery = chữ nhật bo đứng + cực trên + dải accent; Magnet = hình "∩" (hai thanh dọc + thanh ngang trên) với hai đầu nhuộm accent; Bomb = đĩa tròn + ngòi ngắn + tia lửa accent.

**Cân bằng & đánh bóng (AC-3):**
- Tinh chỉnh số liệu 3 `WaveData` trong `VSBuild.BuildContent` để đầu màn dễ thở hơn (giãn `spawnInterval`, hạ `maxAlive` ở các wave đầu; giữ độ dốc tăng dần). Đây là chỉnh dữ liệu trong asset, không logic.
- Sửa nhấp nháy buff dock: khi robot rời một dock nhưng còn đứng trong dock khác, **không** gỡ buff. `ChargingDock.OnTriggerExit2D` chỉ gỡ buff nếu `Physics2D.OverlapCircleAll(player, …, "Pickup")` không còn `ChargingDock` nào khác chứa robot.

**Đấu nối (AC-3):**
- `VSBuild.BuildGameScene`: thêm `camGO.AddComponent<AudioListener>()` (scene do VSBuild dựng chưa có listener nào) và một object `Sfx`; thêm object `PowerUpSpawner` cấu hình với prefab power-up.
- `VSBuild.BuildContent`: thêm `CreatePowerUpPrefab` (layer Pickup, trigger, `PowerUp`); không cần WeaponData/UpgradeData cho power-up (chúng rơi sàn, không qua panel).

- **Global constraints:** Unity 6, 2D URP; namespace `VacuumSurvivor` (editor: `VacuumSurvivor.EditorTools`); mọi art/âm thanh sinh bằng code, không asset ngoài; chữ tiếng Việt legacy `UnityEngine.UI.Text` + `LegacyRuntime.ttf`; build qua Editor đang mở theo [[build-workflow]]; rate âm thanh 44100; âm lượng nhạc ~0.25, SFX ~0.5.

## Tasks

### AC-1 · Âm thanh
- [x] **T1** [M] `ToneSynth` thuần + test
  - files: `Assets/Scripts/Audio/ToneSynth.cs`, `Assets/Tests/EditMode/ToneSynthTests.cs`
  - contract: `enum Wave { Sine, Square, Noise }`. `ToneSynth.Tone(float freq, float seconds, int rate, Wave wave, float attack, float decay) → float[]`: độ dài `Mathf.RoundToInt(seconds*rate)`; mỗi mẫu = dạng sóng(freq) × bao hình; bao hình lên tuyến tính trong `attack` giây đầu, xuống tuyến tính trong `decay` giây cuối, giữa giữ 1; mọi mẫu trong `[-1,1]`. `Arp(float[] freqs, float noteSeconds, int rate, Wave wave) → float[]`: nối `Tone` của từng tần số (attack/decay ngắn cố định). `seconds<=0` → mảng rỗng.
  - tests: `Tone length equals round(seconds times rate)` · `Tone samples stay within minus one and one` · `Tone is not silent` · `Tone envelope decays - a late sample is quieter than a mid sample` · `Tone with zero seconds returns an empty array` · `Arp concatenates one note per frequency`
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh (ToneSynth)
- [x] **T2** [M] `SoundFactory` + `Sfx` + sự kiện/hook âm thanh · depends on T1
  - files: `Assets/Scripts/Audio/SoundFactory.cs`, `Assets/Scripts/Audio/Sfx.cs`, `Assets/Scripts/Core/GameEvents.cs`, `Assets/Scripts/Player/PlayerStats.cs`, `Assets/Scripts/Weapons/ProjectileWeapon.cs`
  - contract: `SoundFactory.Clip(string, float[], int) → AudioClip`; preset `Shoot/Pickup/Hit/LevelUp/Boss/Win/Lose/PowerUp/MusicLoop` → `AudioClip`. `Sfx`: `Awake` dựng clip + 2 `AudioSource` (SFX one-shot, nhạc loop volume ~0.25); map `GameEvents` → clip như design note; khoá chống spam tối thiểu ~0.05s mỗi clip; `LevelChanged`→`musicSource.pitch` theo phòng; huỷ đăng ký trong `OnDisable`. `GameEvents` thêm `PlayerHurt`/`Shot`/`PowerUp(PowerUpKind)` + `Raise…`. `PlayerStats.TakeContactDamage` gọi `RaisePlayerHurt()` khi ăn đòn thật; `ProjectileWeapon` gọi `RaiseShot()` khi bắn một viên.
  - tests: (không - kiểu Unity/phát âm; chứng bởi cổng compile + lượt chơi `AudioSource.isPlaying`)
  - done when: `unity recompile --format json` 0 lỗi

### AC-2 · Power-up rơi sàn
- [x] **T3** [S] `PowerUpRoll` thuần + test
  - files: `Assets/Scripts/Pickups/PowerUpRoll.cs`, `Assets/Tests/EditMode/PowerUpRollTests.cs`
  - contract: `enum PowerUpKind { Battery, Magnet, Bomb }` (trong cùng file hoặc `PowerUp.cs`; T3 chỉ cần enum tồn tại). `PowerUpRoll.ShouldDrop(double roll01, float chance) → roll01 < chance`. `Pick(double roll01) → PowerUpKind`: `<1/3`→Battery, `<2/3`→Magnet, còn lại→Bomb.
  - tests: `ShouldDrop is true below the chance and false at or above it` · `Pick maps the three thirds to Battery, Magnet, Bomb`
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh (PowerUpRoll)
- [x] **T4** [M] `PowerUp` + `PowerUpSpawner` + `PlayerStats.AddBattery` · depends on T3
  - files: `Assets/Scripts/Pickups/PowerUp.cs`, `Assets/Scripts/Pickups/PowerUpSpawner.cs`, `Assets/Scripts/Player/PlayerStats.cs`, `Assets/Scripts/Player/BatteryModel.cs`
  - contract: `BatteryModel.Add(float amount)` kẹp 0..Max. `PlayerStats.AddBattery(float amount)` → `battery.Add` + `RaiseBattery`. `PowerUp`: `GameObjectPool Pool {get;set;}`; `Init(PowerUpKind kind, float batteryAmount, float bombRadius, float bombDamage, float lifetime)` đặt sprite theo kind + hẹn giờ về pool; `OnTriggerEnter2D` gặp `PlayerStats` → `Apply` + `GameEvents.RaisePowerUp(kind)` + về pool. `Apply`: Battery→`stats.AddBattery`; Magnet→`FindObjectsByType<Dust>()` thu hết (AddDust + ReturnToPool); Bomb→`OverlapCircleAll(radius, "Enemy")` mỗi `IDamageable.TakeDamage`. `PowerUpSpawner`: pool riêng; nghe `EnemyBase.EnemyDied`; mỗi chết roll `dropChance` (~0.05) → `pool.Get` + `Init`.
  - tests: (không - va chạm/pool/Unity; chứng bởi cổng + lượt chơi)
  - done when: `unity recompile --format json` 0 lỗi
- [x] **T5** [M] 3 SpriteKind power-up + nới test
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: `SpriteKind` thêm `Battery, Magnet, Bomb`; `Build` route tới 3 generator kết thúc `Shade` (vật phẩm, dùng `Palette.Accent`, không mắt). Battery = RoundedRect đứng + cực trên + dải accent; Magnet = hình "∩" (2 thanh dọc + 1 thanh ngang trên) 2 đầu accent; Bomb = Disc + ngòi ngắn + tia lửa accent.
  - tests: (thêm một phương thức) `Powerup items return non-null in-bounds sprites with coverage` phủ Battery/Magnet/Bomb (giống test Puddle/MopTrail sẵn có)
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh

### AC-3 · Dựng nội dung, cân bằng & sửa dock
- [x] **T6** [M] Dựng trong `VSBuild` + tune WaveData + sửa dock (depends on T2,T4,T5)
  - files: `Assets/Editor/VSBuild.cs`, `Assets/Scripts/Pickups/ChargingDock.cs`
  - contract: `BuildGameScene` thêm `AudioListener` lên Main Camera, object `Sfx`, object `PowerUpSpawner` (cấu hình prefab power-up). `BuildContent` thêm `CreatePowerUpPrefab` (layer Pickup, `CircleCollider2D` trigger, `ProceduralSprite`, `PowerUp`) và giãn số liệu 3 `WaveData` cho dễ sống sót đầu màn (tăng `spawnInterval` + giảm `maxAlive` các wave đầu, giữ dốc tăng). `ChargingDock.OnTriggerExit2D`: chỉ gỡ buff nếu không còn `ChargingDock` nào khác đang chứa robot (quét `OverlapCircleAll` layer Pickup).
  - done when: `eval 'return VacuumSurvivor.EditorTools.VSBuild.BuildContent();'` rồi `...BuildGameScene();` trả thành công; `unity recompile --format json` 0 lỗi

### AC-4 · (không task - chứng bởi cổng cuối + lượt chơi)

## End-to-end verification
1. `unity recompile --format json` → 0 lỗi; `unity status` state `ready`.
2. `unity command run_tests -- --mode EditMode --timeout 180000` → toàn bộ xanh (gồm `ToneSynthTests`, `PowerUpRollTests`, `SpriteFactoryTests` đã nới).
3. `eval` `BuildContent()` rồi `BuildGameScene()` → thành công, không lỗi console.
4. `editor_play` một lượt (nhớ `Application.runInBackground=true`): nhạc nền chạy (`Sfx` music `AudioSource.isPlaying==true`); diệt rác một lúc thấy power-up rơi; chạm Pin thấy pin tăng, chạm Nam châm thấy bụi bị thu sạch, chạm Bom thấy rác quanh chết; không lỗi console. Dừng play.

## Open questions
(không còn - hướng và 1 điểm kỹ thuật âm thanh đã chốt)
