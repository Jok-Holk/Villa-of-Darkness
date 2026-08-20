VIỆC CẦN LÀM — CHAPTER 1

Đọc kỹ trước khi làm, làm đúng từng bước, đừng bỏ bước nào.

CÁCH LÀM CHUNG (ai cũng phải theo):

- Mở Unity, tạo 1 SCENE MỚI riêng để làm việc của mình (File > New Scene). Không sửa trực tiếp vào scene Chapter1 chính.
- Khi làm xong, test thử bằng cách bấm nút Play trong Unity, xem đúng như mô tả không.
- Làm xong đủ mới tính là xong — thiếu 1 trong 3 thứ dưới đây thì coi như CHƯA xong:
  1. Chụp ảnh màn hình Console lúc đang Play — không có dòng chữ đỏ nào.
  2. Chụp ảnh hoặc quay lại lúc test — cho thấy kết quả thật, không chỉ nói "xong rồi".
  3. Trả lời đúng câu hỏi kiểm tra ở cuối phần việc của mình.
- Mỗi ngày lúc 19h00 (7h tối) nhắn lên nhóm tiến độ, dù xong hay chưa.
- Nếu tới 19h00 mà chưa xong hoặc còn thiếu gì (thiếu ảnh, chưa test được...) — tự ghi ra thành 1 danh sách, nhắn lên nhóm.
- Kẹt quá nửa buổi ở 1 chỗ mà không ra — nhắn ngay lên nhóm, đừng ngồi mò một mình lâu quá.

VÀI THAO TÁC UNITY CƠ BẢN (cần cho hầu hết các việc dưới đây):

- Thêm 1 component vào object: chọn object đó trong cửa sổ Hierarchy (bên trái) > bên phải màn hình (Inspector) kéo xuống dưới cùng, bấm nút "Add Component" > gõ tên component cần tìm (ví dụ gõ "Hide Spot") > bấm chọn đúng tên hiện ra.
- Tạo 1 Empty GameObject: chuột phải trong cửa sổ Hierarchy > Create Empty.
- Tìm 1 object có sẵn trong scene: gõ tên vào ô tìm kiếm ở đầu cửa sổ Hierarchy (ô có kính lúp).
- Nhiều việc bên dưới cần dùng object "Player", "Ghost", "DeathScreenUI", "InventorySystem"... vốn nằm sẵn trong scene Chapter1, KHÔNG có trong scene mới của bạn. Cách lấy: đang mở scene của mình, vào File > Open Scene (Additive) > chọn Assets/_Project/Scenes/Chapter1.unity — giờ cả 2 scene cùng hiện trong Hierarchy, tìm object cần trong đó. Lưu ý: khi lưu (Ctrl+S), CHỈ lưu scene của bạn, đừng lưu Chapter1.

---

THUẬN — làm 4 việc

1. Tạo 1 vật phẩm tên "Chìa khoá cũ":
   - Trong Unity: chuột phải ở cửa sổ Assets > Create > Inventory > Item Data.
   - Điền: itemId = key_skeleton, itemName = "Chìa khoá cũ", description = "Một chiếc chìa khoá đồng cũ, mặt ngoài đã xỉn màu theo năm tháng. Có thể mở được ổ khoá nào đó trong nhà."
   - Tick ô "Is Key Item".
   - Kéo 1 icon (tạm dùng icon nào có sẵn trong Assets/\_Project/Textures/UI/ cũng được, đừng để trống).
   - Câu hỏi kiểm tra: file ItemData này bạn lưu ở đường dẫn nào (folder nào trong Assets)?

