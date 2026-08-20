# NỘI DUNG SLIDE THUYẾT TRÌNH — BIỆT THỰ BÓNG TỐI

### Đồ án tốt nghiệp · Lập trình Game · GVHD: Nguyễn Ngọc Chấn

---

## SLIDE 1 — TRANG BÌA

- Tên: BIỆT THỰ BÓNG TỐI (Villa of Darkness)

- Loại: Đồ án tốt nghiệp

- Chuyên ngành: Lập trình Game

- GVHD: Nguyễn Ngọc Chấn

- Nhóm: [tên nhóm]

---

## SLIDE 2 — THÀNH VIÊN

Bảng: Tên · MSSV · Mảng phụ trách

| Thành viên | MSSV | Mảng |

|------------|------|-------|

| [Nguyễn Bùi Phúc Thái]] | | PM / Lead Dev / Architecture |

| Võ Văn Thuận | | Gameplay / Item / Piano |

| Bùi Thành Tân | | Audio / Sanity / 3D Models |

| Nguyễn Hữu Phúc | | Triggers / AI / Mirror |

| Nguyễn Trường Vũ | | UI / Lighting |

| Tuấn Anh | | Level Design / Scene |

---

## SLIDE 3 — BẢNG THEO DÕI TIẾN ĐỘ

| Giai đoạn | Nội dung chính | Trạng thái |

|-----------|---------------|------------|

| Phase 1 | Player controller, inventory, UI cơ bản | ✅ Hoàn thành |

| Phase 2 | Chapter 1: Piano, Ghost AI, Sanity | ✅ Hoàn thành |

| Phase 2.6 | FBX models, audio zones, death screen | ✅ Hoàn thành (merged 13/6) |

| Phase 2.7 | Gộp 5 nhánh về Chapter1.unity, lighting, PSX | ✅ Hoàn thành |

| **Phase 2.8** | **Hoàn thiện map: geometry, props, NavMesh, materials** | 🔄 Đang làm |

| Phase 2.9 | Cutscene, gameplay flow, playable build | 📋 Kế hoạch |

| Phase 3+ | Chapter 2–4, lồng tiếng, polish | 📋 Kế hoạch |

> Ghi chú thật: Phase 2.7 thực chất là **merge về 1 scene** (Chapter1.unity) và áp lighting/PSX — chưa phải integration gameplay đầy đủ. Gameplay integration bắt đầu từ Phase 2.8 trở đi.

---

## SLIDE 4 — GIỚI THIỆU DỰ ÁN

**Biệt Thự Bóng Tối** là game kinh dị sinh tồn góc nhìn thứ nhất, lấy bối cảnh **biệt thự Pháp cổ tại Đà Lạt**.

- Engine: Unity 6 · URP

- Nền tảng: PC Windows

- Thể loại: Horror Survival / Puzzle / Story-driven

- Đồ hoạ: PS1/PS2 low-poly aesthetic, CRT post-process

- Thời gian chơi dự kiến: ~1–2 giờ (4 chapter)

---

## SLIDE 5 — Ý TƯỞNG / TỔNG QUÁT

**Tại sao làm game này?**

- Khai thác hình tượng kinh dị **dân gian Việt Nam** — Ma Vú Dài, Ma Da — ít xuất hiện trong game

- Bối cảnh kiến trúc **Đông Dương** đặc trưng: biệt thự Pháp, Đà Lạt 1940–2020

- Kể chuyện qua **4 thời điểm lịch sử** — cùng một ngôi nhà, 4 số phận khác nhau

- Aesthetic **PS1/PS2 có chủ đích** — không phải retro ngẫu nhiên, phục vụ cảm giác bất an

---

## SLIDE 6 — BỐI CẢNH CHÍNH

**Biệt Thự Đỗ Gia — Đà Lạt**

Xây 1940. Năm 1965: cả gia đình Đỗ biến mất không dấu vết. Bỏ hoang từ đó.

4 chapter = 4 người trẻ vào nhà qua 4 mốc thời gian:

| Chapter | Năm | Nhân vật | Kết cục |

|---------|-----|----------|---------|

| Ch.1 | 2000 | Minh Khoa, 21t | Bị kéo xuống giếng |

| Ch.2 | 1970 | Bích Ngọc, 19t | Bị hút vào gương |

| Ch.3 | 1990 | Tuấn Hùng, 22t | Bị Ma Vú Dài bắt |

| Ch.4 | 2020 | Lan Anh, 23t | Tuỳ người chơi (3 endings) |

**Twist:** Item tìm được ở chapter trước được chapter sau kế thừa.

---

## SLIDE 7 — THỂ LOẠI / ĐỐI TƯỢNG

- **Thể loại:** Horror Survival / Puzzle / Story-driven

- **Đối tượng:** 16+, yêu thích kinh dị tâm lý, fan văn hoá Việt Nam

- **Tham chiếu:** Amnesia, Outlast, Visage

---

## SLIDE 8 — CORE GAMEPLAY MECHANICS

5 cơ chế trung tâm:

