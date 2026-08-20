# Tasks — Hàng đợi ticket nhỏ, độc lập

> Đọc `Architecture_MainMenuFoundation.md` trước — bắt buộc, mọi ticket UI/audio đều cần biết cách test qua Additive Scene.
> Ticket KHÔNG gắn cứng tên người — ai rảnh nhận ticket kế tiếp theo thứ tự dưới đây. Xong 1 ticket = 1 nhánh git `edge/<hạng-mục>/<username>`, báo Jok ngay.
> Mỗi ticket: làm xong trong 1 buổi. Nếu kẹt >1 buổi → trả lại, báo Jok.

## Tiêu chuẩn hoàn thành — đọc kỹ trước khi bắt đầu

- **Làm hoàn thiện từ A đến Z**, không làm nửa vời rồi báo "gần xong". Ticket chỉ tính xong khi đúng đủ **Quy tắc báo cáo** bên dưới, không có ngoại lệ.
- **Bắt buộc tạo scene Unity MỚI riêng cho ticket của mình** (xem `Architecture_MainMenuFoundation.md` mục 2-3) — không làm trực tiếp trên `Chapter1.unity`.
- **Nội dung/giá trị cụ thể (tên, mô tả, thông số...) đã ghi sẵn trong từng ticket bên dưới — dùng ĐÚNG như vậy, không tự đặt/sáng tạo khác đi** trừ khi ticket ghi rõ "tuỳ chọn". Đây không phải gợi ý, là nội dung chính thức.
- **Báo cáo tiến độ mỗi ngày lúc 19h00 (7h tối)** vào group — dù xong hay chưa xong đều phải báo.
- **Nếu tới 19h00 mà ticket chưa xong hoặc thiếu bước nào trong Quy tắc báo cáo** — tự liệt kê rõ những gì còn thiếu thành 1 list, gửi riêng cho Jok (nhóm trưởng), không chờ ai hỏi. Đây là quy trình bắt buộc, báo thiếu không bị trừ gì cả — im lặng hoặc báo "xong" mà không đủ 3 mục ở Quy tắc báo cáo mới là vấn đề.

## 🟢 vs 🔴 — map chưa dựng xong (Jok làm ngày mai), giao ngay phần không phụ thuộc

- 🟢 **LÀM NGAY** — không cần khuôn kiến trúc mới. Test cơ chế trong scene tạm của bạn (1 phòng placeholder bất kỳ, hoặc mở Additive `Chapter1.unity` hiện tại nếu cần object có sẵn như `PlayerCamera`/`SanityManager`/`GlobalVolume`). Vị trí đặt cuối cùng trong map thật sẽ làm lại nhẹ sau khi Jok xong khuôn — không lãng phí công, phần LOGIC/WIRING đã đúng thì chỉ cần kéo-thả lại vị trí.
- 🔴 **CHỜ MAP** — bắt buộc cần vị trí/geometry thật (cửa, hành lang...) từ khuôn kiến trúc Jok đang dựng. Không nhận ticket này cho tới khi Jok báo khuôn xong.

**Giao ngay (🟢): T04–T11, T13–T17, T19, T20. Chờ map (🔴): T01, T02, T03, T12, T18.**

## Phân công cụ thể (không tự chọn nữa — theo đúng vai trò từng người)

| Người | Ticket | Vì sao hợp vai trò |
|---|---|---|
| **Thuận** (gameplay/item) | T04, T05, T06, T07 | Item/Inventory logic — đúng mảng cũ của Thuận |
| **Tân** (audio/sanity/blender) | T09, T10, T11 | Audio Log/Ambient/Sanity — đúng mảng cũ của Tân |
| **Phúc** (trigger/AI/mirror) | T13, T14 | Mirror + Gaze — đúng mảng AI/mirror cũ của Phúc |
| **Vũ** (UI/ambient) | T08, T17, T19, T20 | Piano verify + Well + Dialogue/Inventory UI test — đúng mảng UI cũ của Vũ |
| **Tuấn Anh** (level design) | T15, T16 | Đặt HideSpot lên nội thất — gần nhất với việc bố trí không gian |

