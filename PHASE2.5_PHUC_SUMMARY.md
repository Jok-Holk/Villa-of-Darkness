# Phase 2.5 Implementation Summary - Nguyễn Hữu Phúc

## ✅ Hoàn Thành: Tất Cả 3 Phần

---

## 1️⃣ GhostAI.cs - Phương Thức SetAlertMode()

**File**: `Assets/_Project/Scripts/AI/GhostAI.cs`

**Thay Đổi**:
- Thêm 3 field riêng tư:
  ```csharp
  private float _patrolSpeedOriginal;
  private float _hearingRadiusOriginal;
  private bool _isAlerted = false;
  ```

- Cập nhật `Awake()` lưu giá trị ban đầu:
  ```csharp
  _patrolSpeedOriginal = _patrolSpeed;
  _hearingRadiusOriginal = _hearingRadius;
  ```

- Thêm phương thức công khai:
  ```csharp
  public void SetAlertMode()
  {
      if (_isAlerted) return; // Chỉ alert 1 lần
      _isAlerted = true;
      _patrolSpeed = _patrolSpeedOriginal * 1.1f;      // +10% tốc độ
      _hearingRadius = _hearingRadiusOriginal * 1.25f; // +25% bán kính
  }
  ```

**Kết Nối Inspector**:
- Select **PianoInteractable** GameObject
- Tìm event **OnSequenceComplete** → **+** listener
- Assign **Ghost** object
- Dropdown: `GhostAI.SetAlertMode()`

---

## 2️⃣ WellDeathSequence.cs - Script Mới

**File**: `Assets/_Project/Scripts/Gameplay/WellDeathSequence.cs` ✅ (Đã tạo)

**Chức Năng**:
- Gắn vào GameObject **Well** (hay trigger object tại giếng)
- Lắng nghe event `GazeTrigger.OnGazeComplete` khi nhìn giếng > 3 giây
- **Chuỗi** kích hoạt:
  1. Tắt input player (`PlayerController.DisableInput()`)
  2. Phát âm thanh Ma Da (voice line)
  3. Áp overlay xanh (fade in 1.5s)
  4. Fade màn hình đen (1s)
  5. Hiển thị death screen: **"Minh Khoa"** và **"1979 – 2000"**

**Inspector Fields**:
- `DeathScreenUI` → Assign DeathScreenUI
- `GazeTrigger` → Assign GazeTrigger component từ Well
- `PlayerController` → Assign PlayerController
- `Ma Da Voice Clip` → Audio clip giọng ghost
- `Overlay Fade Duration` → 1.5s (mặc định)
- `Screen Fade Duration` → 1s (mặc định)

---

## 3️⃣ Waypoint Setup - Ch.1 Ghost Patrol

**Hành Động**: Tạo 5 waypoint GameObject + wire vào GhostAI

**5 Waypoint Cần Tạo**:

| Tên | Vị Trí | Y | Mục Đích |
|-----|--------|---|---------|
| `WP_HanhLang_T` | Đầu hành lang Tây | ~1.0 | Patrol point |
| `WP_HanhLang_D` | Đầu hành lang Đông | ~1.0 | Patrol point |
| `WP_Salon` | Góc NW salon | ~1.0 | Gần piano |
| `WP_Veranda` | Hiên phía sau | ~1.0 | Gần cửa |
| `WP_Galerie_Sau` | Galerie sau | ~1.0 | Corridor |

**Các Bước**:
1. Create 5 Empty GameObjects với tên trên
2. Đặt vị trí (X, Y=1.0, Z) theo layout scene
3. Assign vào GhostAI._waypoints[] (size=5) trong Inspector
4. Kiểm tra NavMesh (waypoints phải trên vùng xanh)

**Hành Vi Sau Thiết Lập**:
- Ghost chọn waypoint ngẫu nhiên, đi đến, đợi 1-2s
- Lặp lại → ~60s/loop
- Chuyển thành Chase nếu nghe/nhìn thấy player
- Sau piano: tốc độ +10%, bán kính nghe +25%

---

## 📋 Checklist Hoàn Thành

- [x] Sửa GhostAI.cs thêm `SetAlertMode()`
- [x] Tạo WellDeathSequence.cs
- [x] Tạo hướng dẫn waypoint setup
- [ ] Tạo 5 waypoint Objects trong scene (By Phúc)
- [ ] Assign waypoints vào GhostAI._waypoints[] (By Phúc)
- [ ] Wire PianoInteractable.OnSequenceComplete → GhostAI.SetAlertMode() (By Phúc)
- [ ] Gắn WellDeathSequence vào Well GameObject (By Phúc)
- [ ] Wire GazeTrigger.OnGazeComplete → WellDeathSequence (By Phúc)
- [ ] Test: Ghost patrol → Piano solved → Death at well

---

## 🧪 Test Scenarios

### Test 1: Ghost Patrol
- [ ] Enter Ch.1 scene
- [ ] Ghost di chuyển qua 5 waypoints (ngẫu nhiên)
- [ ] Không bị stuck ở 1 waypoint

### Test 2: Piano Alert
- [ ] Đi đến piano, giải sequence D-E-G-A-F
- [ ] Ghost tăng tốc độ (~10%)
- [ ] Ghost bán kính nghe lớn hơn (debug: check console log)

### Test 3: Well Death
- [ ] Đi đến giếng, nhìn vào liên tục
- [ ] Sau 3s: Input disable, overlay xanh, fade đen
- [ ] Death screen hiển thị: **Minh Khoa** | **1979 – 2000**

---

## 📁 File Tham Khảo

- Waypoint Guide: `WAYPOINT_SETUP_GUIDE.md` (hướng dẫn chi tiết)
- Script Ghost: `Assets/_Project/Scripts/AI/GhostAI.cs`
- Script Death: `Assets/_Project/Scripts/Gameplay/WellDeathSequence.cs`
- Script Piano: `Assets/_Project/Scripts/Gameplay/PianoInteractable.cs`
- Script Gaze: `Assets/_Project/Scripts/Gameplay/GazeTrigger.cs`

---

**Hoàn Thành Phase 2.5 cho Nguyễn Hữu Phúc ✅**
