---
title: Vacuum Survivor - Vertical Slice
status: implemented
lane: fast   # heuristic gợi ý full (code mới, chưa có test, >8 task); người dùng yêu cầu rõ fast — logic thuần vẫn tách ra test EditMode để giữ test-first. Project không phải git repo → "1 commit/AC" đổi thành "1 lần chạy gate/AC".
---

# Vacuum Survivor - Vertical Slice - Detailed Spec

> Lát cắt dọc chơi được của game survivors-like "robot hút bụi": di chuyển, pin thay cho máu,
> 3 loại rác, vũ khí Lực hút tự đánh, hút bụi lên cấp chọn nâng cấp, ụ sạc "tăng áp", và hai
> màn kết thúc (dọn xong / hết pin). Mục tiêu là chứng minh vòng lặp cốt lõi vui và dựng đúng
> kiến trúc (kế thừa, ScriptableObject, object pool) để các spec sau mở rộng mà không đập đi.

## Acceptance at a glance
- **AC-1** (MUST) — Khi người chơi bấm WASD/phím mũi tên, robot di chuyển 8 hướng theo tốc độ cấu
  hình, và camera bám theo robot mượt.
- **AC-2** (MUST) — Pin đóng vai trò máu: tụt nền liên tục theo thời gian; chạm rác mất pin với
  0,4s bất tử giữa hai lần trúng; đứng trên ụ sạc hồi pin; pin về 0 thì thua.
- **AC-3** (MUST) — Đèn LED trên robot và thanh pin trên HUD đổi màu theo ngưỡng pin
  (>60% xanh, 30-60% vàng, <30% đỏ nhấp nháy).
- **AC-4** (MUST) — `EnemySpawner` đọc `WaveData` (ScriptableObject) và sinh rác **lấy từ pool**
  ở ngoài rìa màn hình quanh người chơi; giới hạn số rác sống cùng lúc; rác đi quá xa thì được
  dời về gần thay vì bỏ lang thang.
- **AC-5** (MUST) — Có 3 loại rác dựng trên 2 lớp con của `EnemyBase` (để thể hiện kế thừa/đa hình):
  Bụi bông và Vụn bánh mì đều dùng `ChaserEnemy` (bò thẳng về người chơi), khác nhau ở **dữ liệu**
  (`EnemyData`: máu/tốc độ) và cách spawn (Vụn bánh mì nhanh, sinh theo bầy 5-8 con); Cục tóc rối là
  `SplitterEnemy`, chết tách thành đúng 2 con nhỏ và con nhỏ **không** tách tiếp.
- **AC-6** (MUST) — Vũ khí Lực hút tự động gây sát thương liên tục cho rác trong bán kính, mạnh dần
  theo cấp; rác trúng đòn nhấp nháy trắng và bị đẩy lùi nhẹ, hết máu thì chết.
- **AC-7** (MUST) — Rác chết rơi **bụi lấy từ pool**; bụi trong bán kính nhặt bị hút bay về phía
  robot (tăng tốc dần); nhặt đủ bụi theo công thức `5 + cấp*4` thì lên cấp.
- **AC-8** (MUST) — Khi lên cấp, game dừng hẳn (`Time.timeScale = 0`) và hiện panel 3 lựa chọn
  nâng cấp ngẫu nhiên; chọn 1 thì áp dụng hiệu ứng, đóng panel, game chạy tiếp; lên nhiều cấp cùng
  lúc thì xếp hàng hiện panel liên tiếp.
- **AC-9** (MUST) — Ụ sạc "tăng áp": hiện theo chu kỳ và tồn tại một khoảng thời gian; khi robot
  đứng trên nó, pin hồi dần, tốc độ di chuyển giảm mạnh (≈ -70%), và **bán kính + sát thương Lực
  hút tăng gấp đôi**.
- **AC-10** (MUST) — HUD hiển thị realtime: thanh pin (đổi màu), thanh bụi, đồng hồ đếm giờ màn,
  cấp người chơi, cấp vũ khí Lực hút.
