# Hướng Dẫn Thiết Lập Waypoint Cho Ch.1 (Nguyễn Hữu Phúc)

## Tổng Quan
Ghost sẽ tuần tra qua 5 điểm (waypoint) ở tầng trệt. Mỗi loop khoảng 60 giây, ghost chọn ngẫu nhiên waypoint tiếp theo thay vì đi tuần tự.

## Danh Sách 5 Waypoint

| # | Tên GameObject | Vị Trí Đích | Mô Tả |
|---|---|---|---|
| 1 | `WP_HanhLang_T` | Tây (West) | Cuối hành lang phía Tây tầng trệt |
| 2 | `WP_HanhLang_D` | Đông (East) | Cuối hành lang phía Đông tầng trệt |
| 3 | `WP_Salon` | NW corner | Góc Tây Bắc - Salon/Phòng nhạc (nơi piano) |
| 4 | `WP_Veranda` | Phía sau | Hiên/Veranda - cửa kính ra sân phía sau |
| 5 | `WP_Galerie_Sau` | Giữa | Giữa galerie phía sau |

---

## Hướng Dẫn Thực Hiện (Unity Editor)

### Bước 1: Mở Scene Ch.1
1. Mở **Project** → `Assets/_Project/Scenes/`
2. Tìm và mở scene **Chapter1** hoặc **Ch1** (kiểm tra chính xác tên)
3. Ensure Ghost đã có NavMeshAgent component trên scene

### Bước 2: Tạo Waypoint Objects
1. **Right-click** vào Hierarchy → **Create Empty** (tạo 5 object trống)
   - Hoặc: **Ctrl+Shift+N** → Đặt tên lần lượt:
     - `WP_HanhLang_T`
     - `WP_HanhLang_D`
     - `WP_Salon`
     - `WP_Veranda`
     - `WP_Galerie_Sau`

2. **Optional**: Ghi đè parent thành `Waypoints` hoặc `Landmarks` để keep hierarchy sạch

### Bước 3: Đặt Vị Trí Waypoint

**Sử dụng Transform Position trong Inspector:**

```
WP_HanhLang_T  Position: (-28, 1, 12)  // Đầu hành lang phía Tây
WP_HanhLang_D  Position: (28, 1, 12)   // Đầu hành lang phía Đông
WP_Salon       Position: (-22, 1, -8)  // Góc NW salon/phòng nhạc
WP_Veranda     Position: (0, 1, -25)   // Cửa kính ra sân phía sau
WP_Galerie_Sau Position: (5, 1, -15)   // Giữa galerie phía sau
```

**Ghi chú:**
- **Y = 1** (hoặc ~0.5-1.5): Cao độ của sàn tầng trệt (không floating trên trần hoặc trong lòng đất)
- Điều chỉnh X, Z dựa trên layout cụ thể của scene
- Dùng **Scene View** để xác định vị trí chính xác bằng mắt

### Bước 4: Assign Waypoints vào Ghost Inspector

1. **Select** GameObject Ghost (hoặc **Ghost_Ch1**)
2. **Inspector** → GhostAI script section
3. Tìm field **_waypoints** (hoặc `Waypoints`)
4. Set **Size = 5**
5. Drag từng waypoint vào thứ tự:
   - `[0]` → `WP_HanhLang_T`
   - `[1]` → `WP_HanhLang_D`
   - `[2]` → `WP_Salon`
   - `[3]` → `WP_Veranda`
   - `[4]` → `WP_Galerie_Sau`

### Bước 5: Kiểm Tra NavMesh
- **Shift+Click** tạo waypoint để xem NavMesh (xanh = walkable)
- Ensure tất cả 5 waypoint đều nằm trên vùng xanh NavMesh
- Nếu có waypoint nằm ngoài NavMesh → dịch chuyển cho phù hợp

---

## Kết Nối Piano → Ghost Alert

**Chỉ làm sau khi hoàn thành bước 4:**

1. **Select** GameObject PianoInteractable
2. **Inspector** → PianoInteractable script
3. Tìm event **OnSequenceComplete**
4. **Click +** để thêm listener
5. **Drag Ghost** vào field
6. **Dropdown method** → **GhostAI → SetAlertMode() (void)**
7. **Save scene**

---

## Kỳ Vọng Hành Vi

### Sau Thiết Lập Đúng:

✅ **Trước piano:**
- Ghost đi tuần tra từ waypoint này sang waypoint khác (ngẫu nhiên)
- Chọn waypoint mới mỗi 12-15 giây
- Nhìn thấy/nghe player → chuyển sang Chase/Kill

✅ **Sau piano hoàn thành:**
- Ghost chạy nhanh hơn 10%
- Bán kính nghe tăng ~25%
- Vẫn tuần tra nhưng khó tránh hơn

✅ **Khi nhìn vào giếng > 3s:**
- Input disable
- Chạy death sequence
- Hiện death screen "Minh Khoa 1979 – 2000"

---

## Gỡ Lỗi

| Vấn Đề | Nguyên Nhân | Cách Sửa |
|---|---|---|
| Ghost không di chuyển | Waypoints trống hoặc _waypoints size = 0 | Check Inspector, assign đầy đủ 5 waypoint |
| Ghost bay lơ lửng | Y position quá cao | Điều chỉnh Y ≈ 1 (sàn level) |
| Ghost chìm dưới đất | Y position quá thấp hoặc âm | Điều chỉnh Y ≈ 1 |
| Ghost stuck tại 1 waypoint | Waypoint nằm ngoài NavMesh | Dịch waypoint vào vùng xanh hoặc bake NavMesh lại |
| SetAlertMode() không chạy | PianoInteractable event chưa wire | Kết nối Piano → Ghost.SetAlertMode() |
| Death sequence không play | GazeTrigger chưa wire hoặc DeathScreenUI null | Check GazeTrigger → WellDeathSequence.OnWellGazeComplete |

---

## Tệp Liên Quan
- **Script Ghost**: `Assets/_Project/Scripts/AI/GhostAI.cs`
- **Script Death Seq**: `Assets/_Project/Scripts/Gameplay/WellDeathSequence.cs`
- **Script Piano**: `Assets/_Project/Scripts/Gameplay/PianoInteractable.cs`
- **Script Gaze**: `Assets/_Project/Scripts/Gameplay/GazeTrigger.cs`

---

## Hoàn Thành ✅
Khi tất cả các bước trên xong, Phúc có thể test Phase 2.5:
1. Enter scene Ch.1
2. Quan sát ghost tuần tra
3. Giải piano → ghost chạy nhanh hơn
4. Nhìn giếng 3+ giây → death screen
