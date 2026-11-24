# Bàn giao tạm cho đợt đại tu giao diện Vacuum Survivor

## Bắt đầu phiên Codex mới

Mở Codex tại `D:\GameProjects\Unity\VacuumSurvivor` và gửi lời nhắn sau cùng brief hoặc ảnh tham chiếu mới.

> Đọc `docs/UI-OVERHAUL-HANDOFF.md` trước. Kiểm tra trạng thái Git và xem giao diện đang chạy trong Unity. Dùng brief mình gửi ở phiên này để đại tu giao diện Vacuum Survivor.

## Mục tiêu và phạm vi

- Vacuum Survivor là đồ án Unity 2D của nhóm bốn người. Vòng chơi gồm sinh tồn, tự động tấn công, quản lý pin, nhặt bụi để lên cấp và đánh boss qua ba phòng.
- Góp ý trước của cô tập trung vào asset chưa cập nhật và giao diện cũ còn sơ sài. Ưu tiên tạo khác biệt có thể thấy ngay khi chạy demo.
- Gu đã thống nhất ở lượt trước là sạch, có chủ đích như game indie, ít chi tiết trang trí gây nhiễu. Chữ tiếng Việt phải dễ đọc, robot và quái phải nổi rõ trên nền. Bản HUD hiện tại là điểm xuất phát. Phong cách cuối, ảnh tham chiếu và phạm vi từng màn hình sẽ theo brief mới.
- Gameplay hiện tại theo `VACUUM-SURVIVOR-GDD.md`. Túi chứa rác nhiệm vụ đã được ghi trong phần Hướng phát triển như ý tưởng nếu còn thời gian.
- Các skill pixel art mới cài là công cụ tùy chọn. Hình ảnh hiện tại vẫn theo bộ Soft2D, nên cần chủ đích rõ nếu chuyển cả game sang pixel art.

## Bản đồ file và hành vi

| Bề mặt | Unity prefab | Controller |
| --- | --- | --- |
| Menu chính, hướng dẫn và điểm cao | `Assets/Prefabs/UI/MainMenuScreen.prefab` | `Assets/Scripts/UI/Screens/MainMenuScreen.cs` |
| HUD trong trận | `Assets/Prefabs/UI/HUD.prefab` | `Assets/Scripts/UI/Gameplay/HUD.cs` |
| Chọn nâng cấp | `Assets/Prefabs/UI/LevelUpPanel.prefab` | `Assets/Scripts/UI/Gameplay/LevelUpPanel.cs` |
| Banner chuyển phòng | `Assets/Prefabs/UI/LevelBanner.prefab` | `Assets/Scripts/UI/Gameplay/LevelBanner.cs` |
| Kết quả thắng/thua | `Assets/Prefabs/UI/EndScreen.prefab` | `Assets/Scripts/UI/Screens/EndScreen.cs` |

- Hai scene là `Assets/Scenes/MainMenu.unity` và `Assets/Scenes/Game.unity`. Các màn UI dùng Canvas prefab chỉnh trong Unity, được đưa vào repo ở commit `8373a25`. Trước khi thay prefab, kiểm tra tham chiếu serialized, nút đã bind, anchor và scene override. `Assets/Editor/VSBuild.cs` là trình dựng lại scene.
- `GameEvents.cs` phát dữ liệu cho UI. `GameManager.cs` quản lý timer, boss, tạm dừng, chuyển phòng và kết quả. `UpgradeManager.cs` mở thẻ nâng cấp, áp dụng lựa chọn và xử lý hàng chờ.
- ESC tạm dừng và tiếp tục. Bảng nâng cấp cần giữ quyền tạm dừng cho tới lúc chọn xong. Banner chuyển phòng chạy theo thời gian thực trong khi gameplay dừng. Màn kết quả dùng thống kê lượt chơi và bảng điểm cục bộ.
- HUD mới gom pin, XP và cấp ở góc trên trái, tên phòng và đồng hồ ở giữa, thanh máu boss bên dưới đồng hồ và danh sách vũ khí ở góc dưới trái. Kiểm tra cả lúc ít và nhiều vũ khí, pin thấp, boss xuất hiện và tạm dừng.
- Sàn hiện là `Assets/Art/Soft2D/ParquetFloor.png`, gỗ nâu xám vân nhẹ đã thử trực tiếp trong Game View. `Assets/Prefabs/Maps/Room.prefab` và `RoomBounds.cs` xử lý lát nền, tường và giới hạn phòng. Ba phòng hiện được phân biệt bằng kích thước, màu sàn, đợt quái và boss. Bộ sprite của robot, quái, boss, pickup và vũ khí nằm trong `Assets/Art/Soft2D/`.

## Công cụ tạo asset

- `$pixel-art-gen` xuất PNG từ lưới pixel qua Python và Pillow. Đã render thử.
- `$pixelfx` xuất VFX và sprite sheet. Checkout cục bộ tại `.agents/tools/pixelfx`, nằm ngoài Unity Assets và được Git bỏ qua. Đã xuất thử 60 frame và xác minh Sprite Forge tìm được khi truyền `--pixelfx .agents/tools/pixelfx`.
- Agent Sprite Forge có `$generate2dsprite`, `$video2dsprite`, `$generate2dmap`, `$generate2dmedia` và `$codeart2d`. Giữ năm thư mục skill cạnh nhau để các script tìm được nhau.
- `$native-pixel-art` dùng cấu hình trong `tools/pixel_art/` với năm asset tham chiếu từ game. CLI tại `.agents/tools/native-pixel-art-venv/Scripts/pixel-art.exe`. Đã qua mọi kiểm tra bắt buộc của `doctor` khi thêm thư mục `Scripts` vào PATH của phiên PowerShell. Aseprite và OpenAI API key là tùy chọn và chưa có khi kiểm tra.

## Xác minh và trạng thái bàn giao

- Unity `6000.5.10f1`, dự án Windows. Lệnh `unity command run_tests --mode editor` đã báo **172/172 đạt** sau lần chỉnh HUD và sàn. Xem thêm `Assets/Tests/EditMode/CanvasUITests.cs` và `Assets/Tests/EditMode/PngArtTests.cs`.
- Đã chụp Game View trong Play Mode để rà HUD và sàn. Lượt chơi trực quan qua cả ba boss, giao diện khi đông quái và vật cản riêng của từng phòng vẫn cần kiểm tra.
- `Builds/VacuumSurvivor/VacuumSurvivor.exe` là build Windows từ trước đợt chỉnh HUD và sàn gần nhất. Dùng Unity để xem bản hiện tại hoặc đóng gói lại sau đợt đại tu.
- Tại thời điểm bàn giao, HEAD là `c8354cb`. Workspace có thay đổi chưa commit về gameplay, scene, HUD, sàn, GDD và các skill. Chạy `git status --short` khi bắt đầu và giữ các thay đổi đang có. Nếu chuyển phiên bằng cùng workspace, các file vẫn ở đó. Nếu chuyển sang checkout hoặc máy khác, cần mang theo thay đổi chưa commit và cài lại các công cụ cục bộ bị Git bỏ qua.
