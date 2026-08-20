VIỆC CẦN LÀM — XÂY TỪNG PHÒNG CHAPTER 1 (map detail pass, 2026-07-12)

Đọc kỹ hết phần CHUNG trước, rồi mới xuống đúng phần tên bạn. Đừng bỏ bước nào.

---

# PHẦN CHUNG — AI CŨNG PHẢI ĐỌC TRƯỚC

## 1. Tạo nhánh git riêng trước khi làm

1. Mở terminal/Git Bash tại thư mục project, chạy: `git checkout main` rồi `git pull` để lấy bản mới nhất.
2. Tạo nhánh riêng: `git checkout -b phase2.9/detail/<tên-bạn>` (ví dụ `phase2.9/detail/thuan`).
3. Làm việc trên nhánh này — commit thoải mái trên nhánh riêng, không đụng `main`.

## 2. Cách lấy file để làm (khác mọi lần — đọc kỹ)

Mọi lần trước các bạn làm việc nhỏ (thêm 1 component) thì tạo scene MỚI TRỐNG rồi mở thêm `Chapter1.unity` kiểu Additive. **Lần này khác** — vì bạn cần dựng cả 1 phòng đúng vị trí so với cả căn nhà, nên phải làm trên 1 BẢN SAO của chính `Chapter1.unity`:

1. Trong cửa sổ Project, tìm `Assets/_Project/Scenes/Chapter1.unity`.
2. Chuột phải > **Duplicate**. Unity tạo ra file `Chapter1 1.unity`.
3. Đổi tên file đó thành `Chapter1_TênBạn.unity` (ví dụ `Chapter1_Thuan.unity`).
4. Double-click mở file VỪA ĐỔI TÊN đó ra làm — **KHÔNG BAO GIỜ mở/sửa trực tiếp `Chapter1.unity` gốc**.
5. Bạn chỉ cần build ĐÚNG phòng được giao — không cần đụng gì tới các phòng khác trong bản sao, cứ để nguyên.
6. Lưu bằng Ctrl+S bình thường, rồi `git add`, `git commit` lên nhánh riêng của bạn (`phase2.9/detail/<tên-bạn>`), `git push` lên remote.

## 3. Dựng tường/sàn/trần — DÙNG KHỐI HỘP (Cube), KHÔNG dùng Plane

Lý do dùng Cube: tường/sàn cần có ĐỘ DÀY thật (để có va chạm, đứng lên được, nhìn không bị mỏng dính) — Plane không có độ dày, không dùng.

1. Tạo khối: **GameObject > 3D Object > Cube**.
2. Kéo trong Inspector chỉnh Scale (kích thước) cho đúng số ở bảng phòng của bạn — mỗi mặt tường dày khoảng 0.4m, sàn/trần dày khoảng 0.2m.
3. Tường có cửa/cửa sổ: dựng bằng **2-3 khối Cube ghép cạnh nhau**, chừa 1 khoảng trống ở giữa đúng chỗ đặt cửa/cửa sổ (không cần tính chính xác tới từng centimet — nhìn hợp lý, cân đối 2 bên là được, vì bảng dưới CHỈ ghi kích cỡ mong muốn, không ép toạ độ chính xác).
4. Áp material: chọn khối Cube vừa tạo, kéo file `.mat` từ cửa sổ Project thả vào khối đó.

## 4. Vật liệu (Material) dùng chung — DÙNG NGUYÊN, KHÔNG tự đổi shader hay tạo material mới

PSX rendering đang là việc riêng Jok tự xử lý, KHÔNG đụng vào — chỉ kéo-thả material có sẵn dưới đây, đừng tạo material mới hay đổi Shader của material có sẵn:

| Dùng cho | Material (đường dẫn `Assets/_Project/Materials/...`) |
|---|---|
| Tường ngoài | `Mat_Wall_Exterior_Ochre.mat` |
| Tường trong (phòng chính) | `Mat_Wall_Interior_Cream.mat` |
| Tường trong (Kho — cũ kỹ hơn) | `Mat_Wall_Decay.mat` |
| Sàn phòng chính | `Architecture/Mat_Floor_Teak.mat` |
| Sàn Bếp/Kho | `Architecture/Mat_Floor_CementTile.mat` |
| Mái | `Mat_Roof_TerraCotta.mat` |
| Khung cửa sổ | `Mat_Jalousie_Green.mat` |
| Gỗ nội thất tự chế thêm | `Mat_Wood_Furniture.mat` hoặc `Mat_Wood_Teak.mat` |
| Lan can (cầu thang, ban công) | `Mat_Iron_Railing.mat` |

## 5. Model có sẵn — DÙNG NGUYÊN, đừng tự tìm/tải model khác nếu đã có

Model kiến trúc ở `Assets/_Project/Models/Props/Architecture/`, nội thất ở `Assets/_Project/Models/Props/Furniture/` (và `Furniture/Kenney/` — bộ nội thất phụ), đồ vật nhặt được ở `Assets/_Project/Models/Props/Gameplay/`.

**2 model dùng CHUNG cho mọi phòng:**
- Mọi cửa sổ: `Arch_Window_Jalousie.glb` (đặt vào đúng khoảng trống đã chừa khi ghép tường).
- Mọi cửa đi trong nhà (không phải cửa kho): `Arch_Door_Interior.glb`.

