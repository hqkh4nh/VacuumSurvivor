TRƯỜNG ĐẠI HỌC CÔNG NGHỆ THÔNG TIN &

TRUYỀN THÔNG VIỆT HÀN

**Khoa Khoa Học Máy Tính**

*(Logo trường — chèn ảnh khi nộp báo cáo)*

BÀI TẬP LỚN

LẬP TRÌNH GAME

**GAME VACUUM SURVIVOR**

Sinh viên thực hiện:

Huỳnh Quốc Khánh 23IT124

Trần Hoàng Nhật 23IT199

Ngô Nhật Phong 23IT209

Trịnh Duy Hiếu 23IT080

Nhóm thực hiện: 18

Nhóm học phần: Lập trình game (2)

Đà Nẵng, tháng 10 năm 2026

---

*Tài liệu này là báo cáo bài tập lớn kiêm tài liệu thiết kế game (Game Design Document) của game **Vacuum Survivor**. Phần kịch bản game (Chương 1) được xây dựng trên nền tài liệu thiết kế chi tiết; mọi con số đều là điểm xuất phát để thử nghiệm và cân bằng, không phải con số cuối cùng.*

---

# **BẢNG PHÂN CÔNG CÔNG VIỆC**

| **Tên thành viên**                | **Công việc thực hiện** | **Tỉ lệ đóng góp vào bài tập lớn (chia theo tỉ lệ %)** |
| --------------------------------- | ----------------------- | ------------------------------------------------------ |
| Huỳnh Quốc Khánh<br><br>(23IT124) |                         |                                                        |
| Trần Hoàng Nhật<br><br>(23IT199)  |                         |                                                        |
| Ngô Nhật Phong<br><br>(23IT209)   |                         |                                                        |
| Trịnh Duy Hiếu<br><br>(23IT080)   |                         |                                                        |

**Link mã nguồn:**

---

# **Chương 1. XÂY DỰNG KỊCH BẢN GAME**

## **1.1. Giới thiệu chung**

Tên game: Vacuum Survivor.

Thể loại: sinh tồn đánh quái theo đợt (survivors-like), góc nhìn từ trên xuống, nhân vật tự động tấn công.

Nền tảng: PC (Windows), có thể build thêm bản WebGL để chơi trên trình duyệt.

Công cụ phát triển: Unity 2D, ngôn ngữ lập trình C#.

Số người chơi: 1 người. Game chia thành 3 màn chơi, mỗi màn kéo dài khoảng 5-7 phút.

Nhóm chọn đề tài này vì ý tưởng robot hút bụi ai cũng quen, người chơi nhìn vào là hiểu phải làm gì mà không cần giải thích nhiều. Nhân vật và kẻ địch chủ yếu là các hình tròn có gắn mắt nên phần đồ họa không quá nặng, nhóm có thể dành thời gian cho phần lập trình gameplay.

### 1.1.1. Thông tin nhanh

| Mục | Nội dung |
|---|---|
| Tên game | Vacuum Survivor |
| Thể loại | Survivors-like / Horde survival (top-down, auto-attack) |
| Engine | Unity 2022.3 LTS hoặc Unity 6 (2D URP) |
| Ngôn ngữ | C# |
| Góc nhìn | Từ trên xuống (top-down), 2D |
| Cấu trúc | 3 màn chơi (Phòng khách, Phòng ngủ, Nhà bếp) |
| Thời lượng mỗi màn | Khoảng 5-7 phút |
| Nền tảng | PC (Windows), có thể build WebGL để demo |
| Quy mô | Nhóm 4 người, khoảng 5 tuần |
| Điều khiển | WASD / phím mũi tên, chuột chọn nâng cấp |

**Pitch một câu:** Bạn là con robot hút bụi bị bỏ quên trong nhà lúc nửa đêm. Rác trong nhà sống dậy. Dọn sạch từng phòng và sống sót tới 6 giờ sáng với pin chưa cạn, để chủ tỉnh dậy thấy nhà sạch bong.

### 1.1.2. Vì sao chọn chủ đề này cho đồ án

- **Art rẻ nhất có thể.** Nhân vật chính là một hình tròn. Kẻ địch là các hình tròn xù có mắt. Không có khung xương người, không có animation đi bộ 8 hướng, không có tay chân.
- **Ai cũng hiểu trong 5 giây.** Không cần màn hình cốt truyện, không cần giải thích luật chơi.
- **Chưa đụng hàng.** Trên itch.io có nhiều game robot hút bụi nhưng chủ yếu là mô phỏng, puzzle và platformer. Kết hợp robot hút bụi với công thức survivors-like gần như chưa có ai làm.
- **Mọi cơ chế đều tự giải thích.** Pin tụt dần, ụ sạc, túi bụi đầy — đều là chuyện hợp lý với một con robot hút bụi, không phải bịa ra để phục vụ gameplay.
- **Mở rộng tuyến tính.** Xong vòng lặp cốt lõi rồi thì thêm quái = thêm prefab, thêm vũ khí = thêm ScriptableObject, thêm màn = thêm dữ liệu. Không phải đụng lại kiến trúc.

### 1.1.3. Tông màu

Hài hước nhẹ, hơi cô đơn. Không kinh dị, không máu me. Kiểu "một con robot bé nhỏ nghiêm túc làm nhiệm vụ trong một căn nhà tối om".

## **1.2. Cốt truyện**

11 giờ đêm (23:00), chủ nhà đi vắng. Con robot hút bụi đời 3000 tự khởi động để dọn nhà theo lịch như mọi ngày. Nhưng đêm nay có chuyện lạ: bụi bông biết bò, vụn bánh mì biết chạy, cục tóc rối dưới gầm giường cũng bắt đầu di chuyển. Rác trong nhà đã "sống dậy".

Nhiệm vụ của robot là dọn sạch căn nhà — lần lượt qua từng phòng — và trụ được tới 6 giờ sáng, trước khi chủ nhà thức dậy. Vấn đề là pin của robot có hạn và tụt dần theo thời gian.

## **1.3. Nhân vật và cách điều khiển**

Người chơi điều khiển con robot hút bụi hình tròn, trên đầu có một đèn LED báo mức pin: xanh lá khi pin trên 60%, vàng khi từ 30% đến 60%, đỏ nhấp nháy kèm tiếng bíp khi dưới 30%. Khi pin dưới 10% robot chạy chậm lại.

Game không có nút tấn công. Vũ khí của robot tự động đánh, người chơi chỉ cần tập trung di chuyển để né rác và nhặt bụi — đây là đặc trưng bắt buộc của thể loại survivors-like.

### 1.3.1. Chỉ số khởi đầu

| Chỉ số | Giá trị gốc | Ghi chú |
|---|---|---|
| Pin tối đa | 100 | Vai trò của máu |
| Tốc độ tụt pin | 0,25 / giây | Khoảng 400 giây nếu không bị đánh |
| Tốc độ di chuyển | 5 đơn vị / giây | |
| Sát thương cơ bản | hệ số 1,0 | Nhân vào sát thương mọi vũ khí |
| Vùng ảnh hưởng | hệ số 1,0 | Nhân vào bán kính vũ khí |
| Bán kính nhặt bụi | 1,5 đơn vị | |
| Giảm sát thương nhận | 0% | |
| Thời gian bất tử sau khi trúng đòn | 0,4 giây | Tránh bị "khóa" khi kẹt trong đám đông |

### 1.3.2. Điều khiển

- **WASD / phím mũi tên**: di chuyển 8 hướng
- **Chuột**: chỉ dùng ở menu và panel chọn nâng cấp
- **ESC**: tạm dừng
- **Không có nút tấn công.** Vũ khí tự động đánh — người chơi chỉ tập trung vào việc đi đứng ở đâu.

### 1.3.3. Chi tiết nhỏ nhưng đáng giá — đèn LED báo pin

Đèn LED trên đầu robot đổi màu theo mức pin:

| Mức pin | Màu đèn | Hiệu ứng thêm |
|---|---|---|
| > 60% | Xanh lá | — |
| 30-60% | Vàng | — |
| < 30% | Đỏ nhấp nháy | Tiếng bíp mỗi 1,5 giây, viền màn hình đỏ mờ |
| < 10% | Đỏ nhấp nháy nhanh | Tiếng bíp mỗi 0,5 giây, robot chạy chậm 15% |

Tốn khoảng 15 dòng code, nhưng là thứ người chơi nhớ nhất khi demo.

## **1.4. Luật chơi**

### 1.4.1. Tóm tắt luật

- Pin đóng vai trò như máu của nhân vật. Pin tự tụt 0,25 mỗi giây, bị rác chạm vào hoặc đứng trong vũng nước cũng bị mất pin. Pin về 0 thì thua.
- Cứ khoảng 45 giây sẽ có một ụ sạc xuất hiện ngẫu nhiên trên bản đồ và tồn tại 20 giây. Đứng trên ụ sạc được hồi 15 pin/giây nhưng robot bị giảm 70% tốc độ, nên người chơi phải cân nhắc lúc nào nên về sạc.
- Rác bị tiêu diệt sẽ rơi ra bụi. Robot đi lại gần thì bụi tự bay vào túi. Túi bụi đầy thì robot lên cấp.
- Khi lên cấp, game tạm dừng và hiện ra 3 lựa chọn ngẫu nhiên: nhận vũ khí mới, nâng cấp vũ khí đang có hoặc nhận một nâng cấp bị động. Người chơi chọn 1 rồi chơi tiếp.
- Càng về sau rác xuất hiện càng nhiều và càng mạnh. Mỗi màn kết thúc bằng một con boss: Màn 1 và Màn 2 là boss Con mèo, Màn 3 là boss Máy hút bụi cũ.

> **Ghi chú thiết kế (tiến trình giữa các màn):** vũ khí và các nâng cấp đã chọn được **giữ nguyên sang màn sau** (tiến trình tích lũy như một ván survivors kéo dài). Giữa hai màn, robot được **sạc đầy pin** trước khi vào màn mới. Đây là lựa chọn mặc định hợp lý cho thiết kế 3 màn; có thể chỉnh nếu thử nghiệm thấy quá dễ hoặc quá khó.

### 1.4.2. Vòng lặp cốt lõi

```
        ┌─────────────────────────────────────────┐
        │                                         │
        ▼                                         │
   Di chuyển né rác  ──►  Vũ khí tự động đánh     │
        │                        │                │
        │                        ▼                │
        │                  Rác chết, rơi BỤI      │
        │                        │                │
        ▼                        ▼                │
   Pin tụt dần          Hút bụi → đầy TÚI         │
        │                        │                │
        ▼                        ▼                │
   Tìm Ụ SẠC             LÊN CẤP → chọn 1/3       │
        │                    nâng cấp             │
        └────────────────────────┴────────────────┘
                              │
                              ▼
                  Rác nhiều hơn, mạnh hơn
                              │
                              ▼
            Hạ boss cuối màn → sang màn kế tiếp
                              │
                              ▼
        Hạ boss Màn 3 (Máy hút bụi cũ) = THẮNG
```

### 1.4.3. Bảng ánh xạ với thể loại gốc

Đây là công thức survivors-like chuẩn, chỉ đổi lớp vỏ. Ghi chú lại bảng này trong báo cáo để thể hiện nhóm hiểu mình đang xây trên nền thể loại nào.

| Survivors-like gốc | Trong game này | Ý nghĩa thiết kế |
|---|---|---|
| Máu (HP) | **Pin** | Tụt dần theo thời gian → sức ép liên tục miễn phí |
| Ngọc kinh nghiệm | **Bụi** | Rơi ra từ rác bị tiêu diệt |
| Lên cấp | **Túi bụi đầy** | Mở panel chọn nâng cấp |
| Hồi máu | **Về ụ sạc** | Phải đứng yên → đánh đổi rủi ro |
| Sống sót 15-30 phút | **3 màn, mỗi màn ~5 phút** | Chia nhỏ cho vừa quy mô đồ án |
| Boss giữa chừng | **Con mèo** | Cuối Màn 1 và Màn 2 |
| Boss cuối | **Máy hút bụi cũ** | Cuối Màn 3 |

### 1.4.4. Hệ thống Pin (thay cho máu)

Đây là cơ chế đặc trưng của game, cần làm sớm và làm kỹ.

**Cách pin tụt:**

1. **Tụt nền:** 0,25/giây, luôn luôn. Trong một màn 5 phút (300 giây) tổng cộng là 75 điểm pin — nghĩa là người chơi **bắt buộc** phải sạc ít nhất 1 lần mỗi màn dù không bị đánh trúng phát nào.
2. **Chạm rác:** mất pin theo từng loại (xem bảng kẻ địch mục 1.5), có thời gian hồi 0,4 giây giữa 2 lần trúng.
3. **Đứng trong vũng nước:** rò điện, mất 4 pin/giây liên tục.

**Ụ sạc:**

- Xuất hiện ở vị trí ngẫu nhiên trên bản đồ **mỗi 45 giây**
- Tồn tại **20 giây** rồi biến mất (có hiệu ứng mờ dần 3 giây cuối để báo trước)
- Đứng lên: hồi **15 pin/giây**, nhưng **tốc độ di chuyển giảm 70%** khi đang sạc
- Có mũi tên chỉ hướng ở rìa màn hình khi ụ sạc đang hoạt động ngoài tầm nhìn

**Vì sao thiết kế như vậy:** người chơi luôn phải chọn giữa "lao vào đám đông hút nhiều bụi để lên cấp nhanh" và "rút về sạc cho an toàn". Đây là quyết định thú vị nhất trong game và nó xuất hiện vài chục lần mỗi màn. Nếu chỉ được làm một cơ chế ngoài phần cơ bản, hãy làm cái này.

### 1.4.5. Hệ thống Bụi và lên cấp

**Bụi:**

- Rác chết rơi ra các hạt bụi nhỏ nằm trên sàn.
- Bụi trong **bán kính nhặt** sẽ bị hút bay về phía robot (dùng `Vector2.Lerp` với tốc độ tăng dần — nhìn rất đã, code 5 dòng).
- Bụi tồn tại vĩnh viễn, không biến mất, nên người chơi có thể để dành rồi quét dọn sau.

**Công thức lên cấp:**

```
Lượng bụi cần để lên cấp n = 5 + (n × 4)
```

| Cấp | Bụi cần | Cộng dồn |
|---|---|---|
| 1 → 2 | 9 | 9 |
| 2 → 3 | 13 | 22 |
| 3 → 4 | 17 | 39 |
| 5 → 6 | 25 | 87 |
| 10 → 11 | 45 | 302 |

Mục tiêu cân bằng: do vũ khí và nâng cấp được giữ qua các màn, người chơi đạt **khoảng cấp 12-16** ở cuối Màn 3. Nếu thử nghiệm thấy lệch, chỉnh hệ số 4 lên hoặc xuống.

**Khi lên cấp:**

1. Game **tạm dừng hoàn toàn** (`Time.timeScale = 0`)
2. Hiện panel 3 lựa chọn ngẫu nhiên: có thể là vũ khí mới, nâng cấp vũ khí đang có, hoặc nâng cấp bị động
3. Người chơi bấm chọn 1 → panel đóng, game chạy tiếp
4. Nếu lên nhiều cấp cùng lúc thì xếp hàng, hiện panel liên tiếp

**Lưu ý kỹ thuật:** khi `timeScale = 0` thì `Update()` vẫn chạy nhưng `FixedUpdate()` thì không. Nút bấm UI vẫn hoạt động bình thường. Nếu có animation trên panel, nhớ đặt Animator Update Mode thành `Unscaled Time`.

## **1.5. Kẻ địch**

Kẻ địch trong game là các loại rác trong nhà, mỗi loại có một kiểu hành vi riêng. Tất cả đều là hình tròn hoặc hình chữ nhật bo góc có gắn mắt. Cả bộ vẽ trong một buổi tối.

