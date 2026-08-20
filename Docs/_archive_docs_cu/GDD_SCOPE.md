# GDD Scope — Villa of Darkness

## Tổng quan

- **Tên game:** Biệt Thự Bóng Tối / Villa of Darkness
- **Engine:** Unity 6, URP
- **Thể loại:** Horror Survival / Puzzle / Story-driven, góc nhìn thứ nhất
- **Nền tảng:** PC Windows
- **Đồ hoạ:** PSX low-poly aesthetic, CRT post-process
- **Thời gian chơi dự kiến:** ~1–2 giờ (4 chapter)
- **Trạng thái:** Demo Chapter 1 (không còn mục tiêu bán game)

---

## Nhân vật chính — 4 chapter

| Chapter | Nhân vật | Tuổi | Năm | Mục tiêu vào biệt thự |
|---|---|---|---|---|
| Ch.1 | Minh Khoa | 21 | 2000 | Chụp ảnh cho đề tài kiến trúc |
| Ch.2 | Bích Ngọc | 19 | 1970 | Lấy gương bạc theo lệnh bà ngoại |
| Ch.3 | Tuấn Hùng | 22 | 1990 | Điều tra viết bài báo |
| Ch.4 | Lan Anh | 23 | 2020 | Phong ấn thực thể — mang 3 di vật |

---

## Gia đình Đỗ (lore)

- Dòng họ **Đỗ** (không phải Đặng)
- Ông **Đỗ Văn Minh** — chủ biệt thự, xây 1945
- Bà **Lan** (Đỗ Lan Hương) — vợ, biết về nghi lễ phong ấn
- **Đỗ Minh** — con trai, 12 tuổi
- **Đỗ Linh** — con gái, 8 tuổi
- Năm 1965: cả gia đình biến mất không dấu vết

---

## 3 Kết thúc

| Kết thúc | Điều kiện | Kết quả |
|---|---|---|
| Ending 1 — Giải thoát | 8/8 Audio Log + đặt đúng thứ tự: muối → hộp nhạc → gương | Nghi lễ hoàn chỉnh, linh hồn được giải phóng |
| Ending 2 — Thoát ra | Đặt đúng thứ tự nhưng thiếu Audio Log | Lan Anh thoát, thực thể suy yếu nhưng chưa bị phong ấn |
| Ending 3 — Thất bại | Sai thứ tự / Sanity < 20% / thiếu < 5 Audio Log | Lan Anh chết, vòng lặp tiếp tục |

---

## Vật phẩm phong ấn (Ch.4)

- **Lọ muối đen** — Tuấn Hùng để lại
- **Hộp nhạc đồng** — Minh Khoa để lại (mở bằng 7 nốt)
- **Gương bạc** — Bích Ngọc để lại (vỡ góc, ghép lại ở Ch.4)

---

## Puzzle chính

**Piano — 7 nốt: D - E - G - A - F - B - C#**
- Ch.1: Khoa bấm 5 nốt đầu → cửa thư phòng mở
- Ch.4: Lan Anh bấm đủ 7 nốt → hộp nhạc mở → lấy chìa khoá tầng hầm

---

## Audio Log — 8 log của gia đình Đỗ

| # | Người nói | Kích hoạt |
|---|---|---|
| BL-LOG-01 | Bà Lan | Ch.1 — nhặt hộp nhạc |
| BL-LOG-02 | Bà Lan | Ch.2 — phòng bà Lan T1 |
| BL-LOG-03 | Bà Lan | Ch.2 — tường phòng bé Linh |
| BL-LOG-05 | Bà Lan | Ch.3 — băng reel phòng bà Lan T2 |
| BL-LOG-06 | Bà Lan | Ch.3 — gương phủ vải |
| DVM-01–03 | Đỗ Văn Minh | Ch.3 — nhật ký thư phòng |
| DVM-TAPE | Đỗ Văn Minh | Ch.3 — Hùng phát băng reel |
| BL-ENDING-01 | Bà Lan | Ch.4 — Ending 1 |

---

## Sanity System

| Mức | Biểu hiện |
|---|---|
| 70–100 | Bình thường |
| 40–70 | Vignette nhẹ, thở nhanh |
| 20–40 | Hình méo, nghe thì thầm |
| 0–20 | Camera shake, tự thoát khỏi chỗ trốn sau 30s |

---

## Ma

**Ma Vú Dài** — AI: Patrol → Investigate → Chase → Kill
- Hearing: 8m / Sight: 12m / Patrol speed: 1.5 / Chase speed: 4.0
- Cùng VA với Bà Lan (hai trạng thái của một linh hồn)

**Ma Da** — GazeTrigger
- Nhìn vào mặt nước / gương > 3 giây → chết
- Bị phong ấn trong gương khi Ending 1

---

## Roadmap

| Phase | Nội dung | Trạng thái |
|---|---|---|
| Phase 1–2.6 | Player, Inventory, UI, AI, Sanity, Piano, FBX | ✅ Xong |
| Phase 2.7 | Gộp scene, lighting, PSX shader | ✅ Xong |
| Phase 2.8 | Hoàn thiện map Ch.1 — geometry, doors, props, NavMesh | 🔄 Đang làm |
| Phase 2.9 | Cutscene intro, death sequence, flow end-to-end | ⏳ Chưa làm |
| Phase 3+ | Chapter 2–4, lồng tiếng, 3 endings | ⏳ Chưa làm |

---

## Lồng tiếng

- Tổng ~91 câu thoại, 9 nhân vật cần VA
- Chưa thu âm
- Kịch bản đầy đủ: `KỊCH_BẢN_LỒNG_TIẾNG_v1.md`

---

## Công cụ sản xuất

| Công cụ | Mục đích | Chi phí |
|---|---|---|
| Unity 6 URP | Game engine | Miễn phí |
| Blender | 3D model, UV | Miễn phí |
| 3D AI Studio | Tạo nhanh props nhỏ | ~$0–9 |
| Suno AI | Soundtrack | ~$10 (1 lần) |
| Audacity | Hậu kỳ audio | Miễn phí |

---

## File liên quan

- `KỊCH_BẢN_LỒNG_TIẾNG_v1.md` — toàn bộ 91 câu thoại
- `HUONG_DAN_SLIDE.md` — hướng dẫn làm slide báo cáo
- `TeamTask_Phase3.md` — task chi tiết từng thành viên
- `GDD_BietThuBongToi_BaoCao_v8.docx` — báo cáo GDD (C:\Users\Admin\Downloads)