## 6. Coi như XONG khi nào (giống mọi lần)

- Chụp ảnh Console lúc Play — không dòng đỏ nào.
- Chụp ảnh/quay lại phòng bạn dựng nhìn từ trong ra (đứng giữa phòng, xoay 1 vòng camera).
- Trả lời đúng câu hỏi kiểm tra ở cuối phần việc của bạn.
- 19h00 mỗi ngày báo tiến độ lên nhóm dù xong hay chưa. Kẹt quá nửa buổi thì báo ngay, đừng ngồi mò 1 mình.

## 7. Layer riêng cho SÀN — chuẩn bị trước cho NavMesh bake sau này (ai cũng phải làm ngay khi dựng sàn)

Ma (Ghost) chỉ đi lại được trên NavMesh, mà NavMesh chỉ nên bake đúng phần SÀN trong nhà — không bake nhầm lên tường/nội thất/mái. Chưa bake lúc này (chờ đủ phòng ráp lại đã), nhưng để lúc bake không phải sửa lại từng phòng, làm ngay từ bây giờ:

1. Mọi khối Cube dùng làm SÀN (không phải tường/trần) — chọn Layer tên **`Floor`** ở góc trên phải Inspector (nếu chưa có layer này, bấm dropdown Layer > Add Layer > gõ `Floor` vào 1 ô trống, rồi quay lại gán).
2. Sàn cũng tick **Static > Navigation Static** (dropdown "Static" ở góc trên phải Inspector, cạnh tên object) — đây là cờ Unity dùng để biết vật nào tính vào lúc bake NavMesh, KHÔNG tick cho tường/nội thất/mái.
3. Chỉ cần làm đúng 2 bước trên cho khối sàn phòng mình — không cần tự bake thử, Jok sẽ bake 1 lần khi đủ phòng.

---

# THUẬN — Phòng Khách Lớn + Phòng Ăn

## Phòng Khách Lớn (18×7m, trần cao 4.2m)

Có piano — đây là phòng chính lớn nhất nhà, hướng ra vườn phía sau.

- Mặt trái (hướng hành lang vườn): 2 cửa sổ.
- Mặt sau (hướng vườn, mặt quan trọng nhất): 4 cửa sổ kính lớn.

**Nội thất:**
| Vật | Model |
|---|---|
| Piano | `Gameplay/Prop_Piano_FullKeys.fbx` (đã build sẵn, KHÔNG tạo mới) |
| Ghế piano | `Furniture/Furn_Bench_Piano.glb` |
| Sofa nhung đỏ | `Furniture/Furn_Sofa_Colonial.glb` |
| Tủ cabinet (ngăn kéo kẹt) | `Furniture/Furn_Cabinet_Locked.glb` |
| Nến + đế đồng | `Gameplay/Prop_Candle_Brass.glb` |
| Bàn trà giữa phòng | `Furniture/Kenney/tableCoffee.glb` |
| Đèn chùm (trần) | `Furniture/Kenney/lampSquareCeiling.glb` |
| Thảm trải giữa phòng | `Furniture/Kenney/rugRectangle.glb` |

**Test:** Play, đứng giữa phòng — đủ 4 tường + 4 cửa sổ mặt sau + 2 cửa sổ mặt trái, piano + sofa + tủ cabinet đặt đúng chỗ không đè lên nhau, không hở tường ở góc nào.
**Câu hỏi kiểm tra:** bạn dựng tường mặt sau (4 cửa sổ) bằng mấy khối Cube ghép lại?

## Phòng Ăn (9×7m, trần cao 4.2m)

- Mặt hướng hành lang vườn phải: 2 cửa sổ (1 cái để LỚN hơn hẳn — đây là cửa sổ Khoa chui vào lúc đầu game, đánh dấu rõ ràng).
- Mặt sau (hướng sân sau): 2 cửa sổ.

**Nội thất:**
| Vật | Model |
|---|---|
| Bàn ăn dài | `Furniture/Furn_Table_Dining.glb` |
| Ghế ăn ×6-8 | `Furniture/Furn_Chair_Dining.glb` |
| Tủ chén | `Furniture/Furn_Sideboard_Dining.glb` |
| Tờ nhạc cũ (item, trong ngăn kéo tủ chén) | `Gameplay/Prop_SheetMusic.glb` |
| Đèn treo thấp trên bàn | `Furniture/Kenney/lampRoundTable.glb` |

**Test:** Play, đứng gần cửa sổ điểm vào — phải rõ ràng LỚN hơn cửa sổ còn lại. Bàn ăn không đụng tường.
**Câu hỏi kiểm tra:** cửa sổ điểm vào bạn làm có to hơn rõ rệt so với cửa sổ còn lại không?

## Việc thêm — Test lại Inventory (InventorySystem/InventoryUI, phần bạn từng làm)

- Mở `Chapter1.unity` gốc, tìm object `InventorySystem`.
- Nhặt thử vài vật phẩm đã có sẵn trong scene, mở túi đồ (Tab) — kiểm tra icon + tên hiện đúng từng ô, không ô nào trống/lỗi hình.
- Kiểm tra vật phẩm tick "Is Key Item" viền vàng, vật thường viền thường — đúng như thiết kế gốc.
- Bấm vào từng vật phẩm xem mô tả hiện đúng không, không bị lỗi text/tràn khung.
- Nếu phát hiện lỗi (icon vỡ, thiếu tên, click không phản hồi...) — ghi lại rõ từng lỗi.