1. **Không vũ khí** — chỉ có đèn pin, ẩn nấp, và tư duy

2. **Sanity System** — tâm lý sụp đổ dần, biểu hiện qua visual và audio

3. **Puzzle xuyên chapter** — bài nhạc 7 nốt không ai trong Ch.1–3 có đủ manh mối

4. **Di sản vật phẩm** — item từ người chơi trước ảnh hưởng người chơi sau

5. **3 endings** — phụ thuộc mức độ khám phá (đếm audio log nghe được)

---

## SLIDE 9 — HỆ THỐNG TRÒ CHƠI (tổng quan)

Các system đã implement:

- Hệ thống nhân vật (PlayerController, InteractionSystem)

- Input & Controls

- Hệ thống âm thanh & hiệu ứng (AudioManager, AmbientZone)

- Hệ thống nhiệm vụ / câu đố (Piano, Lock/Key, TriggerZone)

---

## SLIDE 10 — HỆ THỐNG NHÂN VẬT

**PlayerController.cs**

- WASD di chuyển · C cúi · Shift chạy (tạo tiếng ồn)

- Mouse look · gravity

**InteractionSystem.cs**

- Raycast E-key · Giữ E = monologue vật phẩm

**FlashlightController.cs**

- T: bật/tắt · F: lắc phục hồi pin · Flicker khi pin yếu

**HideSpot.cs**

- Ẩn nấp trong tủ/gầm bàn · disable CharacterController khi ẩn

---

## SLIDE 11 — AI — MA VÚ DÀI

**GhostAI.cs** — NavMesh Agent

State machine 4 trạng thái:

```

Patrol → Investigate → Chase → Kill

```

- Hearing radius: 8m · Sight radius: 12m · FOV: 90°

- Patrol speed: 1.5 · Chase speed: 4.0

- Route theo chapter — tăng tốc và expand patrol khi trigger events

---

## SLIDE 12 — AI — MA DA

**GazeTrigger.cs** — không dùng NavMesh

- Trigger zone trên mặt nước / gương

- Camera nhìn vào > 3 giây → chết

- Cảnh báo: 1s gợn sóng · 2s màn hình xanh · 3s chết

- Ch.4: cầm gương bạc → capture thay vì chết

---

## SLIDE 13 — HỆ THỐNG TRIGGERS

**TriggerZone.cs** — trigger collider, one-shot option

**SpawnManager.cs** — spawn prefab tại điểm chỉ định

**DelayEvent.cs** — invoke UnityEvent sau delay

**GazeTrigger.cs** — raycast từ camera, warning + complete events

---

## SLIDE 14 — HỆ THỐNG INVENTORY & ITEM

**ItemData.cs** — ScriptableObject: tên, mô tả, audio monologue, icon

**InventoryUI.cs** — lưới 2×4, icon, mô tả, phát monologue

**InventoryTabHandler.cs** — Tab luôn active dù UI ẩn

**PickupItem.cs / ExamineItem.cs** — nhặt và xem vật phẩm

**ItemLock.cs** — vật phẩm khoá cửa / puzzle

3 di vật đặc biệt (khung vàng, không drop):

- Hộp âm nhạc đồng · Gương bạc · Lọ muối đen

---

## SLIDE 15 — HỆ THỐNG PIANO

**PianoInteractable.cs + PianoKey.cs**

- Ch.1: sequence 5 nốt (D-E-G-A-F) → mở cửa thư phòng