- **AC-11** (MUST) — Sống tới mốc thời gian cấu hình (mặc định 180 giây) thì hiện màn "Dọn xong"; pin
  về 0 thì hiện màn thua; cả hai màn đều có nút Chơi lại khởi động lại ván.
- **AC-12** (SHOULD) — Ràng buộc chéo về chất lượng kỹ thuật:
  - [ ] Mọi con số cân bằng nằm trong ScriptableObject / field Inspector, không hardcode rải rác.
  - [ ] Rác và bụi dùng object pool — không `Instantiate`/`Destroy` trong vòng lặp nóng lúc chơi.
  - [ ] Project biên dịch không lỗi; test logic EditMode (công thức lên cấp, tụt pin, áp dụng nâng
        cấp, giới hạn tách đôi) xanh.
  - [ ] Hình ảnh placeholder sinh bằng code (hình tròn nền tối + mắt sáng + chấm LED), không phụ
        thuộc asset ngoài.

## Scope
- **In:** Di chuyển + camera follow; hệ pin (tụt nền, chạm rác, bất tử, LED/HUD màu, thua khi cạn);
  3 loại rác (2 lớp hành vi: Chaser, Splitter — "Vụn bánh mì" là biến thể dữ liệu của Chaser);
  `EnemySpawner` + `WaveData`; object pool rác & bụi; vũ khí Lực hút (tự đánh, nhiều cấp); bụi +
  nam châm nhặt + công thức lên cấp; panel chọn 1/3 nâng cấp + vài nâng cấp bị động; ụ sạc "tăng
  áp"; HUD; màn Dọn xong / màn Thua + nút Chơi lại; sprite placeholder sinh bằng code; vài test
  logic EditMode.
- **Out:** Boss (mèo, máy hút bụi); 5 vũ khí còn lại (chổi, vòi, vệt lau, LIDAR, còi); 4 loại rác
  phức tạp (tất lẻ, LEGO, dây sạc, vũng nước); 3 bản đồ phòng riêng + Tilemap/vật cản; âm thanh &
  nhạc nền; menu chính + hướng dẫn + bảng điểm cao + lưu JSON; chuyển màn giữa 3 phòng; art thật
  thay placeholder; build Windows/WebGL; mũi tên chỉ ụ sạc ngoài màn hình (để sau nếu kịp).

## Requirements & design notes

**Kiến trúc & tổ chức file** (theo `Assets/Scripts/`, khớp với bố cục full game ở GDD để mở rộng
không đập đi):

- `Core/` — `GameManager` (singleton nhẹ: trạng thái ván, đồng hồ màn, giữ tham chiếu Player, bộ
  đếm để tính điểm sau này, phát sự kiện thắng/thua), `GameEvents` (vài event tĩnh để UI lắng nghe,
  tránh phụ thuộc vòng).
- `Player/` — `PlayerController` (đọc input, di chuyển bằng `Rigidbody2D` trong `FixedUpdate`),
  `PlayerStats` (pin, cấp, bụi, các hệ số nhân; phát event khi đổi), `BatteryLED` (đổi màu chấm
  LED theo ngưỡng), `PlayerMagnet` (hút bụi trong bán kính).
- `Enemies/` — `EnemyBase` (abstract, cài `IDamageable`: máu, `TakeDamage`, `Die`, nhấp nháy,
  đẩy lùi, trả về pool, có `abstract void Move()`), `ChaserEnemy` (bò thẳng về người chơi),
  `SplitterEnemy` (chết sinh 2 con nhỏ, cờ `canSplit` để con nhỏ không tách tiếp), `EnemySpawner`,
  `EnemyData` (SO: máu, tốc độ, sát thương, bụi rơi, prefab, cờ tách đôi), `WaveData` (SO: các mốc
  thời gian → nhịp spawn, trần số rác, danh sách loại rác).
- `Weapons/` — `WeaponBase` (abstract: cấp, hồi chiêu, `abstract void Fire()`), `SuctionWeapon`
  (vòng tròn sát thương liên tục; nhận hệ số "overdrive" từ ụ sạc), `WeaponData` (SO: chỉ số từng
  cấp, bán kính, sát thương/giây), `WeaponManager` (giữ vũ khí đang mang, nâng cấp).