**Test:** mở túi đồ, thử với ít nhất 3 vật phẩm khác loại (1 key item, 2 thường).
**Câu hỏi kiểm tra:** có vật phẩm nào hiện sai (icon/tên/viền màu) không — nếu có thì vật nào, sai chỗ nào?

## Việc thêm — Player + Ghost: làm lại toàn bộ model giảm chất lượng + toàn bộ animation

Jok từng giảm poly thử trực tiếp trên 2 file animation (`Animations/Player/Animation_Player/Breathing Idle.fbx`, `Animations/Monster/Looking Around.fbx` — Ghost đặt tên **"Monster"** trong folder, không phải "Ghost") — nhưng giờ Jok muốn LÀM LẠI TOÀN BỘ từ đầu, không tận dụng lại 2 file thử đó nữa. Bỏ qua 2 file đó, coi như chưa có gì, làm mới hoàn toàn cho cả Player và Monster.

1. Model gốc (nặng, đầy đủ xương/rig): `Animations/Player/Model_Player/Player.fbx` và `Animations/Monster/Thuan.fbx` (đúng file mang tên bạn — Jok để sẵn cho bạn làm). Bản gốc đang ở khoảng **1.4 triệu tris/nhân vật** (xem tên file backup cũ `..._ORIGINAL_1.4Mtris_backup.fbx` để hình dung mức độ nặng hiện tại).
2. **Mục tiêu giảm còn khoảng 5.000–8.000 tris/nhân vật** (giảm ~150-250 lần so với bản gốc). Đây KHÔNG phải số tuỳ tiện — game này chủ đích dùng phong cách đồ hoạ PSX/low-poly retro (xem shader `PSX_Lit.shader` dùng xuyên suốt project), nên nhân vật không cần chi tiết cao — mức 5.000-8.000 tris vẫn đủ rõ hình người/ma, vừa khớp thẩm mỹ PSX vừa nhẹ máy. Sau khi giảm, mở tab **Stats** (góc trên phải cửa sổ Game view lúc Play) hoặc xem thông số "Triangles" ngay trong Import Inspector của file `.fbx` để kiểm tra đúng số, không áng chừng bằng mắt.
3. **QUAN TRỌNG — chỉ giảm poly phần mesh (lưới hình), giữ NGUYÊN bộ xương (skeleton/rig) gốc, không đổi/xoá/gộp xương nào cả.** Animation Mixamo gắn vào đúng theo tên xương — nếu bộ xương bị đổi khi giảm poly, mọi animation cũ lẫn mới đều gắn sai/gãy hết. Nhiều tool giảm poly (kể cả `UnityMeshSimplifier` có sẵn trong project) có tuỳ chọn "giữ nguyên skin/bone weights" — bật đúng tuỳ chọn đó, xuất ra file mesh mới nhưng bộ xương y hệt bản gốc.
4. Sau khi có model mesh-giảm-poly + xương gốc nguyên vẹn, **gắn lại TOÀN BỘ animation từ đầu** (không chỉ 1-2 cái) lên bộ xương đó — cả Player (đi/chạy/cúi — khớp `PlayerController.cs` dùng thông số Animator "Speed"/"IsCrouching") và Monster/Ghost (patrol/investigate/chase/kill — khớp `GhostAI.cs` dùng "Speed"). Wire vào đúng 2 Animator Controller có sẵn: `Animations/Player/Animator/PlayerAnimator.controller` và `Animations/Monster/MonsterAnimator.controller`.
5. **Riêng animation di chuyển của Ma: tìm animation Mixamo kiểu "quái dị"/bất thường** (không đi lại bình thường như người) — dáng đi giật cục, bò trườn, lê lết, tay chân vặn vẹo... Đây là chủ đích của Jok, không phải chọn animation "đi bộ bình thường" cho ma.

**Test:** Play, Player đi/chạy/cúi người mượt đúng animation mới, Ghost patrol/chase animation chạy đúng và nhìn "quái dị" theo đúng ý — cả 2 nhân vật hiện đúng model đã giảm poly (không phải file gốc nặng), animation không bị gãy/lệch xương chỗ nào.
**Câu hỏi kiểm tra:** tris thật sau khi giảm (xem trong Import Inspector hoặc Stats) là bao nhiêu — có nằm trong khoảng 5.000-8.000 không? Và bộ xương gốc còn nguyên không (kiểm tra bằng cách thử 1 animation cũ xem còn khớp không)?

---

# TÂN — Nhà Phụ (Bếp) + Kho + Hành Lang (cả 2 bên)

## Nhà Phụ / Bếp (6×4m, trần cao 3.2m, công trình TÁCH RIÊNG khỏi nhà chính, 4 mặt đều ngoài trời)

- Mặt trước (hướng nhà chính): 1 cửa ra vào.
- 3 mặt còn lại: mỗi mặt 1 cửa sổ nhỏ.

