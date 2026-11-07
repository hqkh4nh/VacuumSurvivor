---
title: finish-roster
status: implemented
lane: fast   # các thực thể mới đều lặp lại một khuôn đã chứng minh (lớp con + asset + prefab + sprite), không interface liên-module mới, không schema/bảo mật/đồng thời; người dùng đã chọn fast và treo máy
---

# Hoàn thiện bộ sưu tập rác & vũ khí - Detailed Spec

> Thêm 3 loại rác còn lại (Tất lẻ, LEGO/Vũng nước, Dây sạc) và 3 vũ khí còn lại (Vệt lau, Tia LIDAR, Còi báo động) theo đúng khuôn mở rộng tuyến tính đã có, để `EnemyBase` đạt 6 lớp con và `WeaponBase` đạt 6 lớp con đúng như GDD và báo cáo tuyên bố. Thuần code + ScriptableObject + prefab, không cần asset ngoài.

## Acceptance at a glance
- **AC-1** (MUST) - Khi vào màn chơi, ba loại rác mới xuất hiện và hành xử đúng đặc trưng: Tất lẻ ngắm-rồi-lao, LEGO/Vũng nước đứng yên gây sát thương, Dây sạc bò ngoằn ngoèo và trói chân người chơi khi chạm.
- **AC-2** (MUST) - Khi lên cấp, panel có thể trao và nâng ba vũ khí mới (Vệt lau, Tia LIDAR, Còi báo động); mỗi vũ khí tự động kích hoạt và lên tới cấp 5.
- **AC-3** (MUST) - Toàn bộ nội dung mới được dựng qua `VSBuild` (prefab, EnemyData/WeaponData, thẻ UpgradeData, sprite, và nhét rác mới vào 3 `WaveData`); cả hai scene build lại không lỗi. `EnemyBase` có đúng 6 lớp con cụ thể, `WeaponBase` có đúng 6.
- **AC-4** (SHOULD) - `unity recompile` 0 lỗi, `run_tests` EditMode xanh, và một lượt chơi thực cho thấy rác mới spawn và vũ khí mới bắn.

## Scope
- **In:** 3 lớp `EnemyBase` con mới + helper thuần nơi có máy trạng thái; 3 lớp `WeaponBase` con mới + helper thuần tính chỉ số; các SpriteKind mới; mở rộng `EnemyData`/`WeaponKind`/`UpgradeType`; một hook `Root` trên `PlayerController` và một hook va-chạm ảo trên `EnemyBase`; dựng mọi thứ trong `VSBuild`; thêm rác mới vào WaveData; test cho hai helper có logic thật.
- **Out:** cân bằng số liệu tinh chỉnh (để lại cho người chơi chỉnh trong Inspector); giới hạn "tối đa 4 vũ khí" (cho sở hữu tới 6 - đơn giản hơn); âm thanh; vật phẩm rơi trên sàn; màn Tạm dừng/Hướng dẫn; stun thật sự (choáng của Còi = cú đẩy mạnh, không có hệ thống stun riêng); vạch báo kiểu line của Tất lẻ (dùng đổi màu/giật).

## Requirements & design notes

Mỗi thực thể theo đúng khuôn hiện có: **1 lớp con + (nếu có máy trạng thái/toán hướng) 1 helper thuần test được trong EditMode + 1 asset dữ liệu + 1 prefab dựng trong `VSBuild` + 1 SpriteKind**. Helper thuần đặt dưới asmdef runtime `VacuumSurvivor`; test dưới `VacuumSurvivor.Tests.EditMode`. Theo yêu cầu "giảm test không cần thiết", chỉ viết test cho helper có nhánh logic thật (`DashCycle`, `SnakeWeave`); các helper tra-mảng-kẹp (giống `OrbitStats`/`ProjectileStats`) không cần test riêng, dựa vào cổng compile + EditMode + lượt chơi.