2. Tạo 1 vật phẩm tên "Ảnh gia đình" — icon PHẢI chụp thật từ tượng có sẵn, không dùng icon tạm:
   - Tạo Item Data: itemId = family_photo, itemName = "Ảnh gia đình", description = "Một bức ảnh đen trắng đã ố vàng, chụp cả nhà đứng trước cổng biệt thự. Không rõ họ là ai.", KHÔNG tick "Is Key Item".
   - Model đã có sẵn trong project là TƯỢNG 4 người (đại diện gia đình), dùng đúng cái này: Assets/\_Project/Models/Props/Decor/Prop_Portrait_Family.glb — KHÔNG cần tự tìm model khác.
   - Cách làm ra tấm ảnh:
     a. Kéo tượng đó vào 1 scene tạm. Đặt Camera đứng thẳng phía trước, canh khung hình sao cho thấy ĐỦ CẢ 4 người trong tượng, giống bố cục 1 tấm ảnh gia đình chụp chung thật sự (không cắt mất ai, không góc nghiêng lệch).
     b. Bấm Play (hoặc xem ở cửa sổ Game), dùng phím tắt Windows Win+Shift+S để chụp đúng vùng khung hình đó.
     c. QUAN TRỌNG — biến ảnh vừa chụp thành ảnh đen trắng/ố vàng thật sự (đúng như mô tả), không để nguyên ảnh màu 3D thô: mở ảnh bằng app "Photos" có sẵn trên Windows > vào mục chỉnh sửa/Filters > chọn kiểu đen trắng (hoặc dùng bất kỳ công cụ chỉnh ảnh online miễn phí nào tìm được, miễn ra đúng tông đen trắng/ngả vàng cũ).
     d. Cắt (crop) lại cho vuông vắn, lưu thành file .png vào Assets/\_Project/Textures/UI/.
     e. Trong Unity, chọn file .png vừa lưu, ở Inspector đổi "Texture Type" thành "Sprite (2D and UI)", bấm Apply.
     f. Kéo Sprite vừa tạo vào ô "icon" của Item Data "Ảnh gia đình".
   - Câu hỏi kiểm tra: bạn dùng công cụ gì để biến ảnh thành đen trắng/ố vàng, và file lưu ở đường dẫn nào?

3. Làm tủ khoá cần chìa mới mở được:
   - Đặt model tủ có sẵn: Assets/\_Project/Models/Props/Furniture/Furn_Cabinet_Locked.glb vào scene.
   - Thêm component "Item Lock" vào tủ.
   - Kéo object InventorySystem (mở thêm Chapter1.unity kiểu Additive nếu không thấy object này trong scene của bạn — xem "Vài thao tác Unity cơ bản" ở đầu file) vào ô "\_inventorySystem".
   - Điền: \_requiredItemId = key_skeleton, \_consumeRequired = tick chọn, \_grantItemId để trống, \_lockedHint = "Cần thêm thứ gì đó để mở...", \_unlockedHint = "Đã mở."
   - Test: chưa nhặt chìa khoá thì bấm E vào tủ phải thấy chữ "locked" trong Console. Nhặt chìa khoá xong bấm E lại phải thấy chữ "UNLOCKED".
   - Câu hỏi kiểm tra: lúc CHƯA có chìa khoá, bấm E vào tủ, dòng chữ hiện trong Console chính xác là gì (đọc và chép lại đúng)?

4. Đặt vật phẩm "Ảnh gia đình" để nhặt được trong scene:
   - Chọn 1 model đồ vật bất kỳ, thêm component "Pickup Item".
   - Kéo Item Data "Ảnh gia đình" (việc 2) vào ô "\_itemData".
   - Kéo object InventorySystem vào ô "\_inventorySystem".
   - Test: bấm E nhặt xong, mở túi đồ (phím Tab) phải thấy đúng tên + icon, không phải ô trống.
   - Câu hỏi kiểm tra: sau khi nhặt xong, model đồ vật đó còn nhìn thấy trong scene không?

---

TÂN — làm 3 việc

1. Làm 1 vật phát ra tiếng ghi âm (Audio Log):
   - Model đã có sẵn, dùng đúng cái này (đúng "hộp nhạc" theo GDD): Assets/\_Project/Models/Props/Gameplay/Prop_MusicBox_Cylinder.glb.
   - Kéo model vào scene, thêm component "Audio Log Item".
   - Điền \_logText = "Bà Lan: Con có nghe thấy tiếng nhạc không... nó vẫn còn vang trong hộp nhạc ấy. Ta đã giấu nó rất kỹ, sợ ai đó tìm thấy sẽ đánh thức... thứ không nên đánh thức."
   - Kéo TẠM 1 file âm thanh bất kỳ có sẵn trong Assets/\_Project/Audio/ vào ô "\_logClip" (thoại thật chưa thu âm, nói rõ trong báo cáo đây là âm thanh tạm).
   - Test: bấm E vào vật đó, nghe thấy âm thanh phát ra, Console hiện đúng dòng chữ.
   - Câu hỏi kiểm tra: bạn dùng file âm thanh tạm nào (tên file) — bấm E xong bạn có THỰC SỰ NGHE thấy tiếng phát ra không, hay chỉ thấy dòng chữ trong Console?

