# Biệt Thự Bóng Tối — Claude Project Memory

## Tổng quan dự án

- **Tên game:** Biệt Thự Bóng Tối (Villa of Darkness)
- **Engine:** Unity 6, URP, C#
- **Studio:** Fictional Station Studio
- **PM / Game Director:** Jok (`fictionalstation.studio@gmail.com`)
- **Assembly runtime:** `BietThuBongToi.Runtime`

## Quy trình Git

- **Jok làm việc thẳng trên `main`** — không tạo nhánh riêng
- Team tạo nhánh từ main, đặt tên theo pattern `phaseX/feature/<slug>`
- Sau khi xong: tạo PR hoặc merge về main

## Nhóm phát triển

| Người | Mảng | Nhánh Phase 2 |
|-------|------|---------------|
| Võ Văn Thuận | Gameplay / UI | `phase2/feature/gameplay-ch1` |
| Bùi Thành Tân | Audio / Sanity | `phase2/feature/audio-sanity-ch1` |
| Nguyễn Hữu Phúc | Triggers / Events | `phase2/feature/triggers-ch1` |
| Nguyễn Trường Vũ | Ambient / UI Panels | `phase2/feature/ambient-ch1` |

---

## Tiến độ Phase 1 — ✅ HOÀN THÀNH (merged vào main)

### Scripts đã có trên main

| Script | Người làm | Trạng thái |
|--------|-----------|-----------|
| `GameManager.cs` | Jok | ✅ |
| `AudioManager.cs` | Jok | ✅ |
| `PlayerController.cs` | Jok | ✅ |
| `InteractionSystem.cs` | Jok | ✅ |
| `FlashlightController.cs` | Jok | ✅ |
| `GameData.cs` | Jok | ✅ |
| `InventorySystem.cs` | Jok | ✅ |
| `DoorController.cs` | Thuận | ✅ |
| `HideSpot.cs` | Thuận | ✅ (cần hoàn chỉnh Phase 2) |
| `PianoInteractable.cs` | Thuận | ✅ (cần hoàn chỉnh Phase 2) |
| `SanitySystem.cs` | Tân | ✅ |
| `SanityPostProcess.cs` | Tân | ✅ (cần nối Volume Phase 2) |
| `AudioLogItem.cs` | Tân | ✅ |
| `TriggerZone.cs` | Phúc | ✅ |
| `GazeTrigger.cs` | Phúc | ✅ |
| `DelayEvent.cs` | Phúc | ✅ |
| `SpawnManager.cs` | Phúc | ✅ |
| `MainMenuUI.cs` | Vũ | ✅ |
| `PauseMenuUI.cs` | Vũ | ✅ (cần thêm ESC Phase 2) |
| `DeathScreenUI.cs` | Vũ | ✅ |
| `ChapterTransition.cs` | Vũ | ✅ (đã fix `_year` field) |
| `IInteractable.cs` | Jok | ✅ |

### Tests Phase 1

- `Assets/Tests/Phase1/Phase1.Tests.asmdef` — references: `BietThuBongToi.Runtime`
- Tests per người: `Phase1.VoVanThuan`, `Phase1.BuiThanhTan`, `Phase1.NguyenHuuPhuc`, `Phase1.NguyenTruongVu`

---

## Tiến độ Phase 2 — 🔄 ĐANG LÀM

### Scripts Jok đã tạo trên main

| Script | Path | Trạng thái |
|--------|------|-----------|
| `CutsceneController.cs` | `Assets/_Project/Scripts/System/` | ✅ Full implementation |
| `ItemPersistence.cs` | `Assets/_Project/Scripts/System/` | ✅ Full implementation |
| `InventoryUI.cs` | `Assets/_Project/Scripts/UI/` | ⚠️ Stub — Thuận implement |
| `AmbientZone.cs` | `Assets/_Project/Scripts/Audio/` | ⚠️ Stub — Vũ implement |

### Tests Phase 2

- `Assets/Tests/Phase2/Phase2.Tests.asmdef`
- `Assets/Tests/Phase2/VoVanThuan/InventoryUITests.cs` — namespace `Phase2.VoVanThuan` (12 tests)
- `Assets/Tests/Phase2/NguyenTruongVu/AmbientZoneTests.cs` — namespace `Phase2.NguyenTruongVu` (7 tests)

### Hotfixes đã apply (trên main)

- `AmbientZoneTests.cs` (Phase1 + Phase2): `_go.StartCoroutine` → `_zone.StartCoroutine`
- `AmbientZone.cs` + `DeathScreenUI.cs`: thêm `#pragma warning disable CS0414`

### Trạng thái từng người Phase 2

| Người | Việc chính | Trạng thái |
|-------|-----------|-----------|
| Thuận | `InventoryUI.cs` + hoàn chỉnh Piano/HideSpot | ⏳ Chưa bắt đầu |
| Tân | Nối SanityPostProcess ↔ URP Volume + AudioLog trong scene | ⏳ Chưa bắt đầu |
| Phúc | GazeTrigger → PlayerDead(), config TriggerZones trong scene | ⏳ Chưa bắt đầu |
| Vũ | `AmbientZone.cs` + Canvas hierarchy 4 panels + ESC PauseMenu | ⏳ Chưa bắt đầu |

---

## Cấu trúc thư mục quan trọng

```
Assets/
├── _Project/
│   └── Scripts/
│       ├── Audio/          AmbientZone.cs (stub)
│       ├── System/         CutsceneController.cs, ItemPersistence.cs, GameManager.cs...
│       └── UI/             InventoryUI.cs (stub), PauseMenuUI.cs, DeathScreenUI.cs...
└── Tests/
    ├── Phase1/             Phase1.Tests.asmdef + tests per người
    └── Phase2/             Phase2.Tests.asmdef + InventoryUITests, AmbientZoneTests
```

## Tài liệu tham chiếu

- `Phase2_TeamGuide.md` — hướng dẫn đầy đủ cho từng người (hierarchy, inspector, auto/manual test, Trello tasks)

---

## Conventions

- Namespace production scripts: **không có** (global namespace)
- Namespace test scripts: `Phase1.<TênViết>` hoặc `Phase2.<TênViết>`
- Private fields: `_camelCase`, serialized với `[SerializeField]`
- Events: `UnityEvent`, đặt tên `OnXxx`
- Stub methods: `throw new System.NotImplementedException()` + TODO comment
- Save system: `PlayerPrefs`, prefix key `VoD_`
- Tags: Player = `"Player"`
