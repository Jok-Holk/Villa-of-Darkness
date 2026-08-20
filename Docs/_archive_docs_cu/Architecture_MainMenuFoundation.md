# Kiến trúc nền — MainMenu là gốc

> Đọc file này TRƯỚC khi làm bất kỳ ticket nào trong `Tasks_Chapter1.md`. Giải thích tại sao mọi UI mới (Inventory, PauseMenu, Dialogue...) phải "lấy MainMenu làm gốc", và cách test scene riêng của bạn mà vẫn có đủ hệ thống nền.

## 1. Vì sao "lấy MainMenu làm gốc"

Toàn bộ hệ thống DÙNG CHUNG của game (âm lượng, đồ hoạ, sương mù/skybox, quản lý chuyển scene, fade màn hình) chỉ được khởi tạo **1 LẦN DUY NHẤT**, trong scene `MainMenu.unity`, lúc game vừa mở. Sau đó chúng **sống xuyên suốt sang mọi scene khác** (Chapter1, Chapter2...) nhờ `DontDestroyOnLoad()`.

Đây là 5 object gốc, tất cả nằm trong `MainMenu.unity`:

| GameObject | Script | Vai trò | Truy cập từ code |
|---|---|---|---|
| `SettingsManager` | `SettingsManager.cs` | Volume/Sensitivity/Graphics Quality/Resolution | `SettingsManager.Instance` |
| `AudioManager` | `AudioManager.cs` | Phát SFX/BGM, âm lượng Music/SFX riêng | `AudioManager.Instance` |
| `LightingManager` | `FogManager.cs` | Sương mù + Skybox tối/sáng | Không cần gọi từ code khác — tự chạy |
| (GameObject nào giữ `GameManager`) | `GameManager.cs` | Load chapter, chết/respawn, DeathScreen | `GameManager.Instance` |
| `ScreenFader_Canvas` | `ScreenFader.cs` | Fade đen khi chuyển scene | `ScreenFader.Instance` |

**Quy tắc bắt buộc:** mọi script mới (Inventory, PauseMenu, item mới...) nếu cần âm lượng/chuyển scene/fade — PHẢI gọi qua `.Instance` của các object trên, KHÔNG được tự viết lại logic riêng (vd không tự viết `AudioSource.PlayOneShot` riêng, phải gọi `AudioManager.Instance.PlaySFX(clip)` để tôn trọng cả setting Volume người chơi đã chỉnh).

## 2. Vấn đề khi bạn tạo scene MỚI để làm ticket riêng

Vì 5 object trên chỉ tồn tại trong `MainMenu.unity`, nếu bạn tạo 1 scene trống mới rồi bấm Play trực tiếp — **`SettingsManager.Instance`, `AudioManager.Instance`, `GameManager.Instance` đều sẽ là `null`**. Bất kỳ script nào gọi tới chúng sẽ:
- Không báo lỗi đỏ gì cả (code có `?.` an toàn, ví dụ `AudioManager.Instance?.PlaySFX(...)` chỉ im lặng không làm gì).
- Kết quả: bạn tưởng mình làm sai, thật ra chỉ vì thiếu hệ thống nền — đây CHÍNH XÁC là loại bug đã tốn rất nhiều thời gian sửa trong các phiên trước (slider Settings "không có tác dụng gì" vì thiếu GameObject SettingsManager trong scene).

## 3. Cách test đúng — Additive Scene (không cần hiểu cách 5 object trên hoạt động)

1. Mở scene MỚI của bạn làm scene chính (`File → Open Scene`).
2. Nạp thêm MainMenu: `File → Open Scene (Additive) → Assets/_Project/Scenes/MainMenu.unity`.
3. Bấm Play — giờ cả 2 scene cùng chạy, `SettingsManager.Instance`/`AudioManager.Instance`/`GameManager.Instance` đều có thật, mọi thứ hoạt động bình thường.
4. **CHỈ SAVE scene của bạn** (`Ctrl+S` khi scene của bạn đang active) — TUYỆT ĐỐI không save lại `MainMenu.unity` (không cần, không được sửa gì trong đó).