- `Upgrades/` — `UpgradeData` (SO: loại enum, giá trị mỗi cấp, tên, mô tả), `UpgradeManager` (bốc 3
  lựa chọn ngẫu nhiên không trùng, áp dụng khi chọn, xếp hàng khi lên nhiều cấp).
- `Pickups/` — `Dust` (pooled, bị `PlayerMagnet` hút), `ChargingDock` (vùng trigger hồi pin +
  overdrive + khoá/giảm di chuyển).
- `Utils/` — `GameObjectPool` (pool dùng `Queue`, bật/tắt `SetActive` thay cho Instantiate/Destroy),
  `IDamageable` (`void TakeDamage(float amount)`).
- `UI/` — `HUD` (thanh pin màu, thanh bụi, đồng hồ, cấp, cấp vũ khí), `LevelUpPanel` (3 thẻ, bấm
  chọn), `EndScreen` (dùng chung cho Dọn xong / Thua, nút Chơi lại).
- `Art/` (Editor util) — sinh sprite hình tròn + mắt bằng `Texture2D` một lần, gán cho prefab.

**Quyết định kỹ thuật:**
- **Input:** dùng **Input System mới** (đã cài sẵn trong project) thay cho `Input.GetAxisRaw` mà
  GDD gợi ý — đọc trục Move từ asset `InputSystem_Actions` có sẵn hoặc `Keyboard.current`. Lý do:
  project đã bật Input System mới, Input Manager cũ có thể không hoạt động.
- **Camera:** script `CameraFollow` ~10 dòng trong `LateUpdate` (chưa cần Cinemachine; thêm sau khi
  polish).
- **Render:** giữ 2D URP đang có. Sprite placeholder PPU thống nhất (ví dụ 64), sinh bằng code nền
  tối + mắt sáng.
- **Vật lý 2D:** `Rigidbody2D` Dynamic, Gravity Scale 0, Freeze Rotation Z; layer `Player`,
  `Enemy`, `Pickup`; tắt va chạm `Pickup`↔`Enemy`.
- **Dừng game khi lên cấp:** `Time.timeScale = 0`; `Update()` vẫn chạy để UI nhận click, tránh logic
  phụ thuộc `FixedUpdate` lúc này; Animator panel (nếu có) để Unscaled Time.

**Điểm sáng tạo so với GDD** (đã duyệt):
1. Ụ sạc là **"bệ tăng áp"**: đứng sạc vừa hồi pin vừa nhân đôi Lực hút, đổi lại tốc độ giảm mạnh
   (≈ -70%) — biến hồi máu thành pha đánh đổi căng hơn (AC-9).
2. **Mắt phát sáng trong đêm** sinh bằng code cho toàn bộ thực thể, LED robot là chấm đổi màu —
   không khí ban đêm mà không cần asset art (AC-12).

**Global constants** (giá trị khởi đầu, đặt trong SO/Inspector — chỉnh khi cân bằng):
- Pin tối đa `100`; tụt nền `0,25/giây`; bất tử sau trúng đòn `0,4s`.
- Tốc độ robot `5`; bán kính nhặt bụi `1,5`.
- Lực hút cấp 1: `4` sát thương/giây, bán kính `1,5`; overdrive = ×2 khi đang sạc.
- Công thức lên cấp: bụi cần cho cấp `n` = `5 + n*4`.
- Ụ sạc: hồi `15/giây`, chu kỳ xuất hiện `~30s` (rút từ 45s của GDD cho vừa màn 3 phút), tồn tại
  `20s`.
- Thời lượng lát cắt (điều kiện Dọn xong): `180s`.

**Chiến lược verify:** EditMode test cho logic thuần; vào Play mode qua `unity command eval` để
spawn rác, kiểm tra console sạch lỗi và chụp màn hình; `unity test` cho suite; phần cảm giác/cân
bằng do người dùng tự chơi đánh giá (Claude không thay thế được).