Ai xong hết phần mình trước — báo Jok, nhận thêm từ phần người khác đang chậm (không đợi ai giao lại).

## Quy tắc báo cáo mọi ticket (bắt buộc)

1. Ảnh Console lúc Play Mode — 0 dòng đỏ.
2. Mô tả/ảnh kết quả TEST THẬT (không chấp nhận "xong rồi" suông).
3. Trả lời đúng **câu hỏi xác nhận** cuối ticket — lấy từ chính nội dung hướng dẫn, không đọc kỹ sẽ trả lời sai.
4. Chỉ sửa/tạo trong scene RIÊNG của bạn — không đụng `Assets/_Project/Scripts/`, không đụng scene người khác, không đụng khuôn kiến trúc gốc của Jok.

---

## Mục 0 — Chỉ Jok (không phải ticket cho team)

- [ ] Dựng khuôn kiến trúc mới: tường ngoài + mặt bằng từng tầng (kể cả tầng trống).
- [ ] Đánh dấu vị trí 2 cửa đôi gãy (garden, kho) trong khuôn mới.
- [ ] Chia khu vực cụ thể cho từng ticket cần vị trí thật (cửa, gương, giếng...).

---

## Hàng đợi ticket (theo thứ tự ưu tiên)

### 🔴 T01 — Cửa gãy #1 (garden) — CHỜ MAP
- Kiểm tra model có sẵn trước: `Assets/_Project/Models/Props/Furniture/Kenney/doorway.glb` / `doorwayFront.glb` / `doorwayOpen.glb`. Nếu không hợp, tìm thêm ở PolyHaven/KennyNL/Fab.com/Unity Asset Store — license CC0/"Free for commercial use".
- Đặt model vào vị trí Jok đánh dấu. Add `Door Controller`. Add `Box Collider`.
- **Set Layer = "Interactable" (layer 8)** — thiếu bước này thì bấm E không có phản ứng, không báo lỗi gì cả.
- Test: Play, tới cửa, nhấn E → mở/đóng đúng animation.
- **Câu hỏi xác nhận:** Layer bạn set cho object cửa là số mấy?

### 🔴 T02 — Cửa gãy #2 (kho) — CHỜ MAP
- Y hệt T01, vị trí thứ 2 Jok đánh dấu.
- **Câu hỏi xác nhận:** Model bạn dùng lấy từ nguồn nào (có sẵn trong project hay tìm mới, nguồn gì)?

### 🔴 T03 — Trigger thoại mở đầu (từ ngoài đi vào) — CHỜ MAP
- Tại cửa chính (Jok chỉ định), tạo `Trigger Zone` + `Box Collider (Is Trigger)`.
- Tạo asset `Trigger Settings` (Create → VillaOfDarkness → Trigger Settings), kéo vào field `_settings`.
- Nối `On Triggered` → gọi `Dialogue Trigger.Play Dialogue()` (kéo object có sẵn `DialogueUI`/`DialogueTrigger` từ MainMenu additive hoặc scene Jok cấp).
- Test: đi từ ngoài vào, băng qua trigger → thoại tự phát.
- **Câu hỏi xác nhận:** `targetTag` trong Trigger Settings của bạn là gì?

### 🟢 T04 — ItemData: chìa khoá — LÀM NGAY
- Assets → Create → Inventory → Item Data.
- **Nội dung dùng đúng:** `itemId = key_skeleton`, `itemName = "Chìa khoá cũ"`, `description = "Một chiếc chìa khoá đồng cũ, mặt ngoài đã xỉn màu theo năm tháng. Có thể mở được ổ khoá nào đó trong nhà."`, `isKeyItem = true`.
- `icon`: chưa có sprite riêng — tạm dùng bất kỳ icon placeholder nào có trong `Assets/_Project/Textures/UI/`, không để trống (để trống thì T20 sẽ không test được đúng).
- Lưu ý: field `slotBorderColor` trên ItemData KHÔNG được `InventoryUI` đọc (đã kiểm tra code) — viền vàng tự động theo `isKeyItem`, không cần set màu tay.
- **Câu hỏi xác nhận:** `itemId` bạn đặt là gì, viết chính xác?

