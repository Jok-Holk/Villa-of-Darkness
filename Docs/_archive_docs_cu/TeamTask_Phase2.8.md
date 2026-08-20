# TeamTask — Phase 2.8 · Biệt Thự Bóng Tối

---

## ⚠️ ĐỌC TRƯỚC KHI LÀM

> **NHIỀU MODEL TRONG SCENE CHƯA ĐƯỢC ĐẶT ĐÚNG VỊ TRÍ hoặc CHƯA CÓ VỊ TRÍ.**
> Trước khi hỏi bất kỳ điều gì — **mở `Chapter1.unity`, kéo camera đi khắp nơi và tự quan sát.**
> Thấy chỗ nào trống hoặc thiếu prop → **tự tìm thêm asset** (xem mục Asset bên dưới).
> Không biết model nào ở đâu → lọc Hierarchy theo tên, bật Wireframe, dùng Frame Selected (F).

### Nguyên tắc tìm asset

- Nguồn được phép: **Fab.com**, **Unity Asset Store**, **PolyHaven**, **KennyNL**
- License bắt buộc: **CC0** hoặc ghi rõ **"Free for commercial use"**
- Import GLB/FBX/PNG trực tiếp vào folder tương ứng trong `Assets/_Project/`
- Hợp theme tối, hoang phế, Đông Dương → dùng. Không cần xin phép nếu license OK.

---

## Phân công khu vực

| Người        | Scene file                                                      | Khu vực                                                |
| ------------ | --------------------------------------------------------------- | ------------------------------------------------------ |
| **Tuấn Anh** | `Chapter1_Zone_Struct.unity`                                    | Kết cấu, cửa, navmesh, occlusion, ngoại thất tường/mái |
| **Thuận**    | `Chapter1_Zone_GF_Decor.unity`                                  | Tầng trệt nội thất · Vùng đất ngoài hàng rào           |
| **Tân**      | `Chapter1_Zone_L2_Decor.unity` + `Chapter1_Zone_Exterior.unity` | Tầng 2 · Sân vườn trong hàng rào · Ambient/Audio       |
| **Phúc**     | `Chapter1_Zone_L1_Decor.unity`                                  | Tầng 1 · Ghost AI · Mirror · Triggers                  |
| **Vũ**       | `Chapter1_Zone_UI_FX.unity`                                     | Ánh sáng · VFX · UI · Test                             |

---

## Yêu cầu chung (tất cả mọi người)

### Hierarchy — BẮT BUỘC tổ chức lại

Trong zone của mình, tất cả object phải được gom vào Empty GameObject đặt tên rõ ràng:

```
=== ARCHITECTURE ===       ← tường, sàn, trần, cầu thang
=== INTERIOR_GF ===        ← nội thất tầng trệt
=== INTERIOR_1F ===        ← nội thất tầng 1
=== INTERIOR_2F ===        ← nội thất tầng 2
=== EXTERIOR ===           ← sân vườn trong hàng rào
=== LANDSCAPE ===          ← cây cối, bụi rậm trong hàng rào
=== LANDSCAPE_OUTER ===    ← vùng đất ngoài hàng rào
=== SYSTEMS ===            ← Player, Ghost, Camera, NavMesh
=== LIGHTING ===           ← đèn, reflection probe, volume
=== VFX ===                ← particle, smoke, dust
=== AUDIO ===              ← ambient zones, audio sources
```

Không được để object tên generic (`GameObject`, `Cube (1)`, `Sphere`). Phải đặt tên rõ ràng theo quy ước `Furn_Tên`, `Prop_Tên`, `Arch_Tên`, `PH_Tên`.

---

## Chi tiết task từng người

### Tuấn Anh — Kết cấu + Ngoại thất

**Cấu trúc:**

- Doors: BoxCollider + AudioSource (tiếng cọt kẹt) + animation mở nếu kịp
- Đảm bảo tường/sàn/trần không lỗ collider, player không rơi xuyên
- HideSpots: đặt 4 điểm — gầm bàn salon, góc bếp, phòng kho, sau cửa phòng ngủ 1F

**Ngoại thất tường + mái:**

- Rêu/dây leo sát chân tường, cao ≤1.5m (prop tự tìm CC0)
- Nóc nhà: vài điểm ngói vỡ, rêu mái
- Kiểm tra lan can balcony khớp geometry

**Bake (làm SAU khi toàn bộ team đặt xong furniture):**

- NavMesh: `Window → AI → Navigation → Bake`
- Occlusion: `Window → Rendering → Lighting → Occlusion Culling → Bake`

---

### Thuận — Tầng trệt + Ngoài hàng rào

**Tầng trệt nội thất:**