**Nội thất — story bible chưa có vật tương tác thật ở đây, toàn bộ dưới đây là đồ trang trí, dùng bộ Kenney bếp có sẵn:**
| Vật | Model |
|---|---|
| Bếp lò | `Furniture/Kenney/kitchenStove.glb` |
| Tủ bếp | `Furniture/Kenney/kitchenCabinet.glb` + `kitchenCabinetUpper.glb` |
| Bồn rửa | `Furniture/Kenney/kitchenSink.glb` |
| Chụp khói | `Furniture/Kenney/hoodLarge.glb` |
| Quầy bar phụ (bàn sơ chế) | `Furniture/Kenney/kitchenBar.glb` |

**Test:** Play, đi vòng quanh bên ngoài — phải thấy đây là 1 khối nhà riêng tách biệt hẳn khỏi nhà chính (không dính tường).
**Câu hỏi kiểm tra:** khoảng cách từ Bếp tới tường ngoài gần nhất của nhà chính là bao nhiêu mét (đo áng chừng)?

## Kho (6×4m, trần cao 3.2m, công trình TÁCH RIÊNG — CỐ TÌNH ít cửa sổ)

- Mặt trước: 1 cửa kho (khoá).
- Mặt sau: 1 cửa sổ nhỏ, đặt cao gần trần.
- 2 mặt còn lại: KHÔNG cửa sổ — để đặc, cố tình tối.

**Nội thất (vật tương tác thật đã có sẵn trong story bible — không tự bịa thêm lore, chỉ đặt model):**
| Vật | Model | Ghi chú |
|---|---|---|
| Cửa kho | `Architecture/Arch_Door_StorageClean.glb` | Gắn thêm `Prop_Padlock.glb` (Gameplay) làm ổ khoá |
| Bảng ký hiệu nốt nhạc | `Gameplay/Prop_Board_Chalk.glb` | Item nhặt được |
| Dụng cụ cũ | tự chọn model phù hợp có sẵn trong Kenney | Chỉ xem, không nhặt |
| Kệ gỗ | `Furniture/Kenney/bookcaseOpen.glb` | trang trí |
| Thùng/bao tải chất góc | `Furniture/Kenney/cardboardBoxClosed.glb` ×3-4 | trang trí |

**Test:** Play, đứng trong Kho bật đèn pin — phòng phải rõ ràng TỐI hơn Bếp rõ rệt, đúng như thiết kế cố tình.
**Câu hỏi kiểm tra:** bạn có thấy Kho tối hơn hẳn Bếp khi đứng giữa 2 phòng so sánh không?

## Hành Lang trái + Hành Lang phải (mỗi ô 9×3m, trần cao 4.2m)

2 dải hành lang vòng quanh Cầu Thang (Tuấn Anh làm riêng Cầu Thang, bạn chỉ làm 2 dải hành lang 2 bên).

- Mỗi dải có 1 cửa thoát ra hành lang vườn bên ngoài (trái ra hành lang vườn trái, phải ra hành lang vườn phải).
- Hành Lang trái là chỗ trốn Ma Vú Dài — đặt thêm 1 cái tủ áo để núp.

**Nội thất:**
| Vật | Model |
|---|---|
| Tủ áo (Hành Lang trái, chỗ núp) | `Furniture/Kenney/cabinetBed.glb` hoặc model tủ áo phù hợp có sẵn |

**Test:** Play, đi từ đầu Hành Lang trái tới cuối, ra được cửa thoát bên ngoài, không bị chặn tường.
**Câu hỏi kiểm tra:** 2 dải hành lang bạn dựng có cùng kích thước với nhau không (9×3m cả 2)?

## Việc thêm — Test lại Sanity (SanitySystem/SanityPostProcess/SanityShake, phần bạn từng làm)

- Mở `Chapter1.unity` gốc, Play, dùng menu **VoD > Temp > Debug - Drain Sanity 20%** bấm vài lần để hạ Sanity dần (100→80→60→40→20→0).
- Ở mỗi nấc, quan sát: hiệu ứng hình (grain/vignette/mờ nhoè) có rõ ràng thay đổi không, camera có rung đúng lúc Sanity xuống thấp không, âm thanh (nếu có) có phát đúng không.
- Dùng **VoD > Temp > Debug - Restore Sanity To Full** để hồi lại, kiểm tra hiệu ứng biến mất mượt, không giật cục/kẹt lại giữa chừng.

**Test:** hạ hết về 0%, hồi lại 100%, toàn bộ hiệu ứng phải theo kịp đúng từng nấc, không bị kẹt ở nấc cũ.
**Câu hỏi kiểm tra:** ở nấc Sanity thấp nhất (gần 0%), bạn thấy hiệu ứng gì rõ nhất (mờ/nhiễu/rung/tối)?

## Việc thêm — Làm lại tính năng trốn (cinematic, không teleport nữa)

Code lõi (`HideSpot.cs`, `DoorController.cs`) đã được sửa sẵn — không cần viết lại từ đầu, chỉ cần dọn scene + test + tinh chỉnh:

**Đã sửa sẵn trong code (chỉ cần biết để test đúng):**
- Vào tủ giờ KHÔNG còn teleport tức thì — camera/nhân vật LƯỚT (lerp) từ vị trí đứng tới vị trí núp trong khoảng 0.6 giây (`_slideDuration` trên `HideSpot`, chỉnh được trong Inspector).
- Sau khi lướt vào xong, hướng nhìn tự xoay theo đúng hướng của object `_hidePosition` (Transform con bạn tự đặt trong tủ) — muốn nhân vật quay lại nhìn ra khe cửa thì xoay `_hidePosition` ngay trong Editor cho đúng hướng đó, không cần sửa code.
- Cửa không đóng kín khi đang trốn nữa — tự động "hé" 1 góc nhỏ (`_ajarAngle` trên `DoorController`, mặc định 15°) để có khe hở nhìn ra ngoài, đúng kiểu núp thật.
- Thoát ra làm ngược lại: cửa mở hẳn → camera lướt ra đúng vị trí/hướng lúc vào → cửa đóng kín hẳn.
- Ma KHÔNG phát hiện được player đang trốn — `GhostAI.cs` đã có sẵn check `HideSpot.IsPlayerHiding`/`AnyPlayerHiding` ở cả 3 chỗ (nhìn thấy, nghe thấy, giết) từ trước, không cần sửa gì thêm ở phần này.

**Việc bạn cần làm:**
1. Trong `Chapter1.unity`, tìm các `HideSpot` cũ dùng **tủ sách/kệ sách** (`bookcaseClosedWide` hoặc tương tự) — Jok quyết định BỎ kiểu núp này, xoá hẳn hoặc tắt (`SetActive(false)`) object đó.
2. Tìm `HideSpot` dùng **giường** (`bedDouble`) — Chapter 1 không có phòng ngủ nào cả (đã kiểm tra story bible, không có "giường"/"phòng ngủ" ở đâu hết), nên bỏ luôn kiểu núp này, KHÔNG cần dựng giường giả chỉ để núp.
3. **Chỉ giữ đúng 1 kiểu núp cho Chapter 1: trốn trong tủ áo** (tủ áo ở Hành Lang trái, phần bạn tự dựng ở trên) — gắn đúng `HideSpot` + `DoorController` (đã có field `_ajarAngle` mới) vào đúng object tủ áo + cửa tủ áo.
4. Test kỹ animation lướt vào/ra — chỉnh `_slideDuration` (mượt quá chậm thì giảm, giật quá thì tăng) và `_ajarAngle` (hé quá nhiều thì lộ hết, hé quá ít thì như đóng kín) cho tới khi thấy "đúng phim" như Jok mô tả.

**Test:** Play, bấm E vào tủ áo — phải thấy: cửa mở → camera LƯỚT (không giật/snap) vào trong → xoay lại nhìn ra khe cửa → cửa hé lại (không đóng kín). Bấm E lần nữa: ngược lại y hệt, cửa đóng kín hẳn ở bước cuối.
**Câu hỏi kiểm tra:** bạn có xoá/tắt hết 2 chỗ núp cũ (tủ sách, giường) chưa — hiện Chapter 1 chỉ còn đúng mấy chỗ núp?

---

# PHÚC — Tiền Sảnh + vải đỏ gương + tâm tròn UI

## Tiền Sảnh (9×6m, trần cao 4.2m)

Phòng có cửa chính ra vào nhà, mặt sau thông thẳng sang khu Cầu Thang (không xây kín mặt này, để trống thông sang).

- Mặt trước (mặt tiền nhà): 1 cửa chính + 2 cửa sổ nhỏ 2 bên cửa chính.
- Mặt sau (phía Cầu Thang): để trống, không xây kín — chỉ xây 2 đoạn ngắn 2 đầu làm trụ góc cho đẹp, còn lại thông hẳn.

**Nội thất:**
| Vật | Model |
|---|---|
| Gương phủ vải đỏ | `Materials/M_MirrorDisplay.mat` (đã có sẵn setup gương) + vải phủ ngoài dùng `Architecture/Prop_Curtain_Torn.glb`, đổi màu material đó thành đỏ đậm (KHÔNG tạo material mới, chỉ đổi màu material có sẵn của Curtain) |
| Tủ giày | tự chọn model phù hợp trong Kenney |
| Giá treo áo | `Furniture/Kenney/coatRackStanding.glb` |
| Thảm chào | `Furniture/Kenney/rugDoormat.glb` |

## Việc riêng 1 — Vải đỏ: bấm E là biến mất

- Vải đỏ (`Prop_Curtain_Torn.glb` tô đỏ, đặt che trước gương) cần thêm 1 script MỚI implement `IInteractable` (interface có sẵn ở `Assets/_Project/Scripts/Interfaces/IInteractable.cs`) — `Interact()` chỉ cần gọi `gameObject.SetActive(false)` là đơn giản nhất.
- Không cần lo chọn phím — hệ thống chung `InteractionSystem.cs` đã tự raycast + gọi `Interact()` khi bấm E, bạn chỉ cần gắn script vào đúng vải là xong.

## Việc riêng 2 — Tâm tròn trắng UI (đã dựng khung sẵn, bạn chỉ cần thay hình)

