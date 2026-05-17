# Phase 2 — Hướng Dẫn Nhóm Biệt Thự Bóng Tối

> **Dành cho:** Võ Văn Thuận · Bùi Thành Tân · Nguyễn Hữu Phúc · Nguyễn Trường Vũ  
> **Main branch đã có sẵn:** `CutsceneController.cs`, `ItemPersistence.cs`, `InventoryUI.cs` (stub), `AmbientZone.cs` (stub), `InventoryUITests.cs`, `AmbientZoneTests.cs`

---

## Quy trình chung

```bash
git checkout main
git pull
git checkout -b <tên-nhánh-của-bạn>
```

Commit và push khi xong:

```bash
git add .
git commit -m "feat(phase2): <mô tả ngắn>"
git push origin <tên-nhánh>
```

---

## 🎮 VÕ VĂN THUẬN

**Nhánh:** `phase2/feature/gameplay-ch1`

```bash
git checkout main && git pull && git checkout -b phase2/feature/gameplay-ch1
```

**Script cần làm:** `InventoryUI.cs` (stub đã có sẵn trên main — mở ra điền vào), hoàn chỉnh `PianoInteractable` (sound feedback + spawn ghost khi đúng sequence), hoàn chỉnh `HideSpot` (ghost AI không detect player khi đang ẩn).

---

### Setup TestScene — Hierarchy

```
TestScene
├── GameManager            [GameManager]
├── AudioManager           [AudioManager, AudioSource, AudioSource]
│                           _bgmSource = AudioSource thứ nhất
│                           _sfxSource = AudioSource thứ hai
├── Player                 [PlayerController, InteractionSystem]   tag = "Player"
│   └── Main Camera
│       └── Flashlight     [Light (Point/Spot), FlashlightController]
│                           _batteryLevel = 1, kéo Light vào field nếu có
├── Door                   [MeshRenderer (Cube scale 0.2×2×1), BoxCollider, DoorController]
├── Cabinet                [MeshRenderer (Cube scale 1×2×0.5), BoxCollider, HideSpot]
├── Piano                  [MeshRenderer (Cube scale 1.5×1×0.5), BoxCollider, PianoInteractable]
│                           _correctSequence = ["D","E","G","A","F"]
│                           OnSequenceComplete → nối Debug.Log object
├── InventorySystem_GO     [InventorySystem]
│                           OnItemAdded → nối Debug.Log object
└── Canvas                 [Canvas (Screen Space – Overlay), CanvasScaler, GraphicRaycaster]
    └── InventoryPanel     [RectTransform, Image, InventoryUI]
        │                   Anchor: center, Size: 420×320, Pivot: 0.5/0.5
        │                   Image color: #000000 alpha 200/255
        │                   _inventorySystem → drag InventorySystem_GO
        ├── Title           [TextMeshProUGUI]  text="HÀNH LÝ"  size=18  color=white
        │                   Anchor: top-center, Pos Y = –20
        ├── Grid            [RectTransform, GridLayoutGroup]
        │                   Anchor: stretch  Padding: 16px mọi phía
        │                   Cell Size: 88×88   Spacing: 8×8   Constraint: Fixed Columns = 2
        │   └── Slot_0..7  [RectTransform, Image (color #1A1A1A), Button]
        │                   Mỗi slot có 2 children:
        │                   ├── Icon  [Image]  size 56×56  anchor=center-top  color #444444 (placeholder)
        │                   └── Label [TextMeshProUGUI]  text=""  size=10  anchor=bottom  color=#CCCCCC
        └── HintText        [TextMeshProUGUI]  text="[TAB] Đóng"  size=11  color=#888888
                            Anchor: bottom-center, Pos Y = 12
```

### InventoryUI — việc cần làm trong từng hàm

- `Open()` → `_isOpen = true` + `gameObject.SetActive(true)` + `OnOpen.Invoke()` + `Refresh()`
- `Close()` → `_isOpen = false` + `gameObject.SetActive(false)` + `OnClose.Invoke()`
- `Toggle()` → `if (_isOpen) Close(); else Open();`
- `Refresh()` → lấy `_inventorySystem.GetAllItems()`, với mỗi slot: nếu có item thì set Label.text = itemId, Icon.color = trắng; nếu không thì xóa text, Icon.color = #444444
- `OnItemClicked(itemId)` → `AudioManager.Instance.PlaySFX(clip)` (clip lấy từ ScriptableObject hoặc tạm hardcode)
- `Update()` → bỏ comment dòng Tab toggle

### Auto test