**Ràng buộc môi trường:** Unity 6000.5.10f1, 2D URP; việc tự ráp scene/prefab/SO thực hiện qua
Unity CLI (`unity command eval`) nên **Editor phải giữ mở** suốt quá trình; không sửa tay file
`.unity`/`.prefab`/`.asset` khi Editor đang mở.

## Tasks

> Quy ước: `[S/M/L]` = khối lượng code; `[P]` = chạy song song an toàn; "done when" là lệnh/khói
> chứng minh task. Logic thuần → test EditMode (`unity test --mode EditMode`, chạy từ thư mục
> project). MonoBehaviour/ráp scene → compile sạch + khói Play mode qua `unity command eval` +
> ảnh chụp, console không lỗi. "biên dịch sạch" = `unity status` về `ready` với 0 compile error.

### AC-1 · Di chuyển + camera
- [x] **T1** [M] Nền tảng dự án + scene Game
  - files: `Assets/Scripts/VacuumSurvivor.asmdef` (ref `Unity.InputSystem`), `Utils/IDamageable.cs`,
    `Utils/GameObjectPool.cs`, `Art/SpriteFactory.cs`, `Core/GameManager.cs`, `Core/GameEvents.cs`,
    scene `Assets/Scenes/Game.unity` (dựng qua eval)
  - contract: `IDamageable { void TakeDamage(float amount); }`. `GameObjectPool` dùng `Queue`,
    `Get(Vector3)`/`Return(GameObject)` bằng `SetActive`, không Instantiate/Destroy khi chơi.
    `SpriteFactory.CircleWithEyes(Color body, int px=64)` → `Sprite` (nền tối, 2 chấm mắt sáng).
    `GameManager` singleton: `State{Playing,Paused,Won,Lost}`, `Transform Player`, đồng hồ màn.
    Layer `Player`/`Enemy`/`Pickup`, tắt va chạm `Pickup`↔`Enemy`.
  - tests: (không có unit — hạ tầng); chứng minh qua compile + mở scene
  - done when: biên dịch sạch; scene `Game` mở được; vào Play mode không lỗi console
- [x] **T2** [M] `PlayerController` + `CameraFollow` (depends on T1)
  - files: `Player/PlayerController.cs`, `Core/CameraFollow.cs`
  - contract: đọc trục Move (Input System mới) → `Rigidbody2D.velocity = dir.normalized *
    moveSpeed` trong `FixedUpdate`; `moveSpeed` mặc định 5, lấy từ `PlayerStats`. `CameraFollow`
    lerp vị trí camera tới Player trong `LateUpdate`.
  - tests: (MonoBehaviour) — khói Play mode
  - done when: Play mode: robot di chuyển 8 hướng theo input mô phỏng, camera bám; ảnh chụp

### AC-2 · Pin = máu
- [x] **T3** [M] `BatteryModel` (thuần) + `PlayerStats` + thua khi cạn (depends on T1)
  - files: `Player/BatteryModel.cs`, `Player/PlayerStats.cs`,
    `Assets/Tests/EditMode/VacuumSurvivor.Tests.EditMode.asmdef`, `Assets/Tests/EditMode/BatteryModelTests.cs`
  - contract: `BatteryModel{ float Max; float Current; void Tick(float dt,float drainPerSec);
    void Damage(float d); void Charge(float dt,float perSec); bool IsDead=>Current<=0 }`, kẹp
    `0..Max`. `PlayerStats` bọc model + i-frame 0,4s cho contact damage; `Max=100`, `drain=0,25/s`;
    pin=0 → `GameManager.Lose()`. Phát event khi pin đổi.
  - tests: `Tick trừ drain*dt` · `kẹp tại 0` · `kẹp tại Max` · `Damage trừ đúng` · `IsDead khi <=0`
  - done when: `unity test --mode EditMode` xanh cho `BatteryModelTests`; Play mode: pin tụt dần