- Trong `Chapter1.unity` gốc (KHÔNG phải bản sao của bạn — việc này làm ở scene chung vì là UI toàn cục) đã có sẵn `Canvas/InteractPrompt/PromptRoot/Dot` — 1 ô vuông trắng tạm.
- Việc của bạn: đổi `Image` component của object `Dot` này sang 1 sprite hình tròn trắng thật (`Assets > Create > Sprites > Circle` có sẵn ngay trong Unity, không cần tìm file ngoài), chỉnh size/độ mờ tuỳ ý cho đẹp.
- Test: Play, đi lại gần bất kỳ vật tương tác nào (piano, tủ, gương...) — phải thấy tâm tròn trắng + chữ "E" hiện lên đúng lúc, biến mất khi quay đi chỗ khác.
- Nếu chưa thấy object `InteractPrompt` trong Canvas — vào menu **VoD > Temp > Setup - Interact Prompt UI (E + tâm tròn)** để tool tự dựng ra trước.

**Câu hỏi kiểm tra (cả 3 việc):** (1) bấm E vào vải đỏ, vải có biến mất đúng không hay còn lỗi gì? (2) tâm tròn bạn đổi có phải hình tròn thật không hay vẫn là ô vuông? (3) đứng trong Tiền Sảnh, quay lưng lại phía Cầu Thang, có thấy thông thoáng không bịt kín không?

---

# VŨ — Phòng Tiếp Khách

## Phòng Tiếp Khách (9×6m, trần cao 4.2m)

Phòng này giống hệt Thư Phòng của Tuấn Anh nhưng nằm ở phía bên kia Tiền Sảnh (2 phòng đối xứng nhau).

- Mặt trước (mặt tiền nhà): 2 cửa sổ.
- Mặt hướng hành lang vườn phải: 2 cửa sổ.

**Nội thất:**
| Vật | Model |
|---|---|
| Chân dung ông chủ nhà | dùng lại `Prop_Portrait_Family.glb` (Decor) |
| Lò sưởi | `Architecture/Arch_Fireplace_Stone.glb` |
| Ghế bành ×2 | `Furniture/Furn_Chair_Armchair.glb` |
| Bàn trà nhỏ | `Furniture/Kenney/sideTable.glb` |
| Đồng hồ quả lắc | tự chọn model gần đúng có sẵn trong Kenney |

**Test:** Play, so sánh đứng trong phòng này với đứng trong Thư Phòng (bên kia Tiền Sảnh) — bố cục cửa sổ mặt tiền phải giống hệt nhau (đối xứng gương thật sự, không lệch).
**Câu hỏi kiểm tra:** 2 cửa sổ mặt tiền phòng bạn có giống kích thước/vị trí với 2 cửa sổ mặt tiền Thư Phòng không (nhờ Tuấn Anh gửi ảnh so sánh nếu cần)?

## Việc riêng 1 — SFX cho mọi nút bấm + thanh trượt trong UI

Mục tiêu: MỌI Button và Slider trong toàn bộ UI (MainMenu, PauseMenu, SettingsUI, Inventory, DeathScreen...) đều phát 1 tiếng "click" khi bấm/kéo, luân phiên đổi giữa 2 âm thanh khác nhau để không bị lặp nhàm.

1. Kiếm/tạm dùng 2 file âm thanh click ngắn (nếu chưa có SFX thật, dùng tạm 2 file bất kỳ có sẵn trong `Assets/_Project/Audio/` — nói rõ trong báo cáo đây là âm thanh tạm).
2. Tạo 1 script mới `UIButtonSFX.cs` (đặt ở `Assets/_Project/Scripts/UI/`):
   - 2 field `AudioClip _clipA, _clipB` kéo tay 2 file âm thanh vào.
   - 1 biến `static bool _useA = true` (dùng chung cho MỌI instance, để lần bấm nào cũng luân phiên đổi bài, không phải riêng từng nút tự đếm).
   - Hàm `PlayClick()`: gọi `AudioManager.Instance?.PlaySFX(_useA ? _clipA : _clipB)` rồi đảo `_useA = !_useA`.
   - Với `Button`: lấy component `Button`, thêm listener `onClick.AddListener(PlayClick)` trong `Start()`.
   - Với `Slider`: lấy component `Slider`, thêm listener `onValueChanged.AddListener((_) => PlayClick())` trong `Start()` — nhưng chỉ gọi khi người chơi THẢ tay ra (kéo xong), không phải gọi liên tục mỗi frame lúc đang kéo (dễ bị dí tiếng liên tục nghe khó chịu) — dùng `EventTrigger` bắt sự kiện `PointerUp` thay vì `onValueChanged` nếu thấy kéo bị dí tiếng quá nhiều.
3. Gắn script này vào TỪNG Button/Slider trong toàn bộ UI liên quan tới 2 màn hình chính (SettingsUI + PauseMenu) trước, sau đó lan ra MainMenu/Inventory/DeathScreen nếu còn thời gian.

**Test:** Play, mở Settings, bấm vài nút + kéo vài slider liên tục — phải nghe tiếng click LUÂN PHIÊN 2 âm khác nhau, không lặp y hệt liên tiếp.
**Câu hỏi kiểm tra:** bạn bấm 4 lần liên tiếp vào cùng 1 nút, thứ tự âm thanh nghe được là A-B-A-B hay lộn xộn?

## Việc riêng 2 — Icon đèn pin đổi màu theo % pin