**Kẻ địch (mở rộng `EnemyBase`):**
- `DasherEnemy` (Tất lẻ): sở hữu một `DashCycle` thuần (Aim → Dash → Recover theo timer, mô phỏng `BossCatBrain`). Pha Aim đứng yên + báo trước (đổi màu/giật); pha Dash lao thẳng theo hướng đã khóa lúc bắt đầu lao với `dashSpeed`; Recover nghỉ ngắn rồi lặp. Đọc vị trí người chơi tại thời điểm bắt đầu lao.
- `HazardEnemy` (LEGO **và** Vũng nước): đứng yên (`Move` đặt vận tốc 0), `RecycleWhenFar => false`, `dustValue = 0`. Dữ liệu quyết định biến thể: `indestructible` + `lifetime > 0` = LEGO (bỏ qua `TakeDamage`, tự về pool sau `lifetime` giây); `indestructible = false`, `lifetime = 0` = Vũng nước (máu thấp, **Lực hút sẵn có đánh trúng = làm khô** → hết máu về pool). Sát thương "đứng trong vũng" dùng lại `OnCollisionStay2D` + i-frame 0,4s của người chơi (≈ cùng áp lực), không thêm drain-mỗi-giây. `TakeDamage` không hất văng (vật tĩnh).
- `SnakeEnemy` (Dây sạc): kế thừa hành vi đuổi nhưng lệch hướng bằng `SnakeWeave` thuần (offset vuông góc theo `sin(time)`); khi chạm người chơi gọi `PlayerController.Root(rootDuration)`.

**Hook nhỏ (surgical):**
- `EnemyBase`: `OnCollisionStay2D` gọi thêm một hook ảo `OnContactPlayer(PlayerStats)` (mặc định rỗng) để `SnakeEnemy` trói chân mà không phải nhân đôi logic va chạm nền.
- `PlayerController.Root(float seconds)`: đặt mốc `rootedUntil`; trong `FixedUpdate`, nếu còn trói thì vận tốc = 0 (dùng lại đường vận tốc sẵn có).

**Vũ khí (mở rộng `WeaponBase`, tự động kích hoạt, đọc level theo khuôn `OrbitWeapon`/`ProjectileWeapon`):**
- `TrailWeapon` (Vệt lau): mỗi `interval` thả một `TrailPatch` (pooled) tại vị trí robot; patch sống `lifetime` giây, mỗi frame gây `dps*dt` cho rác chồng lên nó. Chỉ số theo cấp qua `TrailStats` thuần.
- `LaserWeapon` (Tia LIDAR): mỗi `cycle` quét tia xoay qua một cửa sổ ngắn, gây sát thương rác bị tia cắt (quét góc + `OverlapCircleAll` trong tầm); hình ảnh bằng `LineRenderer`. Chỉ số qua `LaserStats` thuần.
- `PushWeapon` (Còi báo động): mỗi `cycle` lấy `OverlapCircleAll` trong bán kính, hất mỗi con ra xa bằng xung lực + sát thương nhẹ; "choáng" thể hiện bằng cú đẩy mạnh (không hệ thống stun). Chỉ số qua `PushStats` thuần; vòng sóng dùng lại `SpriteKind.Ring`.

**Đấu nối dùng chung:**
- `WeaponKind` thêm `Trail, Laser, Push`. `UpgradeType` thêm `TrailWeapon, LaserWeapon, PushWeapon`; `UpgradeManager.ApplyEffect` định tuyến mỗi loại tới `WeaponManager.AcquireOrUpgrade(kind)` theo đúng khuôn Orbit/Projectile.
- `SpriteKind` thêm `Sock, Lego, Puddle, Cord, MopTrail`; mỗi kind một generator kết thúc bằng `Shade`, route trong `SpriteFactory.Build`. `SpriteFactoryTests` nới test coverage sẵn có để phủ các kind mới (không thêm phương thức test mới).
- `VSBuild.BuildContent`: thêm 4 EnemyData + prefab (Sock/Lego/Puddle/Cord) qua `CreateEnemyPrefab` (hazard cần xử lý tĩnh), 3 WeaponData (chỉ tên + maxLevel, giống ChoiQuay/VoiXit), 3 UpgradeData + thêm tên vào `LoadUpgrades`, pool cho `TrailPatch`, và nhét rác mới vào 3 asset WaveData. `VSBuild.BuildPlayer`: thêm `BuildTrailWeapon/BuildLaserWeapon/BuildPushWeapon` dạng component/con trên Player.