- Salon: sofa, bàn cà phê, nến, khung tranh, thảm → dùng PH_Sofa_01, PH_ThrowPillows, PH_FancyFrame_01/02 có sẵn
- Study: bàn làm việc, ghế, sách, đèn bàn, giấy tờ → PH_DecorBooks_01, PH_Encyclopedia_01, PH_OilLamp
- Bếp: dụng cụ bếp, thùng đựng nước, lò cũ (tự tìm thêm nếu thiếu)
- Đảm bảo PickupItem + ItemLock đã set đúng ItemData, test pickup hoạt động
- **Piano:** làm phần base đơn giản (đặt đúng vị trí, đúng collider). Hệ thống gameplay piano sẽ được rework sau — không cần làm sâu.

**Ngoài hàng rào (biệt lập):**

- Vùng đất phía ngoài cổng/hàng rào: cây hoang, bụi rậm cao, cỏ dại um tùm
- Không cần walkable, chỉ cần visual depth tạo cảm giác biệt lập
- Gom vào `=== LANDSCAPE_OUTER ===`

---

### Tân — Tầng 2 + Sân vườn + Ambient

**Tầng 2:**

- Phòng ngủ: giường, tủ, gương cũ, hộp nhạc, ảnh ố vàng → PH_HangingFrame, PH_MarbleBust_01
- Hành lang 2F: tối, tường ố, đồ vật rơi trên sàn
- AudioLog: đặt tại 2-3 điểm player đi qua (`AudioLogItem` component)
- AmbientZone: resize collider khớp phạm vi từng phòng
- ⚠️ Xóa `DebugSanity.cs` trước khi merge
- ⚠️ `SanityShake.cs` phải gắn trên **Camera con** trong Player, KHÔNG phải Player root

**Sân vườn trong hàng rào:**

- Driveway: cây 2 bên cân đối, lá khô rải dọc lối vào
- Sân sau: cỏ dại, đá dăm, cụm bụi quanh giếng
- Gom vào `=== EXTERIOR ===` và `=== LANDSCAPE ===`

---

### Phúc — Tầng 1 + Ghost + Mirror + Triggers

**Tầng 1 nội thất:**

- Phòng ngủ chính: đồ dùng cá nhân cũ → PH_VintageSuitcase, PH_HorseStatue_01
- Phòng kho: hộp gỗ chồng, vải phủ furniture cũ
- Tự tìm thêm nếu thiếu: crate, dust sheet, moth-eaten fabric — CC0

**Ghost AI:**

- Cập nhật waypoints theo tọa độ mới (Player tại world origin)
- Kiểm tra patrol đúng phòng, không xuyên tường
- `GazeTrigger.OnGazeComplete` → `GameManager.PlayerDead()`

**Mirror:**

- Tạo layer `MirrorOnly`
- Camera gương: `cullingMask = MirrorOnly` → render RenderTexture 512×512 → assign vào plane material
- Ghost + Player character → layer `MirrorOnly`

**Triggers:**

- Mỗi phòng 1F có ít nhất 1 scare trigger (TriggerZone + event)
- Tự thêm thủ công nếu thấy thiếu

---

### Vũ — Ánh sáng + VFX + UI

**Ánh sáng per-room (Baked):**

| Phòng                   | Màu               | Intensity | Range |
| ----------------------- | ----------------- | --------- | ----- |
| Salon, Study, Phòng ngủ | `#FFB347` amber   | 0.35      | 4m    |
| Bếp                     | `#FF8C00` cam tối | 0.4       | 3m    |
| Hành lang               | Không đèn riêng   | —         | —     |
| Ngoại thất              | Moonlight đã có   | —         | —     |

- Tắt real-time shadow trên tất cả Point Light → dùng baked shadow

**VFX:**

- Bụi lơ lửng trong vệt sáng: Particle System slow, sparse
- Nến: flicker trên Point Light (random intensity ±0.05 theo thời gian)
- Tự tìm thêm: cobweb mesh, local fog patch, candle smoke — CC0

**UI + Test:**

- Confirm PauseMenu, DeathScreen, MainMenu transition hoạt động đúng
- **Test end-to-end bắt buộc:** Start → Chapter1 load → chơi → chết → DeathScreen → Retry → Chapter1 lại → ESC pause → về MainMenu

---

## Git workflow

```
# Mỗi người tạo nhánh từ main:
git checkout -b phase2.8/detail/<username>

# Commit thường xuyên, message rõ ràng:
git commit -m "feat(1F): thêm furniture phòng ngủ chính + HideSpot"

# Khi xong push lên:
git push origin phase2.8/detail/<username>

# Báo Jok review → merge về main
```

**Không push thẳng lên main.**

---

## Checklist trước khi báo xong

- [ ] Hierarchy trong zone của mình đã tổ chức theo nhóm `=== ... ===`
- [ ] Không có object tên generic (`GameObject`, `Cube (1)`)
- [ ] Đã test gameplay trong khu vực của mình (pickup, trigger, AI, UI...)
- [ ] Không có Console Error liên quan đến code của mình
- [ ] Scene đã Ctrl+S trước khi push
