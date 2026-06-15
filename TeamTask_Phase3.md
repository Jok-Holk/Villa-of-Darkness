# Task Phase 2.7 — Chi tiết hoàn thiện Chapter1.unity

Scene chính: `Assets/_Project/Scenes/Chapter1.unity`
Deadline: trước khi merge vào main — mỗi người tạo nhánh `phase3/detail/<tên>` từ main.

---

## Tuấn Anh — Geometry & Static
**Khu vực: Toàn bộ cấu trúc phòng, sàn, trần, cửa**

- Kiểm tra và đặt đúng mesh tường/sàn/trần cho mỗi phòng (Room_Kitchen, Room_FamilyRoom, Room_Study, Room_GuestRoom, Room_Linh, Room_Master, Room_Basement)
- Đặt cửa (Door_Interior FBX) vào đúng khung cửa mỗi phòng, xoay và scale cho vừa
- Mark tất cả geometry tĩnh: Static → Lightmap Static + Occluder/Occludee Static
- Bake NavMesh: Window → AI → Navigation → Bake (agent radius 0.3, height 1.8)
- Bake Occlusion Culling: Window → Rendering → Occlusion Culling → Bake
- Thêm BoxCollider vào các tường/sàn thiếu collider
- Kiểm tra HideSpot 5 cái (Closet, UnderStairs_GF, Basement_Corner, Wardrobe_Master, UnderBed_Linh) — test xem player chui vào được không

---

## Võ Văn Thuận — Gameplay Props & Piano
**Khu vực: Phòng khách (piano), kho, phòng ăn**

- Đặt Piano prefab vào phòng khách đúng vị trí, gán PianoInteractable + 7 PianoKey
- Gán SpawnManager: link Ghost spawn point, link Player spawn point
- Đặt Prop_Candle_Brass, Prop_MusicBox_Cylinder, SheetMusic vào đúng phòng (xem scene cũ gameplay-ch1-test-scene để tham khảo vị trí)
- Đặt Furn_Cabinet_Locked vào kho — gán ItemLock component, set required key = Key_Skeleton
- Đặt PickupItem lên Prop_Key01_Skeleton (prefab đã có) — đặt ở phòng Linh
- Kiểm tra InventorySystem_GO và EventSystem còn trong scene
- Test flow: nhặt key → mở tủ → lấy item bên trong

---

## Bùi Thành Tân — Audio & FBX Models
**Khu vực: AmbientZones, AudioLogs, reassign FBX**

- Resize và reposition 4 AmbientZone triggers (DiningRoom, Kho, Salon, SanSau) cho khớp geometry phòng thật trong Chapter1
- Gán AudioClip vào từng AmbientZone (BGM ambient phù hợp từng phòng)
- Đặt AudioLog_Diary đúng vị trí phòng Linh, AudioLog_MusicBox cạnh đàn piano
- Reassign mesh: những object nào đang dùng GLB cũ → đổi sang FBX tương ứng trong Props/Architecture/ và Props/Furniture/
  - Door_Interior.fbx, Fireplace_Stone.fbx, Prop_Piano.fbx, Cabinet.fbx, Sideboard_Bedside.fbx
- Gán material đúng cho từng FBX (Mat_Wood_Teak, Mat_Iron_Railing, etc.)
- RandomAmbientTrigger (11 phòng) — gán SFX clips phù hợp vào `_sfxClips` array

---

## Nguyễn Hữu Phúc — Ghost AI & Triggers
**Khu vực: Ghost path, trigger zones, mirror**

- Điều chỉnh 5 waypoint (WP_Galerie_Sau, WP_HanhLang_D, WP_HanhLang_T, WP_Salon, WP_Veranda) cho đúng geometry phòng — tránh đi xuyên tường
- Link waypoints vào GhostAI component (`_waypoints` array)
- Test GhostAI 4 state (Idle/Patrol/Chase/Search) — đảm bảo ghost không bị kẹt
- Đặt Zone_Entry, Zone_Delay, Zone_CancelDelay đúng vị trí (cửa chính, hành lang, basement)
- Mirror: MirrorCamera (culling mask = MirrorOnly layer) + Mirror_Surface plane trong phòng giếng — test reflection hiển thị đúng
- GazeTrigger hook OnGazeComplete → GameManager.PlayerDead() — test nhìn quá lâu sẽ die
- Test WellDeathSequence sequence hoạt động

---

## Nguyễn Trường Vũ — UI & Lighting Detail
**Khu vực: UI screens, ambient lighting per-room**

- DeathScreenUI: test Retry (reload Chapter1) và GoMenu (load MainMenu) hoạt động
- PauseMenuUI: test ESC toggle, GoToMainMenu button
- MainMenuUI: test StartGame → load Chapter1 (đã fix)
- Điều chỉnh point lights trong từng phòng cho đúng mood:
  - Basement: cool blue, intensity thấp ~0.3
  - Phòng Linh: warm amber yếu, flicker nếu có thể (Light.enabled toggle coroutine)
  - Hành lang: gần như tối, chỉ ánh lọt từ cửa phòng
  - Phòng khách/salon: candlelight ấm từ candle props
- Kiểm tra GlobalVolume post-process (Bloom/Vignette/FilmGrain) không quá nặng tay
- Thêm particle nhỏ (dust motes) vào 1-2 phòng nếu có sẵn asset

---

## Ghi chú chung
- Mỗi người tạo nhánh `phase2.7/detail/<username>` từ main
- Không sửa file của người khác nếu không cần thiết
- Merge về main khi xong — Jok review và merge
- Scene save thường xuyên (Ctrl+S) trước khi push
