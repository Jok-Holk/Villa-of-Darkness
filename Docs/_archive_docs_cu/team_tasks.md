# Task Phase 2.7 — Hoàn thiện chi tiết Chapter1

## Quy trình làm việc

**QUAN TRỌNG — Không làm trực tiếp trên `Chapter1.unity`.**

Mỗi người tạo test scene riêng từ Chapter1 để tránh conflict:

1. Tạo nhánh mới từ main: `phase2.7/detail/<tên_nhánh>`
2. Duplicate scene: copy `Assets/_Project/Scenes/Chapter1.unity` → đặt tên `Chapter1_Test_<tên>.unity`
3. Làm việc trên scene test đó
4. Khi xong, export/ghi chép lại transform, component settings của từng object
5. Apply vào `Chapter1.unity` theo từng phần đã assign — không đụng khu vực của người khác
6. Commit + push nhánh, báo để review

---

## Tuấn Anh — Geometry & Static

**Scene test:** `Chapter1_Test_Geometry.unity`

- Kiểm tra mesh tường/sàn/trần từng phòng (Room_Kitchen, Room_FamilyRoom, Room_Study, Room_GuestRoom, Room_Linh, Room_Master, Room_Basement)
- Đặt cửa (Door_Interior FBX) vào đúng khung cửa mỗi phòng — xoay, scale cho vừa
- Mark geometry tĩnh: Inspector → Static → Lightmap Static + Occluder/Occludee
- Bake NavMesh: Window → AI → Navigation → Bake (agent radius 0.3, height 1.8)
- Bake Occlusion Culling: Window → Rendering → Occlusion Culling → Bake
- Kiểm tra 5 HideSpot (Closet, UnderStairs_GF, Basement_Corner, Wardrobe_Master, UnderBed_Linh) — player chui vào được không

---

## Thuận — Gameplay Props & Piano

**Scene test:** `Chapter1_Test_Gameplay.unity`

- Đặt Piano prefab đúng vị trí phòng khách, gán PianoInteractable + 7 PianoKey
- Gán SpawnManager: link Ghost spawn point, Player spawn point
- Đặt Prop_Candle_Brass, Prop_MusicBox_Cylinder, SheetMusic đúng vị trí
- Đặt Furn_Cabinet_Locked vào kho — gán ItemLock, set required key = Key_Skeleton
- Đặt PickupItem lên Prop_Key01_Skeleton — đặt ở phòng Linh
- Kiểm tra InventorySystem_GO và EventSystem còn trong scene
- Test: nhặt key → mở tủ → lấy item

---

## Tân — Audio & FBX Models

**Scene test:** `Chapter1_Test_Audio.unity`

- Resize và reposition 4 AmbientZone triggers (DiningRoom, Kho, Salon, SanSau) cho khớp geometry phòng thật
- Gán AudioClip vào từng AmbientZone
- Đặt AudioLog_Diary đúng vị trí phòng Linh, AudioLog_MusicBox cạnh piano
- Reassign mesh từ GLB → FBX tương ứng (Door_Interior.fbx, Fireplace_Stone.fbx, Prop_Piano.fbx, Cabinet.fbx, Sideboard_Bedside.fbx)
- Gán material đúng cho từng FBX
- RandomAmbientTrigger (11 phòng) — gán SFX clips vào `_sfxClips` array

---

## Phúc — Ghost AI & Triggers

**Scene test:** `Chapter1_Test_AI.unity`

- Điều chỉnh 5 waypoint (WP_Galerie_Sau, WP_HanhLang_D, WP_HanhLang_T, WP_Salon, WP_Veranda) đúng geometry — không xuyên tường
- Link waypoints vào GhostAI component (`_waypoints` array)
- Test GhostAI 4 state: Idle / Patrol / Chase / Search
- Đặt Zone_Entry, Zone_Delay, Zone_CancelDelay đúng vị trí
- Mirror: MirrorCamera (culling mask = MirrorOnly) + Mirror_Surface plane — test reflection
- GazeTrigger: hook OnGazeComplete → GameManager.PlayerDead() — test nhìn lâu sẽ die
- Test WellDeathSequence

---

## Vũ — UI & Lighting Detail

**Scene test:** `Chapter1_Test_UI.unity`

- Test DeathScreenUI: Retry (reload Chapter1) và GoMenu (load MainMenu)
- Test PauseMenuUI: ESC toggle, GoToMainMenu
- Test MainMenuUI: StartGame → load Chapter1
- Điều chỉnh point lights per-room:
  - Basement: cool blue, intensity ~0.3
  - Phòng Linh: warm amber yếu, flicker nếu có
  - Hành lang: gần tối, chỉ ánh lọt từ cửa phòng
  - Salon/phòng khách: candlelight ấm từ candle props
- Kiểm tra GlobalVolume post-process không quá nặng
- Thêm dust motes particle vào 1–2 phòng nếu có asset sẵn

---

## Checklist chung trước khi merge

- [ ] Không còn object tên generic (Cube, Cylinder, GameObject...)
- [ ] Tất cả collider đặt đúng, không overlap
- [ ] NavMesh bake xong, ghost không đi xuyên tường
