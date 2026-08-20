# TeamTask Phase 3 — Biệt Thự Bóng Tối

> Cập nhật: 2026-06-18 | PM: Jok

---

## Luồng làm việc mới (Additive Scenes)

### Tại sao đổi sang Additive Scenes?

Trước: 5 người cùng sửa `Chapter1.unity` → merge conflict liên tục, token tốn nhiều.

Mới: Mỗi người có scene riêng, scene master chỉ load additive.

```
Chapter1.unity              ← MASTER (chỉ Jok + PM touch)
  ↳ Chapter1_Zone_Struct       ← Tuấn Anh: shell building
  ↳ Chapter1_Zone_GF_Decor     ← Thuận: props tầng trệt
  ↳ Chapter1_Zone_L1_Decor     ← Tân: props tầng 1 + ambient
  ↳ Chapter1_Zone_L2_Decor     ← Phúc: props tầng 2 + ghost
  ↳ Chapter1_Zone_Exterior     ← Tuấn Anh: vườn, hàng rào, allee
  ↳ Chapter1_Zone_UI_FX        ← Vũ: lighting, particles, post-process
```

### Cách làm việc

**Mở scene của mình:**
```
File → Open Scene → Scenes/Zones/Chapter1_Zone_GF_Decor.unity
                                               (thay tên tương ứng)
```

**Load thêm master để xem bối cảnh:**
```
File → Open Scene (Additive) → Scenes/Chapter1.unity
File → Open Scene (Additive) → Scenes/Zones/Chapter1_Zone_Struct.unity
```

**Chỉ save scene của mình** — đừng save scene người khác.

**Git commit theo zone:**
```
git add Assets/_Project/Scenes/Zones/Chapter1_Zone_GF_Decor.unity
git commit -m "feat(ch1-gf): add dining room props"
```

---

## Phân công chi tiết Phase 3

### Tuấn Anh (Geometry + Exterior)
**Scene:** `Chapter1_Zone_Struct` + `Chapter1_Zone_Exterior`

| Task | Chi tiết | Done? |
|------|----------|-------|
| NavMesh bake | Sau khi đặt hết furniture, Window > AI > Navigation → Bake | ☐ |
| Occlusion bake | Window > Rendering > Occlusion Culling → Bake | ☐ |
| HideSpot colliders | Gắn `HideSpot.cs` lên tủ, giường, gậm bàn | ☐ |
| Cửa kép entrance | 2 cánh cửa gỗ teak tại X=21, Y=-0.7 đến 0.7 | ☐ |
| Cổng sắt | GatePillar_L/R đã có — thêm 2 cánh cổng sắt | ☐ |
| Exterior light posts | Lamp posts dọc allee — assign `Mat_Iron_Fence` | ☐ |

### Thuận (Gameplay Props — GF)
**Scene:** `Chapter1_Zone_GF_Decor`

| Task | Chi tiết | Done? |
|------|----------|-------|
| Piano puzzle | `PianoInteractable.cs` → Prefab/Interactables/Piano | ☐ |
| PickupItem props | Assign `PickupItem.cs` + `ItemData` SO cho từng vật | ☐ |
| ItemLock | Cửa kho khóa cần chìa khóa | ☐ |
| ExamineItem | Thư từ, ảnh cũ ở bàn ăn + kệ sách | ☐ |
| FlashlightData | SO cho đèn pin (bắt đầu tắt) | ☐ |
| HideSpot GF | Gầm bàn ăn, tủ bếp | ☐ |

### Tân (Audio + L1 Props)
**Scene:** `Chapter1_Zone_L1_Decor`

| Task | Chi tiết | Done? |
|------|----------|-------|
| AmbientZone resize | Resize trigger colliders khớp phòng thực tế | ☐ |
| AudioLog placement | Đặt `AudioLogItem.cs` props (máy hát, điện thoại cũ) | ☐ |
| RandomAmbientTrigger | Trigger âm thanh lạ ở hành lang L1 | ☐ |
| FBX reassign | Relink các FBX model của Tân đã export (10 cái) | ☐ |
| GhostProximitySanity | Verify radius sphere colliders đúng tầm | ☐ |