### 🟢 T05 — ItemData: 1 vật phẩm thường (không phải chìa khoá) — LÀM NGAY
- **Nội dung dùng đúng:** `itemId = family_photo`, `itemName = "Ảnh gia đình"`, `description = "Một bức ảnh đen trắng đã ố vàng, chụp cả nhà đứng trước cổng biệt thự. Không rõ họ là ai."`, `isKeyItem = false`.
- **Câu hỏi xác nhận:** Vật phẩm này mô tả (`description`) nội dung gì?

### 🟢 T06 — Setup tủ khoá (ItemLock) — LÀM NGAY (test trong scene tạm, dời vị trí thật sau)
- Phụ thuộc T04 xong trước.
- Đặt `Furn_Cabinet_Locked.glb` (có sẵn), add `Item Lock`.
- Kéo `InventorySystem` (mở additive từ scene Jok cấp) vào `_inventorySystem`.
- **Nội dung dùng đúng:** `_requiredItemId = key_skeleton` (khớp T04), `_consumeRequired = true` (dùng chìa xong thì mất, không cần dùng lại), `_grantItemId` để TRỐNG (tủ này chỉ cần mở ra, không cộng thêm item vào túi — vật phẩm T05 đặt riêng ở T07, không liên quan tủ này), `_lockedHint = "Cần thêm thứ gì đó để mở..."`, `_unlockedHint = "Đã mở."`.
- Test: chưa có chìa → E vào tủ → log "locked". Có chìa (dùng ContextMenu test trên `InventorySystem` nếu cần) → E → log "UNLOCKED".
- **Câu hỏi xác nhận:** `_requiredItemId` bạn điền có khớp `itemId` ở T04 không, khớp thế nào?

### 🟢 T07 — PickupItem cho vật phẩm T05 — LÀM NGAY
- Add `Pickup Item` lên model vật phẩm T05. Kéo `ItemData` (T05) vào `_itemData`, kéo `InventorySystem` vào `_inventorySystem`.
- Test: E nhặt → mở Inventory (Tab) → icon/tên hiện đúng, không phải ô trống.
- **Câu hỏi xác nhận:** Sau khi nhặt, model gốc trong scene còn nhìn thấy không? (Xem code `PickupItem.cs` — có `SetActive(false)` hay không?)

### 🟢 T08 — Verify wiring Piano (test trong scene tạm, dời vào phòng thật sau) — LÀM NGAY
- Đặt `Prop_Piano` vào phòng mới. Kiểm tra 7 `PianoKey` còn field `_piano`/`_noteDefinition` (nếu mất, kéo lại multi-select).
- Test: E zoom vào, A/D chọn phím, Space chơi đúng nốt.
- **Câu hỏi xác nhận:** Nếu field bị mất sau khi di chuyển scene, bạn phát hiện bằng cách nào?

### 🟢 T09 — AudioLogItem (BL-LOG-01) — LÀM NGAY
- Chọn model máy phát (hộp nhạc/máy hát cũ). Add `Audio Log Item`.
- **Nội dung dùng đúng cho `_logText`:** `"Bà Lan: Con có nghe thấy tiếng nhạc không... nó vẫn còn vang trong hộp nhạc ấy. Ta đã giấu nó rất kỹ, sợ ai đó tìm thấy sẽ đánh thức... thứ không nên đánh thức."`
- `_logClip`: chưa có file thoại thật (lồng tiếng chưa thu theo GDD) — TẠM dùng bất kỳ audio clip có sẵn trong `Assets/_Project/Audio/` để test cơ chế phát đúng, ghi rõ trong báo cáo là placeholder, chờ Tân thay bằng thoại thật sau khi thu âm.
- Test: E → nghe clip phát, Console log đúng dòng.
- **Câu hỏi xác nhận:** `_logText` bạn điền nội dung gì (chỉ cần tóm tắt)?