Window → General → Test Runner → Edit Mode → chạy `Phase1.VoVanThuan` + `Phase2.VoVanThuan` → tất cả xanh.

### Manual test Play Mode

- Đến Door nhấn E → mở, nhấn E lại → đóng, Console log event
- Đến Cabinet nhấn E → player freeze tại chỗ (`PlayerController.enabled = false`), nhấn E → thoát
- Đến Piano nhấn E → nhập D E G A F qua keyboard → Console log "Piano done!" + ghost spawn
- Nhấn Tab → `InventoryPanel` hiện lên; dùng `InventorySystem_GO.AddItem("music_box")` qua context menu → Tab → slot hiện text "music_box"

---

## 🔊 BÙI THÀNH TÂN

**Nhánh:** `phase2/feature/audio-sanity-ch1`

```bash
git checkout main && git pull && git checkout -b phase2/feature/audio-sanity-ch1
```

**Script cần làm:** Hoàn chỉnh nối `SanityPostProcess` với URP Volume (visual thay đổi theo 3 mức sanity), gán `AudioLogItem` vào di vật trong TestScene. Không viết script mới.

---

### Setup TestScene — Hierarchy

```
TestScene
├── GameManager            [GameManager]
├── AudioManager           [AudioManager, AudioSource, AudioSource]
│                           _bgmSource = Source 1 (loop = true)
│                           _sfxSource = Source 2 (loop = false)
├── SanityManager          [SanitySystem, SanityPostProcess]
│                           SanitySystem.OnLevelChanged → SanityPostProcess hàm xử lý
├── Player                 [PlayerController, InteractionSystem]   tag = "Player"
│   └── Main Camera
│       └── GlobalVolume   [Volume – Mode: Global, Weight: 1]
│                           VolumeProfile: tạo profile mới, thêm:
│                           • Film Grain (Intensity 0 → 0.6)
│                           • Lens Distortion (Intensity 0 → –40)
│                           • Vignette (Intensity 0.2 → 0.6)
│                           Kéo Volume object vào SanityPostProcess._volume
├── AudioLog_Diary         [MeshRenderer (Cube 0.3×0.4×0.1), BoxCollider, AudioLogItem]
│                           _logClip = bất kỳ AudioClip nào (tạm: Project→Import audio ngắn)
│                           _logText = "Nhật ký bà Lan ngày 3/9/1963"
└── AudioLog_MusicBox      [MeshRenderer (Cube 0.2×0.2×0.2), BoxCollider, AudioLogItem]
                            _logClip = AudioClip thứ 2
                            _logText = "Hộp âm nhạc đồng"
```

### SanityPostProcess — việc cần kiểm tra

Script nhận `SanityLevel` từ `SanitySystem.OnLevelChanged` và điều chỉnh override các effect trên VolumeProfile:

| SanityLevel | Film Grain | Lens Distortion | Vignette |
|-------------|-----------|-----------------|----------|
| `High`      | 0         | 0               | 0.2      |
| `Medium`    | 0.2       | –10             | 0.3      |
| `Low`       | 0.45      | –25             | 0.45     |
| `Critical`  | 0.6       | –40             | 0.6      |

### Âm thanh cần set

- BGM: 1 file `.wav` hoặc `.mp3` loop (drone/ambient) — import vào `Assets/_Project/Audio/BGM/`
- SFX: 1 file ngắn (tiếng chạm vật, tiếng mở hộp) — import vào `Assets/_Project/Audio/SFX/`
- AudioLog clip: lời thoại ngắn hoặc text-to-speech tạm — import vào `Assets/_Project/Audio/Voice/`

### Auto test

Test Runner → Edit Mode → chạy `Phase1.BuiThanhTan` → tất cả xanh.

### Manual test Play Mode

- Gọi `SanityManager.GetComponent<SanitySystem>().DecreaseSanity(0.35f)` qua script debug → screen grain nhẹ
- Gọi thêm 0.35f → grain + distortion tăng rõ
- Gọi thêm 0.25f → Critical → visual méo mạnh, vignette đậm
- Đến `AudioLog_Diary` nhấn E → âm thanh phát 1 lần
- Nhấn E lần 2 → không phát lại
- Kiểm tra `GameData.audioLogsHeard` trong Inspector = 1 sau lần đầu

---

## ⚡ NGUYỄN HỮU PHÚC

**Nhánh:** `phase2/feature/triggers-ch1`

```bash
git checkout main && git pull && git checkout -b phase2/feature/triggers-ch1
```