### 1.5.1. Danh sách các loại rác

- **Bụi bông:** loại cơ bản, bò thẳng về phía robot. Xuất hiện ngay từ đầu.
- **Vụn bánh mì:** máu ít nhưng chạy nhanh, xuất hiện theo bầy 5 đến 8 con.
- **Cục tóc rối:** chậm và nhiều máu, khi bị tiêu diệt thì tách ra thành 2 cục nhỏ.
- **Tất lẻ:** đứng yên ngắm khoảng 1 giây (có vạch báo trước) rồi lao thẳng về phía robot.
- **Cục LEGO:** không tiêu diệt được, nằm yên trên sàn như bãi mìn, sau 25 giây thì tự biến mất.
- **Dây sạc:** bò ngoằn ngoèo như rắn, chạm vào sẽ bị quấn chân, không di chuyển được trong 1 giây.
- **Vũng nước:** vật cản đứng yên, robot đứng trong đó bị rò điện mất pin. Có thể dùng lực hút để làm khô.

### 1.5.2. Bảng chỉ số

| Tên | Máu | Tốc độ | Sát thương | Bụi rơi | Hành vi |
|---|---|---|---|---|---|
| **Bụi bông** | 10 | 1,8 | 5 | 1 | Bò thẳng về phía người chơi |
| **Vụn bánh mì** | 6 | 3,5 | 3 | 1 | Nhanh, spawn theo bầy 5-8 con |
| **Cục tóc rối** | 30 | 1,2 | 8 | 3 | Chậm, trâu bò. **Chết thì tách làm 2 con nhỏ** (10 máu, rơi 1 bụi) |
| **Tất lẻ** | 20 | 0 → 9 | 12 | 2 | Đứng yên ngắm 1 giây (có vạch báo), rồi lao thẳng một đường |
| **Cục LEGO** | — | 0 | 20 | — | **Không thể tiêu diệt.** Nằm im làm bãi mìn. Tự biến mất sau 25 giây |
| **Dây sạc** | 25 | 2,5 | 6 | 3 | Bò kiểu rắn. Chạm vào bị **quấn chân, không đi được 1 giây** |
| **Vũng nước** | — | 0 | 4/giây | — | Vật cản tĩnh, đứng trong đó bị rò điện. Hút liên tục 3 giây thì khô |

### 1.5.3. Ghi chú thiết kế từng loại

- **Bụi bông** — quái nền, dạy người chơi luật cơ bản. Chiếm khoảng 50% tổng số quái suốt game.
- **Vụn bánh mì** — nhanh hơn người chơi một chút nếu không nâng tốc độ, buộc phải đánh chứ không chạy được mãi.
- **Cục tóc rối** — cơ chế tách đôi dạy người chơi rằng "giết sạch" không phải lúc nào cũng tốt. Nhớ giới hạn không cho con nhỏ tách tiếp, kẻo lag.
- **Tất lẻ** — loại duy nhất đòi phản xạ né. Vạch báo trước khi lao là bắt buộc, không có thì người chơi thấy game ăn gian.
- **Cục LEGO** — câu đùa ai cũng hiểu, và nó biến bản đồ thành mê cung dần dần. Đây là loại "quái" rẻ nhất để làm: không AI, không animation, chỉ có collider và sát thương.
- **Dây sạc** — kẻ địch khó chịu nhất, nên đừng spawn quá 3 con cùng lúc.
- **Vũng nước** — chia cắt bản đồ, buộc người chơi đi đường vòng. Cho nó hút được để người chơi có cách xử lý chứ không chỉ né.

### 1.5.4. Boss Con mèo *(cuối Màn 1 và Màn 2)*

To gấp khoảng 8 lần robot. Không hung dữ, chỉ là một con mèo đang sống cuộc đời của nó, và điều đó tình cờ rất phiền.

| Thông số | Màn 1 | Màn 2 |
|---|---|---|
| Máu | 400 | 600 |
| Tốc độ | 2,0 (đi lững thững) | 2,5 |
| Bụi rơi khi chết | 40 | 50 |

**Pattern (không cần AI phức tạp):**

1. Đi tới một vị trí ngẫu nhiên trên bản đồ, tới nơi thì dừng 2 giây.
2. **Mỗi 3 giây rụng lông:** sinh ra 1 cục tóc rối ở vị trí hiện tại.
3. **Mỗi 6 giây tát một phát:** nếu người chơi trong bán kính 3, đẩy văng 6 đơn vị và mất 15 pin. Có vạch báo trước 0,7 giây.
4. Dưới 30% máu: tốc độ tăng lên, rụng lông mỗi 2 giây.

Toàn bộ pattern này là một máy trạng thái 3 trạng thái (Đi → Dừng → Tát). Khoảng 80 dòng code.

### 1.5.5. Boss Máy hút bụi cầm tay cũ *(cuối Màn 3)*

Tổ tiên bị thay thế, giờ quay lại báo thù. Hình dáng: một hình chữ nhật cao có ống vòi và một con mắt giận dữ.

| Thông số | Giá trị |
|---|---|
| Máu | 800 |
| Tốc độ | 1,5 |
| Kết thúc | Hạ được boss này = THẮNG toàn bộ game |

**Pattern — luân phiên 3 đòn theo thứ tự cố định, nghỉ 1,5 giây giữa các đòn:**

1. **Hút:** kéo người chơi về phía nó với lực 3 đơn vị/giây trong 3 giây. Người chơi vẫn đi được nhưng bị kéo ngược.
2. **Thổi:** phun ra 8 cục bụi bông theo vòng tròn.
3. **Lao:** lao thẳng một đường qua bản đồ, có vạch báo 1 giây.

## **1.6. Vũ khí và nâng cấp**

Robot có sẵn vũ khí **Lực hút** (vùng tròn quanh robot gây sát thương liên tục). Trong lúc chơi có thể mở thêm các vũ khí khác, mang **tối đa 4 vũ khí** cùng lúc, mỗi vũ khí nâng được tới **cấp 5**. Tất cả vũ khí **tự động kích hoạt**. Bảng dưới ghi cấp 1 và cấp 5.

### 1.6.1. Lực hút *(có sẵn từ đầu)*

Vùng tròn quanh robot gây sát thương liên tục. Vừa là vũ khí vừa là công cụ hút vũng nước.

| Cấp | Sát thương/giây | Bán kính |
|---|---|---|
| 1 | 4 | 1,5 |
| 5 | 10 | 2,8 |

### 1.6.2. Chổi quay

Các cây chổi xoay quanh robot, chạm vào rác thì gây sát thương. Đây là kiểu vũ khí quỹ đạo kinh điển của thể loại.

| Cấp | Số chổi | Sát thương mỗi lần chạm | Tốc độ xoay | Bán kính |
|---|---|---|---|---|
| 1 | 1 | 8 | 120°/giây | 1,8 |
| 5 | 4 | 15 | 200°/giây | 2,2 |

*Cách code: tạo một object rỗng làm "trục", xoay nó bằng `transform.Rotate`, gắn các chổi làm con của nó ở khoảng cách bán kính. Không cần tính toán gì phức tạp.*

### 1.6.3. Vòi xịt nước lau

Bắn một viên nước về phía kẻ địch gần nhất.

| Cấp | Sát thương | Nhịp bắn | Số viên | Tầm |
|---|---|---|---|---|
| 1 | 12 | 1,2 giây | 1 | 6 |
| 5 | 18 | 0,7 giây | 3 | 8 |

### 1.6.4. Vệt lau sàn

Để lại vệt nước phía sau robot, rác đi qua bị trượt và mất máu.

| Cấp | Sát thương/giây | Thời gian tồn tại | Độ rộng |
|---|---|---|---|
| 1 | 5 | 3 giây | 0,8 |
| 5 | 12 | 5 giây | 1,4 |

### 1.6.5. Tia quét LIDAR

Mỗi vài giây quét một tia laser xuyên qua mọi thứ, xoay 360 độ.