Mục tiêu: 1 icon hình đèn pin trên HUD, đổi màu theo % pin còn lại — data đọc từ `FlashlightController.BatteryLevel01` (đã có sẵn field này, đọc thẳng không cần sửa gì thêm bên gameplay) và 3 ngưỡng có sẵn trong asset `FlashlightData_Ch1` (`flickerMediumThresh=0.5`, `flickerLowThresh=0.3`, `flickerCriticalThresh=0.15`) — dùng ĐÚNG 3 số này, không tự bịa ngưỡng khác, để icon đổi màu đúng lúc đèn thật bắt đầu nhấp nháy.

**4 màu theo mốc pin:**
| Pin còn | Màu icon | Chữ "T" bên dưới |
|---|---|---|
| > 50% | Trắng | Không hiện |
| 30% – 50% | Vàng | Hiện, màu trắng, NHẤP NHÁY vừa (khoảng 1 lần/giây) |
| 15% – 30% | Đỏ | Hiện, màu trắng, NHẤP NHÁY nhanh hơn (khoảng 2 lần/giây) — cảnh báo gấp hơn vàng |
| < 15% | Xám | Hiện, màu trắng, ĐỨNG YÊN không nhấp nháy (lúc này đèn thật đã tự chớp tắt liên tục rồi, chữ đứng yên cho dễ đọc giữa lúc đèn đang nhấp nháy loạn) |

Chữ "T" là gợi ý phím tắt lắc đèn hồi pin (`KeyCode.T`, xem `FlashlightController.cs` — đã có cơ chế lắc rồi, chỉ cần thêm UI gợi ý).

**Cách làm:**
1. Tạo 1 Image (icon đèn pin — tìm sprite phù hợp có sẵn trong `Assets/_Project/Textures/UI/`, hoặc dùng tạm hình vuông đổi màu nếu chưa có icon thật) + 1 TextMeshProUGUI (chữ "T") đặt trong Canvas HUD.
2. Script mới `FlashlightBatteryUI.cs` (`Assets/_Project/Scripts/UI/`): field kéo `FlashlightController _flashlight`, `Image _icon`, `TextMeshProUGUI _tLabel`, `FlashlightData _data`. Trong `Update()`, đọc `_flashlight.BatteryLevel01`, so với 3 ngưỡng, đổi `_icon.color` + bật/tắt `_tLabel` theo bảng trên.
3. Nhấp nháy: dùng `Time.time` với hàm sin hoặc `InvokeRepeating` bật/tắt `_tLabel.enabled` theo đúng tốc độ ở bảng trên — không cần phức tạp, đổi alpha hoặc SetActive xen kẽ là đủ.

**Test:** Play, bật đèn pin đứng yên cho pin hao dần (hoặc sửa tạm `drainRate` trong `FlashlightData_Ch1` lên cao để test nhanh, nhớ đổi lại số cũ sau khi test xong) — icon phải đổi màu đúng thứ tự Trắng→Vàng→Đỏ→Xám, chữ T hiện/nhấp nháy đúng bảng.
**Câu hỏi kiểm tra:** bạn có đổi tạm `drainRate` để test nhanh không — nếu có, đã đổi lại đúng số gốc chưa (kiểm tra kỹ, quên đổi lại là bug thật khi build)?

## Việc riêng 3 — Thanh Stamina (chạy Shift), làm cho nghệ thuật, đừng làm thanh HP trơn nhàm chán

Vừa thêm xong cơ chế giới hạn chạy (Shift) — trước đây chạy vô hạn, giờ có "pin chạy" hết dần khi giữ Shift, hồi lại khi đi bộ/đứng yên. Data đọc từ `PlayerController.Stamina01` (0 = hết hơi, 1 = đầy) — field mới thêm sẵn, đọc thẳng được luôn.

**Chỉ cần đúng 1 thanh duy nhất** (Jok nói rõ — không làm thêm số %, không làm icon phụ ngoài ý đồ dưới đây). Chọn 1 trong 3 hướng sau (hoặc tự nghĩ hướng khác nếu thấy hợp phong cách game hơn):

1. **Vòng tròn quanh tâm ngắm** — 1 vòng tròn mỏng bao quanh chính tâm tròn trắng Phúc làm (`Canvas/InteractPrompt/PromptRoot/Dot` — xem việc của Phúc ở trên, hỏi Phúc phối hợp nếu cần chỉnh chung khu vực UI này) hoặc 1 tâm ngắm riêng nếu không tiện dùng chung — dùng `Image` với `Image Type = Filled, Fill Method = Radial 360`, `fillAmount = Stamina01`. Vơi dần theo hình vòng cung khi chạy, đầy lại khi đi bộ. Ưu điểm: không thêm 1 khối UI mới choán màn hình, gắn liền vào tâm ngắm sẵn có — đúng ý "để gì đó ngồi trong" 1 thành phần UI có sẵn thay vì thêm 1 thanh riêng biệt.
2. **Ngọn nến/đèn dầu cháy dần** — 1 thanh dọc nhỏ style ngọn lửa nến (khớp mood đồ vật nến/đèn dầu thời Pháp thuộc đã có trong game — `Prop_Candle_Brass`), cao dần/thấp dần theo `Stamina01`, có hiệu ứng rung nhẹ ánh sáng (lerp scale/alpha ngẫu nhiên nhỏ) giống lửa nến thật thay vì thanh progress bar phẳng lì. Hợp không khí Đông Dương/kinh dị hơn thanh HP kiểu game hành động thông thường.
3. **Nhịp thở/tim đập** — thanh nằm ngang nhưng KHÔNG đứng yên, tự "thở" (scale nhẹ to-nhỏ theo chu kỳ sin) khi còn nhiều stamina, nhịp thở NHANH DẦN và đổi màu trắng→đỏ khi gần cạn — truyền tải cảm giác đuối sức/hụt hơi thay vì chỉ là số liệu khô khan.