### 🟢 T10 — AmbientZone (kích thước tạm, chỉnh lại khi có phòng thật) — LÀM NGAY
- Tạo Empty GameObject bao khu vực, add `Collider` (Is Trigger) + `Audio Source` (gán clip) + `Ambient Zone`.
- **Thông số dùng đúng:** `_targetVolume = 0.7`, `_fadeDuration = 1.5` (mặc định script, giữ nguyên trừ khi test thấy quá to/quá nhỏ thì báo lại, đừng tự đổi im lặng).
- Test: đi vào/ra nghe fade mượt.
- **Câu hỏi xác nhận:** `_fadeDuration` bạn đặt bao nhiêu giây?

### 🟢 T11 — Fix Sanity Volume wiring (nợ từ trước, ưu tiên cao) — LÀM NGAY (mở trực tiếp `Chapter1.unity` hiện tại, không cần chờ map mới)
- Tìm 2 object `PlayerCamera` và `SanityManager` (mở additive scene Jok cấp có sẵn 2 object này).
- Cả 2 đều có `Sanity Post Process` với field `_volume` đang trống.
- Kéo object `GlobalVolume` (có sẵn) vào field này ở **CẢ HAI**.
- Vì sao: thiếu bước này thì hiệu ứng mờ/nhiễu khi Sanity thấp không bao giờ chạy, không có lỗi báo.
- **Câu hỏi xác nhận:** Bạn gán Volume cho đúng 2 object tên gì?

### 🔴 T12 — Ghost waypoints — CHỜ MAP (cần hành lang/phòng thật để đặt điểm patrol có nghĩa)
- Tạo Empty GameObject dọc đường đi (`WP_01`, `WP_02`...), kéo hết vào `_waypoints` trên `GhostAI`.
- Test: Play, Ghost patrol không xuyên tường.
- **Câu hỏi xác nhận:** Bạn đặt bao nhiêu waypoint?

### 🟢 T13 — Setup Mirror (dùng plane tạm làm gương, dời vào phòng thật sau) — LÀM NGAY
- Tạo Layer `MirrorOnly`. Gán Ghost + Player vào layer này.
- Tạo Camera con, `Culling Mask = MirrorOnly`, render ra `RenderTexture` (512×512), gán vào Material mặt gương.
- **Câu hỏi xác nhận:** RenderTexture bạn tạo kích thước bao nhiêu?

### 🟢 T14 — GazeTrigger cho gương (Ma Da) — LÀM NGAY
- Phụ thuộc T13 xong trước.
- Tạo `Gaze Settings` asset.
- **Thông số dùng đúng (khớp đúng GDD — Ma Da nhìn gương >3 giây thì chết):** `gazeThreshold = 3`, `warningThreshold = 1`, `maxDistance = 8` (phòng nhỏ, để dư an toàn hơn cần thiết).
- Add `Gaze Trigger` lên gương, kéo Gaze Settings vào `_settings`.
- Test: nhìn gương liên tục ≥3s → chết đúng kịch bản.
- **Câu hỏi xác nhận:** `gazeThreshold` bạn set bao nhiêu giây, test thực tế mất bao lâu để chết?

### 🟢 T15 — HideSpot #1 — LÀM NGAY
- **Model dùng đúng:** giường đôi — `Assets/_Project/Models/Props/Furniture/Kenney/bedDouble.glb` (đã có sẵn). Núp dưới gầm giường.
- Add `Hide Spot`, kéo Player vào `_playerController`, tạo Empty GameObject con đặt NGAY DƯỚI gầm giường làm `_hidePosition`.
- Test: E vào núp → Player dịch chuyển đúng chỗ; E lần nữa → về đúng vị trí cũ.
- **Câu hỏi xác nhận:** Khi đang núp, Collider của Player tắt hay bật? (đọc code `HideSpot.cs`)

### 🟢 T16 — HideSpot #2 (điểm núp thứ 2, vị trí khác) — LÀM NGAY
- **Model dùng đúng:** tủ sách đóng — `Assets/_Project/Models/Props/Furniture/Kenney/bookcaseClosedWide.glb` (đã có sẵn), hoặc `Furn_Cabinet.fbx` nếu hợp cảnh hơn. Núp bên trong/sau tủ.
- Y hệt T15 về cách setup.
- **Câu hỏi xác nhận:** Model bạn dùng cho điểm núp này tên gì?