- Ch.4: sequence 7 nốt (D-E-G-A-F-B-C#) → mở hộp nhạc

- Gõ sai: reset sequence + ghost tăng tốc ngắn

- Manh mối phân tán qua 4 chapter — không ai có đủ một mình

---

## SLIDE 16 — PHÍM TẮT TRONG GAME

| Phím | Chức năng |

|------|-----------|

| WASD | Di chuyển |

| C | Cúi xuống |

| Shift | Chạy (tạo tiếng, thu hút ma) |

| E (giữ) | Tương tác / nghe monologue |

| T | Bật/tắt đèn pin |

| F | Lắc đèn phục hồi pin |

| Tab | Mở/đóng túi đồ |

| ESC | Pause menu |

---

## SLIDE 17 — HỆ THỐNG ÂM THANH

**AudioManager.cs** — singleton BGM + SFX

**AmbientZone.cs** — fade in/out ambient theo vùng

**AudioLogItem.cs** — phát ký ức khi tương tác, one-play guard

**RandomAmbientTrigger.cs** — ambient ngẫu nhiên tạo bầu không khí

Design âm thanh:

- Không dùng jump scare volume đột ngột

- Im lặng 1.5s → tiếng lạ nhỏ ở hướng ngược

- Nhạc nền: drone ambient, mix theo sanity level qua Audio Mixer Snapshots

---

## SLIDE 18 — HỆ THỐNG SANITY

**SanitySystem.cs** — giá trị 0–1, 4 nấc

**SanityPostProcess.cs** — lerp FilmGrain / Chromatic Aberration / Vignette

**SanityShake.cs** — Perlin camera sway (gắn Camera con)

**SanityZone.cs** — trigger safe/danger zone

| Mức | Biểu hiện |

|-----|-----------|

| High 75–100% | Bình thường |

| Medium 40–75% | Màn hình rung góc, thở nhanh |

| Low 10–40% | Ảo giác, nghe gọi tên |

| Critical <10% | Tự thì thầm, tự thoát ẩn nấp sau 30s |

---

## SLIDE 19 — NHÂN VẬT & LỒNG TIẾNG

9 vai cần lồng tiếng:

**Nhân vật chính (4):** Minh Khoa · Bích Ngọc · Tuấn Hùng · Lan Anh

**Ma (2):** Ma Vú Dài · Ma Da

**Ký ức gia đình Đỗ (3):** Ông Đỗ · Bà Lan · Đỗ Minh · Đỗ Linh

Tổng ~91 câu thoại · ~28–35 phút thu âm

→ Kịch bản đầy đủ đã hoàn thiện · Thuê studio sau khi xong gameplay

---

## SLIDE 20 — TIẾN ĐỘ PHÁT TRIỂN

| Phase | Nội dung | Trạng thái |

|-------|----------|------------|

| Phase 1 | Player, Inventory, UI nền | ✅ |

| Phase 2 | Chapter 1 systems: Piano, AI, Sanity, Triggers | ✅ |

| Phase 2.6 | FBX models, Audio zones, Death screen | ✅ |

| Phase 2.7 | Gộp 5 nhánh → Chapter1.unity · Lighting · PSX shader | ✅ |

| **Phase 2.8** | **Hoàn thiện map: geometry, doors, props, NavMesh, materials** | 🔄 |

| Phase 2.9 | Cutscene intro, death sequence, gameplay flow end-to-end | 📋 |

| Phase 3+ | Chapter 2–4 · Lồng tiếng · Polish · Build | 📋 |

> Phase 2.7 = **merge về 1 scene** + lighting. Chưa phải integration gameplay. Gameplay integration bắt đầu Phase 2.8 khi map hoàn chỉnh.

---

## SLIDE 21 — TẠI SAO DEMO HÔM NAY LÀ ENVIRONMENT?

**Kiến trúc module-based:**

Mỗi thành viên phụ trách 1 system độc lập → merge về scene chính sau khi xong.

Trạng thái hiện tại:

- ✅ Tất cả systems đã chạy được (AI, Sanity, Piano, Inventory, Audio)

- ✅ Map Chapter 1 + PSX shader + horror lighting

- 🔄 Map đang hoàn thiện geometry và props (Phase 2.8)

- 📋 Gameplay flow end-to-end: Phase 2.9

**Demo hôm nay:** Environment preview — không phải thiếu gameplay, mà gameplay cần map hoàn chỉnh trước khi đặt vào đúng chỗ.

---

## SLIDE 22 — DEMO: ENVIRONMENT PREVIEW

[NHÚNG VIDEO ~1–2 phút]

- Walkthrough Chapter 1 — Biệt Thự Đỗ Gia · Đà Lạt · Năm 2000

- PSX post-process: scanline, CRT, film grain

- Horror lighting: moonlight lạnh, fog, candlelight ambient

- Geometry: tầng trệt — salon, phòng ăn, hành lang, sân sau, giếng

---

## SLIDE 23 — DEMO: SYSTEMS ĐÃ HOÀN THIỆN

[Ảnh screenshot hoặc video clip ngắn từng system]

| System | Demo |

|--------|------|

| Ghost AI patrol + chase | ✅ |

| Piano puzzle 5 nốt | ✅ |

| Sanity visual 4 mức | ✅ |

| Inventory UI + item examine | ✅ |

| HideSpot mechanic | ✅ |

| Audio Log phát ký ức | ✅ |

---

## SLIDE 24 — ĐIỂM ĐẶC BIỆT

- **Văn hoá Việt Nam gốc:** Ma Vú Dài, Ma Da — dân gian, không bị Tây hoá

- **Kiến trúc Đông Dương:** nghiên cứu thực tế, thiết kế từng phòng có tên Pháp chuẩn

- **Puzzle xuyên chapter:** narrative và gameplay liên kết 4 chapter — hiếm gặp ở game indie

- **Không HUD:** tất cả thông tin truyền qua gameplay và môi trường

- **3 endings** phụ thuộc hành vi người chơi, không phải lựa chọn cuối

---

## SLIDE 25 — HƯỚNG TIẾP THEO

**Phase 2.8 (đang làm):** Hoàn thiện map — doors hoạt động, props đặt đúng, NavMesh bake, materials

**Phase 2.9:** Cutscene intro Ch.1, death sequence, flow MainMenu → Chapter1 → die/retry

**Phase 3+:** Chapter 2–4 · Lồng tiếng · Polish · Build release

Mục tiêu: **Bản playable Chapter 1 hoàn chỉnh**

---

## SLIDE 26 — CẢM ƠN / Q&A

**BIỆT THỰ BÓNG TỐI**

_Villa of Darkness_

_"Ngôi nhà không giết người — nó chỉ giữ họ lại."_