Đây là kỹ thuật đã dùng thành công trước đây khi team làm việc song song trên nhiều zone khác nhau — không có gì mới, chỉ là áp dụng lại cho cách chia việc hiện tại.

## 4. Quy ước UI bắt buộc — Inventory/PauseMenu/mọi Canvas mới đều phải theo

Rút ra từ việc sửa MainMenu/Settings phiên trước, tránh lặp lại đúng những lỗi đã tốn nhiều thời gian:

- **Canvas Render Mode = Screen Space - Overlay** (KHÔNG dùng Screen Space - Camera) — Camera mode sẽ bị sương mù/hậu kỳ đè lên làm UI bị nhoè theo cảnh 3D.
- **`localScale` của mọi RectTransform phải = (1,1,1)`** trước khi chỉnh `sizeDelta`/`anchoredPosition` — object kế thừa từ thiết kế cũ hay dính scale lạ (2.9, 2.7...) gây phóng to sai kích thước dù số liệu Inspector trông "đúng".
- **`sortingOrder` của Canvas mới phải THẤP HƠN `ScreenFader_Canvas`** (hiện tại fader = 1000) — nếu không, lúc fade đen sẽ che luôn UI của bạn.
- **Chữ tiếng Việt có dấu KHÔNG dùng font `JustMeAgainDownHere SDF`** — font này thiếu glyph dấu tiếng Việt (ẫ, ố, ữ...), hiển thị lỗi. Dùng tiếng Anh cho UI kỹ thuật (nút, label), tiếng Việt chỉ dùng in-game dialogue/lore sau khi có font hỗ trợ đủ dấu.
- **Màu chủ đạo:** đỏ đậm/đen (`#0F0A0A`–`#8C1A1A` tuỳ độ sáng), viền `Outline` component màu đỏ nhạt hơn nền — đồng bộ với MainMenu/Settings đã làm.
- **Mọi thay đổi cài đặt phải áp dụng NGAY (live preview)** qua gọi trực tiếp `.Instance` tương ứng — không tạo hệ thống "Apply" riêng cho từng UI, dùng chung logic Apply/Back-revert đã có sẵn trong `SettingsUI.cs` làm mẫu nếu cần tính năng tương tự.

## 5. Cách 3 UI hiện có liên hệ với hệ thống gốc (tham khảo khi sửa/mở rộng)

- **`InventoryUI.cs`** — sống trong `Chapter1.unity` (không phải DontDestroyOnLoad, không cần vì chỉ dùng lúc chơi Chapter1), lấy dữ liệu từ `InventorySystem.cs` (cũng scene-local), gọi `AudioManager.Instance?.PlaySFX(...)` khi click item — ĐÃ đúng chuẩn, dùng làm mẫu.
- **`PauseMenuUI.cs`** — cũng scene-local trong Chapter1, dùng `Time.timeScale` để dừng game, gọi `SceneManager.LoadScene("MainMenu")` trực tiếp khi bấm về Main Menu (KHÔNG qua `ScreenFader` — nếu muốn có hiệu ứng fade khi thoát về Menu, đây là chỗ có thể cải thiện, tham khảo cách `MainMenuUI.StartGame()` dùng `ScreenFader.Instance.FadeToScene(...)`).
- **`GameManager.cs`** — DontDestroyOnLoad thật, `LoadChapter(int)` dùng để chuyển scene theo số chapter, `PlayerDead()` tự tìm `DeathScreenUI` trong scene hiện tại (không cần gán tay).

## 6. Checklist nhanh trước khi báo cáo 1 ticket UI

- [ ] Canvas Render Mode = Overlay?
- [ ] Test qua Additive Scene với MainMenu, không phải scene trống?
- [ ] localScale mọi RectTransform = 1?
- [ ] Gọi `AudioManager.Instance`/`SettingsManager.Instance` thay vì tự viết logic riêng?
- [ ] Chữ tiếng Việt có dấu → đã đổi sang tiếng Anh (nếu là UI kỹ thuật)?