### AC-3 · LED & màu thanh pin
- [x] **T4** [S] `BatteryColor` (thuần) + `BatteryLED` (depends on T3)
  - files: `Player/BatteryColor.cs`, `Player/BatteryLED.cs`, `Assets/Tests/EditMode/BatteryColorTests.cs`
  - contract: `BatteryColor.For(float percent)→Color`: `>0.6` xanh `#4ADE80`, `0.3..0.6` vàng
    `#FACC15`, `<0.3` đỏ `#EF4444`. `BatteryLED` đổi màu chấm LED theo `PlayerStats` (nhấp nháy khi
    `<0.3`). HUD (AC-10) tái dùng `BatteryColor`.
  - tests: `>60% xanh` · `biên 60%` · `30-60% vàng` · `<30% đỏ`
  - done when: `unity test --mode EditMode` xanh cho `BatteryColorTests`; Play mode: LED đổi màu

### AC-5 · 3 loại rác (EnemyBase + lớp con)
- [x] **T5** [M] `Health` (thuần) + `EnemyData` SO + `EnemyBase` + `ChaserEnemy` (depends on T1)
  - files: `Enemies/Health.cs`, `Enemies/EnemyData.cs`, `Enemies/EnemyBase.cs`,
    `Enemies/ChaserEnemy.cs`, `Assets/Tests/EditMode/HealthTests.cs`
  - contract: `Health{ float Max; float Current; void Damage(float); bool IsDead }`. `EnemyData` SO:
    `maxHealth, moveSpeed, contactDamage, dustValue, prefab, splitsOnDeath, splitInto, splitCount`.
    `EnemyBase` abstract : `IDamageable`, nhấp nháy trắng 0,1s + đẩy lùi khi trúng, `Die()`→rơi
    bụi + trả pool, `abstract Move()`. `ChaserEnemy.Move()` = bò thẳng về Player.
  - tests: `Damage giảm máu` · `IsDead tại 0` · `không âm`
  - done when: `unity test --mode EditMode` xanh cho `HealthTests`; Play mode: rác bò về, trúng đòn nhấp nháy
- [x] **T6** [M] `SplitterEnemy` (depends on T5)
  - files: `Enemies/SplitterEnemy.cs`, `Assets/Tests/EditMode/SplitLogicTests.cs`
  - contract: `SplitLogic.ChildCount(bool canSplit)→ canSplit?2:0` (thuần). `SplitterEnemy` ghi đè
    `Die()`: nếu `canSplit` thì sinh đúng 2 con `splitInto` với `canSplit=false`, rồi trả pool;
    con nhỏ không tách tiếp.
  - tests: `canSplit→2` · `không canSplit→0`
  - done when: `unity test --mode EditMode` xanh; Play mode: tóc rối chết tách đúng 2, con nhỏ không tách

### AC-4 · Spawner + WaveData + pool
- [x] **T7** [M] `WaveData` SO + `WaveSchedule` (thuần) + `EnemySpawner` (depends on T5)
  - files: `Enemies/WaveData.cs`, `Enemies/WaveSchedule.cs`, `Enemies/EnemySpawner.cs`,
    `Assets/Tests/EditMode/WaveScheduleTests.cs`
  - contract: `WaveData` SO = list `{startTime, spawnInterval, maxAlive, EnemyData[] types,
    int burst}`. `WaveSchedule.Current(float t, entries)→entry` (thuần, chọn entry có startTime
    lớn nhất ≤ t). `EnemySpawner` lấy rác từ pool, spawn trên vòng tròn ngoài rìa camera (~2 đv),
    tôn trần `maxAlive`, rác xa >30 đv thì dời về trước mặt Player; Vụn bánh mì spawn theo `burst` 5-8.
  - tests: `t<đầu→entry[0]` · `chọn entry mới nhất ≤ t` · `đúng biên startTime`
  - done when: `unity test --mode EditMode` xanh cho `WaveScheduleTests`; Play mode: rác spawn quanh
    người chơi, không vượt trần, không lang thang quá xa