| Cấp | Sát thương | Chu kỳ | Số tia |
|---|---|---|---|
| 1 | 25 | 5 giây | 1 |
| 5 | 45 | 3 giây | 3 |

### 1.6.6. Còi báo động

Đẩy lùi toàn bộ rác xung quanh và làm choáng. Vũ khí thoát vây, không gây nhiều sát thương.

| Cấp | Sát thương | Đẩy lùi | Choáng | Chu kỳ |
|---|---|---|---|---|
| 1 | 5 | 4 đơn vị | 1 giây | 12 giây |
| 5 | 15 | 7 đơn vị | 2 giây | 8 giây |

### 1.6.7. Nâng cấp bị động

Mỗi loại tối đa 5 cấp. Xuất hiện trộn lẫn với vũ khí trong panel chọn nâng cấp.

| Tên | Hiệu ứng mỗi cấp | Ghi chú |
|---|---|---|
| **Pin dung lượng lớn** | +20 pin tối đa (hồi luôn phần vừa tăng) | |
| **Tấm pin mặt trời** | +0,15 pin hồi/giây | Cấp 2 là đã bù được tốc độ tụt nền |
| **Động cơ mạnh** | +8% tốc độ di chuyển | |
| **Nam châm** | +35% bán kính nhặt bụi | Nâng cấp "thoải mái" mà ai cũng thích |
| **Lọc HEPA** | +12% sát thương mọi vũ khí | |
| **Chổi to** | +15% vùng ảnh hưởng mọi vũ khí | |
| **Chip xử lý** | -8% thời gian hồi chiêu | |
| **Vỏ cao su** | -8% sát thương nhận vào | |
| **Túi bụi nhỏ** | Lên cấp cần ít bụi hơn 15%, nhưng nhận thêm 10% sát thương | Nâng cấp đánh đổi — loại này làm game thú vị hẳn lên |

### 1.6.8. Vật phẩm rơi trên sàn

Trên sàn đôi khi rơi ra vật phẩm hỗ trợ:

| Vật phẩm | Hiệu ứng | Tần suất |
|---|---|---|
| **Ụ sạc** | Hồi 15 pin/giây khi đứng lên | Mỗi 45 giây |
| **Giẻ lau thần thánh** | Xoá sạch mọi rác trên màn hình (không rơi bụi) | 2% khi rác chết |
| **Chổi vàng** | Gấp đôi sát thương trong 10 giây | 1,5% khi rác chết |
| **Túi bụi bỏ quên** | Nhận ngay 15 bụi | 3% khi rác chết |

## **1.7. Diễn biến các màn chơi**

Game được thiết kế gồm **3 màn chơi**, tương ứng với 3 khu vực khác nhau trong ngôi nhà: **Phòng khách, Phòng ngủ và Nhà bếp**. Mỗi màn có bố cục bản đồ, loại rác và mức độ khó khác nhau. Người chơi phải hoàn thành màn trước để mở khóa màn tiếp theo. Mỗi màn kéo dài khoảng 5 phút và kết thúc bằng một con boss.

**Nguyên tắc spawn (áp dụng cho cả 3 màn):** rác sinh ra ở ngoài rìa màn hình, cách camera khoảng 2 đơn vị, ở vị trí ngẫu nhiên trên vòng tròn quanh người chơi. Rác đi quá xa (hơn 30 đơn vị) thì dịch chuyển về phía trước mặt người chơi thay vì để nó lang thang — mẹo này vừa tiết kiệm hiệu năng vừa giữ nhịp game. Các con số nhịp spawn và số rác tối đa dưới đây là điểm xuất phát, nên để trong ScriptableObject `WaveData` để chỉnh trực tiếp trong Inspector.

### 1.7.1. Màn 1 – Phòng khách

Đây là màn chơi đầu tiên, giúp người chơi làm quen với cách điều khiển robot, cơ chế tự động tấn công, thu thập bụi và sử dụng ụ sạc. Bản đồ rộng rãi, ít vật cản.

| Mốc thời gian | Nhịp spawn | Số rác tối đa | Loại rác có mặt | Ghi chú |
|---|---|---|---|---|
| 0:00 – 1:30 | 1,2 giây | 25 | Bụi bông (số lượng ít) | Học cách chơi |
| 1:30 – 3:00 | 1,0 giây | 40 | + Cục LEGO, một lượng nhỏ Vụn bánh mì | |
| 3:00 – 4:30 | 0,8 giây | 55 | + Cục tóc rối, mật độ rác tăng dần | |
| **4:30** | tạm ngừng | — | — | **BOSS: CON MÈO** xuất hiện |
| **5:00** | — | — | — | Hạ Con mèo → hoàn thành màn, **mở khóa Phòng ngủ** |

### 1.7.2. Màn 2 – Phòng ngủ

Phòng ngủ có không gian hẹp hơn và nhiều chướng ngại vật, khiến việc di chuyển và né tránh kẻ địch khó khăn hơn.

| Mốc thời gian | Nhịp spawn | Số rác tối đa | Loại rác có mặt | Ghi chú |
|---|---|---|---|---|
| 0:00 – 1:30 | 1,0 giây | 45 | Bụi bông + Cục tóc rối (số lượng vừa phải) | |
| 1:30 – 3:00 | 0,8 giây | 60 | Tất lẻ xuất hiện nhiều hơn, kết hợp Cục tóc rối | |
| 3:00 – 4:00 | 0,7 giây | 70 | + Cục LEGO, Dây sạc (gây cản trở di chuyển) | |
| 4:00 – 4:30 | 0,6 giây | 85 | Mật độ Tất lẻ, Cục tóc rối và Dây sạc tăng mạnh | |
| **4:30** | tạm ngừng | — | — | **BOSS: CON MÈO** (máu và sức mạnh cao hơn) |
| **5:00** | — | — | — | Hạ boss → hoàn thành màn, **mở khóa Nhà bếp** |

### 1.7.3. Màn 3 – Nhà bếp

Đây là màn chơi cuối với độ khó cao nhất. Người chơi phải đối mặt với mật độ rác lớn, nhiều chướng ngại vật và các loại rác gây mất pin.

| Mốc thời gian | Nhịp spawn | Số rác tối đa | Loại rác có mặt | Ghi chú |
|---|---|---|---|---|
| 0:00 – 1:30 | 0,7 giây | 85 | Các loại rác cơ bản (số lượng lớn) | |
| 1:30 – 3:00 | 0,6 giây | 110 | Vụn bánh mì tăng mạnh, thêm Cục tóc rối | |
| 3:00 – 4:00 | 0,5 giây | 130 | + Vũng nước, Dây sạc | |
| 4:00 – 4:30 | 0,4 giây | 160 | Mật độ Vụn bánh mì, Dây sạc và Vũng nước tăng mạnh | Cao trào |
| **4:30** | ngừng spawn | — | — | **BOSS: MÁY HÚT BỤI CŨ** với các đòn tấn công đặc biệt |
| **5:00** | — | — | — | Hạ boss Máy hút bụi cũ → **hoàn thành toàn bộ game (THẮNG)** |

## **1.8. Điều kiện thắng, thua và tính điểm**

### 1.8.1. Thắng

Hạ được boss **Máy hút bụi cũ** ở cuối Màn 3. Màn hình chuyển sang **6 giờ sáng**, nắng chiếu vào nhà, sàn nhà sạch bong, hiện dòng chữ **"NHÀ SẠCH BONG"**.

### 1.8.2. Thua

Pin về 0. Robot tắt đèn, quay chậm rồi dừng hẳn. Hiện dòng chữ: **"Chủ về nhà và thấy một đống bừa bộn."**

### 1.8.3. Tính điểm và bảng thống kê

**Điểm** = số bụi × 10 + số rác tiêu diệt × 5 + số giây sống sót × 2, thắng được cộng thêm **5000** điểm.

Kết thúc ván (hiện cả khi thắng lẫn khi thua), game hiển thị bảng thống kê gồm:

- Thời gian sống sót
- Tổng lượng bụi đã hút (hiển thị theo đơn vị gam cho vui: `bụi × 12g`)
- Số rác đã tiêu diệt, chia theo loại
- Cấp đạt được
- Danh sách vũ khí và nâng cấp đã chọn
- **Điểm** theo công thức trên
- Bảng **5 điểm cao nhất**, lưu bằng PlayerPrefs hoặc file JSON

## **1.9. Giao diện và âm thanh**

### 1.9.1. Trong lúc chơi (HUD)

```
┌──────────────────────────────────────────────┐
│ ▓▓▓▓▓▓▓░░░  PIN 68%     Màn 1 · 02:47  Cấp 7 │  ← trên cùng
│ ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░ (thanh bụi)           │
│                                              │
│                                              │
│                  [khu vực chơi]              │
│                                              │
│                                              │
│ [Lực hút 3][Chổi 2][Vòi 1]     Bụi: 1.284g  │  ← dưới cùng
└──────────────────────────────────────────────┘
```

- **Thanh pin**: đổi màu theo mức, nhấp nháy khi dưới 30%
- **Thanh bụi**: chạy từ trái sang phải, đầy thì lên cấp
- **Đồng hồ**: đếm thời gian của màn hiện tại (và báo màn đang chơi)
- **Icon vũ khí**: kèm số cấp, có vòng tròn hồi chiêu xoay quanh
- **Mũi tên chỉ ụ sạc**: hiện ở rìa màn hình khi ụ sạc đang hoạt động ngoài tầm nhìn

### 1.9.2. Các màn hình cần làm

| Màn hình | Nội dung |
|---|---|
| Menu chính | Tên game, nút Chơi, Hướng dẫn, Điểm cao, Thoát |
| Hướng dẫn | 1 trang tĩnh giải thích điều khiển và luật |
| Panel nâng cấp | 3 thẻ, mỗi thẻ có icon, tên, mô tả, cấp hiện tại |
| Chọn màn / chuyển màn | Hiện màn vừa mở khóa, chuyển tiếp sang màn sau |
| Tạm dừng | Tiếp tục, Về menu, thanh âm lượng |
| Thắng | Thống kê + nút Chơi lại |
| Thua | Thống kê + nút Chơi lại |
| Điểm cao | Top 5 |

Dùng **TextMeshPro** cho toàn bộ chữ. Nhớ tạo Font Asset có đủ ký tự tiếng Việt, nếu không sẽ bị ô vuông ở các chữ có dấu.

### 1.9.3. Âm thanh

Game lấy bối cảnh ban đêm nên màu chủ đạo là tông tối, mắt của rác phát sáng nhẹ. Chỉ cần 8-10 file âm thanh. Lấy ở **freesound.org** (nhớ kiểm tra giấy phép) hoặc tự tạo bằng **jsfxr** / **Chiptone**.

| Âm thanh | Ghi chú |
|---|---|
| Tiếng máy hút vo vo | Loop nền liên tục, **cao độ tăng khi nâng cấp Lực hút** — chi tiết này rất đáng làm |
| Hút được bụi | Ngắn, nhẹ, phát rất nhiều lần nên phải êm tai |
| Rác chết | 2-3 biến thể để đỡ nhàm |
| Trúng đòn | Tiếng "bzzt" điện |
| Bíp pin yếu | Nghe là biết nguy |
| Lên cấp | Vui vẻ |
| Mèo kêu | Khi boss xuất hiện |
| Sạc pin | Tiếng rè khi đang sạc |
| Nhạc nền | 1 track yên tĩnh ban đêm + 1 track căng hơn khi boss |

**Mẹo:** ngẫu nhiên hoá cao độ (`audioSource.pitch = Random.Range(0.9f, 1.1f)`) cho các âm phát nhiều lần. Một dòng code, khác biệt rất lớn.

---

# **Chương 2. KẾT QUẢ XÂY DỰNG GAME**

## **2.1. Mô tả các scene**

Cây thư mục dự án:

```
Assets/
├── Art/
│   ├── Sprites/
│   ├── Tiles/
│   └── UI/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Prefabs/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Pickups/
│   └── VFX/
├── ScriptableObjects/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Upgrades/
│   └── Waves/
├── Scenes/
│   ├── MainMenu.unity
│   └── Game.unity
└── Scripts/
    ├── Core/
    ├── Player/
    ├── Enemies/
    ├── Weapons/
    ├── Upgrades/
    ├── Pickups/
    ├── UI/
    └── Utils/
```

**Các scene chính:**

- **MainMenu.unity** — menu chính, hướng dẫn, bảng điểm cao.
- **Game.unity** — scene chơi chính. Ba màn (Phòng khách, Phòng ngủ, Nhà bếp) dùng chung scene này; mỗi màn được nạp từ một `WaveData` + bố cục bản đồ (Tilemap, vật cản) riêng, chuyển màn bằng cách đổi dữ liệu màn thay vì tải nhiều scene. *(Có thể tách thành 3 scene riêng nếu muốn mỗi phòng là một scene — tùy cách cài đặt của nhóm.)*

## **2.2. Mô tả các game object và các component liên quan**

**Thiết lập Unity:**

- **Phiên bản:** Unity 2022.3 LTS (ổn định, nhiều tài liệu) hoặc Unity 6. Đừng dùng bản beta.
- **Template:** 2D (URP) nếu muốn dùng ánh sáng 2D, 2D thường nếu muốn đơn giản.
- **Package nên cài:** Cinemachine (camera bám mượt), TextMeshPro (chữ tiếng Việt), DOTween phiên bản miễn phí (hiệu ứng UI).
- **Input:** dùng Input Manager cũ (`Input.GetAxisRaw`) cho nhanh.
- **Layer:** `Player`, `Enemy`, `PlayerAttack`, `Pickup`, `Obstacle`.
- **Ma trận va chạm (Project Settings > Physics 2D):** tắt va chạm `Pickup` với `Enemy`, tắt `PlayerAttack` với `Obstacle`. Giữ `Enemy` với `Enemy` bật để rác không chồng lên nhau thành một cục.
- **Rigidbody2D của rác:** Dynamic, Gravity Scale = 0, tích Freeze Rotation Z, Collision Detection = Continuous.
- **Pixels Per Unit:** thống nhất một con số cho toàn bộ sprite (ví dụ 16 hoặc 32), nếu không mọi thứ sẽ lệch kích thước lung tung.

**Các game object chính trong scene Game:** Player (Rigidbody2D, Collider2D, các script điều khiển và vũ khí), Camera + Cinemachine Virtual Camera bám theo Player, EnemySpawner, các Object Pool (rác/đạn/bụi/số sát thương), Canvas HUD và các panel UI, Tilemap sàn nhà + vật cản.

## **2.3. Mô tả các lớp C# script**