### Phúc (Ghost + Triggers — L2)
**Scene:** `Chapter1_Zone_L2_Decor`

| Task | Chi tiết | Done? |
|------|----------|-------|
| Ghost waypoints | Đặt waypoint chain: L2 corridor → staircase → GF | ☐ |
| TriggerZone L2 | Trigger âm thanh khi bước vào phòng L2 | ☐ |
| GazeTrigger | Hook `OnGazeComplete` → `GameManager.PlayerDead()` | ☐ |
| Mirror setup | MirrorCamera culling mask = MirrorOnly → RenderTexture → plane | ☐ |
| GhostAI patrol | FOV 120°, assign waypoint list | ☐ |

### Vũ (UI + Lighting)
**Scene:** `Chapter1_Zone_UI_FX`

| Task | Chi tiết | Done? |
|------|----------|-------|
| Per-room lighting | Point light màu nến cho từng phòng | ☐ |
| Dust particles | Particle System → nhẹ, PSX style | ☐ |
| Post-process profile | Vignette + Color Grading (horror) | ☐ |
| UI test pass | DeathScreen + PauseMenu test end-to-end | ☐ |
| Ambient sound mood | Mỗi phòng 1 AmbientZone khác nhau | ☐ |

---

## Kỹ thuật còn lại (Jok/mọi người)

- [ ] `SanityShake.cs` gắn lên **Camera con** (không phải PlayerController)
- [ ] `InventoryTabHandler.cs` gắn lên **Player** GameObject
- [ ] **XÓA `DebugSanity.cs`** trước khi build release
- [ ] Test end-to-end: MainMenu → Chapter1 → die → retry

---

## Quy tắc Git cho multi-scene

```bash
# Branch naming
phaseX/zone/<zone>/<username>
# Ví dụ:
phase3/zone/gf-decor/tvo04086

# Commit prefix
feat(ch1-gf): ...       # Ground floor
feat(ch1-l1): ...       # First floor  
feat(ch1-l2): ...       # Second floor
feat(ch1-ext): ...      # Exterior
feat(ch1-fx): ...       # FX/lighting

# KHÔNG commit vào Chapter1.unity trừ khi Jok cho phép
```

---

## Materials đã có trong Unity

| Material | Dùng cho |
|----------|----------|
| `Mat_Wall_Exterior_Ochre` | Tường ngoài vàng ochre |
| `Mat_Wall_Interior_Cream` | Tường trong màu kem |
| `Mat_Roof_TerraCotta` | Mái ngói đỏ |
| `Mat_Floor_CementTile` | Sàn xi măng / tile cũ |
| `Mat_Floor_Teak` | Sàn gỗ teak |
| `Mat_Iron_Fence` | Hàng rào sắt, lan can |
| `Mat_Wall_Stone` | Đá xây — chimney, cột |
| `Mat_Cornice_White` | Phào chỉ trắng |
| `Mat_Jalousie_Green` | Cửa chớp xanh lá |
| `Mat_Garden_Grass` | Cỏ |
| `Mat_Garden_Gravel` | Sỏi vườn |
| `Mat_Palm_Trunk` | Thân cây |
| `Mat_Wood_Furniture` | Đồ gỗ nội thất |

---

## GLB đã export

`Assets/_Project/Models/Architecture/Chapter1_WithHipRoof.glb` — 28.6 MB

Chạy **`VoD → 1 – Setup Chapter1 Scene`** trong Unity để:
1. Import GLB vào Chapter1.unity
2. Tự động gán materials theo bảng trên
3. Tổ chức hierarchy theo group
4. Tạo 6 zone scenes trong `Scenes/Zones/`