### AC-6 · Vũ khí Lực hút
- [x] **T8** [M] `WeaponData` SO + `WeaponBase` + `SuctionWeapon` + `WeaponManager` (depends on T5)
  - files: `Weapons/WeaponData.cs`, `Weapons/WeaponBase.cs`, `Weapons/SuctionWeapon.cs`,
    `Weapons/WeaponManager.cs`, `Weapons/SuctionStats.cs`, `Assets/Tests/EditMode/SuctionStatsTests.cs`
  - contract: `WeaponData` SO: `float[] dps`, `float[] radius`, `int maxLevel`. `SuctionStats.For(
    WeaponData,int level,float overdrive)→(dps,radius)` (thuần, overdrive nhân cả hai, mặc định 1).
    `SuctionWeapon`: mỗi tick `OverlapCircle` layer Enemy, gây `dps*dt`. L1 = dps 4, radius 1,5.
    `WeaponManager` giữ vũ khí đang mang + hàm nâng cấp.
  - tests: `dps/radius theo cấp` · `overdrive=2 gấp đôi` · `overdrive=1 giữ nguyên`
  - done when: `unity test --mode EditMode` xanh cho `SuctionStatsTests`; Play mode: rác trong vòng mất máu & chết

### AC-7 · Bụi + lên cấp
- [x] **T9** [M] `Dust` pooled + `PlayerMagnet` + `LevelFormula` (thuần) (depends on T3, T5)
  - files: `Pickups/Dust.cs`, `Player/PlayerMagnet.cs`, `Player/LevelFormula.cs`,
    `Assets/Tests/EditMode/LevelFormulaTests.cs`
  - contract: `LevelFormula.DustForLevel(int n)→5+n*4` (thuần). `Dust` lấy từ pool, `PlayerMagnet`
    hút bụi trong `pickupRadius=1,5` bay về Player bằng Lerp tăng tốc dần; chạm thì `PlayerStats
    .AddDust(data.dustValue)`. Đủ ngưỡng → tăng cấp + phát event lên cấp (xếp hàng nếu nhiều cấp).
  - tests: `DustForLevel(1)=9` · `DustForLevel(2)=13` · `cộng dồn đúng` · `vượt ngưỡng→lên cấp` ·
    `dư nhiều→xếp hàng đúng số lần`
  - done when: `unity test --mode EditMode` xanh cho `LevelFormulaTests`; Play mode: rác chết rơi bụi,
    hút về, thanh bụi đầy → lên cấp

### AC-8 · Panel chọn nâng cấp
- [x] **T10** [M] `UpgradeData` SO + `UpgradeManager` + `LevelUpPanel` (depends on T8, T9)
  - files: `Upgrades/UpgradeData.cs`, `Upgrades/UpgradeManager.cs`, `Upgrades/UpgradeRoll.cs`,
    `UI/LevelUpPanel.cs`, `Assets/Tests/EditMode/UpgradeRollTests.cs`
  - contract: `UpgradeType{SuctionLevel,MaxBattery,MoveSpeed,MagnetRadius,SuctionDamage}`.
    `UpgradeData` SO: `type,title,description,float[] valuePerLevel,maxLevel=5`. `UpgradeRoll.Pick(
    IList available,int n,System.Random)→n mục khác nhau` (loại mục đã max). `UpgradeManager.Apply`
    cộng `valuePerLevel[level]` vào hệ số tương ứng ở `PlayerStats`/`WeaponManager`. `LevelUpPanel`:
    `Time.timeScale=0`, hiện 3 thẻ, click chọn → Apply → đóng → `timeScale=1`; xếp hàng nhiều cấp.
  - tests: `Pick trả 3 mục` · `3 mục khác nhau` · `bỏ mục đã max` · `Apply cộng đúng giá trị`
  - done when: `unity test --mode EditMode` xanh cho `UpgradeRollTests`; Play mode: lên cấp → dừng game,
    chọn thẻ → áp dụng, chạy tiếp