### 🟢 T17 — Well + WellDeathSequence (model giếng đặt tạm) — LÀM NGAY
- **Model dùng đúng:** `Assets/_Project/Models/Props/Architecture/Arch_Well_Stone.glb` (đã có sẵn, đúng "giếng" GDD nhắc tới).
- Add `Gaze Trigger` (Gaze Settings riêng, `gazeThreshold=3` giống T14) + `Well Death Sequence`.
- Kéo `_gazeTrigger`, `_playerController`, `_deathScreenUI` vào đúng field. **`_requiredDistance = 2`** (mặc định script, giữ nguyên).
- Test: nhìn nước ≥ ngưỡng, đứng đủ gần → chuỗi chết chạy (âm thanh → đốm sáng → fade đen → DeathScreen).
- **Câu hỏi xác nhận:** `_requiredDistance` bạn set bao nhiêu mét?

### 🔴 T18 — Test end-to-end UI flow — CHỜ MAP (cần Chapter1 chơi được ổn định)
- Chạy đúng thứ tự, không bỏ bước: MainMenu → Start → Chapter1 load → chơi thử → chết (HideSpot thất bại hoặc Gaze) → DeathScreenUI hiện → Retry → Chapter1 load lại → ESC → PauseMenuUI hiện → về MainMenu.
- **Câu hỏi xác nhận:** Bước nào (nếu có) không chạy đúng, mô tả cụ thể hiện tượng?

### 🟢 T19 — Test/polish DialogueUI (4 dòng thoại mở đầu ĐÃ CÓ SẴN, không cần viết mới) — LÀM NGAY
- `Assets/_Project/Data/Triggers/DialogueAsset.asset` đã có sẵn 4 dòng thoại thật (Minh Khoa đứng trước biệt thự → thử cửa → khoá → không có chìa). KHÔNG cần viết thêm nội dung, chỉ cần TEST HIỂN THỊ đúng.
- Mở additive scene có `DialogueUI`/`DialogueTrigger` (từ Chapter1.unity hiện tại). Tạo tạm 1 UI Button gọi `Dialogue Trigger.Play Dialogue()` để test (không cần TriggerZone/map thật).
- Test: chữ hiện hiệu ứng gõ máy chữ đúng, mũi tên nhấp nháy đúng lúc, Space/Enter chuyển dòng, hết 4 dòng thì tự đóng panel.
- **Câu hỏi xác nhận:** Dòng thoại thứ 3 nội dung gì (đọc trong `DialogueAsset.asset`)?

### 🟢 T20 — Test/polish InventoryUI (dùng ItemData từ T04/T05) — LÀM NGAY (phụ thuộc T04, T05, T07 xong trước)
- Mở Inventory (Tab) sau khi đã nhặt các vật phẩm từ T07. Kiểm tra: icon/tên hiện đúng từng ô, ô trống hiện đúng màu xám, viền vàng cho vật phẩm `isKeyItem=true`, viền thường cho vật phẩm khác.
- Click item → xem mô tả (`OnItemClicked`) hoạt động đúng, không lỗi.
- Nếu có `ExamineItem` proxy riêng cho item nào — test click từ Inventory mở đúng model 3D xoay được, nhấn E thoát về lại Inventory.
- **Câu hỏi xác nhận:** Vật phẩm nào trong Inventory của bạn có viền vàng, vì sao (đọc field nào trên `ItemData` quyết định việc này)?

---

## Việc phi-dev (song song, không đổi so với trước)

Viết báo cáo · Vẽ use case · Slide bảo vệ · Kịch bản thuyết trình · Quay dựng video demo · Chụp hình minh hoạ · Test theo checklist · Ghi bug đầy đủ cho Jok sửa · Chuẩn bị dữ liệu test · Build+test nhiều máy · Chuẩn bị file nộp.

## Model/prop có sẵn — xem trước khi tìm asset mới

`Assets/_Project/Models/Props/Furniture/Kenney/` (giường, tủ, bàn ghế — rất nhiều), `Assets/_Project/Models/Props/Decor/PolyHaven/`, `Assets/_Project/Models/Props/Architecture/` (cửa, cửa sổ, lò sưởi, hàng rào).