2. Làm 1 vùng có âm thanh nền riêng:
   - Tạo 1 Empty GameObject bao quanh khu vực, thêm Collider (tick "Is Trigger"), thêm Audio Source (kéo 1 clip nhạc nền vào), thêm component "Ambient Zone".
   - Điền: \_targetVolume = 0.7, \_fadeDuration = 1.5.
   - Test: đi vào vùng đó nghe âm lượng tăng dần, đi ra nghe giảm dần, không giật cục.
   - Câu hỏi kiểm tra: đi vào vùng đó, âm lượng tăng dần lên bạn cảm nhận mất khoảng mấy giây (áng chừng theo tai bạn nghe, không cần chính xác tuyệt đối)?

3. Sửa 1 lỗi còn thiếu từ trước (Sanity effect chưa hoạt động):
   - Mở scene Chapter1 hiện tại (không cần chờ scene mới), tìm 2 object tên "PlayerCamera" và "SanityManager".
   - Cả 2 đều có component "Sanity Post Process" với ô "Volume" đang để trống.
   - Kéo object "GlobalVolume" (đã có sẵn trong scene) vào ô Volume này ở CẢ 2 object.
   - Câu hỏi kiểm tra: sau khi gán xong, thử hạ Sanity xuống thấp (dùng ContextMenu test có sẵn hoặc cách nào cũng được) — màn hình có THỰC SỰ mờ/nhiễu đi không, hay vẫn không có gì thay đổi?

---

PHÚC — làm 2 việc

1. Làm gương phản chiếu được — làm đủ theo đúng thứ tự sau, thiếu bước nào sẽ không chạy:
   a. Mở thêm Chapter1.unity kiểu Additive (xem hướng dẫn "Vài thao tác Unity cơ bản" ở đầu file) để lấy được object Player và Ghost.
   b. Tạo mặt gương: trong scene của bạn, chuột phải Hierarchy > 3D Object > Plane (hoặc Quad). Xoay/đặt đứng thẳng như 1 tấm gương.
   c. Tạo Layer mới tên "MirrorOnly": góc trên phải Inspector (khi đang chọn 1 object bất kỳ) > bấm dòng "Layer" > Add Layer > gõ tên "MirrorOnly" vào 1 ô trống.
   d. Gán layer này cho Ghost và Player: chọn từng object đó > góc trên phải Inspector > dropdown "Layer" > chọn "MirrorOnly".
   e. Tạo 1 Render Texture: chuột phải cửa sổ Assets > Create > Render Texture > đổi Size thành 512x512.
   f. Tạo 1 Material mới cho mặt gương: chuột phải Assets > Create > Material > kéo Render Texture vừa tạo vào ô "Base Map"/"Albedo" của material này > kéo material này vào object Plane/Quad (mặt gương) đã tạo ở bước b.
   g. Tạo 1 Camera con của mặt gương (kéo Camera vào làm con của Plane trong Hierarchy), chỉnh Culling Mask = chỉ chọn "MirrorOnly", kéo Render Texture (bước e) vào ô "Target Texture" của Camera này.
   - Test: bấm Play, đứng trước mặt gương vừa tạo, phải thấy hình phản chiếu của Player/Ghost hiện đúng trên mặt gương.
   - Câu hỏi kiểm tra: đứng trước gương trong Play mode, bạn có thấy đúng hình phản chiếu của mình không? Nếu gương bị đen thui/trắng xoá/không hiện gì, mô tả đúng bạn thấy gì.