**Script cần làm:** Hoàn chỉnh death trigger giếng (nối `GazeTrigger` vào death sequence CH1), đặt và config tất cả TriggerZone đúng vị trí trong TestScene. Không viết script mới.

---

### Setup TestScene — Hierarchy

```
TestScene
├── GameManager            [GameManager]
│                           PlayerDead() → nối DeathScreen hoặc Debug.Log
├── SpawnManager_GO        [SpawnManager]
├── Player                 [PlayerController, InteractionSystem]   tag = "Player"
│   └── Main Camera
│
├── --- TRIGGER ZONES ---
├── Zone_Entry             [BoxCollider (isTrigger=true), TriggerZone]
│                           Scale: 3×2×3 — đặt ở cửa vào
│                           _targetTag = "Player"
│                           _triggerOnce = false
│                           OnTriggered → SpawnManager_GO.SpawnAt()
│                           (kéo GhostCube prefab + SpawnPoint vào SpawnManager)
├── Zone_Delay             [BoxCollider (isTrigger=true), TriggerZone, DelayEvent]
│                           Scale: 2×2×2 — đặt giữa phòng
│                           TriggerZone.OnTriggered → DelayEvent.StartDelay()
│                           DelayEvent._delaySeconds = 3
│                           DelayEvent.OnDelayComplete → Debug.Log("Delay fired")
├── Zone_CancelDelay       [BoxCollider (isTrigger=true), TriggerZone]
│                           OnTriggered → DelayEvent.CancelDelay()
│                           (test: bước vào Delay zone rồi chạy ra Zone_Cancel trước 3s)
│
├── --- MIRROR / GIẾNG ---
├── Mirror_Surface         [Plane (scale 0.5×1×0.5), BoxCollider, GazeTrigger]
│                           Đặt đứng (rotate X=90) hoặc nằm ngang mô phỏng mặt giếng
│                           _gazeThreshold = 3
│                           OnGazeWarning → Debug.Log("⚠ Cảnh báo 1 giây")
│                           OnGazeComplete → GameManager.PlayerDead()
│
├── --- SPAWN ---
├── SpawnPoint             [Empty GameObject]  đặt cách Player 4m
└── GhostCube              [Prefab: Cube 1×2×1, màu đỏ]  lưu vào Assets/_Project/Prefabs/Ghosts/
```

### Lưu ý quan trọng — GazeTrigger

Script chạy raycast từ Main Camera mỗi frame. Để test đúng: `Mirror_Surface` phải có Collider, player phải nhìn thẳng vào mặt Collider đó từ khoảng cách ≤ 10m. Không cần trigger zone bao quanh — chỉ cần nhìn thẳng.

### Auto test

Test Runner → Edit Mode → chạy `Phase1.NguyenHuuPhuc` → tất cả xanh.

### Manual test Play Mode

- Bước qua `Zone_Entry` → GhostCube spawn tại SpawnPoint
- Bước vào `Zone_Delay`, đứng yên → sau 3 giây Console log "Delay fired"
- Bước vào `Zone_Delay` rồi chạy nhanh qua `Zone_CancelDelay` trước 3s → không có gì xảy ra
- Nhìn thẳng vào `Mirror_Surface` → sau 1s log cảnh báo → sau 3s `PlayerDead()` gọi
- Quay mặt đi trước 3s → không có gì

---

## 🎨 NGUYỄN TRƯỜNG VŨ

**Nhánh:** `phase2/feature/ambient-ch1`

```bash
git checkout main && git pull && git checkout -b phase2/feature/ambient-ch1
```

**Script cần làm:** Hoàn chỉnh `AmbientZone.cs` (FadeIn/FadeOut + OnTriggerEnter/Exit), thêm ESC toggle vào `PauseMenuUI.Update()`, tích hợp Canvas panels với CanvasGroup để show/hide đúng.

---

### Setup TestScene — Hierarchy