### AC-9 · Ụ sạc "tăng áp"
- [x] **T11** [M] `ChargingDock` (depends on T3, T8)
  - files: `Pickups/ChargingDock.cs`
  - contract: dock sinh mỗi `~30s`, tồn tại `20s` (mờ dần 3s cuối). Khi Player trong trigger:
    `PlayerStats.Charge(15/s)`, tốc độ di chuyển ×0,3 (giảm ~70%), và đặt `SuctionWeapon.overdrive=2`
    (bán kính + sát thương ×2); rời dock thì khôi phục.
  - tests: (MonoBehaviour) — khói Play mode
  - done when: Play mode: đứng sạc → pin tăng, robot chậm hẳn, vòng Lực hút to gấp đôi; rời ra hồi bình thường

### AC-10 · HUD
- [x] **T12** [M] `HUD` (depends on T3, T4, T8, T9)
  - files: `UI/HUD.cs`
  - contract: bind vào event của `PlayerStats`/`GameManager`/`WeaponManager`: thanh pin (màu theo
    `BatteryColor`, nhấp nháy <30%), thanh bụi (tiến tới ngưỡng cấp kế), đồng hồ đếm giờ màn, số cấp
    người chơi, cấp Lực hút. Dùng TextMeshPro.
  - tests: (UI) — khói Play mode
  - done when: Play mode: mọi chỉ số cập nhật realtime khi chơi; ảnh chụp

### AC-11 · Màn kết thúc + chơi lại
- [x] **T13** [M] `SliceClock` (thuần) + win/lose + `EndScreen` (depends on T1, T3)
  - files: `Core/SliceClock.cs`, `UI/EndScreen.cs`, `Core/GameManager.cs` (win/lose/timer)
  - contract: `SliceClock.IsCleared(float t,float duration=180)→t>=duration` (thuần). `GameManager`
    đếm giờ: đạt 180s → `Win()`; `PlayerStats` cạn → `Lose()`. `EndScreen` hiện "DỌN XONG LÁT NÀY"
    (thắng) / "Chủ về thấy nhà bừa bộn" (thua) + nút Chơi lại = reload scene `Game`.
  - tests: `chưa tới hạn→false` · `đúng hạn→true` · `quá hạn→true`
  - done when: `unity test --mode EditMode` xanh cho `SliceClock`; Play mode: tới 180s → màn Dọn xong;
    ép pin=0 → màn Thua; bấm Chơi lại → ván khởi động lại

### AC-12 · Ràng buộc kỹ thuật — (no task - proven by the gate)
- Chứng minh bởi gate + review code: compile sạch; suite EditMode xanh; pool dùng ở rác/bụi (T1,
  T5, T9); số liệu nằm trong SO (T5–T10); sprite sinh bằng code (T1, `SpriteFactory`). Không tạo
  task riêng để thỏa coverage.

## End-to-end verification
Chứng minh toàn lát cắt chạy (chạy từ thư mục project, Editor đang mở):

1. **Biên dịch:** `unity status --format json` → `state: ready`, 0 compile error.
2. **Test logic:** `unity test --mode EditMode --format json` → tất cả suite xanh (BatteryModel,
   BatteryColor, Health, SplitLogic, WaveSchedule, SuctionStats, LevelFormula, UpgradeRoll, SliceClock).
3. **Khói Play mode** (qua `unity command eval`, một mạch): vào Play → robot di chuyển → rác spawn
   quanh người chơi và bị Lực hút giết → bụi hút về, thanh bụi đầy → panel lên cấp hiện ra, chọn 1
   thẻ, game chạy tiếp → ụ sạc xuất hiện, đứng lên thấy pin tăng + vòng hút gấp đôi → HUD cập nhật
   đúng. Ép `PlayerStats` pin=0 → màn Thua; đặt đồng hồ ≥180s → màn Dọn xong; bấm Chơi lại reload.
   Trong suốt quá trình **console không có lỗi**; chụp ảnh các mốc chính.
4. **Người dùng tự chơi** một ván ~3 phút để đánh giá cảm giác & cân bằng (Claude không thay thế được).

## Open questions
<!-- trống — đã chốt ở bước brainstorming -->