2. Làm cơ chế "nhìn gương quá lâu thì chết":
   - Tạo file Gaze Settings (chuột phải Assets > Create > VillaOfDarkness > Gaze Settings).
   - Điền: gazeThreshold = 3, warningThreshold = 1, maxDistance = 8.
   - Thêm component "Gaze Trigger" vào gương, kéo Gaze Settings vừa tạo vào ô "\_settings".
   - Test: nhìn thẳng vào gương liên tục 3 giây, nhân vật phải chết đúng kịch bản.
   - Câu hỏi kiểm tra: bạn đếm thời gian thật từ lúc nhìn vào gương tới lúc chết là khoảng bao nhiêu giây? Có khớp gần đúng 3 giây không hay lệch nhiều?

---

VŨ — làm 4 việc

1. Kiểm tra lại cây đàn piano còn hoạt động không:
   - Prefab đã có sẵn, dùng đúng cái này: Assets/\_Project/Materials/Piano/Piano.prefab — kéo vào 1 phòng bất kỳ trong scene mới của bạn.
   - Kiểm tra 7 phím đàn còn đủ thông tin kéo-thả không: mở prefab ra (double-click), bấm lần lượt từng phím con bên trong, xem ở Inspector 2 ô "\_piano" và "\_noteDefinition" có đang trống (None) không — nếu trống, kéo lại (chọn nhiều phím cùng lúc kéo 1 lần được).
   - Test: bấm E lại gần đàn, phím A/D chọn nốt, phím Space đánh đàn, phải nghe đúng nốt.
   - Câu hỏi kiểm tra: nếu phát hiện thông tin phím đàn bị mất, bạn phát hiện bằng cách nào?

2. Làm giếng nước có thể gây chết nếu nhìn lâu:
   - Mở thêm Chapter1.unity kiểu Additive (xem "Vài thao tác Unity cơ bản" ở đầu file) để lấy Player và object DeathScreenUI.
   - Đặt model giếng có sẵn: Assets/\_Project/Models/Props/Architecture/Arch_Well_Stone.glb.
   - Thêm "Gaze Trigger" (gazeThreshold = 3, giống Phúc làm) và thêm "Well Death Sequence".
   - Kéo đúng: gaze trigger vừa tạo, Player, và object DeathScreenUI vào các ô tương ứng. Điền \_requiredDistance = 2.
   - Test: đứng gần giếng, nhìn xuống nước liên tục → phải chạy đúng chuỗi: có âm thanh, có đốm sáng, màn hình tối dần, hiện màn hình chết.
   - Câu hỏi kiểm tra: kể lại đúng thứ tự những gì bạn thấy/nghe xảy ra khi chết (cái gì xuất hiện/phát ra trước, cái gì sau)?

3. Test lại hộp thoại (không cần viết nội dung, đã có sẵn):
   - File Assets/\_Project/Data/Triggers/DialogueAsset.asset đã có sẵn 4 câu thoại thật rồi, không cần viết thêm gì.
   - Mở scene Chapter1 hiện tại (không cần Additive, mở trực tiếp scene này để test), tìm object có component "Dialogue Trigger" trong Hierarchy (gõ tìm "Dialogue" ở ô search).
   - Tạo 1 nút bấm để test: chuột phải Hierarchy > UI > Button. Chọn Button vừa tạo, ở Inspector kéo xuống mục "On Click ()" > bấm dấu "+" > kéo object DialogueTrigger vào ô Object vừa hiện ra > ở dropdown bên cạnh chọn "DialogueTrigger > PlayDialogue()".
   - Test: bấm Play, bấm nút vừa tạo — chữ hiện ra kiểu gõ máy chữ, có mũi tên nhấp nháy, bấm Space/Enter chuyển câu tiếp theo, hết 4 câu thì tự tắt.
   - Câu hỏi kiểm tra: câu thoại thứ 3 nội dung là gì?