```
TestScene
├── GameManager            [GameManager]
├── Player                 [PlayerController]   tag = "Player"
│   └── Main Camera
│
├── Canvas                 [Canvas – Screen Space Overlay]
│   [CanvasScaler: Scale With Screen Size, Ref 1920×1080]
│   [GraphicRaycaster]
│   │
│   ├── MainMenu_Panel     [RectTransform, Image, CanvasGroup, MainMenuUI]
│   │   Anchor: stretch full  │  Image color: #000000 alpha 255
│   │   CanvasGroup: Alpha=1, Interactable=true, BlocksRaycasts=true
│   │   ├── Title          [TextMeshProUGUI]
│   │   │                   text = "BIỆT THỰ BÓNG TỐI"
│   │   │                   size = 52, color = #FFFFFF, style = Bold
│   │   │                   Anchor: top-center, Pos Y = –120
│   │   ├── Subtitle       [TextMeshProUGUI]
│   │   │                   text = "Villa of Darkness"
│   │   │                   size = 20, color = #888888
│   │   │                   Anchor: top-center, Pos Y = –180
│   │   ├── Btn_Start      [Button, Image]
│   │   │                   Size: 240×56  │  color: #FFFFFF  │  Anchor: center, Pos Y = 40
│   │   │                   └── Text [TextMeshProUGUI] "BẮT ĐẦU"  size=20  color=#000000
│   │   │                   OnClick → MainMenuUI.StartGame()
│   │   └── Btn_Quit       [Button, Image]
│   │                       Size: 240×56  │  color: #444444  │  Anchor: center, Pos Y = –20
│   │                       └── Text [TextMeshProUGUI] "THOÁT"  size=20  color=#FFFFFF
│   │                       OnClick → MainMenuUI.QuitGame()
│   │
│   ├── DeathScreen_Panel  [RectTransform, Image, CanvasGroup, DeathScreenUI]
│   │   Anchor: stretch full  │  Image color: #000000 alpha 255
│   │   CanvasGroup: Alpha=0, Interactable=false, BlocksRaycasts=false  ← ẩn lúc đầu
│   │   ├── CharName_Text  [TextMeshProUGUI]
│   │   │                   text = "TÊN NHÂN VẬT"  (placeholder)
│   │   │                   size = 36, color = #FFFFFF, style = Bold
│   │   │                   Anchor: center, Pos Y = 40
│   │   ├── Year_Text      [TextMeshProUGUI]
│   │   │                   text = "19XX – 20XX"  (placeholder)
│   │   │                   size = 20, color = #AAAAAA
│   │   │                   Anchor: center, Pos Y = 0
│   │   └── Btn_Retry      [Button, Image]
│   │                       Size: 200×48  │  color: #333333  │  Anchor: center, Pos Y = –80
│   │                       └── Text "THỬ LẠI"  size=18  color=#FFFFFF
│   │                       OnClick → DeathScreenUI.Retry()
│   │
│   ├── PauseMenu_Panel    [RectTransform, Image, CanvasGroup, PauseMenuUI]
│   │   Anchor: stretch full  │  Image color: #000000 alpha 160  ← semi-transparent
│   │   CanvasGroup: Alpha=0, Interactable=false, BlocksRaycasts=false  ← ẩn lúc đầu
│   │   └── Box            [RectTransform, Image]
│   │       Size: 320×200  │  color: #1A1A1A  │  Anchor: center
│   │       ├── Title      [TextMeshProUGUI] "TẠM DỪNG"  size=28  color=#FFFFFF
│   │       │               anchor=top-center, Pos Y = –20
│   │       ├── Btn_Resume [Button, Image]
│   │       │               Size: 200×48  │  color: #FFFFFF  │  Anchor: center, Pos Y = 20
│   │       │               └── Text "TIẾP TỤC"  size=18  color=#000000
│   │       │               OnClick → PauseMenuUI.Resume()
│   │       └── Btn_Quit   [Button, Image]
│   │                       Size: 200×48  │  color: #444444  │  Anchor: center, Pos Y = –36
│   │                       └── Text "THOÁT VỀ MENU"  size=18  color=#FFFFFF
│   │                       OnClick → GameManager.LoadMainMenu()
│   │
│   └── Transition_Panel   [RectTransform, Image, CanvasGroup, ChapterTransition]
│       Anchor: stretch full  │  Image color: #000000 alpha 255
│       CanvasGroup: Alpha=0, Interactable=false, BlocksRaycasts=false  ← ẩn lúc đầu
│       ├── Chapter_Text   [TextMeshProUGUI]
│       │                   text = "CHƯƠNG 1 — NĂM 2000"  (placeholder)
│       │                   size = 32, color = #FFFFFF, style = Bold
│       │                   Anchor: center, Pos Y = 20
│       └── Year_Sub       [TextMeshProUGUI]
│                           text = "Đêm Đầu Tiên"
│                           size = 18, color = #888888
│                           Anchor: center, Pos Y = –20
│
└── AmbientZone_Salon      [BoxCollider (isTrigger=true), AudioSource, AmbientZone]
                            BoxCollider Scale: 6×3×6  (đặt giữa phòng test)
                            AudioSource: Play On Awake=false, Loop=true, Volume=0
                            AudioClip: import file .wav ambient ngắn (gió/mưa/drone)
                            AmbientZone._targetVolume = 0.7
                            AmbientZone._fadeDuration = 1.5
                            AmbientZone._targetTag = "Player"
```