- **Global constraints:** Unity 6, 2D URP; namespace `VacuumSurvivor` (editor: `VacuumSurvivor.EditorTools`); chữ tiếng Việt dùng legacy `UnityEngine.UI.Text` + `LegacyRuntime.ttf`; số liệu khởi điểm theo GDD mục 1.5-1.6 (đặt trong asset để chỉnh trong Inspector); build qua Editor đang mở theo [[build-workflow]].

## Tasks

### AC-1 · Ba loại rác mới
- [x] **T1** [M] `DasherEnemy` + `DashCycle` (Tất lẻ)
  - files: `Assets/Scripts/Enemies/DasherEnemy.cs`, `Assets/Scripts/Enemies/DashCycle.cs`, `Assets/Tests/EditMode/DashCycleTests.cs`
  - contract: `DashCycle(float aimTime, float dashDuration, float recoverTime)`; `Tick(float dt)` → `struct { Phase (Aiming|Dashing|Recovering); bool StartedDash; bool Telegraphing }`. Chu kỳ Aiming→Dashing→Recovering→Aiming theo timer; `StartedDash` true đúng một tick khi vào Dashing; `Telegraphing` true chỉ trong Aiming. `DasherEnemy`: Aiming đứng yên + nháy màu; tại `StartedDash` khóa hướng = về phía người chơi; Dashing đặt vận tốc = hướng×`dashSpeed`; Recovering vận tốc 0.
  - tests: `cycles Aiming→Dashing→Recovering→Aiming on the configured timers` · `StartedDash is true for exactly one tick entering Dashing` · `Telegraphing is true only during Aiming`
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh (DashCycle)
- [x] **T2** [M] `HazardEnemy` tĩnh, dữ liệu quyết định (LEGO + Vũng nước) · depends on none
  - files: `Assets/Scripts/Enemies/HazardEnemy.cs`, `Assets/Scripts/Enemies/EnemyData.cs`
  - contract: `EnemyData` thêm `float lifetime`, `bool indestructible`, và (dùng cho T1/T3) `float aimTime, dashSpeed, dashDuration, recoverTime, rootDuration, weaveAmp, weaveFreq`. `HazardEnemy`: `Move` đặt vận tốc 0; `RecycleWhenFar => false`; `OnEnable` nếu `lifetime>0` hẹn giờ về pool; override `TakeDamage`: nếu `indestructible` bỏ qua, ngược lại trừ máu **không hất văng**, chết thì `ReturnToPool`. `dustValue=0` nên không rơi bụi.
  - tests: (không - chỉ timer/không nhánh; chứng bởi cổng compile + lượt chơi)
  - done when: `unity recompile --format json` 0 lỗi
- [x] **T3** [M] `SnakeEnemy` + `SnakeWeave` + hook trói chân (Dây sạc)
  - files: `Assets/Scripts/Enemies/SnakeEnemy.cs`, `Assets/Scripts/Enemies/SnakeWeave.cs`, `Assets/Scripts/Enemies/EnemyBase.cs`, `Assets/Scripts/Player/PlayerController.cs`, `Assets/Tests/EditMode/SnakeWeaveTests.cs`
  - contract: `SnakeWeave.Direction(Vector2 toward, float time, float amp, float freq)` → `(toward.normalized + perp(toward)×amp×sin(freq×time)).normalized`. `EnemyBase.OnCollisionStay2D` gọi thêm ảo `OnContactPlayer(PlayerStats)` (mặc định rỗng); `SnakeEnemy.OnContactPlayer` → `PlayerController.Root(rootDuration)`. `PlayerController.Root(float s)`: `rootedUntil = Time.time + s`; `FixedUpdate` nếu còn trói đặt vận tốc 0.
  - tests: `direction stays within 90° of the toward vector across a full period` · `the perpendicular offset flips sign between a quarter and three-quarter period`
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh (SnakeWeave)