**Core/**
| Class | Nhiệm vụ |
|---|---|
| `GameManager` | Trạng thái game (đang chơi / tạm dừng / thắng / thua), màn hiện tại, đồng hồ, điểm. Singleton |
| `AudioManager` | Phát nhạc và hiệu ứng âm thanh, quản lý âm lượng |
| `SaveSystem` | Lưu/đọc bảng điểm cao bằng JSON |

**Player/**
| Class | Nhiệm vụ |
|---|---|
| `PlayerController` | Đọc input, di chuyển bằng Rigidbody2D |
| `PlayerStats` | Pin, cấp, bụi, các hệ số nhân. Phát sự kiện khi thay đổi |
| `PlayerMagnet` | Hút bụi trong bán kính |
| `BatteryLED` | Đổi màu đèn theo mức pin |

**Enemies/**
| Class | Nhiệm vụ |
|---|---|
| `EnemyBase` *(abstract)* | Máu, `TakeDamage()`, `Die()`, rơi bụi. Có `abstract void Move()` |
| `ChaserEnemy` | Bụi bông, vụn bánh mì — bò thẳng về phía người chơi |
| `SplitterEnemy` | Cục tóc rối — chết thì tách đôi |
| `DasherEnemy` | Tất lẻ — ngắm rồi lao |
| `HazardEnemy` | Cục LEGO, vũng nước — đứng yên gây sát thương |
| `SnakeEnemy` | Dây sạc — bò kiểu rắn, gây hiệu ứng trói |
| `BossCat` | Boss cuối Màn 1 và Màn 2, máy trạng thái 3 trạng thái |
| `BossVacuum` | Boss cuối Màn 3, 3 pattern xoay vòng |
| `EnemySpawner` | Đọc `WaveData`, spawn rác ngoài rìa màn hình |
| `EnemyData` *(ScriptableObject)* | Máu, tốc độ, sát thương, bụi rơi, prefab |

**Weapons/**
| Class | Nhiệm vụ |
|---|---|
| `WeaponBase` *(abstract)* | Cấp, thời gian hồi chiêu, `abstract void Fire()` |
| `SuctionWeapon` | Lực hút — vùng tròn sát thương liên tục |
| `OrbitWeapon` | Chổi quay |
| `ProjectileWeapon` | Vòi xịt |
| `TrailWeapon` | Vệt lau sàn |
| `LaserWeapon` | Tia LIDAR |
| `PushWeapon` | Còi báo động |
| `WeaponData` *(ScriptableObject)* | Chỉ số từng cấp, icon, mô tả |
| `WeaponManager` | Danh sách vũ khí đang mang, thêm/nâng cấp |

**Upgrades/**
| Class | Nhiệm vụ |
|---|---|
| `UpgradeData` *(ScriptableObject)* | Loại, giá trị mỗi cấp, icon, mô tả |
| `UpgradeManager` | Bốc 3 lựa chọn ngẫu nhiên, áp dụng khi chọn |

**Utils/**
| Class | Nhiệm vụ |
|---|---|
| `ObjectPool<T>` | Pool chung cho rác, đạn, bụi, số sát thương |
| `IDamageable` *(interface)* | `void TakeDamage(float amount)` |
| `Timer` | Đếm giờ tiện dụng |

### 2.3.1. Ba đoạn code then chốt

**Object pooling** — thứ bắt buộc phải có khi màn hình có 160 con rác. Đây cũng là chỗ ghi điểm kỹ thuật cao nhất trong báo cáo.

```csharp
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 50;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(CreateNew());
    }

    private GameObject CreateNew()
    {
        var obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get(Vector3 position)
    {
        var obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

**Class kẻ địch trừu tượng** — đây chính là thứ bảng chấm điểm hay yêu cầu về kế thừa. Khi code review, mở đúng file này ra giải thích.

```csharp
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyData data;
    protected float currentHealth;
    protected Transform player;

    protected virtual void OnEnable()
    {
        currentHealth = data.maxHealth;
        player = GameManager.Instance.PlayerTransform;
    }

    void Update() => Move();

    protected abstract void Move();   // mỗi loại rác tự định nghĩa

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        StartCoroutine(FlashWhite());
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        DustSpawner.Instance.Spawn(transform.position, data.dustValue);
        GameManager.Instance.AddKill(data.enemyName);
        EnemyPool.Instance.Return(gameObject);
    }
}
```

**Nâng cấp bằng ScriptableObject** — cho phép thêm nâng cấp mới mà không đụng tới code.

```csharp
[CreateAssetMenu(menuName = "VacuumSurvivor/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeType type;        // enum: MaxBattery, MoveSpeed, Damage...
    public float[] valuePerLevel;   // giá trị ở từng cấp
    public int maxLevel = 5;
}
```

## **2.4. Âm thanh**

Hệ thống âm thanh do `AudioManager` quản lý (nhạc nền + hiệu ứng, điều chỉnh âm lượng). Danh sách file và ghi chú xem ở mục **1.9.3**. Điểm nhấn kỹ thuật: cao độ tiếng máy hút tăng dần khi nâng cấp Lực hút, và ngẫu nhiên hoá cao độ cho các hiệu ứng phát nhiều lần.

## **2.5. Lưu thông tin**

- Bảng **5 điểm cao nhất** được lưu bằng **JSON** (hoặc PlayerPrefs) qua lớp `SaveSystem`, đọc lại khi mở màn hình Điểm cao.
- Mỗi bản ghi gồm: điểm, thời gian sống sót, cấp đạt được, (tùy chọn) ngày chơi.

## **2.6. Các nội dung khác (AI, công nghệ, tối ưu…)**

- **AI kẻ địch:** 7 hành vi rác khác nhau (bò thẳng, bầy đàn, tách đôi, ngắm-lao, bãi mìn tĩnh, bò kiểu rắn gây trói, vật cản gây mất pin) + 2 boss dùng **máy trạng thái** (Con mèo: Đi → Dừng → Tát; Máy hút bụi cũ: Hút → Thổi → Lao).
- **Kế thừa và đa hình:** `EnemyBase` với 6 lớp con, `WeaponBase` với 6 lớp con.
- **ScriptableObject** điều khiển dữ liệu (EnemyData, WeaponData, UpgradeData, WaveData) → cân bằng số liệu ngay trong Inspector, không cần build lại.
- **Tối ưu hiệu năng:** Object pooling cho rác/đạn/bụi; dịch chuyển rác đi quá xa về gần người chơi; tắt bớt va chạm không cần thiết trong ma trận Physics 2D.
- **Cấu trúc dữ liệu:** Queue cho object pool, List cho danh sách vũ khí đang mang, Dictionary cho thống kê giết rác theo loại.

---

# **KẾT LUẬN**

## 1. Kết quả đạt được

**\- Nội dung đã làm được:**

*(Nhóm điền phần thực tế đã hoàn thành. Tham chiếu danh sách bản tối thiểu ở Phụ lục D — thường gồm: di chuyển, hệ thống pin, các loại rác cơ bản, hút bụi và lên cấp, vài vũ khí, boss, menu, điểm cao, âm thanh cơ bản.)*

**\- Nội dung chưa làm được:**

*(Nhóm điền phần còn thiếu hoặc đã cắt gọt — xem Phụ lục D để biết thứ tự ưu tiên cắt.)*

## 2. Hướng phát triển cho game

- Thêm khu vực / màn chơi mới trong ngôi nhà (nhà tắm, nhà kho, sân vườn…).
- Thêm loại rác và vũ khí mới (mỗi loại chỉ là thêm prefab + ScriptableObject).
- Chế độ chơi bất tận (endless) sau khi hoàn thành 3 màn.
- Bảng xếp hạng trực tuyến thay cho bảng điểm cao cục bộ.
- Thêm boss và nâng cấp đánh đổi để tăng chiều sâu chiến thuật.

---

# PHỤ LỤC

Các phụ lục dưới đây là tài liệu sản xuất và cân bằng chi tiết, hỗ trợ quá trình làm game và viết báo cáo.

## Phụ lục A — Hướng nghệ thuật và danh sách asset

### A.1. Nguyên tắc

**Mọi thứ đều là hình tròn hoặc hình chữ nhật bo góc, có gắn mắt.** Không vẽ chi tiết. Không vẽ tay chân. Không vẽ mặt ngoài hai con mắt.

Nhìn từ trên xuống có một lợi thế lớn: không cần vẽ mặt trước, mặt bên, mặt sau của bất cứ thứ gì. Robot hút bụi nhìn từ trên xuống đúng nghĩa đen chỉ là một cái đĩa tròn.

### A.2. Bảng màu (ban đêm)

| Vai trò | Mã màu | Dùng cho |
|---|---|---|
| Nền tối | `#1B1B2F` | Sàn nhà trong bóng tối |
| Nền nhạt | `#2E2E48` | Sàn nhà vùng có ánh trăng |
| Ánh trăng | `#6B7A99` | Vệt sáng qua cửa sổ |
| Robot | `#E8E8F0` | Thân robot |
| Đèn xanh | `#4ADE80` | Pin cao |
| Đèn vàng | `#FACC15` | Pin trung bình |
| Đèn đỏ | `#EF4444` | Pin thấp |
| Rác | `#8B8B9E` | Bụi bông, tóc rối |
| Điểm nhấn | `#F472B6` | Bụi, vật phẩm, hiệu ứng |

Ban đêm là lựa chọn cố ý: **tối thì không cần vẽ chi tiết**, mà lại nhìn có gu. Mắt của rác phát sáng nhẹ trong bóng tối là đủ tạo không khí.

### A.3. Danh sách asset cần làm

**Nhân vật và rác**
- [ ] Robot (hình tròn + 2 mắt + 1 chấm LED) — 1 sprite, xoay bằng code
- [ ] Bụi bông (tròn xù + 2 mắt) — 1 sprite
- [ ] Vụn bánh mì (tròn nhỏ vàng + 2 mắt) — 1 sprite
- [ ] Cục tóc rối (tròn rối đen) — 2 sprite (to, nhỏ)
- [ ] Tất lẻ (chữ nhật bo góc) — 1 sprite
- [ ] Cục LEGO (vuông + 4 chấm) — 1 sprite
- [ ] Dây sạc (đoạn dây) — 1 sprite lặp
- [ ] Vũng nước (blob xanh mờ) — 1 sprite
- [ ] Con mèo (nhìn từ trên xuống: 1 elip + 2 tai + 1 đuôi) — 1 sprite
- [ ] Máy hút bụi cũ (chữ nhật + vòi + 1 mắt) — 1 sprite

**Môi trường (cho 3 phòng)**
- [ ] Tile sàn gỗ, tile gạch hoa, tile thảm — 3 tile
- [ ] Nội thất làm vật cản: sofa, chân bàn, thùng rác, chậu cây — 4 sprite chữ nhật bo góc
- [ ] Vệt sáng cửa sổ — 1 sprite mờ

**Vũ khí và hiệu ứng**
- [ ] Vòng lực hút (vòng tròn mờ) — 1 sprite
- [ ] Chổi — 1 sprite
- [ ] Viên nước — 1 sprite
- [ ] Vệt lau — 1 sprite
- [ ] Tia laser — 1 sprite
- [ ] Hạt bụi (exp) — 1 sprite nhỏ
- [ ] Ụ sạc — 1 sprite

**UI**
- [ ] Icon cho 6 vũ khí và 9 nâng cấp — 15 icon đơn giản
- [ ] Khung thẻ nâng cấp, thanh pin, thanh bụi

### A.4. Nếu không muốn tự vẽ

- **kenney.nl** — asset CC0, dùng thoải mái, có sẵn nhiều bộ hình khối đơn giản hợp với phong cách này
- **itch.io mục Game Assets, lọc Free** — rất nhiều bộ 2D top-down miễn phí
- **Aseprite** (có phí) hoặc **LibreSprite** / **Piskel** (miễn phí) để vẽ pixel art
- Nhớ ghi nguồn asset trong báo cáo, phần tài liệu tham khảo.

## Phụ lục B — Danh sách việc "game feel" nên làm

Mỗi mục tốn 10-30 phút nhưng cộng lại thì game khác hẳn.

- [ ] Rung camera nhẹ khi trúng đòn và khi boss xuất hiện (dùng Cinemachine Impulse)
- [ ] Số sát thương bay lên rồi mờ dần
- [ ] Rác nhấp nháy trắng 0,1 giây khi trúng đòn
- [ ] Rác bị đẩy lùi nhẹ khi trúng đòn
- [ ] Bụi bay theo đường cong về phía robot, tốc độ tăng dần
- [ ] Robot nghiêng nhẹ theo hướng rẽ
- [ ] Hiệu ứng nổ bụi nhỏ khi rác chết (particle)
- [ ] Viền màn hình đỏ mờ khi pin thấp
- [ ] Thời gian chậm lại 0,15 giây khi boss chết
- [ ] Thẻ nâng cấp nảy lên khi hiện ra (dùng DOTween, 1 dòng)

## Phụ lục C — Lộ trình phát triển 5 tuần

Nguyên tắc số một: **tuần 1 phải có thứ chơi được**, dù xấu. Dùng ô vuông trắng làm robot, ô đỏ làm rác. Đừng đụng vào đồ họa trước khi gameplay chạy.

### Tuần 1 — Lát cắt dọc (vertical slice)
Mục tiêu cuối tuần: **có thể chơi và thua được.**

- [ ] Tạo project, cấu trúc thư mục, cài Cinemachine và TextMeshPro
- [ ] `PlayerController`: di chuyển WASD bằng Rigidbody2D
- [ ] Camera Cinemachine bám theo robot
- [ ] `PlayerStats`: pin, tụt dần theo thời gian
- [ ] `EnemyBase` + `ChaserEnemy`: rác bò về phía người chơi
- [ ] `EnemySpawner`: spawn ngoài rìa màn hình theo nhịp cố định
- [ ] Chạm rác thì mất pin, có thời gian bất tử ngắn
- [ ] Pin về 0 → màn hình Game Over, nút chơi lại
- [ ] Bản đồ tạm: một hình chữ nhật có tường bao

### Tuần 2 — Vòng lặp cốt lõi
Mục tiêu cuối tuần: **game đã vui.** Đây là tuần quan trọng nhất.

- [ ] `SuctionWeapon`: vùng tròn tự động gây sát thương
- [ ] Rác chết rơi bụi, bụi bị hút về phía robot
- [ ] Thanh bụi, công thức lên cấp
- [ ] Panel chọn nâng cấp: dừng game, 3 thẻ, bấm chọn
- [ ] `UpgradeData` ScriptableObject + 4 nâng cấp bị động đầu tiên
- [ ] `OrbitWeapon` (chổi quay) và `ProjectileWeapon` (vòi xịt)
- [ ] **Ụ sạc** — cơ chế đánh đổi quan trọng nhất
- [ ] Đồng hồ đếm giờ, điều kiện hoàn thành màn khi hạ boss

**Chốt chặn:** cuối tuần 2 cho 2-3 người bạn chơi thử. Nếu họ không muốn chơi lại lần 2, phải sửa gì đó trước khi đi tiếp. Đừng thêm nội dung lên trên một vòng lặp không vui.

### Tuần 3 — Nội dung
- [ ] Thêm `SplitterEnemy`, `DasherEnemy`, `HazardEnemy`, `SnakeEnemy`
- [ ] `WaveData` ScriptableObject + 3 màn (Phòng khách, Phòng ngủ, Nhà bếp) với đường cong độ khó riêng
- [ ] `BossCat` ở cuối Màn 1 và Màn 2
- [ ] 3 vũ khí còn lại
- [ ] 5 nâng cấp bị động còn lại
- [ ] Các vật phẩm rơi (giẻ lau, chổi vàng, túi bụi)
- [ ] `ObjectPool` cho rác, đạn, bụi

### Tuần 4 — Phần ăn điểm
Tuần này ít logic phức tạp nhưng nâng điểm nhiều nhất.

- [ ] `BossVacuum` (Màn 3) và màn hình chiến thắng
- [ ] Menu chính, hướng dẫn, chuyển màn, tạm dừng, điểm cao
- [ ] Lưu/đọc bảng điểm cao bằng JSON
- [ ] Toàn bộ âm thanh và 2 track nhạc nền
- [ ] Làm đồ họa thật: thay hết ô vuông bằng sprite
- [ ] Tilemap sàn nhà và nội thất làm vật cản cho 3 phòng
- [ ] Danh sách game feel ở Phụ lục B

### Tuần 5 — Đánh bóng và báo cáo
- [ ] Cho 4-5 người chơi thử, ghi lại chỗ họ vấp
- [ ] Cân bằng lại số liệu theo phản hồi
- [ ] Sửa bug, kiểm tra hiệu năng khi đông rác
- [ ] Build Windows và WebGL
- [ ] Quay video demo 2-3 phút
- [ ] Viết báo cáo
- [ ] Dọn code, thêm chú thích ở các class chính

## Phụ lục D — Bản tối thiểu và phần cắt được

Nếu hết thời gian, cắt theo thứ tự từ dưới lên.

**Bắt buộc phải có (vẫn nộp được, vẫn đủ điểm):**
- Di chuyển, pin, 3 loại rác, hút bụi, lên cấp, 3 vũ khí, boss mèo, menu, điểm cao, âm thanh cơ bản

**Nên có:**
- Ụ sạc, 4 loại rác còn lại, boss thứ hai, 3 vũ khí còn lại, object pooling, đủ cả 3 màn

**Cắt được nếu gấp:**
- Vũng nước và dây sạc (2 loại rác phức tạp nhất)
- Boss máy hút bụi cũ (thay bằng "hoàn thành Màn 3 là thắng")
- Tia LIDAR và còi báo động
- Nâng cấp đánh đổi "Túi bụi nhỏ"
- Nhạc nền riêng cho boss
- Rút còn 1-2 màn nếu không kịp làm đủ 3 phòng

## Phụ lục E — Rủi ro và cách xử lý

| Rủi ro | Dấu hiệu | Cách xử lý |
|---|---|---|
| **Cân bằng sai** | Quá dễ hoặc chết ở phút thứ 2 | Đưa hết số liệu vào ScriptableObject để chỉnh trong Inspector mà không cần build lại. Đây là lý do chính để dùng ScriptableObject |
| **Giật lag khi đông rác** | FPS tụt dưới 30 ở màn cuối | Object pooling từ tuần 3, đừng để tới tuần 5. Tắt bớt va chạm không cần thiết trong ma trận Physics 2D |
| **Tham lam thêm nội dung** | Tuần 4 vẫn đang thêm vũ khí mới | Bám danh sách cắt gọt ở Phụ lục D. 3 vũ khí làm tử tế hơn 6 vũ khí làm dở |
| **Sa đà vào đồ họa** | Tuần 1 đang vẽ sprite | Cấm bản thân đụng vào đồ họa trước tuần 4 |
| **Chữ tiếng Việt bị ô vuông** | Thấy ô vuông ở chữ có dấu | Tạo TextMeshPro Font Asset với character set gồm đủ ký tự tiếng Việt |
| **Mất code** | — | Tạo repo Git riêng tư ngay từ ngày đầu, commit mỗi ngày. Nhớ thêm `.gitignore` cho Unity |

## Phụ lục F — Ánh xạ sang tiêu chí chấm điểm

| Tiêu chí thường gặp | Game này đáp ứng ở đâu |
|---|---|
| Hệ thống tính điểm | Mục 1.8.3, có cả bảng điểm cao |
| Mạng / máu người chơi | Hệ thống pin, mục 1.4.4 |
| Chơi lại được nhiều ván | Nâng cấp ngẫu nhiên → mỗi ván khác nhau |
| Nhiều đối tượng tương tác | Hàng trăm rác, đạn, bụi, vật phẩm cùng lúc |
| Phát hiện va chạm | Collider2D, trigger cho bụi và vật phẩm |
| Vật lý | Rigidbody2D, lực đẩy lùi, lực hút của boss |
| AI | 7 hành vi rác khác nhau + 2 boss có máy trạng thái |
| Âm thanh | Mục 1.9.3 và 2.4 |
| Đồ họa | Mục 1.9 và Phụ lục A |
| Hiển thị trạng thái | HUD ở mục 1.9.1 |
| Điều khiển chuột và bàn phím | WASD khi chơi, chuột ở menu và panel nâng cấp |
| Lưu / đọc dữ liệu | Bảng điểm cao bằng JSON, mục 2.5 |
| Kế thừa và đa hình | `EnemyBase` với 6 class con, `WeaponBase` với 6 class con |
| Cấu trúc dữ liệu | Queue cho object pool, List cho vũ khí đang mang, Dictionary cho thống kê giết rác |
| Tối ưu hiệu năng | Object pooling, dịch chuyển rác đi quá xa |
| Tài liệu | Chính là tài liệu này |

## Phụ lục G — Bảng số liệu để chỉnh

Gom toàn bộ con số cần cân bằng vào một chỗ. Khi thử nghiệm thấy sai thì sửa ở đây.

| Tham số | Giá trị đề xuất | Chỉnh khi |
|---|---|---|
| Pin tối đa | 100 | Game quá khó → tăng lên 120 |
| Tốc độ tụt pin | 0,25/giây | Ít áp lực → tăng lên 0,35 |
| Tốc độ robot | 5,0 | Cảm giác ì → tăng lên 5,5 |
| Chu kỳ ụ sạc | 45 giây | Hay chết vì hết pin → giảm xuống 35 |
| Thời gian ụ sạc tồn tại | 20 giây | |
| Tốc độ sạc | 15/giây | |
| Hệ số lên cấp | 5 + n×4 | Lên cấp quá chậm → đổi thành 4 + n×3 |
| Thời gian bất tử | 0,4 giây | Hay bị khóa trong đám đông → tăng lên 0,6 |
| Thời lượng mỗi màn | ~300 giây (5 phút) | Demo quá dài → rút ngắn |
| Số màn | 3 | Không kịp làm → rút còn 1-2 |
| Số rác tối đa | 160 | Máy yếu → giảm xuống 100 |

## Phụ lục H — Dàn ý báo cáo và kịch bản demo

### H.1. Dàn ý báo cáo

1. **Giới thiệu** — ý tưởng, thể loại, lý do chọn đề tài
2. **Phân tích thể loại** — survivors-like là gì, các game tham khảo, bảng ánh xạ ở mục 1.4.3
3. **Thiết kế game** — vòng lặp cốt lõi, các hệ thống, 3 màn chơi, bảng số liệu cân bằng
4. **Thiết kế kỹ thuật** — sơ đồ class, giải thích kế thừa và đa hình, ScriptableObject, object pooling
5. **Cài đặt** — môi trường, package, các vấn đề gặp phải và cách giải quyết (phần này nên viết thật, kể cả bug ngớ ngẩn, giảng viên đánh giá cao)
6. **Kiểm thử** — cho ai chơi, họ phản hồi gì, nhóm sửa gì
7. **Kết quả và hướng phát triển**
8. **Tài liệu tham khảo** — ghi rõ nguồn mọi asset và mọi đoạn code tham khảo từ tutorial

### H.2. Kịch bản demo 2 phút

1. 0:00-0:15 — Mở menu, giải thích một câu: "bạn là robot hút bụi, sống sót và dọn sạch nhà tới 6h sáng"
2. 0:15-0:45 — Chơi 30 giây đầu Màn 1, chỉ vào thanh pin đang tụt
3. 0:45-1:10 — Lên cấp, chọn nâng cấp, cho thấy game thay đổi thế nào
4. 1:10-1:30 — Nhảy tới đoạn đông rác, cho thấy hiệu năng vẫn mượt, nhắc tới object pooling
5. 1:30-1:50 — Boss mèo
6. 1:50-2:00 — Màn hình kết thúc và bảng thống kê

Chuẩn bị sẵn một file save hoặc chế độ cheat để nhảy nhanh tới boss, đừng ngồi chơi 5 phút thật trước lớp.

## Phụ lục I — Tài nguyên tham khảo

**Học thể loại**
- Tìm khoá: `Unity survivors-like tutorial`, `Unity 2D horde survival`
- Series có bài viết kèm video và file project tải về theo từng phần của Terresquall
- Game tham khảo: Vampire Survivors, Brotato (gần quy mô đồ án nhất), 20 Minutes Till Dawn, HoloCure

**Kỹ thuật Unity**
- `Unity object pooling 2D`
- `Unity ScriptableObject upgrade system`
- `Unity Cinemachine 2D follow camera`
- Unity Learn — khoá 2D Beginner: Adventure Game, có project hoàn chỉnh và asset miễn phí

**Asset và âm thanh**
- kenney.nl — asset CC0
- itch.io, mục Game Assets, lọc Free
- freesound.org — âm thanh
- jsfxr / Chiptone — tự tạo hiệu ứng âm thanh retro trong trình duyệt

---

*Tài liệu này là bản thiết kế kiêm báo cáo. Mọi con số đều là điểm xuất phát để thử nghiệm, không phải con số cuối cùng. Cứ chỉnh thoải mái theo cảm giác chơi thực tế.*