### PauseMenuUI — thêm vào `Update()`

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
}
```

### Show/Hide panels — cách làm với CanvasGroup

Trong `Open()` / `Show()` của từng script, thay vì `gameObject.SetActive(true)`, dùng:

```csharp
var cg = GetComponent<CanvasGroup>();
cg.alpha = 1;
cg.interactable = true;
cg.blocksRaycasts = true;
```

Trong `Close()` / `Hide()`:

```csharp
var cg = GetComponent<CanvasGroup>();
cg.alpha = 0;
cg.interactable = false;
cg.blocksRaycasts = false;
```

### Auto test

Test Runner → Edit Mode → chạy `Phase1.NguyenTruongVu` + `Phase2.NguyenTruongVu` → tất cả xanh.

### Manual test Play Mode

- Start game → thấy MainMenu_Panel với 2 nút
- Nhấn ESC → PauseMenu_Panel hiện mờ phía trên, game freeze (`Time.timeScale = 0`)
- Nhấn TIẾP TỤC hoặc ESC lại → resume
- Gọi `DeathScreenUI.Show("Minh Khoa", "1979 – 2000")` từ script test → DeathScreen_Panel hiện, text đúng tên và năm
- Nhấn THỬ LẠI → Console log event
- Bước Player vào `AmbientZone_Salon` → volume AudioSource tăng dần lên 0.7 trong ~1.5s
- Bước ra → volume giảm dần về 0

---

## 📋 Trello Tasks

Tạo các card sau, đặt vào cột **To Do**:

### Thuận

**`[P2] InventoryUI — Grid 2×4, Tab toggle, item click monologue`**
> Implement InventoryUI.cs: Open/Close/Toggle/Refresh/OnItemClicked. Setup Canvas Grid 2 cột 4 hàng trong TestScene. Auto test Phase2.VoVanThuan xanh. Manual: Tab mở/đóng, item hiện đúng slot.

**`[P2] Piano Complete — sound feedback + spawn ghost khi đúng sequence`**
> PianoInteractable.OnSequenceComplete nối SpawnManager. Thêm AudioManager.PlaySFX mỗi note đúng.

**`[P2] HideSpot Complete — ghost không detect player khi ẩn`**
> Khi `_playerIsHiding = true`, GhostAI không tính player là target. Test: vào tủ, ghost đi qua không chase.

### Tân

**`[P2] Sanity Visual — 3 mức grain/distortion theo SanityLevel`**
> Nối SanityPostProcess với URP Volume Profile. 4 level = 4 bộ giá trị Film Grain + Lens Distortion + Vignette. Manual test thay đổi sanity thấy visual thay đổi rõ.

**`[P2] AudioLog Integration CH1 — gán clip vào di vật TestScene`**
> Import 2 audio clip tạm, gán vào AudioLog_Diary và AudioLog_MusicBox. Verify audioLogsHeard đếm đúng.

### Phúc

**`[P2] Death Trigger Giếng — GazeTrigger 3s → PlayerDead()`**
> Đặt Mirror_Surface trong TestScene, nối GazeTrigger.OnGazeComplete → GameManager.PlayerDead(). Manual test nhìn 3s → chết, nhìn đi trước 3s → reset.

**`[P2] TriggerZone Integration CH1 — spawn ghost + delay event`**
> Đặt Zone_Entry, Zone_Delay, Zone_CancelDelay đúng vị trí TestScene. Config đủ như hướng dẫn. Manual test pass hết.

### Vũ

**`[P2] AmbientZone — FadeIn/FadeOut audio khi vào vùng`**
> Implement AmbientZone.cs. Setup trong TestScene với AudioClip loop. Manual: bước vào/ra zone nghe tiếng fade mượt.

**`[P2] Canvas UI Panels — build hierarchy MainMenu/Death/Pause/Transition`**
> Dựng đủ 4 panel trong Canvas theo đúng spec hierarchy. Nối tất cả Button.OnClick vào đúng hàm. CanvasGroup show/hide đúng.

**`[P2] PauseMenu ESC + Chapter Transition Integration`**
> Thêm ESC input vào PauseMenuUI.Update(). Nối CutsceneController.OnCutsceneEnd → ChapterTransition.PlayTransition(). Manual test ESC pause/resume.