### AC-2 · Ba vũ khí mới
- [x] **T4** [S] Mở enum + định tuyến nâng cấp
  - files: `Assets/Scripts/Weapons/WeaponKind.cs`, `Assets/Scripts/Upgrades/UpgradeData.cs`, `Assets/Scripts/Upgrades/UpgradeManager.cs`
  - contract: `WeaponKind` thêm `Trail, Laser, Push`; `UpgradeType` thêm `TrailWeapon, LaserWeapon, PushWeapon`; `ApplyEffect` thêm 3 case → `WeaponManager.AcquireOrUpgrade(kind)` theo đúng khuôn Orbit/Projectile.
  - done when: `unity recompile --format json` 0 lỗi
- [x] **T5** [M] `TrailWeapon` + `TrailPatch` + `TrailStats` (Vệt lau) · depends on T4
  - files: `Assets/Scripts/Weapons/TrailWeapon.cs`, `Assets/Scripts/Weapons/TrailPatch.cs`, `Assets/Scripts/Weapons/TrailStats.cs`
  - contract: `TrailStats.For(float[] dps, float[] interval, float[] lifetime, float[] width, int level)` kẹp chỉ số như `OrbitStats`. `TrailWeapon`: cổng `Owned`+Playing như `OrbitWeapon`; mỗi `interval` `pool.Get` tại vị trí robot rồi `patch.Init(dps,lifetime,width)`. `TrailPatch`: mỗi frame `OverlapCircleAll(width, enemyMask)` gây `dps×dt`; hết `lifetime` về pool.
  - tests: (không - `TrailStats` tra-mảng tầm thường; chứng bởi cổng + lượt chơi)
  - done when: `unity recompile --format json` 0 lỗi
- [x] **T6** [M] `LaserWeapon` + `LaserStats` (Tia LIDAR) · depends on T4
  - files: `Assets/Scripts/Weapons/LaserWeapon.cs`, `Assets/Scripts/Weapons/LaserStats.cs`
  - contract: `LaserStats.For(float[] damage, float[] cycle, int[] beams, float range, int level)` kẹp như `OrbitStats`. `LaserWeapon`: cổng `Owned`+Playing; mỗi `cycle` mở một lượt quét trong cửa sổ ngắn, xoay góc và gây `damage` cho rác trong `range` nằm trong góc nêm quét; hình ảnh bằng `LineRenderer` từ `Palette.Accent`.
  - tests: (không - `LaserStats` tra-mảng; chứng bởi cổng + lượt chơi)
  - done when: `unity recompile --format json` 0 lỗi
- [x] **T7** [S] `PushWeapon` + `PushStats` (Còi báo động) · depends on T4
  - files: `Assets/Scripts/Weapons/PushWeapon.cs`, `Assets/Scripts/Weapons/PushStats.cs`
  - contract: `PushStats.For(float[] damage, float[] knockback, float[] cycle, float[] radius, int level)` kẹp như `OrbitStats`. `PushWeapon`: cổng `Owned`+Playing; mỗi `cycle` `OverlapCircleAll(radius, enemyMask)`, mỗi con `rb.AddForce(away×knockback, Impulse)` + `TakeDamage(damage)`; vòng sóng bằng một `SpriteKind.Ring` phình nhanh (choáng = cú đẩy mạnh, không hệ thống stun).
  - tests: (không - `PushStats` tra-mảng; chứng bởi cổng + lượt chơi)
  - done when: `unity recompile --format json` 0 lỗi