Hướng nào cũng được, miễn: (a) chỉ 1 thanh duy nhất, (b) khi Stamina chạm 0 phải có phản hồi RÕ RỆT (đổi màu mạnh, rung, hoặc nhấp nháy) để người chơi biết ngay là hết hơi không chạy được nữa, không chỉ im lặng trống rỗng.

**Test:** Play, giữ Shift chạy liên tục tới khi hết hơi (quay lại đi bộ tự động) — thanh phải thể hiện rõ quá trình cạn dần và báo hiệu rõ lúc chạm đáy, rồi hồi dần khi đi bộ/đứng yên.
**Câu hỏi kiểm tra:** bạn chọn hướng nào trong 3 gợi ý (hay tự nghĩ hướng khác) — vì sao chọn hướng đó?

---

# TUẤN ANH — Thư Phòng + Cầu Thang

## Thư Phòng (9×6m, trần cao 4.2m)

- Mặt trước (mặt tiền nhà): 2 cửa sổ.
- Mặt hướng hành lang vườn trái: 2 cửa sổ.

**Nội thất:**
| Vật | Model |
|---|---|
| Tủ sách | `Furniture/Furn_Bookshelf_Colonial.glb` |
| Bàn viết + ghế | `Furniture/Furn_Desk_Study.glb` + `Furn_Chair_Study.glb` |
| Nhật ký dang dở | đặt trên bàn viết, dùng model sách/giấy nhỏ có sẵn trong Kenney (`books.glb`) làm tạm |
| Hộp âm nhạc đồng | `Gameplay/Prop_MusicBox_Cylinder.glb` (item nhặt được — di vật quan trọng nhất Chapter 1) |
| Đèn dầu bàn | `Furniture/Kenney/lampRoundTable.glb` |
| Thảm nhỏ | `Furniture/Kenney/rugRectangle.glb` (scale nhỏ lại) |

**Test:** Play, đứng giữa phòng — đủ đồ đúng bảng, hộp âm nhạc đặt ở vị trí dễ thấy.
**Câu hỏi kiểm tra:** cửa sổ mặt tiền phòng bạn (2 cái) có giống cửa sổ mặt tiền Phòng Tiếp Khách của Vũ không?

## Cầu Thang (9×3m, trần cao 4.2m — nằm giữa nhà, giữa Hành Lang trái/phải của Tân)

- Mặt trước (phía Tiền Sảnh) và 2 mặt trái/phải (phía Hành Lang): KHÔNG xây kín — để thông thoáng, chỉ xây 2 đoạn ngắn 2 đầu mỗi mặt làm trụ góc.
- Mặt sau (phía Phòng Khách Lớn): xây kín, không cửa — chặn hẳn, muốn qua Phòng Khách Lớn phải đi vòng qua Hành Lang.
- **Cầu thang gỗ thật** — nếu project đã có sẵn model/thiết kế cầu thang nào dùng được (kiểm tra `Furniture/Kenney/stairs.glb` và `stairsOpen.glb` trước, xem cái nào hợp) thì tái sử dụng luôn, không cần tự làm mới từ đầu. Chỉ khi không có cái nào dùng được mới tự dựng thêm.

**Test:** Play, đứng trong Tiền Sảnh nhìn xuyên qua khu Cầu Thang — phải thấy rõ 2 bên Hành Lang, không bị tường chắn khuất. Đứng ở Phòng Khách Lớn nhìn lên — mặt đó phải kín hẳn.
**Câu hỏi kiểm tra:** bạn có tái sử dụng được model cầu thang có sẵn trong project không, hay phải tự dựng mới?

## Việc thêm — Test lại Intro + UI lời thoại

- Mở `Chapter1.unity` gốc, Play từ đầu (tắt `skipIntroEntirely` trên object `IntroManager` nếu đang bật) — xem hết đoạn mở đầu: chớp mắt đen mở ra, camera xoay/quay cảnh, tới đoạn thoại 4 câu của Minh Khoa.
- Kiểm tra: chữ hiện kiểu gõ máy chữ đúng nhịp, không bị vỡ dòng/tràn khung; bấm Space đúng lúc thấy "Nhấn [ SPACE ] để tiếp tục" thì chuyển câu tiếp; hết 4 câu thì tự đóng hộp thoại và trả lại điều khiển (đi lại/xoay chuột được bình thường).
- Thử bấm Space liên tục ngay lúc chữ đang gõ (chưa gõ xong) — phải hiện hết câu ngay lập tức (skip), không bị lỗi/kẹt.
- Kiểm tra Console không có dòng đỏ nào trong suốt đoạn intro.

**Test:** Play từ đầu tới hết intro + 4 câu thoại, không dừng giữa chừng, không lỗi Console.
**Câu hỏi kiểm tra:** sau khi hộp thoại đóng, bạn có điều khiển lại nhân vật (đi/xoay chuột) ngay được không, hay bị đứng khựng?