4. Test lại túi đồ (Inventory) — không cần chờ Thuận, tự tạo vật phẩm tạm để test:
   - Mở Chapter1.unity, tìm object InventorySystem có sẵn.
   - Tự tạo tạm 1-2 Item Data đơn giản (xem cách làm ở phần việc của Thuận, làm y hệt nhưng đặt itemId khác, ví dụ test_item_1) — chỉ để có cái gì đó test giao diện, không cần đúng nội dung thật.
   - Đặt 1-2 vật phẩm này lên model bất kỳ, thêm Pickup Item, kéo đủ field như hướng dẫn của Thuận việc 4.
   - Nhặt xong, mở túi đồ (Tab). Kiểm tra: icon + tên hiện đúng từng ô, ô trống thì màu xám, vật phẩm tick "Is Key Item" thì viền màu vàng, vật phẩm thường thì viền màu thường.
   - Bấm vào 1 vật phẩm xem có hiện mô tả không, không bị lỗi gì.
   - Sau này khi Thuận merge xong vật phẩm thật (chìa khoá, ảnh gia đình), test lại 1 lần nữa cho chắc — nhưng phần việc này của bạn coi như xong khi test bằng vật phẩm tạm đã ra đúng kết quả.
   - Câu hỏi kiểm tra: vật phẩm nào trong túi đồ (của bạn tự tạo) có viền vàng, vì sao?

---

TUẤN ANH — làm 3 việc

1. Làm 1 chỗ núp trốn dưới gầm giường:
   - Dùng model có sẵn: Assets/\_Project/Models/Props/Furniture/Kenney/bedDouble.glb.
   - Thêm component "Hide Spot" vào giường.
   - Mở thêm Chapter1.unity kiểu Additive (xem "Vài thao tác Unity cơ bản" ở đầu file) để lấy object Player, kéo vào ô "\_playerController". Tạo 1 Empty GameObject con đặt ngay dưới gầm giường, kéo vào ô "\_hidePosition".
   - Test: lại gần giường bấm E → nhân vật chui vào đúng chỗ núp. Bấm E lần nữa → về đúng chỗ cũ.
   - Câu hỏi kiểm tra: lúc đang núp, nhân vật có bị va chạm (Collider) hay không? (xem trong code HideSpot.cs sẽ biết)

2. Làm 1 chỗ núp trốn khác, sau tủ sách:
   - Dùng model có sẵn: Assets/\_Project/Models/Props/Furniture/Kenney/bookcaseClosedWide.glb.
   - Làm y hệt việc 1 (kể cả bước mở thêm Chapter1.unity kiểu Additive để lấy Player) nhưng với tủ sách này.
   - Câu hỏi kiểm tra: bấm E thoát khỏi chỗ núp ra, nhân vật có về ĐÚNG 100% vị trí cũ không, hay bị lệch/kẹt vào tường chút nào?

3. Chỉnh lại cây/cỏ/đường đi hai bên lối vào Main Menu (việc này làm TRỰC TIẾP trong scene MainMenu, không cần tạo scene mới vì đang chỉnh chính cảnh Main Menu):
   - Mở scene MainMenu.
   - Chọn object Terrain, ở thanh công cụ Inspector bấm icon "Paint Trees" — chỉnh lại SỐ LƯỢNG cây ven đường sao cho vừa mắt, không quá thưa cũng không quá dày, không cây nào mọc đè lên đường đi.
   - Kiểm tra kích cỡ đường đi (path/lối đi) hiện có bị méo, bị giãn hình, hoặc bị lệch tỉ lệ không — nếu có, chỉnh lại texture/kích thước cho path thẳng và đúng tỉ lệ.
   - Bấm icon "Paint Details" — chỉnh lại cỏ hai bên đường cho hợp lý (không quá trống, không quá rậm che khuất đường).
   - Mục tiêu cuối: bấm Play, xem camera tự chạy giới thiệu (lúc mở Main Menu) — cảnh 2 bên đường phải nhìn RÕ NÉT, không bị lỗi hình, không cây/cỏ kỳ lạ lọt vào khung hình.
   - Câu hỏi kiểm tra: bạn đã chỉnh sửa mấy chỗ (cây / path / cỏ), mỗi chỗ sửa gì cụ thể?

---

Việc khác (không đổi so với trước): viết báo cáo, vẽ use case, làm slide, viết kịch bản thuyết trình, quay video demo, chụp hình minh hoạ, test theo checklist, ghi bug đầy đủ, chuẩn bị dữ liệu test, build thử nhiều máy, chuẩn bị file nộp.