### AC-3 · Dựng nội dung & scene
- [x] **T8** [M] SpriteKind + generator mới
  - files: `Assets/Scripts/Art/SpriteFactory.cs`, `Assets/Tests/EditMode/SpriteFactoryTests.cs`
  - contract: `SpriteKind` thêm `Sock, Lego, Puddle, Cord, MopTrail`; `Build` route mỗi kind tới generator kết thúc bằng `Shade`. `Sock`=chữ nhật bo góc + mắt; `Lego`=vuông + 4 chấm studs; `Puddle`=blob mềm mờ (không mắt); `Cord`=đoạn dây lượn sóng + mắt; `MopTrail`=vệt mềm (không mắt, không phát sáng). Nới mảng kind trong test coverage sẵn có để phủ `Sock/Lego/Cord`.
  - tests: (mở rộng test sẵn có) `each creature kind returns a non-null sprite of the requested size` · `the creature kinds are not all the same silhouette` - nay bao gồm các kind mới
  - done when: `unity command run_tests -- --mode EditMode --timeout 180000` xanh
- [x] **T9** [L] Dựng toàn bộ trong `VSBuild` (depends on T1-T8)
  - files: `Assets/Editor/VSBuild.cs`
  - contract: `BuildContent` thêm 4 `EnemyData`+prefab theo số liệu GDD 1.5 - `TatLe`→`DasherEnemy` (`SpriteKind.Sock`, máu 20, dmg 12, bụi 2, aimTime 1, dashSpeed 9, dashDuration ~0.5, recover ~0.6), `CucLego`→`HazardEnemy` (`Lego`, `indestructible=true`, `lifetime=25`, dmg 20, bụi 0), `VungNuoc`→`HazardEnemy` (`Puddle`, máu ~12, dmg 4, bụi 0), `DaySac`→`SnakeEnemy` (`Cord`, máu 25, speed 2.5, dmg 6, bụi 3, rootDuration 1); 3 `WeaponData` (`VetLau`/`TiaLidar`/`CoiBaoDong`, chỉ tên + `maxLevel=5`); 3 `UpgradeData` (type Trail/Laser/Push, valuePerLevel toàn 0, maxLevel 5) và thêm tên vào `LoadUpgrades`; prefab + pool cho `TrailPatch`; nhét rác mới vào 3 `WaveData` theo phòng (Phòng khách: thêm Tất lẻ+LEGO; Phòng ngủ: thêm Dây sạc; Nhà bếp: thêm Vũng nước+Dây sạc). `BuildPlayer` thêm `BuildTrailWeapon`/`BuildLaserWeapon`/`BuildPushWeapon`.
  - done when: `eval 'return VacuumSurvivor.EditorTools.VSBuild.BuildContent();'` rồi `...BuildGameScene();` trả thành công; `unity recompile --format json` 0 lỗi

### AC-4 · (không task - chứng bởi cổng cuối + lượt chơi)

## End-to-end verification
1. `unity recompile --format json` → 0 lỗi; `unity status` state `ready`.
2. `unity command run_tests -- --mode EditMode --timeout 180000` → toàn bộ xanh (gồm `DashCycleTests`, `SnakeWeaveTests`, `SpriteFactoryTests` đã nới).
3. `eval` `BuildContent()` rồi `BuildGameScene()` → thành công, không lỗi console.
4. Kiểm số lớp con: `grep` các khai báo cho thấy `EnemyBase` có 6 lớp con cụ thể (`ChaserEnemy, SplitterEnemy, DasherEnemy, HazardEnemy, SnakeEnemy, BossCat`/`BossVacuum` - lưu ý boss cũng kế thừa) và `WeaponBase` có 6 (`Suction, Orbit, Projectile, Trail, Laser, Push`).
5. `editor_play` một lượt (nhớ `Application.runInBackground=true`): rác mới spawn theo phòng; Tất lẻ ngắm-rồi-lao; đứng cạnh LEGO/Vũng nước mất pin; chạm Dây sạc bị khựng; nhận đủ 3 vũ khí mới qua panel và thấy chúng kích hoạt. Dừng play.

## Open questions
(không còn - 4 điểm đơn giản hóa đã được duyệt)
