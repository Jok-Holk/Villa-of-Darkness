# BIỆT THỰ BÓNG TỐI — Villa of Darkness

### GAME DESIGN DOCUMENT

**Horror · Survival · Puzzle · PC · Unity 3D · Phong cách PS1/PS2**

---

## 1. TỔNG QUAN DỰ ÁN

| Thuộc tính     | Nội dung                                                                  |
| -------------- | ------------------------------------------------------------------------- |
| Tên game       | Biệt Thự Bóng Tối (Villa of Darkness)                                     |
| Thể loại       | Horror Survival / Puzzle / Story-driven                                   |
| Engine         | Unity 3D (URP)                                                            |
| Đồ hoạ         | Low-poly PS1/PS2, model 3D retopology thủ công, CRT/grain post-process    |
| Âm thanh       | Lồng tiếng người thật — 9 nhân vật: nhân vật chính, ma, ký ức gia đình Đỗ |
| Nền tảng       | PC Windows                                                                |
| Thời gian chơi | ~1–2 giờ (toàn bộ 4 chapter)                                              |

### 1.1 Tóm Tắt Câu Chuyện

Một ngôi biệt thự Pháp cổ nằm giữa rừng thông Đà Lạt đã giam cầm một thứ ác quỷ từ năm 1965, khi toàn bộ gia đình Đỗ biến mất không dấu vết. Qua bốn mốc thời gian — 2000, 1970, 1990, 2020 — bốn người trẻ lần lượt bước vào, không ai thoát ra nguyên vẹn.

Mỗi người để lại một mảnh ghép: vật phẩm họ tìm thấy nhưng không hiểu công dụng, nhật ký viết dở, câu hỏi còn dang dở. Người đến cuối — Lan Anh, năm 2020 — là người duy nhất sở hữu đủ ba mảnh ghép để đối mặt với bí ẩn thực sự.

### 1.2 Concept Cốt Lõi

- **Di sản vật phẩm xuyên thời gian:** Item tìm được ở chapter trước được chapter sau kế thừa. Người chơi cảm nhận được sự liên kết giữa các nạn nhân.
- **Cái chết là tất yếu ở Ch.1–3:** Người chơi biết trước kết cục — nhưng không biết cái chết đến như thế nào. Tạo ra sức căng thay vì hy vọng sinh tồn.
- **Bí ẩn piano xuyên chapter:** Bài nhạc phong ấn được mã hoá qua 4 chapter. Không ai trong Ch.1–3 có đủ manh mối để hiểu toàn bộ — chỉ Lan Anh, với di sản của cả ba người trước, mới có thể hoàn thành.
- **Ma thuần Việt Nam:** Ma Vú Dài và Ma Da — hình tượng kinh dị dân gian ít được khai thác trong game.
- **Lồng tiếng đầy đủ:** Không chỉ 4 nhân vật chính — ma có tiếng thì thầm, nhân vật trong ký ức có lời thoại khi người chơi tương tác với di vật.

---

## 2. BỐI CẢNH & THẾ GIỚI

### 2.1 Biệt Thự Đỗ Gia — Lịch Sử

Xây dựng năm 1945 bởi Đỗ Văn Minh — địa chủ giàu có người Việt. Kiến trúc Đông Dương Late phase (1920–1945), 3 tầng, tháp canh góc đông, vườn cây cổ thụ, giếng đá ở sân sau.

Kiến trúc chuẩn: Tường vàng ochre, mái dốc 35° ngói đất nung, galerie bao quanh, cửa sổ cao 2.4m, jalousie gỗ xanh lá. Tầng trệt nâng 0.8m khỏi mặt đất, perron rộng 4m phía trước.

| Năm       | Sự kiện                                                                                                                                                                                                                                                 |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1945      | Đỗ Văn Minh hoàn thiện biệt thự. Mang về ba vật phẩm từ một cái giếng ngoài rừng để phong ấn thứ sống trong đó.                                                                                                                                         |
| 1963      | Vợ ông Đỗ — bà Lan — bắt đầu viết nhật ký về thứ trong giếng. Bà che tất cả gương trong nhà bằng vải. Gương vestibule được phủ vải đỏ — không ai được chạm. Bà Lan phát hiện ra bài nhạc phong ấn và bắt đầu ghi lại bằng phấn lên tường phòng bé Linh. |
| 1965      | Toàn bộ gia đình Đỗ biến mất. Chính quyền điều tra, không tìm thấy thi thể. Nhà bị niêm phong, sau đó bỏ hoang. Nét phấn bà Lan ghi còn lại trên tường — 5 nốt, phần còn lại bị xóa mờ hoặc phấn hết.                                                   |
| 1970–2020 | Bốn người trẻ lần lượt bước vào. Không ai thoát.                                                                                                                                                                                                        |

### 2.2 Bản Đồ — Các Khu Vực

Thiết kế: Mỗi chapter dùng 1 khu vực chính của biệt thự. Asset được reuse giữa các chapter với lighting và dressing khác nhau. Tất cả các tầng có cùng footprint (~16m × 20m). Bếp là nhà phụ tách biệt phía sau — chuẩn kiến trúc Đông Dương.

- **Chapter 1 (2000) — Tầng trệt:** Vestibule, salon (phòng khách + piano), phòng ăn, thư phòng, sân trước, sân sau + giếng, nhà phụ (bếp + kho).
- **Chapter 2 (1970) — Tầng 1:** Hành lang dài, thư phòng (tuyến xuống từ cầu thang), 3 phòng ngủ, phòng tắm, cầu thang chính.
- **Chapter 3 (1990) — Tầng 2 + Tháp canh:** 2 phòng ngủ con, phòng của vợ (bà Lan), tháp canh, sân sau.
- **Chapter 4 (2020) — Toàn bộ (reuse) + Tầng hầm bí mật:** Chỉ mở khoá ở Chapter 4.

| Khu vực                | Dùng ở Chapter         | Ghi chú                                                                                                        |
| ---------------------- | ---------------------- | -------------------------------------------------------------------------------------------------------------- |
| Vestibule + salon      | Ch.1, Ch.4             | Vestibule: gương phủ vải đỏ. Salon: piano — trung tâm gameplay cả hai chapter                                  |
| Phòng ăn               | Ch.1                   | Tờ nhạc trong ngăn kéo bàn ăn — 5 nốt được khoanh tròn: D E G A F                                              |
| Thư phòng (tầng trệt)  | Ch.1, Ch.2, Ch.3, Ch.4 | Hộp nhạc (Ch.1), nhật ký (Ch.2), máy ghi âm (Ch.3), cửa hầm (Ch.4)                                             |
| Nhà phụ (bếp + kho)    | Ch.1                   | Bảng ký hiệu nốt nhạc. Foreshadow sân sau và giếng                                                             |
| Hành lang tầng 1       | Ch.2, Ch.4             | Tối nhất, nhiều tranh ảnh gia đình Đỗ                                                                          |
| Phòng ngủ bà Lan       | Ch.2                   | Nhật ký + audio ký ức. Gương phủ vải trên tường                                                                |
| Phòng bé Linh          | Ch.2                   | Tường phía tây: nét phấn của bà Lan — 5/7 nốt piano, phần còn lại mờ. Gió không đổi hướng — clue tìm gương bạc |
| Phòng tắm              | Ch.2, Ch.4             | Bồn tắm nhập khẩu (lore: ông Đỗ đặt riêng) = nơi Ma Da ẩn nấp                                                  |
| Phòng ngủ con (tầng 2) | Ch.3                   | Đồ chơi cũ, audio ký ức Đỗ Minh, mảnh bản đồ #1                                                                |
| Phòng bà Lan (tầng 2)  | Ch.3                   | Audio ký ức, mảnh bản đồ #2. Horror: player vào phòng của chính con ma đang patrol                             |
| Tháp canh              | Ch.3                   | View toàn sân, mảnh bản đồ #3, công tắc tần số #3                                                              |
| Sân sau + giếng        | Ch.1 (chết), Ch.4      | Điểm kết thúc Ch.1, điểm phong ấn Ch.4                                                                         |
| Tầng hầm               | Ch.4 only              | Phòng thờ cuối — bàn thờ 3 vật phẩm                                                                            |

### 2.2.1 Danh Sách Phòng — Tên Pháp + Việt (theo bản vẽ tay)

Biệt thự 3 tầng, footprint ~16×20m. Mỗi tầng có hành lang hình dấu "+" ở trung tâm — chuẩn kiến trúc Đông Dương Late phase. Galerie bao quanh từng tầng (rộng 2.5m, mái che). Cầu thang chính ở trung tâm. Cầu thang riêng dẫn lên tháp canh từ tầng 2.

**Tầng Trệt (Rez-de-chaussée)**

| Tên Việt (bản vẽ)         | Tên Pháp chuẩn        | Diện tích      | Vị trí trên bản đồ | Ghi chú gameplay                                                                          |
| ------------------------- | --------------------- | -------------- | ------------------ | ----------------------------------------------------------------------------------------- |
| Hành lang trung tâm       | Galerie centrale      | ~40m² (hình +) | Trung tâm          | Trục di chuyển chính. Galerie ngoài trời bao quanh 3 mặt.                                 |
| Phòng Khách / Salon piano | Salon de réception    | ~25m²          | Dưới trái          | Piano Bösendorfer cổ. Lò sưởi (lò sưởi đá). Tủ cabinet bị kẹt ngăn kéo.                   |
| Phòng Ăn                  | Salle à manger        | ~20m²          | Trên trái          | Bàn ăn gỗ tếch 10 chỗ. Vào từ đây (cửa sổ mở). Ngăn kéo: tờ nhạc 5 nốt.                   |
| WC / Buồng vệ sinh        | Cabinet / WC          | ~6m²           | Trên phải          | Không có Ma Da. Atmospheric decay.                                                        |
| Phòng Sân / Véranda       | Véranda               | ~15m²          | Dưới phải          | Kết nối hành lang → sân sau. Cửa sổ nhìn ra giếng.                                        |
| Thư Phòng                 | Cabinet de travail    | ~18m²          | Phía sau Véranda   | Bị khoá từ bên trong cho đến khi piano giải xong. Hộp nhạc, nhật ký, máy ghi âm, cửa hầm. |
| Nhà Phụ — Bếp             | Cuisine (dépendance)  | ~15m²          | Tách biệt, sân sau | Tường loang lổ. Bếp củi mục nát. Không có item quan trọng.                                |
| Nhà Phụ — Kho             | Débarras (dépendance) | ~10m²          | Liền bếp           | **Bị khoá.** Bảng ký hiệu nốt nhạc treo tường. Khoá bằng KEY_01.                          |

**Tầng 1 (Premier étage)**

| Tên Việt (bản vẽ)         | Tên Pháp chuẩn      | Diện tích      | Vị trí trên bản đồ | Ghi chú gameplay                                                                                            |
| ------------------------- | ------------------- | -------------- | ------------------ | ----------------------------------------------------------------------------------------------------------- |
| Hành lang tầng 1          | Couloir du premier  | ~30m² (hình +) | Trung tâm          | Tối nhất. Tranh ảnh gia đình Đỗ trên tường. Audio ký ức đầu tiên.                                           |
| Phòng Tắm tầng 1          | Salle de bains      | ~8m²           | Trái               | **Ma Da trong bồn tắm.** Để cửa ngỏ = nguy hiểm.                                                            |
| Phòng Trống               | Chambre vide        | ~16m²          | Phải trên          | Không có đồ đạc — gia đình không kịp dọn vào. Ghost dừng trước cửa trong patrol. 1 tranh lật mặt vào tường. |
| Phòng Bà Lan I            | Chambre de Madame I | ~18m²          | Trái giữa          | Phòng ngủ chính bà Lan. Nhật ký + audio. Gương phủ vải đỏ. Tủ khoá: KEY_05 bên trong.                       |
| Phòng Bà Lan II / Boudoir | Boudoir de Madame   | ~14m²          | Phải giữa          | Phòng thay đồ / riêng tư. Lore: bà Lan trốn vào đây những đêm cuối 1964. Cửa có then từ bên trong.          |
| Phòng Ông Đỗ              | Chambre de Monsieur | ~18m²          | Trái dưới          | **Riêng với vợ — đúng chuẩn Đông Dương.** Bàn làm việc nhỏ. Dưới bàn: KEY_05 (Ch.2).                        |
| Phòng Bé Linh             | Chambre de la fille | ~16m²          | Phải dưới          | **Tường phía tây: nét phấn bà Lan — 5/7 nốt piano.** Gió không đổi hướng (clue tìm cửa ẩn).                 |
| Ban Công tầng 1           | Balcon              | ~10m²          | Giữa dưới          | Nhìn xuống sân trước. Galerie tiếp nối hai bên.                                                             |

**Tầng 2 (Deuxième étage)**

| Tên Việt (bản vẽ)          | Tên Pháp chuẩn      | Diện tích      | Vị trí trên bản đồ | Ghi chú gameplay                                                                                                               |
| -------------------------- | ------------------- | -------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------ |
| Hành lang tầng 2           | Couloir du deuxième | ~25m² (hình +) | Trung tâm          | Lá rừng lọt qua khe. Mạng nhện dày nhất nhà.                                                                                   |
| Kho tầng 2                 | Débarras            | ~8m²           | Trái trên          | Đồ cũ lỉnh kỉnh. Không có item quan trọng — decoy room.                                                                        |
| Thang lên tháp             | Escalier de la tour | ~4m²           | Giữa trên          | Cầu thang hẹp sắt. **Cửa thép — chỉ mở được từ tháp xuống.**                                                                   |
| Phòng Con Trai — Đỗ Minh   | Chambre du fils     | ~18m²          | Phải trên          | Con tàu gỗ, đồ chơi cũ. Audio ký ức Đỗ Minh. **Mảnh bản đồ #1.**                                                               |
| Phòng Trà                  | Salon de thé        | ~15m²          | Trái giữa          | Bàn trà cổ, ghế mây. Tranh sơn dầu. Lore: ông Đỗ tiếp khách riêng. Manh mối về giếng (Ch.3).                                   |
| Phòng Tắm tầng 2           | Salle de bains (2e) | ~8m²           | Phải giữa          | Bồn tắm cạn — không có Ma Da ở đây.                                                                                            |
| Phòng Bà Lan tầng 2        | Chambre de retraite | ~18m²          | Trái dưới          | **Phòng bà Lan dùng những tháng điên loạn cuối đời.** Audio ký ức kinh dị nhất. **Mảnh bản đồ #2.** Ma patrol VÀO phòng này.   |
| Phòng Bé Linh / Phòng Chơi | Salle de jeux       | ~16m²          | Phải dưới          | Đồ chơi, bức vẽ trẻ con. Audio ký ức bé Linh.                                                                                  |
| Ban Công tầng 2            | Balcon (2e)         | ~10m²          | Giữa dưới          | Nhìn ra vườn và giếng.                                                                                                         |
| **Tháp Canh**              | **Tour de guet**    | **~12m²**      | Góc đông           | **Safe zone tuyệt đối — Ma Vú Dài KHÔNG vào.** View 360° sân. Mảnh bản đồ #3. Công tắc #3. Nơi ông Đỗ mất trí ngồi nhìn giếng. |

> **Lưu ý kiến trúc:** Ông Đỗ và bà Lan có phòng ngủ **riêng biệt** (Chambre de Monsieur tầng 1, Chambre de Madame I tầng 1) — đúng chuẩn kiến trúc biệt thự Đông Dương 1920–1945 cho gia đình khá giả. Bà Lan về sau lui về tầng 2 (Chambre de retraite) khi tình trạng tâm thần xấu đi.

---

### 2.3 Timeline 4 Chapter

| Chapter | Năm  | Nhân vật       | Di vật để lại                    | Kết cục            |
| ------- | ---- | -------------- | -------------------------------- | ------------------ |
| Ch.1    | 2000 | Minh Khoa, 21t | Hộp âm nhạc đồng (không mở được) | Bị kéo xuống giếng |
| Ch.2    | 1970 | Bích Ngọc, 19t | Tấm gương bạc (vỡ một góc)       | Bị hút vào gương   |
| Ch.3    | 1990 | Tuấn Hùng, 22t | Lọ muối đen có ký tự cổ          | Bị Ma Vú Dài bắt   |
| Ch.4    | 2020 | Lan Anh, 23t   | Kế thừa tất cả                   | Tuỳ người chơi     |

---

## 3. HỆ THỐNG NHÂN VẬT & LỒNG TIẾNG

**Triết lý:** Không chỉ 4 nhân vật chính. Ma CÓ lời thoại. Nhân vật trong ký ức CÓ lời thoại khi người chơi tương tác với di vật. Tổng cộng 9 nhân vật cần lồng tiếng.

### 3.1 Nhân Vật Chính — 4 Người

| Tên       | Tuổi/Năm     | Giọng                        | Đặc điểm VA                                     |
| --------- | ------------ | ---------------------------- | ----------------------------------------------- |
| Minh Khoa | 21, năm 2000 | Nam, miền Nam, hơi hồn nhiên | Chuyển từ tò mò → hoảng loạn                    |
| Bích Ngọc | 19, năm 1970 | Nữ, miền Trung, chậm rãi     | Giọng dịu dàng nhưng kiên định. Độc thoại dài   |
| Tuấn Hùng | 22, năm 1990 | Nam, miền Nam, nhanh, tự tin | Hoài nghi rõ ràng → vỡ oà khi đối mặt thực tế   |
| Lan Anh   | 23, năm 2020 | Nữ, trung tính, trầm ấm      | Quyết đoán nhưng đau đớn. Cảm xúc phức tạp nhất |

### 3.2 Nhân Vật Ma — 2 Thực Thể

#### Ma Vú Dài — Bà Lan

Linh hồn của bà Lan — vợ ông Đỗ — bị biến dạng sau khi chết oan. Cô không hẳn là ác; cô đang tìm kiếm điều gì đó.

- **Ngoại hình:** Thân trên phụ nữ lơ lửng, váy trắng ố vàng, tóc xõa che mặt, không có chân.
- **Di chuyển:** Chậm, lướt, xuyên tường. Tăng tốc khi người chơi chạy.
- **Phát hiện:** Nghe âm thanh (bán kính 8m) + thị giác thẳng (góc 90 độ, 12m).
- **Điểm yếu:** Cửa gỗ đóng kín + muối rải ngưỡng cửa (Ch.3 trở đi).

**Lời Thoại — Ma Vú Dài:**

> "Con ơi... về nhà với má đi..."
> "Sao các con cứ chạy trốn... má không làm gì đâu..."
> "(gọi tên nhân vật) Khoa ơi... Ngọc ơi... Hùng ơi... Anh ơi..."
> "(đứng trước cửa phòng bà Lan, Ch.3) Con đừng vào... đó là phòng của má... má chưa đi đâu cả..."
> "(đứng trước tầng hầm) Đừng xuống dưới đó... nó đang đợi ở dưới..."
> "(tiếng hát khe khẽ khi patrol) À ơi... con ngủ ngoan... à ơi..."
> _[Phát trong khi đang patrol — không định hướng, như âm thanh môi trường]_

#### Ma Da — Thứ Trong Giếng

Không rõ nguồn gốc. Cổ hơn ngôi biệt thự. Tồn tại trong bất kỳ mặt phẳng nước nào. Bà Lan phủ vải lên tất cả gương trong nhà năm 1963 chính vì thực thể này.

- **Cơ chế:** Nếu player nhìn vào mặt nước/gương quá 3 giây → bị kéo vào → chết.
- **Cảnh báo:** Mặt nước gợn sóng không có gió. Màn hình đổ xanh lạnh.
- **Gương vestibule (Ch.1):** Phủ vải đỏ — không trigger. Nếu player cố nhấc vải thì trigger ngay.
- **Tiêu diệt (Ch.4):** Dùng tấm gương bạc đã phục hồi soi vào mặt nước — bị nhốt trong gương.

**Lời Thoại — Ma Da:**

> "Xuống đây... xuống đây cùng tao..."
> "Mày thấy tao chưa... mày đã thấy tao rồi..."
> "(khi player né) Sao không nhìn tao... mày sợ à..."
> "(bị gương nhốt) KHÔNG... KHÔNG ĐƯỢC... THẢ TAO RA..."

### 3.3 Nhân Vật Ký Ức — Gia Đình Đỗ

Khi người chơi tìm thấy di vật, game phát đoạn audio ngắn — như mảnh ký ức bị giam cầm. Màn hình nhạt, giọng như từ xa vọng lại.

| Nhân vật                | Vai trò                    | Giọng                              | Xuất hiện ở                           |
| ----------------------- | -------------------------- | ---------------------------------- | ------------------------------------- |
| Đỗ Văn Minh (Cha)       | Chủ nhà, người mang thứ về | Nam trung niên, uy quyền → sụp đổ  | Ch.3 (máy ghi âm + nhật ký thư phòng) |
| Bà Lan (Mẹ)             | Người đầu tiên thấy ma     | Nữ trung niên, lo lắng → điên loạn | Ch.2 (nhật ký phòng ngủ)              |
| Đỗ Minh (Con trai, 12t) | Bảo vệ em                  | Nam trẻ, ngây thơ → khủng hoảng    | Ch.3 (đồ chơi phòng con)              |
| Đỗ Linh (Con gái, 8t)   | Nhìn thấy nhiều nhất       | Nữ trẻ, giọng con nít trong trẻo   | Ch.2 + Ch.3 (búp bê, bức vẽ)          |

**Mẫu Lời Thoại Ký Ức:**

_[Người chơi tìm thấy nhật ký bà Lan (Ch.2 — phòng ngủ bà Lan)]_

> Bà Lan (ký ức): "Ngày 3 tháng 9, 1963. Con Linh nói thấy mặt người trong giếng. Tôi bịt giếng lại bằng vải đỏ. Sáng hôm sau tấm vải biến mất."

_[Người chơi tìm thấy nét phấn trên tường phòng bé Linh (Ch.2)]_

> Bà Lan (ký ức): "Tôi đã tìm ra bài nhạc. Năm nốt đầu — tôi khắc lên đây để nhớ. Nhưng phấn hết rồi. Hai nốt còn lại... tôi sẽ tìm cách khác."

_[Người chơi tìm thấy đồ chơi con tàu của Đỗ Minh (Ch.3 — phòng ngủ con)]_

> Đỗ Minh (ký ức): "Ba ơi, ba đừng xuống tầng hầm nữa. Con nghe tiếng ba nói chuyện với ai đó ở dưới đó. Nhưng nhà mình không ai ở dưới đó cả."

_[Người chơi tìm thấy bức vẽ của Linh (Ch.3 — phòng bà Lan)]_

> Đỗ Linh (ký ức): "(giọng con nít) Cái người trong giếng... nó nói nó ở đây lâu lắm rồi. Nó nói nó đói. Nó nói... nó muốn mình xuống chơi cùng."

_[Người chơi tìm thấy nhật ký cuối Đỗ Văn Minh (Ch.3 — thư phòng)]_

> Đỗ Văn Minh (ký ức): "Tôi mang nó về từ cái giếng ngoài rừng. Ba vật phẩm tôi làm ra để phong ấn: hộp nhạc, gương bạc, muối đen. Riêng lẻ chúng vô dụng. Cùng nhau chúng đủ mạnh. Đừng để ba vật chia lìa."

---

## 4. INTRO SEQUENCES & STORYTELLING IN-GAME

**Nguyên tắc:** Bối cảnh nhân vật không truyền đạt qua text hay loading screen. Mỗi chapter có một intro sequence riêng — dùng in-engine cutscene. Control chuyển về người chơi đúng lúc nhân vật bước qua ngưỡng cửa.

### 4.1 Cấu Trúc Chung

1. Static noise (TV cũ) 1.5 giây → Văn bản trắng: tên chapter + năm.
2. Fade in cảnh trước khi vào — nhân vật đứng ngoài cổng, tự nói monologue.
3. Nhân vật bước qua cổng — CONTROL chuyển về người chơi khi bàn chân chạm ngưỡng cửa biệt thự.
4. Gameplay bắt đầu. Tutorial hint nhỏ ở góc màn hình (chỉ Ch.1).

### 4.2 Intro Chapter 1 — Năm 2000

_Cảnh: Cổng sắt gỉ sét. 9 giờ tối. Sương Đà Lạt._

> [First-person. Tay cầm đèn pin và máy ảnh film. Ánh đèn chiếu lên bảng tên 'Đỗ Gia' hoen rỉ.]
>
> Minh Khoa: "Biệt thự Đỗ Gia. Năm 1945, kiến trúc sư Pháp phối hợp ông Đỗ Văn Minh. Đề tài tốt nghiệp của mình không thể thiếu cái này."
>
> [Cửa chính khoá. Thấy cửa sổ phòng ăn hé mở.]
>
> Minh Khoa: "Chụp nhanh vài tấm, phác thảo mặt tiền, rồi về. Không dám ở lại đêm đâu."
>
> Minh Khoa: "À... cửa sổ phòng ăn mở rồi. Thôi vào đường đó vậy. Xin lỗi ông Đỗ, mình không cố ý xâm phạm đâu."
>
> [Chui qua cửa sổ. Fade 0.5s. Fade in bên trong phòng ăn — GAMEPLAY BẮT ĐẦU.]

### 4.3 Intro Chapter 2 — Năm 1970

_Cảnh: Đường mòn vào biệt thự. Ban ngày nhưng bầu trời xám._

> [First-person. Tay cầm đèn dầu đang cháy. Tay kia cầm cuốn sổ nhỏ bọc vải.]
>
> Bích Ngọc: "Bà ơi, con đã đến rồi. Bà nói cái nhà này giữ thứ có thể cứu họ. Con không hiểu hết — nhưng con tin bà."
>
> [Mở sổ ra — sơ đồ đơn giản và chữ bút chì: 'Tấm gương bạc — phòng ngủ tầng 1 — tường phía tây'.]
>
> Bích Ngọc: "Tấm gương bạc. Tìm được rồi đem về. Bà dặn đừng nhìn vào mặt nước trong nhà — tuyệt đối không."
>
> [Đẩy cửa chính — mở dễ dàng. GAMEPLAY BẮT ĐẦU trong vestibule. Đèn dầu dao động nhẹ dù không có gió.]

### 4.4 Intro Chapter 3 — Năm 1990

_Cảnh: Hoàng hôn. Tuấn Hùng đứng chụp ảnh biệt thự từ xa._

> [Third-person brief 5 giây — nhìn Hùng từ sau lưng. Sau đó chuyển first-person khi anh quay về phía cổng.]
>
> Tuấn Hùng: "Biệt thự Đỗ Gia. Hai mươi lăm năm không ai dám vào. Bài báo này mà lên trang nhất... mình được thăng chức chắc."
>
> [Lấy từ túi lọ muối đen. Xem xét.]
>
> Tuấn Hùng: "Cái lọ này... người gửi không để tên. Chỉ có mảnh giấy: 'Khi vào nhà đó — mang cái này.' Thôi mang theo cho chắc bụng."
>
> Tuấn Hùng: "Ổn. Chỉ là một cái nhà cũ thôi. Mình là nhà báo — mình cần bằng chứng, không phải tin đồn."
>
> [Bước vào. GAMEPLAY BẮT ĐẦU. Máy ghi âm trong túi tự click dù Hùng không bấm.]

### 4.5 Intro Chapter 4 — Năm 2020

_Cảnh: Ban đêm. Lan Anh đứng trước cổng, điện thoại trên tay._

> [First-person ngay từ đầu. Nhìn vào ảnh trên điện thoại — Tuấn Hùng năm 1990 chụp trước biệt thự.]
>
> Lan Anh: "Chú Hùng. Mười hai tuổi con đã mất chú. Giờ con hai mươi ba. Mười một năm con mới tìm ra được đây."
>
> [Mở ba lô — ba vật phẩm nằm trong đó. Hộp nhạc. Gương bạc. Lọ muối đen. Và một tờ giấy gấp — nét bút chú Hùng.]
>
> Lan Anh: "Ba thứ này — chú để lại trong di chúc. Ghi rõ tên từng thứ. Ghi 'đừng để ba vật chia lìa'. Con không hiểu lúc đó. Giờ thì hiểu rồi."
>
> Lan Anh: "Và cả tờ nhạc này. Bảy nốt. Chú không giải thích gì thêm. Nhưng con biết nó quan trọng."
>
> Lan Anh: "Minh Khoa. Bích Ngọc. Chú Hùng. Các người đã không thoát được. Con sẽ thoát — cho tất cả."
>
> [Bước vào. GAMEPLAY BẮT ĐẦU. Tiếng đàn piano tự chơi từ xa — bài hát ru của bà Lan.]

---

## 5. THIẾT KẾ GAMEPLAY

### 5.1 Cơ Chế Cốt Lõi

#### Di Chuyển & Tương Tác

- Góc nhìn thứ nhất (first-person). Không có arms model phức tạp.
- Di chuyển: WASD. Cúi (C). Chạy (Shift) — tạo tiếng ồn, thu hút ma.
- Tương tác: E = tương tác nhanh. Giữ E = nghe monologue về vật phẩm.
- Nhân vật chết ngay khi bị ma chạm — không có HP, không có hồi máu.

#### Hệ Thống Nguồn Sáng

| Chapter | Nguồn sáng       | Cơ chế đặc biệt                                        |
| ------- | ---------------- | ------------------------------------------------------ |
| Ch.1    | Đèn pin (pin AA) | Lắc đèn (F) để phục hồi pin khi pin yếu. Toggle (T).   |
| Ch.2    | Đèn dầu          | Lửa dao động = ma gần. Tắt = 10s trước khi ma tấn công |
| Ch.3    | Đèn pin mạnh     | Tắt đèn chủ động để ẩn náu — không thấy đường          |
| Ch.4    | Điện thoại       | Dùng camera để nhìn vào gương an toàn (tránh Ma Da)    |

#### Hệ Thống Sanity

Thanh sanity không hiện trực tiếp — biểu hiện qua visual và audio.

- **Sanity giảm khi:** Nhìn thấy ma, ở bóng tối hoàn toàn, nghe tiếng khóc liên tục.
- **High (75–100%):** Bình thường.
- **Medium (40–75%):** Màn hình rung nhẹ ở góc, tiếng thở nhanh hơn.
- **Low (10–40%):** Ảo giác — thấy bóng người, nghe tên mình được gọi.
- **Critical (<10%):** Nhân vật tự thì thầm mê sảng. Sau 30 giây tự bước ra khỏi nơi ẩn náu.
- **Hồi sanity:** Đứng trong vùng sáng, hoàn thành câu đố, nghe audio ký ức.

#### Galerie & Hành Lang Ngoài Trời

Biệt thự có galerie bao quanh từng tầng — hành lang mái che rộng 2.5m, kết nối tất cả cửa ra vào bên ngoài và ban công. Galerie là **con đường thứ hai** song song với hành lang bên trong.

**Cơ chế:**

- Galerie **không cần chìa khoá** — di chuyển tự do bất kỳ lúc nào.
- **Sanity drain:** Ở galerie ban đêm = sanity giảm nhẹ liên tục (~30% tốc độ bình thường). Lý do: bóng tối + âm thanh vườn dồn vào tai (côn trùng, gió, tiếng giếng xa xa).
- **Ch.1:** Ma Vú Dài không patrol galerie → galerie là đường tắt an toàn nhưng trả bằng sanity.
- **Ch.2 trở đi:** Ma có thể ra galerie. Trời mưa (Ch.2) = galerie tối hơn, sanity drain ×1.5.
- **Âm thanh môi trường:** Tiếng côn trùng nhiệt đới (ban đêm) + tiếng lá rừng gió đập vào cột galerie. Khi sanity thấp: tiếng bước chân hư ảo trên galerie dù không ai ở đó.

#### Đèn Pin Tắt — Di Chuyển Nhưng Không Đọc

Khi đèn pin tắt (toggle T) hoặc pin hết (battery = 0):

| Hành động       | Có đèn      | Không đèn                                                |
| --------------- | ----------- | -------------------------------------------------------- |
| Di chuyển WASD  | ✅          | ✅ — dùng môi trường làm hướng (ánh trăng, khe sáng cửa) |
| Tương tác E     | ✅          | ❌ — prompt E biến mất, không interact được              |
| Đọc tài liệu    | ✅          | ❌ — không thể đọc chữ trong bóng tối                    |
| Ẩn nấp HideSpot | ✅          | ✅ — cơ thể biết cách trốn                               |
| Nghe audio      | ✅          | ✅ — âm thanh không thay đổi                             |
| Ma phát hiện    | Bình thường | Bình thường — đèn tắt KHÔNG giúp né tránh ma             |

> **Design intent:** Tắt đèn là chiến thuật để dừng drain pin — KHÔNG phải để tàng hình. Ma Vú Dài nghe âm thanh là chính (hearingRadius 8m). Cần lắc đèn (F) để phục hồi, không phải chờ đèn sạc.

#### Biệt Thự Bỏ Hoang — Logic Bảo Tồn Có Chủ Ý

Biệt thự bỏ hoang từ 1965 nhưng không đổ nát hoàn toàn. Lý do lore: **Ma Da (thực thể trong giếng) duy trì ngôi nhà** ở trạng thái "đủ để dụ người vào". Nhà hỏng đúng chỗ cần hỏng, còn đúng chỗ cần còn.

**Decay state per floor (thời điểm Ch.1 — năm 2000, 35 năm bỏ hoang):**

| Khu vực   | Tình trạng       | Chi tiết visual                                                                 |
| --------- | ---------------- | ------------------------------------------------------------------------------- |
| Tầng trệt | Decay trung bình | Bụi 2–3cm, vải rèm mục ố vàng, gỗ sàn kẽo kẹt, vài tấm trần bong nhẹ            |
| Tầng 1    | Decay nặng       | Một tấm ván sàn mục (né được), tranh ố vàng, mùi ẩm mốc rõ                      |
| Tầng 2    | Decay nặng nhất  | Lá rừng lọt qua khe cửa, mạng nhện dày, đồ chơi rải rác như chưa ai dọn từ 1965 |
| Tháp canh | Gần nguyên vẹn   | Kính cửa sổ không vỡ. Bàn và ghế thẳng hàng. Thực thể cố tình bảo tồn.          |
| Nhà phụ   | Hoang phế nhất   | Mái thủng một góc, cỏ dại mọc qua sàn, cửa rỉ sét nhưng vẫn xoay được           |
| Galerie   | Lá mục tích tụ   | Cột lan can sắt uốn gỉ đỏ nhẹ, không mục, vẫn vững                              |

**Cửa và khoá:**

- Cửa kẹt = có thể mở bằng cách đúng (không bị hỏng hẳn).
- Khoá gỉ = vẫn hoạt động nếu có chìa (thực thể duy trì cơ chế khoá để tạo rào cản có kiểm soát).
- Nến tự bắt lửa ở một số nơi nhất định — không giải thích trong game, chỉ có ở nơi thực thể "muốn" người chơi đến.

### 5.2 Hệ Thống AI Ma

#### Ma Vú Dài — State Machine (GhostAI.cs)

| Trạng thái  | Trigger vào         | Hành vi                                                   | Trigger ra                               |
| ----------- | ------------------- | --------------------------------------------------------- | ---------------------------------------- |
| Patrol      | Mặc định            | Di chuyển route định sẵn (random waypoints), phát ambient | Nghe/thấy suspect                        |
| Investigate | Nghe/thấy suspect   | Đến điểm phát hiện, nhìn quanh 8 giây                     | Không thấy gì → Patrol / Thấy → Chase    |
| Chase       | Confirm thấy player | Di chuyển nhanh, phát tiếng khóc cao                      | Mất dấu ngay → Investigate / Chạm → Kill |
| Kill        | Chạm player         | Death screen                                              | —                                        |

**Thông số:** `_hearingRadius = 8f`, `_sightRadius = 12f`, `_sightAngle = 90f`, `_patrolSpeed = 1.5f`, `_chaseSpeed = 4f`.

> **Note:** Trạng thái "Blocked" (gặp cửa/muối) chưa implement — ghost hiện tại không bị chặn bởi muối hoặc cửa đóng. Cần quyết định có thêm state này không trước Ch.3.

#### Ma Da — Trigger System

- Không dùng NavMesh — chỉ dùng trigger zones trên mặt nước/gương.
- Camera hướng về mặt nước + player trong zone: bắt đầu đếm 3 giây.
- Tại 1s: Mặt nước gợn. Tại 2s: Màn hình xanh. Tại 3s: Chết.
- Quay mặt đi = reset đếm giây.
- Gương vestibule (Ch.1): Phủ vải đỏ — không trigger. Cố nhấc vải → trigger ngay (tutorial cảnh báo).
- Ch.4 override: Cầm gương bạc + nhìn vào nước = capture sequence, không chết.

#### Ghost Patrol Routes — Per Chapter

**Chapter 1 (2000) — Tầng trệt:**

- Ma Vú Dài **PATROL TỪ ĐẦU GAME** — không cần trigger spawn. Player phải né tránh ngay từ phút đầu.
- Route vòng (~60 giây/vòng): Hành lang trung tâm → dừng trước cửa salon 8s → Phòng Sân → hành lang → ra galerie sân sau → quay vào qua cửa bếp (bên ngoài) → lại vào trong.
- **Không vào:** Phòng ăn, thư phòng (khoá), kho nhà phụ.
- Sau khi piano giải xong (thư phòng mở): ghost tốc độ +10%, hearingRadius tăng lên 10m.
- **Tactical note:** Player cần timing gap khi ghost vào galerie để tiếp cận piano.

**Chapter 2 (1970) — Tầng 1:**

- Route ban đầu: Hành lang tầng 1 → Phòng trống (dừng, nhìn vào 3s) → Boudoir bà Lan → hành lang → ban công → quay vào.
- **Trigger đặc biệt:** Sau khi gương bạc bị chạm và gương đập vỡ đồng loạt → Ma Vú Dài bắt đầu patrol cả 2 tầng (trệt + 1). Speed tăng từ 1.5f → 2.0f. Route không còn cố định.
- **Không vào:** Thư phòng (cửa khép), tháp (chưa có tháp access Ch.2).

**Chapter 3 (1990) — Tầng 2 + toàn nhà sau trời tối:**

- 10 phút đầu (hoàng hôn): Ma chưa active — player có cửa sổ thám hiểm.
- Sau trời tối: Route tầng 2: Hành lang tầng 2 → Phòng Bà Lan tầng 2 (dừng lâu 15s, như đang ở nhà) → Phòng Con Trai → Phòng Trà → lại hành lang.
- **Tháp canh — safe zone cứng:** Ma đi đến chân cầu thang tháp, dừng lại, phát tiếng khóc ~10s rồi quay đi. KHÔNG leo lên. Lý do lore: tháp là nơi ông Đỗ từng "hợp tác" với thực thể — còn dư âm bảo vệ.
- Sau công tắc #3 kích hoạt: Ma thêm route tầng 1 (patrol cả 3 tầng), tốc độ chase lên 4.5f.

**Chapter 4 (2020) — Cả hai ma đồng thời:**

- Ma Vú Dài patrol toàn bộ 3 tầng (không vào tháp). Speed 2.0f patrol / 5.0f chase.
- Ma Da: trigger trên TẤT CẢ bề mặt nước/gương còn lại. Gaze timer giảm từ 3s → 1.5s.
- Sau khi piano 7 nốt giải: Ma Vú Dài immediate chase từ bất kỳ đâu trong nhà. Race sequence đến tầng hầm.

### 5.3 Bí Ẩn Piano Xuyên Chapter

Piano là trục narrative chính kết nối cả 4 chapter. Bài nhạc phong ấn gồm **7 nốt** — không ai trong Ch.1–3 biết đầy đủ.

#### Trục manh mối piano

| Chapter | Manh mối tìm được                                                           | Nguồn                               |
| ------- | --------------------------------------------------------------------------- | ----------------------------------- |
| Ch.1    | Tờ nhạc 5 nốt (D E G A F) — khoanh tròn                                     | Ngăn kéo bàn ăn                     |
| Ch.2    | Nét phấn của bà Lan: 5/7 nốt trên tường phòng bé Linh                       | Tường phía tây                      |
| Ch.3    | Tờ giấy của Tuấn Hùng ghi đủ 7 nốt — ghép từ nét phấn + nhật ký cuối ông Đỗ | Trong ba lô Hùng (trao cho Lan Anh) |
| Ch.4    | Lan Anh có tờ giấy của Hùng → gõ đủ 7 nốt                                   | Piano salon                         |

#### Cài cắm lore: Tại sao bà Lan biết bài nhạc?

Bà Lan tìm thấy bài nhạc trong nhật ký của ông Đỗ — ông ghi lại trước khi mất trí. Bà cố ghi lại lên tường phòng con gái vì sợ sẽ quên. Phấn hết khi còn 2 nốt. Bà không còn cơ hội tìm phấn khác.

Tuấn Hùng, năm 1990, đọc nhật ký cuối của ông Đỗ trong thư phòng — ông Đỗ có ghi 2 nốt còn lại trong đó. Hùng tự ghép với 5 nốt trên tường phòng Linh (anh đã lên tầng 2 để điều tra). Anh ghi đủ 7 nốt ra giấy trước khi chết, để lại trong di chúc.

### 5.4 Chuỗi Câu Đố Mỗi Chapter

#### Chapter 1 — Tầng Trệt

> **Ghost active từ đầu** — Ma Vú Dài patrol từ phút 0. Player phải né tránh trong suốt quá trình giải câu đố.

**Flow:**

1. **[START]** Bắt đầu trong phòng ăn — vừa chui qua cửa sổ. Ma Vú Dài đang ở đâu đó ngoài hành lang. Tiếng bước chân lướt qua khe cửa. Tutorial hint nhỏ: _"Lắng nghe hướng âm thanh."_

2. **[EXAMINE]** Ngăn kéo bàn ăn → Tờ nhạc cũ, 5 nốt được khoanh tròn bằng bút đỏ: `D E G A F`. Khoa: _"Nốt nhạc... Ai khoanh vào đây vậy? Nhưng mình không biết chơi đàn."_

3. **[EXAMINE]** Vestibule (ra từ hành lang): Gương phủ vải đỏ. Khoa: _"Ủa, sao phủ vải? Kỳ vậy."_ Cố nhấc vải → trigger Ma Da ngay lập tức. Tutorial warning: vải đỏ = không chạm.

4. **[PUZZLE — Chuỗi ngăn kéo bị kẹt]**
   - Salon: Tủ cabinet cổ góc phòng. `[EXAMINE]` → _"Có ngăn kéo dưới cùng bị kẹt. Cần gì đó cứng làm đòn bẩy."_
   - Lò sưởi (fireplace) trong salon: `[EXAMINE]` → Nến cắm vào đế bằng đồng nặng. `[PICKUP]` cây nến → vào inventory.
   - `[INTERACT]` Nến lên ngăn kéo → pry open animation → ngăn kéo mở. Bên trong: **KEY_01** (chìa khoá nhỏ rỉ sét, nhãn "Kho").
   - Khoa: _"Chìa khoá kho... Nhà phụ ở đâu nhỉ?"_

5. **[NAVIGATE + STEALTH]** Ra galerie phía sau (né ma đang patrol hành lang). Galerie = đường tắt an toàn (Ch.1 ma chưa ra đây). Qua sân sau → **lần đầu thấy giếng** (foreshadow — không tương tác được). Vào nhà phụ. Kho: `[USE]` KEY_01 → LOCK_01 mở → bên trong: **Bảng ký hiệu nốt nhạc** (hình vẽ phím piano với tên nốt Việt: Đô-Rê-Mi...).
   - Khoa: _"À! Cái nốt D là Rê, E là Mi... vậy bài này là Rê Mi Sol La Fa. Thử đánh xem."_

6. **[PUZZLE — Piano]** Quay vào salon (phải né ma). Gõ đúng 5 nốt `D-E-G-A-F` trên piano. Âm thanh hộp nhạc. **Cửa thư phòng tự mở khoá.** Khoa: _"Kỳ lạ... cái cửa tự mở?"_

7. **[EXPLORE]** Thư phòng: `[PICKUP]` hộp âm nhạc đồng — không mở được dù cố gắng. Khoa: _"Hộp khóa lạ... chìa đâu không biết."_ → vào inventory (di vật Ch.1 → Ch.4).

8. **[TUTORIAL ẨN NÁU]** Khi quay ra hành lang: Ma Vú Dài bước vào hành lang từ phía salon — đối mặt gần. Khoa: _"Cái gì vậy—"_ Panic — tủ áo hành lang highlight. `[INTERACT]` trốn vào. HideSpot mechanic kích hoạt. Ghost đứng trước tủ 15s → bỏ đi. Khoa thở: _"Ở lại đây thêm 5 phút nữa. Cho an toàn."_

9. **[DEATH SEQUENCE]** Thoát ra khỏi tủ. Tìm đường ra sân sau. Thấy ánh sáng xanh lờ mờ từ giếng → tiến lại → **DEATH SEQUENCE.**

#### Chapter 2 — Tầng 1

**Flow:**

1. Bắt đầu trong vestibule. Gương phủ vải đỏ — Ngọc biết ngay (bà dặn rồi).
2. Thư phòng (tầng trệt): đọc nhật ký bà Lan. Clue: _"gió không đổi hướng"_ + nhắc đến bài nhạc bà đã ghi.
3. Lên tầng 1 qua cầu thang chính. Hành lang dài, tranh ảnh gia đình Đỗ. Nghe audio ký ức đầu tiên.
4. Kiểm tra từng phòng bằng đèn dầu. Phòng bà Lan: nhật ký + audio ký ức. Gương phòng ngủ bị phủ vải.
5. Phòng tắm: Ma Da trong bồn tắm — băng qua hành lang không nhìn vào.
6. **Phòng bé Linh:** Ngọn lửa đứng yên (clue gió). Tường phía tây — **nét phấn bà Lan: 5 nốt nhạc, phần sau mờ/hết phấn**. Ngọc ghi vào sổ nhưng không hiểu dùng làm gì. Gõ tường → tiếng rỗng ô thứ 3 → cửa ẩn → gương bạc.
7. Chạm vào gương → tất cả gương đập vỡ đồng loạt → Ma Vú Dài thức dậy.
8. Chạy xuống cầu thang, tránh mảnh gương vỡ (tạo tiếng thu hút ma).
9. Ra sân sau — cố dùng gương → thiếu hai vật kia → **DEATH SEQUENCE**.

#### Chapter 3 — Tầng 2 + Tháp Canh

**Flow:**

1. Bắt đầu lúc hoàng hôn — 10 phút đầu tương đối an toàn.
2. Thư phòng (tầng trệt): tìm máy ghi âm. Bật công tắc #1. **Đọc nhật ký cuối ông Đỗ — có ghi 2 nốt nhạc còn lại** (nốt 6 và 7).
3. Tầng 1: tìm công tắc #2 trong hành lang (sau tủ ảnh gia đình). Mảnh bản đồ từ phòng ngủ bà Lan + audio ký ức.
4. Tầng 2: Phòng con trai Đỗ Minh — đồ chơi + audio ký ức + mảnh bản đồ #1.
5. Phòng bà Lan (tầng 2): audio ký ức + mảnh bản đồ #2. Horror định hướng: đây là phòng của con ma đang patrol.
6. **Tháp canh:** Nhìn thấy nét phấn bà Lan qua cửa sổ phòng bé Linh (5 nốt). Cộng với 2 nốt trong nhật ký ông Đỗ → **Hùng ghi đủ 7 nốt ra giấy**. Công tắc #3 + mảnh bản đồ #3. Trời tối — Ma Vú Dài bắt đầu patrol.
7. Ghép 3 mảnh bản đồ, quay về thư phòng, phát băng ghi âm.
8. Rải muối tạo hàng rào chạy về cửa — bị chặn ở sân trước → **DEATH SEQUENCE**.

#### Chapter 4 — Kết Hợp Toàn Bộ

**Flow:**

1. Bắt đầu trong vestibule — tiếng piano tự chơi từ xa.
2. Salon: **Gõ 7 nốt trên piano** (từ tờ giấy Hùng để lại). Hộp nhạc tự mở — bên trong có chìa khoá và mảnh gương còn thiếu.
3. Sửa gương: Ghép mảnh → gương hoàn chỉnh. Gương vestibule có thể an toàn xé vải (dùng camera điện thoại nhìn trước — nếu thấy bóng trong camera thì không xé).
4. Sân sau: Rải 8 điểm muối quanh giếng (vòng tròn, visualize rõ bằng ánh nến hắt ra từ cửa sổ) thành vòng tròn.
5. Thư phòng: Dùng chìa khoá mở cửa sau tủ sách. Xuống tầng hầm.
6. Đặt ba vật lên bàn thờ đúng vị trí → cửa tự mở. Trận cuối.

---

## 6. CÁC CHAPTER — FLOW & CUTSCENE CHẾT

### 6.1 Chapter 1 — Cảnh Chết

> [Khoa ra sân sau. Thở phào. Thấy ánh sáng xanh từ giếng.]
>
> Minh Khoa: "Cái gì vậy... ánh sáng trong giếng? Không lẽ có người bị kẹt dưới đó?"
>
> [Cúi xuống nhìn. Mặt nước tối. Gương mặt méo mó nhìn thẳng lên. Mắt trắng đục.]
>
> Minh Khoa: "Ơ... cái gì—"
>
> [Bàn tay từ giếng kéo xuống trong 0.3 giây. Màn hình đen. Tiếng nước. Im lặng.]
>
> Minh Khoa: _(tiếng cuối, như từ dưới nước)_ "...Ai đó... giúp..."
>
> **ĐỖ MINH KHOA · 1979 – 2000**

### 6.2 Chapter 2 — Cảnh Chết

> [Ngọc chạy ra sân sau, gương bạc trên tay. Đứng giữa sân, trăng soi.]
>
> Bích Ngọc: "Gương bạc... bà nói soi vào trăng thì có thể... nhưng bà không nói rõ làm gì tiếp theo. Con thiếu gì đó."
>
> [Hạ gương xuống. Trong mặt gương phản chiếu không phải mặt đất — gương mặt Thứ Trong Giếng đang nhìn lên.]
>
> Bích Ngọc: "(đang nhìn xuống gương) Trời ơi..."
>
> [Gương kéo Ngọc vào. Cô biến mất. Gương rơi, vỡ thêm một góc.]
>
> **NGUYỄN THỊ BÍCH NGỌC · 1951 – 1970**

### 6.3 Chapter 3 — Cảnh Chết

> [Hùng chạy ra sân trước. Không có gì theo. Dừng lại thở.]
>
> Tuấn Hùng: "Thoát rồi... thoát rồi. Ổn. Mình ổn."
>
> [Máy ghi âm trong tay tự bật lên.]
>
> Tuấn Hùng (trong băng): "— nó đứng sau mày rồi. Đừng quay lại. Đừng —"
>
> [Hùng đứng im. Trong bóng tối trước mặt, bóng Ma Vú Dài đổ lên tường — bóng đến từ phía sau anh.]
>
> Tuấn Hùng: "(đang nhìn thấy bóng trước mặt) Không quay lại. Không quay lại. Không—"
>
> [Tiếng vải kéo. Tối.]
>
> **TRẦN TUẤN HÙNG · 1968 – 1990**

### 6.4 Chapter 4 — Ba Kết Thúc

#### Ending 1 — Giải Thoát _(Đủ 8 audio ký ức)_

> [Ba vật tan biến vào ánh sáng trắng. Tiếng bài hát ru của bà Lan — nhẹ nhàng, không biến dạng.]
>
> Bà Lan (ký ức): "Con ơi... cảm ơn con. Chúng tôi được về rồi."
>
> Lan Anh: "Minh Khoa. Bích Ngọc. Chú Hùng. Các người yên nghỉ đi nhé."
>
> [Fade to white. Tiếng bốn giọng người cùng thở ra. Im lặng tuyệt đối.]
>
> _Credits: Bốn bức tranh vẽ tay — bốn người đứng trước biệt thự dưới ánh ban ngày._

#### Ending 2 — Thoát Ra _(Đủ 3 vật, thiếu audio ký ức)_

> Lan Anh: "Tôi đã làm xong việc của tôi. Nhưng... chưa đủ. Vẫn còn gì đó chưa được giải phóng."
>
> [Biệt thự tối, im lặng. Ánh đèn cửa sổ tầng 2 vẫn còn sáng. Fade to black.]
>
> _Credits. Sau credits: tiếng trẻ con cười xa xa._

#### Ending 3 — Thất Bại

> [Lan Anh ngã xuống tầng hầm. Ba vật phẩm vỡ vụn.]
>
> Bà Lan (biến dạng): "Rồi sẽ có người khác đến... rồi sẽ có người khác..."
>
> **LÊ THỊ LAN ANH · 1997 – 2020**
>
> _VÀ RỒI SẼ CÓ NGƯỜI KHÁC._

---

## 7. KỸ THUẬT & ĐỒ HOẠ

### 7.1 Phong Cách PS1/PS2 trong Unity

| Yếu tố            | Thông số                                                         | Ghi chú implementation                       |
| ----------------- | ---------------------------------------------------------------- | -------------------------------------------- |
| Polygon count     | Nhân vật 500–1500 tri. Ma 300–1000 tri. Môi trường 200–800/asset | Retopo thủ công trong Blender                |
| Texture           | 64×64 đến 256×256. Không normal map                              | Pixelate thủ công sau khi bake từ high-poly  |
| Render resolution | 480p internal → upscale 1080p                                    | Pixel Render Target trong URP Camera         |
| CRT Effect        | Scanline overlay + chromatic aberration                          | Custom post-process shader hoặc URP Volume   |
| Film grain        | Strength 0.4–0.6, tăng khi sanity giảm                           | Kết nối sanity float vào post-process volume |
| Vertex jitter     | Snapping verts đến grid 0.05                                     | Vertex snap script trên camera (PS1 wobble)  |
| Fog               | Exponential fog, density 0.03–0.08                               | Dày hơn ở tầng hầm và sân sau                |
| Màu sắc           | Desaturated, lạnh. Highlight đỏ khi nguy hiểm                    | LUT baked sẵn 4 variants theo sanity level   |

### 7.2 Âm Thanh Môi Trường & Nhạc Nền

- **Ambient base layer:** Tiếng gió qua khe cửa + gỗ cũ kẽo kẹt + mưa xa xa (loop).
- **Ma Vú Dài ambient:** Tiếng khóc trẻ con pitch-down reverb — phát khi ma patrol gần player.
- **Ma Da ambient:** Tiếng nước nhỏ giọt + bubble khi thực thể thức.
- **Jump scare design:** Không dùng volume đột ngột. Im lặng 1.5 giây → âm thanh lạ nhỏ ở hướng ngược.
- **Nhạc nền:** Drone ambient không melody, chỉ tần số thấp. Mix theo sanity level qua Unity Audio Mixer Snapshots.
- **Lồng tiếng — 9 nhân vật:** Lồng tiếng là hạng mục thuê dịch vụ bên ngoài, không nằm trong workflow nội bộ. Danh sách đầy đủ 9 nhân vật + spec từng vai được nêu ở Mục 3.

### 7.3 Unity Implementation — Scripts

#### Shader & Rendering

- URP với custom Shader Graph cho PS1 dithering effect.
- Vertex snapping script simulate PS1 vertex jitter.
- Post-process Volume cho CRT, grain, vignette theo từng zone.

#### AI Ma

- `GhostAI.cs` — NavMesh Agent cho Ma Vú Dài: hearing radius + sight cone. State machine 4 trạng thái (Patrol / Investigate / Chase / Kill).
- Ma Da: `GazeTrigger.cs` — trigger zone + raycast từ camera để detect gaze. Không dùng NavMesh.
- `GhostData.cs` — ScriptableObject: tên, prefab, speed, detection radius.
- `GhostInteractionData.cs` — ScriptableObject: gaze duration, warning, detection distance.

#### Item & Inventory

- `ItemData.cs` — ScriptableObject cho mỗi item: tên, mô tả, audio clip monologue, sprite icon, key item flag.
- `InventorySystem.cs` — quản lý inventory, bảo vệ key items không thể drop.
- `InventoryUI.cs` — lưới 2×4, icon, mô tả, monologue playback.
- `InventoryTabHandler.cs` — bắt Tab trên Player (luôn active dù InventoryUI bị SetActive(false)).

#### Save System

- `ItemPersistence.cs` — lưu/load qua **PlayerPrefs** với prefix `VoD_`. Keys: `VoD_Items`, `VoD_Chapter`, `VoD_AudioLogs`.
- `GameData.cs` — static class lưu runtime state: `collectedItems`, `currentChapter`, `audioLogsHeard`.

#### Gameplay Systems

- `PlayerController.cs` — WASD + crouch + run + mouse look + gravity.
- `InteractionSystem.cs` — raycast E-key interaction.
- `FlashlightController.cs` — pin drain, shake-to-recover (F), flicker states. Key: T = toggle, F = shake.
- `HideSpot.cs` — ẩn nấp, disable CharacterController + Collider. Static flag `AnyPlayerHiding`.
- `DoorController.cs` — mở/đóng cửa với animation, locking, UnityEvents.
- `PianoInteractable.cs` + `PianoKey.cs` — sequence puzzle, visual/audio feedback, spawn ghost on completion.
- `GazeTrigger.cs` — continuous raycast, warning + complete events.
- `TriggerZone.cs` — trigger collider, one-shot option, Gizmo.
- `SpawnManager.cs` — spawn prefab tại điểm chỉ định, single-spawn guard.
- `DelayEvent.cs` — invoke UnityEvent sau delay.

#### Sanity

- `SanitySystem.cs` — giá trị 0–1, 4 nấc (High/Medium/Low/Critical), drain/recovery, events.
- `SanityPostProcess.cs` — lerp FilmGrain / ChromaticAberration / ColorAdjustments / Vignette.
- `SanityShake.cs` — Perlin sway camera, gắn lên **Camera con** (không phải PlayerController).
- `SanityZone.cs` — trigger safe/danger zone.
- `SanityData.cs` — ScriptableObject cấu hình 4 nấc.

#### UI & System

- `GameManager.cs` — singleton: game state, chapter loading, PlayerDead().
- `MainMenuUI.cs`, `PauseMenuUI.cs`, `DeathScreenUI.cs`, `InventoryUI.cs`.
- `ChapterTransition.cs` — màn hình chuyển chapter với tên + năm.
- `CutsceneController.cs` — camera path animation, lock/unlock player input.
- `AudioManager.cs` — singleton BGM/SFX, null sfxSource fallback.
- `AmbientZone.cs` — fade in/out ambient audio theo zone.
- `AudioLogItem.cs` — IInteractable, one-play guard, tăng `GameData.audioLogsHeard`.

#### Debug (xóa trước build)

- `DebugSanity.cs` — phím số giảm sanity để test.

---

## 8. GIAO DIỆN NGƯỜI DÙNG

### 8.1 Triết Lý UI

Không có HUD lớn. Không có thanh máu. Không có minimap. Mọi thông tin được truyền đạt qua gameplay và môi trường.

| Thông tin    | Truyền đạt bằng                                | Không dùng    |
| ------------ | ---------------------------------------------- | ------------- |
| Máu/sức khoẻ | Thở dốc + tay run + màn hình rung khi gần chết | Thanh HP      |
| Sanity       | Độ méo màn hình + grain + ảo giác tăng dần     | Thanh Sanity  |
| Ma gần       | Đèn dao động + nhịp tim audio                  | Icon cảnh báo |
| Vật phẩm gần | Dot nhỏ pixel art ở giữa màn hình              | Tooltip lớn   |
| Pin/dầu còn  | Độ sáng đèn giảm + flicker khi sắp hết         | Thanh pin/dầu |

### 8.2 Inventory

- Lưới đơn giản 2×4 ô. Pixel art icon. Nhấn Tab để mở/đóng.
- Nhấn vào item: hiện tên + mô tả ngắn + phát monologue.
- Item quan trọng (3 di vật): Khung vàng, không thể drop.
- Combine tự động khi đến đúng vị trí — không combine thủ công.

---

## 9. KẾ HOẠCH PHÁT TRIỂN

**Nguyên tắc:** Lồng tiếng là hạng mục thuê ngoài — không nằm trong task nội bộ. Nội bộ tập trung vào engine, gameplay, level, và audio môi trường.

### Giai Đoạn 1 — Nền Móng ✅ HOÀN THÀNH

| Task                                          | Người chính  | Trạng thái |
| --------------------------------------------- | ------------ | ---------- |
| Setup Unity URP + PS1 shader prototype        | Lead Dev     | ✅         |
| Player Controller: WASD, đèn pin, tương tác E | Gameplay Dev | ✅         |
| Blockout biệt thự — tầng trệt + tầng 1        | Level Design | ✅         |
| Inventory system cơ bản: nhặt, xem, cầm       | Gameplay Dev | ✅         |
| Layout UI: Main Menu + Death Screen           | UI/Cutscene  | ✅         |

### Giai Đoạn 2 — Chapter 1 Hoàn Chỉnh ✅ HOÀN THÀNH (2025-05-17)

| Task                                              | Người chính  | Trạng thái |
| ------------------------------------------------- | ------------ | ---------- |
| Câu đố Piano: sequence 5 nốt → unlock thư phòng   | Gameplay Dev | ✅         |
| AI Ma Vú Dài: Patrol → Investigate → Chase → Kill | Gameplay Dev | ✅         |
| Audio Log system: phát khi tương tác di vật       | Gameplay Dev | ✅         |
| Sanity system 4 nấc + post-process + shake        | Audio Dev    | ✅         |
| PauseMenuUI + MainMenuUI                          | UI Dev       | ✅         |
| GazeTrigger + TriggerZone                         | Gameplay Dev | ✅         |
| FlashlightController + FlashlightData             | Gameplay Dev | ✅         |
| HideSpot mechanic                                 | Gameplay Dev | ✅         |
| DoorController                                    | Gameplay Dev | ✅         |
| AmbientZone + AudioManager                        | Audio Dev    | ✅         |
| CutsceneController + ChapterTransition            | UI Dev       | ✅         |
| ItemPersistence (PlayerPrefs)                     | Gameplay Dev | ✅         |

### Giai Đoạn 3 — Chapter 2

| Task                                                         | Người chính                 | Ghi chú                                                 |
| ------------------------------------------------------------ | --------------------------- | ------------------------------------------------------- |
| Redress tầng 1: reuse asset + prop bổ sung Ch.2              | Level Design                | Nhật ký bà Lan nằm ở thư phòng (tầng trệt)              |
| Đèn dầu mechanic: lửa dao động theo gió vùng                 | Gameplay Dev                | Wind zone per room, đèn dầu đọc direction               |
| Ma Da: trigger zone system + bồn tắm                         | Gameplay Dev                | Camera gaze raycast + 3s timer + death                  |
| Lore bồn tắm: thêm item/note                                 | Level Design                | 1 dòng trong nhật ký đủ: 'ông Đỗ đặt riêng từ Sài Gòn'  |
| Câu đố gió + tường giả: nét phấn 5 nốt trên tường phòng Linh | Level Design + Gameplay Dev | Phòng bé Linh, góc khuất galerie                        |
| Intro + death sequence Ch.2                                  | UI/Cutscene                 | Vào qua cửa chính. Gương vỡ đồng loạt là cue quan trọng |
| Playtest Ch.2 toàn bộ                                        | Cả nhóm                     | Fix bug ưu tiên cao trước khi qua Ch.3                  |

### Giai Đoạn 4 — Chapter 3

| Task                                                                     | Người chính  | Ghi chú                                          |
| ------------------------------------------------------------------------ | ------------ | ------------------------------------------------ |
| Blockout + dressing tầng 2 + tháp canh                                   | Level Design | Phòng bà Lan: horror design rõ                   |
| Nhật ký cuối ông Đỗ: có 2 nốt nhạc còn lại                               | Level Design | Đặt trong thư phòng tầng trệt                    |
| Công tắc #1 (thư phòng), #2 (hành lang), #3 (tháp canh)                  | Level Design | Vị trí cố định                                   |
| Muối mechanic: rải collider chặn ma qua                                  | Gameplay Dev | Collider spawn khi player rải, giới hạn số lượng |
| Câu đố máy ghi âm: combo công tắc 3 tầng                                 | Gameplay Dev | Bật sai → không phát băng                        |
| Hùng ghi 7 nốt ở tháp canh: trigger sau khi xem qua cửa sổ + đọc nhật ký | Level Design | Trao giấy vào inventory                          |
| Ma Vú Dài v2: tốc độ tăng, detect rộng hơn                               | Gameplay Dev | Điều chỉnh GhostData per chapter                 |
| Câu đố bản đồ: ghép 3 mảnh từ 3 tầng                                     | Level Design | Đặt mảnh ở chỗ hợp lý trong narrative            |
| Intro + death sequence Ch.3                                              | UI/Cutscene  | Băng ghi âm phát lại = twist quan trọng nhất     |
| Playtest Ch.3 toàn bộ                                                    | Cả nhóm      | Balance độ khó so với Ch.1 và Ch.2               |

### Giai Đoạn 5 — Chapter 4

| Task                                                     | Người chính  | Ghi chú                                          |
| -------------------------------------------------------- | ------------ | ------------------------------------------------ |
| Tầng hầm scene: phòng thờ + bàn thờ 3 vật                | Level Design | Tối nhất game — chỉ ánh nến bàn thờ              |
| Piano 7 nốt → mở hộp nhạc                                | Gameplay Dev | Reuse PianoInteractable, chỉ đổi sequence length |
| Sửa gương: ghép mảnh → gương hoàn chỉnh                  | Gameplay Dev |                                                  |
| Rải muối 8 điểm quanh giếng                              | Gameplay Dev | Visualize bằng ánh nến                           |
| Hệ thống 3 ending: flag counter audioLogsHeard           | Lead Dev     | Field đã có trong GameData.cs                    |
| Trận cuối: 2 ma đồng thời + logic phong ấn               | Gameplay Dev | Gương capture Ma Da, muối chặn Ma Vú Dài         |
| Camera điện thoại: nhìn gương an toàn qua màn hình phone | Gameplay Dev | Ch.4 specific mechanic                           |
| Ba cutscene ending                                       | UI/Cutscene  | Ending 1 phức tạp nhất                           |
| Intro sequence Ch.4                                      | UI/Cutscene  | Tiếng piano tự chơi từ xa ngay khi fade in       |
| Nhạc nền ambient: mix theo sanity level                  | Audio        | Unity Audio Mixer Snapshots — 4 mức              |
| Playtest Ch.4 + full run 4 chapter liên tiếp             | Cả nhóm      | Test 3 endings, test flag counter edge cases     |

### Giai Đoạn 6 — Polish & Launch

| Task                                             | Người chính  | Ghi chú                                           |
| ------------------------------------------------ | ------------ | ------------------------------------------------- |
| Full bug fix pass — không thêm tính năng mới     | Cả nhóm      | Ưu tiên: crash > gameplay break > visual          |
| Cân bằng độ khó: ma, câu đố, sanity curve        | Gameplay Dev | Dựa trên data từ playtest giai đoạn 5             |
| Polish post-process: CRT, grain, LUT 4 mức       | Lead Dev     | Sanity visual phải đủ rõ nhưng không gây khó chịu |
| Audio mix toàn bộ: balance, reverb, sanity blend | Audio        | Export final mix                                  |
| Settings menu + ESC pause                        | UI/Cutscene  | Volume, fullscreen, resolution tối thiểu          |
| Trailer 90 giây từ in-game footage               | UI/Cutscene  | Không spoil death Ch.3 và Ch.4                    |
| Build final + test trên clean machine            | Lead Dev     | Test cả 3 endings                                 |
| itch.io page + publish                           | PM           | Mô tả, screenshot, thumbnail                      |

---

## 10. CHAPTER FLOW MASTER DOCUMENT

> Tài liệu tham chiếu chi tiết — dùng khi setup scene. Phòng nào chứa gì, cửa nào khoá, chìa khoá ở đâu, trigger nào kích hoạt gì, luồng từ đầu đến kết thúc từng chapter.

---

### 10.1 Master Key/Lock Table

| ID     | Tên                       | Vị trí lấy                                     | Mở khoá                            | Chapter   |
| ------ | ------------------------- | ---------------------------------------------- | ---------------------------------- | --------- |
| KEY_01 | Chìa khoá kho             | Ngăn kéo bị kẹt tủ salon — cần nến làm đòn bẩy | LOCK_01 (Cửa kho nhà phụ)          | Ch.1      |
| KEY_02 | Piano 5 nốt D-E-G-A-F     | Giải piano sau khi có bảng ký hiệu             | LOCK_02 (Cửa thư phòng)            | Ch.1      |
| KEY_03 | Chìa khoá tủ phòng ông Đỗ | Dưới bàn làm việc phòng ông Đỗ (tầng 1)        | LOCK_03 (Tủ khoá hành lang tầng 1) | Ch.2      |
| KEY_04 | Cửa ẩn phòng Linh         | Gõ tường tường giả (ô rỗng thứ 3)              | Tiết lộ ngách ẩn + gương bạc       | Ch.2      |
| KEY_05 | Công tắc #1               | Thư phòng — sau tủ sách                        | Unlock băng ghi âm phát            | Ch.3      |
| KEY_06 | Công tắc #2               | Hành lang tầng 1 — sau tủ ảnh gia đình         | Mảnh bản đồ #2 tiếp cận            | Ch.3      |
| KEY_07 | Công tắc #3               | Tháp canh                                      | Băng ghi âm phát, mảnh bản đồ #3   | Ch.3      |
| KEY_08 | Tờ giấy 7 nốt             | Hùng viết tại tháp canh (sau ghép clue)        | Input piano 7 nốt Ch.4             | Ch.3→Ch.4 |
| KEY_09 | Piano 7 nốt Ch.4          | Salon — gõ đúng từ tờ giấy Hùng                | Hộp nhạc tự mở → KEY_10            | Ch.4      |
| KEY_10 | Chìa khoá tầng hầm        | Bên trong hộp nhạc đồng (mở bằng KEY_09)       | LOCK_04 (Cửa hầm sau tủ sách)      | Ch.4      |

| Di vật        | Mô tả                                                         | Từ                | Đến            |
| ------------- | ------------------------------------------------------------- | ----------------- | -------------- |
| Hộp nhạc đồng | Không mở ở Ch.1. Mở bằng piano 7 nốt Ch.4 → KEY_10            | Ch.1 (thư phòng)  | Ch.4 inventory |
| Gương bạc     | Vỡ một góc Ch.2. Ghép hoàn chỉnh Ch.4 với mảnh trong hộp nhạc | Ch.2 (phòng Linh) | Ch.4 inventory |
| Lọ muối đen   | Tuấn Hùng đem theo từ đầu Ch.3                                | Ch.3 (intro)      | Ch.4 inventory |

---

### 10.2 Chapter 1 — Flow Chi Tiết (Tầng Trệt, Năm 2000)

**Nhân vật:** Minh Khoa, 21 tuổi. **Ánh sáng:** Đèn pin. **Ma:** Ma Vú Dài (patrol từ đầu).

**Điểm bắt đầu:** Phòng ăn (Salle à manger) — vừa leo qua cửa sổ.

#### Phòng Ăn (Salle à manger)

| Vật phẩm                | Vị trí          | Tương tác   | Kết quả                                                                                |
| ----------------------- | --------------- | ----------- | -------------------------------------------------------------------------------------- |
| Tờ nhạc cũ              | Ngăn kéo bàn ăn | `[EXAMINE]` | Item: 5 nốt D E G A F được khoanh đỏ. Monologue Khoa: _"Nốt nhạc? Ai khoanh vào đây?"_ |
| Bát đĩa vỡ              | Kệ bếp          | `[EXAMINE]` | Lore: _"Bàn ăn 10 chỗ... cả nhà ăn ở đây. Giờ trống không."_                           |
| Tranh phong cảnh Đà Lạt | Tường           | `[EXAMINE]` | Lore: _"Vẽ đẹp. Ghi năm 1962."_                                                        |

**Ghost timing:** Tiếng bước chân Ma Vú Dài nghe từ hành lang ngay khi player vào. Dạy player: nghe tiếng → dừng lại → chờ im → mới di chuyển.

#### Vestibule

| Vật phẩm         | Vị trí      | Tương tác    | Kết quả                                                     |
| ---------------- | ----------- | ------------ | ----------------------------------------------------------- |
| Gương phủ vải đỏ | Tường chính | `[EXAMINE]`  | Monologue: _"Kỳ vậy, sao phủ vải?"_ WARNING: đừng nhấc vải. |
| Cố nhấc vải      | Vải đỏ      | `[INTERACT]` | Ma Da trigger ngay → death (tutorial: tuyệt đối không).     |
| Tủ giày          | Góc         | `[EXAMINE]`  | Lore: giày da của ông Đỗ còn đó.                            |

#### Salon (Salle de réception)

| Vật phẩm           | Vị trí     | Tương tác                             | Kết quả                                                                                |
| ------------------ | ---------- | ------------------------------------- | -------------------------------------------------------------------------------------- |
| Piano Bösendorfer  | Trung tâm  | `[INTERACT]` khi chưa có bảng ký hiệu | Khoa: _"Không biết chơi đàn, không biết nốt nào với nốt nào."_                         |
| Tủ cabinet         | Góc trái   | `[EXAMINE]`                           | _"Ngăn kéo dưới cùng bị kẹt. Cần đòn bẩy."_                                            |
| Lò sưởi            | Tường phải | `[EXAMINE]`                           | _"Không còn lửa. Có cây nến cắm đế đồng nặng."_                                        |
| Nến + đế đồng      | Lò sưởi    | `[PICKUP]`                            | Item: Cây nến (đế đồng cứng). Monologue: _"Đế này khá nặng... có thể dùng làm gì đó."_ |
| Ngăn kéo bị kẹt    | Tủ cabinet | `[USE]` Nến lên ngăn kéo              | Đòn bẩy → mở → **KEY_01** (chìa khoá kho).                                             |
| Tranh ảnh gia đình | Tường      | `[EXAMINE]`                           | Lore: ảnh gia đình Đỗ năm 1960. Ông bà + 2 con nhỏ.                                    |

**Ghost nav:** Ma patrol từ hành lang vào salon mỗi ~60 giây. Player phải chú ý tiếng bước chân trước khi vào salon.

#### Nhà Phụ — Kho (Débarras, dépendance)

_Đường đi: Phòng Sân → galerie sân sau → nhà phụ. Ma không ra galerie Ch.1._

| Vật phẩm              | Vị trí         | Tương tác      | Kết quả                                                                                                         |
| --------------------- | -------------- | -------------- | --------------------------------------------------------------------------------------------------------------- |
| Cửa kho (LOCK_01)     | Cửa kho        | `[USE]` KEY_01 | Mở cửa.                                                                                                         |
| Bảng ký hiệu nốt nhạc | Treo tường kho | `[EXAMINE]`    | Item + monologue Khoa: _"À! D = Rê, E = Mi, G = Sol, A = La, F = Fa. Vậy bài trên tờ nhạc là Rê Mi Sol La Fa."_ |
| Dụng cụ cũ            | Kệ gỗ          | `[EXAMINE]`    | Lore: đồ dùng gia đình ngày xưa.                                                                                |

**Giếng (foreshadow):** Đi ngang qua sân sau → giếng đá không tương tác được. Nước trong giếng gợn nhẹ dù không có gió. Khoa: _"Giếng đá... nước tối quá không thấy đáy."_

#### Salon — Câu đố Piano

_Điều kiện: Có tờ nhạc + bảng ký hiệu._

| Bước   | Hành động                        | Kết quả                                                     |
| ------ | -------------------------------- | ----------------------------------------------------------- |
| 1      | `[INTERACT]` Piano               | Giao diện piano xuất hiện (PianoInteractable)               |
| 2      | Gõ D → E → G → A → F đúng thứ tự | Âm nhạc hộp nhạc. Cửa thư phòng tự mở (tween + click khoá). |
| Gõ sai | Tiếng phím lạc điệu              | Reset sequence. Nếu sai 3 lần liên tiếp: Ma tăng tốc 5s.    |
| 3      | Khoa monologue                   | _"Kỳ lạ... cửa tự mở? Bài nhạc này mở cửa à?"_              |

#### Thư Phòng (Cabinet de travail)

| Vật phẩm         | Vị trí       | Tương tác   | Kết quả                                                                 |
| ---------------- | ------------ | ----------- | ----------------------------------------------------------------------- |
| Hộp âm nhạc đồng | Bàn làm việc | `[PICKUP]`  | Di vật Ch.1. Khoa cố mở: _"Khoá lạ... không có khe chìa thông thường."_ |
| Nhật ký dang dở  | Ngăn kéo bàn | `[EXAMINE]` | Lore: ông Đỗ ghi năm 1964 — dừng đột ngột giữa câu.                     |
| Tủ sách          | Tường        | `[EXAMINE]` | Lore. Không tương tác Ch.1. Ch.4: có cửa hầm sau đây.                   |

#### Tutorial Ẩn Náu + Kết Thúc

_Sau khi lấy hộp nhạc, quay ra hành lang:_

1. Ma Vú Dài đang ở hành lang — đối mặt trực tiếp. Không thể chạy kịp.
2. Tủ áo hành lang highlight → `[INTERACT]` → HideSpot. Khoa nấp.
3. Ma đứng trước tủ 15 giây (tiếng thở phát ra ngay bên ngoài). Bỏ đi.
4. Khoa ra khỏi tủ. _"Không phải ảo giác... thật sự có cái gì đó ở đây."_
5. Tìm đường ra sân sau. Thấy ánh sáng xanh từ giếng.
6. **→ DEATH SEQUENCE** (xem Mục 6.1).

**Di vật để lại:** Hộp âm nhạc đồng — Khoa bị kéo xuống giếng, hộp văng ra nằm cạnh giếng.

---

### 10.3 Chapter 2 — Flow Chi Tiết (Tầng 1, Năm 1970)

**Nhân vật:** Bích Ngọc, 19 tuổi. **Ánh sáng:** Đèn dầu. **Ma:** Ma Vú Dài (patrol từ đầu, Ch.2 route) + Ma Da (bồn tắm).

**Điểm bắt đầu:** Vestibule (cửa chính mở dễ dàng).

#### Vestibule + Tầng Trệt (nhanh)

| Vật phẩm         | Vị trí                    | Tương tác   | Kết quả                                                                                      |
| ---------------- | ------------------------- | ----------- | -------------------------------------------------------------------------------------------- |
| Gương phủ vải đỏ | Vestibule                 | `[EXAMINE]` | Ngọc: _"Bà đã nói rồi — đừng nhìn vào gương."_ Cố nhấc vải → trigger Ma Da.                  |
| Nhật ký bà Lan   | Thư phòng tầng trệt       | `[EXAMINE]` | Audio ký ức bà Lan #1. Clue: _"gió không đổi hướng"_ + nhắc bài nhạc 5 nốt đã ghi lên tường. |
| Hộp nhạc đồng    | Cạnh giếng (lấy từ ngoài) | `[PICKUP]`  | _"Cái hộp này... ai để đây?"_ Di vật Minh Khoa để lại. Ngọc không mở được.                   |

#### Tầng 1 — Hành Lang

_Lên tầng 1 qua cầu thang chính._

| Sự kiện            | Khu vực      | Chi tiết                                                           |
| ------------------ | ------------ | ------------------------------------------------------------------ |
| Tranh ảnh gia đình | Hành lang    | `[EXAMINE]` → audio ký ức #2: bé Linh hát                          |
| Đèn dầu dao động   | Hành lang    | Lửa dao động nhẹ = Ma Vú Dài gần (WindZone gắn trên ghost).        |
| Phòng Trống        | Chambre vide | Cửa hé. Không có gì bên trong. Ghost dừng trước cửa 5s mỗi patrol. |

#### Phòng Bà Lan I (Chambre de Madame I)

| Vật phẩm                   | Vị trí         | Tương tác       | Kết quả                                                        |
| -------------------------- | -------------- | --------------- | -------------------------------------------------------------- |
| Nhật ký bà Lan (phòng ngủ) | Bàn đầu giường | `[EXAMINE]`     | Audio ký ức bà Lan #3: mô tả thứ trong giếng, nỗi sợ năm 1963. |
| Gương phủ vải              | Tường          | Không tương tác | Atmospheric.                                                   |
| Tủ khoá                    | Tường          | `[EXAMINE]`     | _"Tủ khoá... chìa khoá ở đâu?"_ LOCK_03.                       |

#### Phòng Ông Đỗ (Chambre de Monsieur)

| Vật phẩm      | Vị trí            | Tương tác                                | Kết quả                                          |
| ------------- | ----------------- | ---------------------------------------- | ------------------------------------------------ |
| KEY_03        | Dưới bàn làm việc | `[EXAMINE]` bàn → thấy kẹp dưới ngăn kéo | Chìa khoá tủ khoá hành lang (KEY_03).            |
| Thư từ ông Đỗ | Bàn làm việc      | `[EXAMINE]`                              | Lore: thư gửi đối tác kinh doanh, dừng đột ngột. |

#### Tủ Khoá Hành Lang (LOCK_03)

`[USE]` KEY_03 → tủ mở → bên trong: **Mảnh gương bạc nhỏ** (chìa khoá hiểu ngách ẩn phòng Linh) + **ghi chú của ông Đỗ:** _"Phòng con Linh — bức tường thứ ba — tiếng rỗng."_

#### Phòng Bé Linh (Chambre de la fille)

| Sự kiện                    | Chi tiết                                                                                                                              |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Vào phòng                  | Ngọn lửa đèn dầu **đứng thẳng, không dao động** dù cửa sổ hé.                                                                         |
| `[EXAMINE]` tường phía tây | Audio ký ức bà Lan #4: _"Tôi khắc nốt nhạc lên đây... phấn hết rồi."_ Hiện ra nét phấn: **5 nốt D E G A F**, phần sau mờ/còn vết xóa. |
| Ngọc ghi vào sổ            | Tự động: Ngọc: _"Năm nốt... bà Lan để lại. Nhưng còn thiếu."_                                                                         |
| Gõ tường phía tây          | `[INTERACT]` từng ô → ô thứ 3 tiếng rỗng                                                                                              | Prompt: _"Ô này rỗng bên trong."_                    |
| `[INTERACT]` ô rỗng        | Cửa ngách bật mở                                                                                                                      | Ngách tối → **Gương bạc** treo trên móc, vỡ một góc. |
| `[PICKUP]` gương bạc       |                                                                                                                                       | Di vật Ch.2. Ngọc: _"Đây rồi... gương bạc bà dặn."_  |

**Trigger:** Chạm vào gương bạc → tất cả gương trong nhà đập vỡ đồng loạt (âm thanh kính vỡ toàn nhà). Ma Vú Dài "thức dậy" — speed 2.0f, patrol cả 2 tầng không cố định route.

#### Phòng Tắm (Salle de bains, tầng 1)

- `[EXAMINE]` bồn tắm → Ma Da trigger zone. Nhìn vào bồn quá 3 giây → chết.
- Đèn dầu nhấp nháy mạnh khi đứng gần = cảnh báo.
- **Cách qua:** Đi nhanh, không nhìn xuống bồn. Hoặc đi galerie tránh phòng này hoàn toàn.

#### Sân Sau + Kết Thúc

1. Chạy xuống cầu thang (mảnh gương vỡ rải sàn = tiếng thu hút ma nếu bước vào).
2. Ra sân sau với gương bạc.
3. Cố dùng gương → Ngọc nhận ra thiếu hai vật kia (hộp nhạc đã nhặt, nhưng thiếu muối).
4. **→ DEATH SEQUENCE** (xem Mục 6.2).

**Di vật để lại:** Gương bạc — rơi, vỡ thêm một góc. Hộp nhạc Ngọc đã giữ → cả hai nằm cạnh giếng.

---

### 10.4 Chapter 3 — Flow Chi Tiết (Tầng 2 + Tháp, Năm 1990)

**Nhân vật:** Tuấn Hùng, 22 tuổi. **Ánh sáng:** Đèn pin mạnh (tắt chủ động để ẩn). **Ma:** Ma Vú Dài (xuất hiện sau trời tối ~10 phút). **Đặc biệt:** Hùng có lọ muối đen từ đầu (đem theo từ intro).

**Điểm bắt đầu:** Vestibule. Hoàng hôn — còn sáng.

#### 10 Phút Đầu — Thám Hiểm An Toàn

_Ma chưa xuất hiện. Tận dụng cửa sổ thời gian này để collect clues._

**Tầng trệt — Thư phòng:**

| Vật phẩm             | Vị trí                          | Tương tác    | Kết quả                                                                                                                        |
| -------------------- | ------------------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------ |
| Máy ghi âm           | Bàn thư phòng                   | `[PICKUP]`   | Item: máy ghi âm (cần bật đúng 3 công tắc để phát).                                                                            |
| Công tắc #1          | Sau tủ sách thư phòng           | `[INTERACT]` | Click. Đèn xanh nhỏ bật.                                                                                                       |
| Nhật ký cuối ông Đỗ  | Ngăn kéo bàn thư phòng          | `[EXAMINE]`  | Audio ký ức ông Đỗ — điên loạn. **Có ghi 2 nốt còn lại: B (Si) và C# (Đô thăng).** Hùng: _"Bài nhạc... nốt cuối. Ghi lại đã."_ |
| Hộp nhạc + gương bạc | Cạnh giếng (nhặt trước khi vào) | `[PICKUP]`   | Hai di vật từ Khoa và Ngọc. Hùng: _"Sao mấy thứ này lại ở đây?"_                                                               |

**Tầng 1 — Hành Lang:**

| Vật phẩm        | Vị trí    | Tương tác                  | Kết quả                                                                                  |
| --------------- | --------- | -------------------------- | ---------------------------------------------------------------------------------------- |
| Tủ ảnh gia đình | Hành lang | `[EXAMINE]`                | Audio ký ức bé Linh #1. Sau đó: `[MOVE]` tủ sang một bên → **Công tắc #2.** Click.       |
| Phòng Bà Lan I  | Phòng ngủ | `[EXAMINE]` bàn đầu giường | **Mảnh bản đồ #2** kẹp dưới gối. Audio ký ức bà Lan — giọng điên loạn hơn lần Ngọc nghe. |

**Tầng 2:**

_Cầu thang lên tầng 2. Bụi dày, mạng nhện._

| Phòng                    | Vật phẩm           | Kết quả                                                                                  |
| ------------------------ | ------------------ | ---------------------------------------------------------------------------------------- |
| Phòng Con Trai (Đỗ Minh) | Con tàu gỗ         | Audio ký ức Đỗ Minh: _"Ba ơi đừng xuống hầm nữa."_                                       |
| Phòng Con Trai           | **Mảnh bản đồ #1** | Cuộn trong con tàu gỗ.                                                                   |
| Phòng Trà                | Bình trà cổ        | `[EXAMINE]` → lore ông Đỗ tiếp khách. Ghi chú: _"Giếng không bao giờ cạn — dù hạn hán."_ |

#### Trời Tối — Ma Vú Dài Xuất Hiện

_Sau ~10 phút thực: ánh sáng ngoài trời tắt dần. Ambient shift. Tiếng khóc xa từ tầng 1 vọng lên._

Hùng: _"Ổn... bình tĩnh. Chỉ là tiếng gió thôi."_ (Ma bắt đầu patrol tầng 2.)

#### Phòng Bà Lan Tầng 2 (Chambre de retraite) — Phòng Horror Chính

> **Đây là phòng của con ma đang patrol.** Player biết từ trước (clue: "bà Lan patrol vào phòng này"). Căng thẳng nhất chapter.

| Sự kiện                    | Chi tiết                                                                                                       |
| -------------------------- | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Cửa phòng                  | Tiếng khóc từ trong vọng ra.                                                                                   |
| Vào phòng                  | Bàn trang điểm cũ. Gương che. Tường: nét phấn loạn, chữ bị xóa đi xóa lại.                                     |
| `[EXAMINE]` bàn trang điểm | Audio ký ức bà Lan #5 — điên loạn nhất. _"Tôi nghe nó gọi tên con Linh... mỗi đêm... con ơi đừng xuống đó..."_ |
| **Mảnh bản đồ #2**         | Dưới tấm chiếu trên sàn                                                                                        | Phải cúi xuống (C) để nhặt.                                              |
| Ghost enters               | Ma Vú Dài vào phòng trong mid-exploration                                                                      | Tủ quần áo góc phòng = HideSpot. Nấp trong khi ma "đứng ở nhà" ~20 giây. |

#### Tháp Canh (Tour de guet) — Safe Zone

_Đường lên: Hành lang tầng 2 → thang hẹp sắt. Cửa thép đóng từ trong = phải tìm cách mở từ tầng 2 ra ngoài galerie → leo cầu thang ngoài lên tháp._

_Cụ thể: Cửa tháp mở từ bên trong (latch). Galerie tầng 2 → cổng sắt nhỏ ra cầu thang tháp. Player đẩy được._

| Khu vực        | Vật phẩm                         | Kết quả                                                                                                                                                        |
| -------------- | -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tháp canh      | Cửa sổ nhìn về phòng Linh tầng 1 | `[EXAMINE]` qua kính → thấy nét phấn 5 nốt trên tường tây phòng Linh. Tự động: Hùng ghép với 2 nốt từ nhật ký ông Đỗ → **tờ giấy 7 nốt KEY_08 vào inventory.** |
| Tháp canh      | **Mảnh bản đồ #3**               | Trên bàn cạnh cửa sổ.                                                                                                                                          |
| Tháp canh      | **Công tắc #3**                  | Trên tường. Click → đèn xanh.                                                                                                                                  |
| Chân cầu thang | Ghost dừng lại bên dưới          | Tiếng khóc vọng lên. 10 giây. Ghost bỏ đi. Hùng: _"Nó không lên được... may quá."_                                                                             |

**Hùng ghi tờ giấy 7 nốt (KEY_08) — inventory item, trao cho Lan Anh theo di chúc.**

#### Ghép Bản Đồ + Máy Ghi Âm

_3 mảnh ghép lại → bản đồ tầng hầm. Bổ sung vào inventory._

_Xuống thư phòng tầng trệt. Đặt máy ghi âm lên bàn. `[INTERACT]`:_

- Công tắc 1✅ 2✅ 3✅ → băng phát: giọng ông Đỗ ghi âm ngày cuối cùng.
- _"Ba vật phẩm tôi làm ra để phong ấn: hộp nhạc, gương bạc, muối đen. Riêng lẻ chúng vô dụng..."_
- Hùng: _"Vậy là phải có đủ ba thứ và đặt đúng vị trí. Mình đã có hộp nhạc, gương bạc. Muối... đây rồi."_ (Lọ muối đen lấy ra — đã có từ đầu.)

#### Kết Thúc Ch.3

1. Hùng cố rải muối quanh cửa thoát → bị chặn ở sân trước (muối không đủ để thoát, thiếu hai vật kia hoàn chỉnh).
2. **→ DEATH SEQUENCE** (xem Mục 6.3).

**Di vật để lại:** Tờ giấy 7 nốt — Hùng đặt vào di chúc trước khi chết (game không hiện cảnh này, chỉ implied qua intro Ch.4).

---

### 10.5 Chapter 4 — Flow Chi Tiết (Toàn bộ, Năm 2020)

**Nhân vật:** Lan Anh, 23 tuổi. **Ánh sáng:** Điện thoại (camera mode). **Ma:** Ma Vú Dài + Ma Da đồng thời. **Inventory ban đầu:** Hộp nhạc, gương bạc (vỡ một góc), lọ muối đen, tờ giấy 7 nốt.

**Điểm bắt đầu:** Vestibule. Ban đêm. Tiếng piano tự chơi từ xa.

#### Salon — Piano 7 Nốt

| Bước   | Chi tiết                                                            |
| ------ | ------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| 1      | Tiếng piano tự chơi dừng khi Lan Anh bước vào salon.                |
| 2      | `[INTERACT]` Piano + `[USE]` tờ giấy 7 nốt → Giao diện piano 7 nốt. |
| 3      | Gõ đúng `D-E-G-A-F-B-C#`                                            | Hộp nhạc đồng tự mở (animation). Bên trong: **KEY_10** (chìa khoá tầng hầm) + **mảnh gương bạc còn thiếu.** |
| 4      | Lan Anh: _"Chú Hùng... chú đã biết hết rồi."_                       |
| Gõ sai | Reset + Ma Vú Dài tăng tốc 10 giây                                  |

**Sau khi piano giải:** Ma Vú Dài immediate aggro — bắt đầu chase Lan Anh. Race sequence về phòng ẩn.

#### Sửa Gương Bạc

`[COMBINE]` Gương bạc (vỡ) + mảnh còn thiếu trong inventory → Gương bạc hoàn chỉnh.

Lan Anh: _"Hoàn chỉnh rồi. Bây giờ có thể dùng được."_

#### Vestibule — Gương Phủ Vải Đỏ (An Toàn Kiểm Tra)

- `[EQUIP]` Camera điện thoại → `[LOOK AT]` gương vestibule qua camera: thấy bóng Ma Da trong camera hay không.
- Nếu **không thấy bóng trong camera** → `[INTERACT]` gỡ vải đỏ an toàn → Gương vestibule lộ ra (dùng để nhốt Ma Da cuối game).
- Nếu **thấy bóng** → đừng gỡ, chờ Lan Anh rời đi 10 giây rồi thử lại.

#### Sân Sau — Rải Muối

1. Ra sân sau. Giếng phát sáng xanh mờ.
2. `[USE]` Lọ muối đen → Giao diện rải: 8 điểm quanh giếng (vòng tròn, hiện dấu X màu vàng khi đủ khoảng cách).
3. Ánh nến từ cửa sổ tầng trệt hắt ra → visualize vòng muối.
4. Rải đủ 8 điểm → Giếng tối lại. Ma Da không thể thoát ra từ giếng nữa.

#### Tầng Hầm — Đoạn Cuối

1. Thư phòng: `[MOVE]` tủ sách → cửa hầm lộ ra. `[USE]` KEY_10 → LOCK_04 mở.
2. Xuống tầng hầm — **tối nhất game.** Chỉ ánh nến bàn thờ phía cuối.
3. Bàn thờ 3 chỗ. Đặt đúng vị trí:
   - Trái: Hộp nhạc đồng
   - Giữa: Gương bạc hoàn chỉnh
   - Phải: Lọ muối đen
4. **Trận cuối:**
   - Ma Da áp sát → `[USE]` Gương bạc hướng về phía Ma Da → Capture sequence → Ma Da bị nhốt.
   - Ma Vú Dài xuống hầm → Lan Anh rải muối ngưỡng cửa → Ma Vú Dài bị chặn.
   - Ba vật phẩm sáng lên. Bàn thờ kích hoạt phong ấn.
5. **→ KẾT THÚC** theo flag `audioLogsHeard` (xem Mục 6.4).

---

### 10.6 Audio Log Master Table

| #   | Nhân vật           | Nội dung ngắn                                 | Vị trí vật phẩm                    | Chapter | Trigger                |
| --- | ------------------ | --------------------------------------------- | ---------------------------------- | ------- | ---------------------- |
| 1   | Bà Lan             | Ngày 3/9/1963 — con Linh thấy mặt trong giếng | Nhật ký thư phòng tầng trệt        | Ch.2    | `[EXAMINE]`            |
| 2   | Bé Linh            | Tiếng hát khe khẽ — kêu người xuống giếng     | Tranh ảnh hành lang tầng 1         | Ch.2    | `[EXAMINE]`            |
| 3   | Bà Lan             | Ghi bài nhạc lên tường — phấn hết             | Nhật ký phòng bà Lan I             | Ch.2    | `[EXAMINE]`            |
| 4   | Bà Lan             | Lý do phủ vải tất cả gương                    | Nét phấn tường phòng Linh          | Ch.2    | `[EXAMINE]`            |
| 5   | Đỗ Minh            | _"Ba đừng xuống hầm nữa"_                     | Con tàu gỗ phòng con trai          | Ch.3    | `[EXAMINE]`            |
| 6   | Bà Lan (điên loạn) | Nghe thứ trong giếng gọi tên Linh             | Bàn trang điểm phòng bà Lan tầng 2 | Ch.3    | `[EXAMINE]`            |
| 7   | Đỗ Linh            | _"Nó nói nó đói"_                             | Bức vẽ phòng bé Linh / phòng chơi  | Ch.3    | `[EXAMINE]`            |
| 8   | Đỗ Văn Minh        | Ba vật phong ấn — đừng để chia lìa            | Băng ghi âm thư phòng (3 công tắc) | Ch.3    | Giải câu đố máy ghi âm |

> **Ending flag:** `GameData.audioLogsHeard >= 8` → Ending 1 (Giải Thoát). `audioLogsHeard >= 4` → Ending 2 (Thoát Ra). `< 4` → Ending 3 (Thất Bại).

---

### 10.7 Kế Thừa Di Vật Xuyên Chapter

| Di vật        | Ch.1                                                         | Ch.2                                                                      | Ch.3                                                     | Ch.4                                        |
| ------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------- | -------------------------------------------------------- | ------------------------------------------- |
| Hộp nhạc đồng | Lấy từ thư phòng. Minh Khoa bị kéo xuống giếng, hộp văng ra. | Bích Ngọc nhặt cạnh giếng. Ngọc bị hút vào gương, hộp và gương ở sân sau. | Tuấn Hùng nhặt cả hai. Hùng chết, để lại trong di chúc.  | Lan Anh có trong ba lô từ đầu.              |
| Gương bạc     | —                                                            | Tìm thấy trong ngách ẩn phòng Linh. Vỡ một góc khi rơi.                   | Hùng nhặt cùng hộp nhạc.                                 | Lan Anh có. Ghép hoàn chỉnh trong hộp nhạc. |
| Lọ muối đen   | —                                                            | —                                                                         | Người gửi ẩn danh trao trước khi Hùng vào.               | Lan Anh có (di chúc Hùng ghi rõ).           |
| Tờ giấy 7 nốt | —                                                            | —                                                                         | Hùng ghép clue ở tháp canh, ghi ra giấy. Để lại di chúc. | Lan Anh dùng để giải piano 7 nốt.           |

---

---

> ⚠️ **LƯU Ý:** Các mục 10.0–10.7 dưới đây là **bản cũ (pre-fix design)** — được giữ lại chỉ để tham khảo sơ đồ phòng ASCII. Mọi key/lock, puzzle flow, và ghost route đều đã được cập nhật ở **10.1–10.7 phía trên**. Khi có mâu thuẫn, ưu tiên dùng phần phía trên.

### 10.A Sơ Đồ Kết Nối Phòng (Reference Nhanh)

#### Tầng trệt (dùng Ch.1, Ch.2 một phần, Ch.4)

```
[ Sân trước ]
      |
[ Vestibule ] ←──────────────────────────────────────────── [ Phòng tiếp khách nhỏ ]
      |                                                              |
[ Salon / Phòng khách ] ←──── cửa nối ────► [ Thư phòng tầng trệt ]
      |
[ Phòng ăn ] ←── cửa sổ (điểm vào Ch.1)
      |
[ Hành lang phía sau ] ──────────────────────────────────── [ Cầu thang chính ]
      |
[ Sân sau + Giếng ]
      |
[ Nhà phụ: Bếp ] ── cửa kho ──► [ Kho ]
```

**Phòng mới so với thiết kế ban đầu:**

- **Phòng tiếp khách nhỏ** — off vestibule, có tranh chân dung ông Đỗ + tủ rượu cũ. Lore room.
- **Kho** — bên trong nhà phụ, có lối đi tắt ra sân sau qua cửa phụ.

---

#### Tầng 1 (dùng Ch.2, Ch.4)

```
[ Cầu thang chính ] ──► [ Hành lang tầng 1 ] ──► [ Ban công ]
                               |
    ┌──────────┬───────────────┼───────────────────┬──────────┐
[ P.Tắm ] [ P.Bà Lan I ] [ Boudoir / P.Bà Lan II ] [ P.Trống ] [ P.Ông Đỗ ]
                                                                      |
                                                              [ P.Bé Linh ]
```

- **P.Ông Đỗ & P.Bà Lan I:** Phòng riêng — chuẩn Đông Dương. P.Ông Đỗ chứa KEY_03.
- **Boudoir:** Phòng thay đồ/riêng tư bà Lan. Cửa có then trong.
- **P.Trống:** Không có đồ đạc. Ghost dừng trước cửa khi patrol.
- **P.Bé Linh:** Tường phía tây: nét phấn 5 nốt + cửa ẩn (gõ ô rỗng thứ 3).

---

#### Tầng 2 + Tháp canh (dùng Ch.3)

```
[ Cầu thang chính ] ──► [ Hành lang tầng 2 ] ──► [ Ban công T2 ]
                               |
    ┌──────────┬───────────────┼──────────────────┬─────────────┐
  [ Kho T2 ] [ P.Trà ] [ P.Bà Lan T2 ] [ P.Tắm T2 ] [ P.Đỗ Minh ] [ P.Linh/Chơi ]
                                |
                        [ Galerie T2 ] ──► [ Cổng sắt ] ──► [ Thang ngoài ]
                                                                    |
                                                            [ THÁP CANH ] ← Safe zone
```

- **P.May / thêu:** Nhỏ, nối hành lang → galerie T2 → tháp. Tạo loop tránh dead-end. Làm chậm ghost khi đuổi.
- **Tháp canh:** Ghost KHÔNG vào. View 360°. Nhìn qua kính thấy nét phấn P.Linh T1.

---

### 10.1 Nguồn 7 Nốt Piano — Master Reference

| Nốt | Ký hiệu  | Nguồn gốc           | Tìm thấy ở Chapter | Vật phẩm chứa                                  |
| --- | -------- | ------------------- | ------------------ | ---------------------------------------------- |
| 1   | D (Rê)   | Tờ nhạc bà Lan      | Ch.1               | Ngăn kéo bàn ăn — phòng ăn                     |
| 2   | E (Mi)   | Tờ nhạc bà Lan      | Ch.1               | Ngăn kéo bàn ăn — phòng ăn                     |
| 3   | G (Sol)  | Tờ nhạc bà Lan      | Ch.1               | Ngăn kéo bàn ăn — phòng ăn                     |
| 4   | A (La)   | Tờ nhạc bà Lan      | Ch.1               | Ngăn kéo bàn ăn — phòng ăn                     |
| 5   | F (Fa)   | Tờ nhạc bà Lan      | Ch.1               | Ngăn kéo bàn ăn — phòng ăn                     |
| 6   | B (Si)   | Nhật ký cuối ông Đỗ | Ch.3               | Thư phòng tầng trệt — bên trong cuốn sách khoá |
| 7   | C# (Đô#) | Nhật ký cuối ông Đỗ | Ch.3               | Thư phòng tầng trệt — bên trong cuốn sách khoá |

**Xác nhận nét phấn Ch.2:** Bích Ngọc thấy 5 nốt (D E G A F) trên tường phòng bé Linh — đúng với tờ nhạc Ch.1. Xác nhận tính chính xác nhưng không bổ sung thêm nốt mới.

**Ch.3 flow ghép 7 nốt:**
Tuấn Hùng đọc nhật ký ông Đỗ ở thư phòng (nốt 6–7). Sau khi lên tháp canh nhìn qua cửa sổ thấy nét phấn trong phòng bé Linh (nốt 1–5, nhìn từ bên ngoài qua kính). Anh ghép đủ 7 nốt trên tờ giấy ghi chú → trigger inventory item "Tờ Nhạc Hoàn Chỉnh" (7 nốt). Item này tự động có trong ba lô Lan Anh ở Ch.4 (di sản Hùng để lại).

---

### 10.2 Master Key & Lock Reference

#### Chìa khoá vật lý (inventory item)

| ID     | Tên chìa                    | Tìm thấy tại                                                | Mở cửa nào                       | Chapter dùng |
| ------ | --------------------------- | ----------------------------------------------------------- | -------------------------------- | ------------ |
| KEY_01 | Chìa khoá nhà phụ           | Vestibule — trong tủ đứng (ngăn trên)                       | Cửa nhà phụ (sân sau)            | Ch.1         |
| KEY_02 | Chìa khoá kho               | Nhà phụ/bếp — treo trên móc tường bên cạnh bếp củi          | Cửa kho (bên trong nhà phụ)      | Ch.1         |
| KEY_03 | Chìa khoá cổng cầu thang    | Thư phòng tầng trệt — ngăn kéo bàn (hint từ nhật ký bà Lan) | Cổng sắt nhỏ đầu cầu thang chính | Ch.2         |
| KEY_04 | Chìa khoá phòng bà Lan (T1) | Phòng ông Đỗ (tầng 1) — trên bàn phấn trang điểm            | Cửa phòng bà Lan (tầng 1)        | Ch.2         |
| KEY_05 | Chìa khoá phòng bà Lan (T2) | Phòng ngủ Đỗ Minh (tầng 2) — trong hộp đồ chơi gỗ           | Cửa phòng bà Lan (tầng 2)        | Ch.3         |
| KEY_06 | Chìa khoá tháp canh         | Hành lang tầng 2 — treo sau khung tranh gia đình            | Cửa tháp canh                    | Ch.3         |
| KEY_07 | Chìa khoá tầng hầm          | Bên trong hộp nhạc (mở sau piano 7 nốt)                     | Cửa hầm sau tủ sách thư phòng    | Ch.4         |

#### Unlock bằng puzzle / sự kiện

| Trigger                                  | Cửa/khu vực mở                         | Chapter |
| ---------------------------------------- | -------------------------------------- | ------- |
| Piano 5 nốt đúng thứ tự                  | Cửa thư phòng (từ salon)               | Ch.1    |
| Ngọn lửa đứng yên + gõ tường ô thứ 3     | Cửa ẩn trong tường phòng bé Linh       | Ch.2    |
| Switch #1 + #2 + #3 đều ON + băng đã nạp | Máy ghi âm phát băng                   | Ch.3    |
| Ghép 3 mảnh bản đồ                       | Hiện UI map → highlight vị trí cửa hầm | Ch.3    |
| Piano 7 nốt đúng thứ tự                  | Hộp nhạc tự mở                         | Ch.4    |
| Đặt 3 vật lên bàn thờ đúng vị trí        | Cửa phòng thờ mở, trigger trận cuối    | Ch.4    |

---

### 10.3 Chapter 1 — Flow Chi Tiết

**Nhân vật:** Minh Khoa | **Năm:** 2000 | **Khu vực:** Tầng trệt | **Target:** ~25–30 phút

**Nguồn sáng:** Đèn pin — bắt đầu 60% pin. Shake (F) để phục hồi khi yếu.

---

#### Phòng ăn (Điểm xuất phát)

**Cửa:**

- Cửa sổ phía tây: ĐÃ VÀO — không dùng lại được
- Cửa ra vestibule: MỞ
- Cửa ra hành lang phía sau: MỞ

**Items trong phòng:**

- `[EXAMINE]` Bộ đồ ăn cũ trên bàn → Khoa: _"Bụi phủ dày thế này... không ai ở đây mấy chục năm rồi."_
- `[EXAMINE]` Bức tranh phong cảnh Đà Lạt trên tường → Khoa: _"Chụp ảnh cái này đã. Bố cục đẹp."_
- `[PICKUP]` Ngăn kéo bàn ăn → **Tờ nhạc (5 nốt: D E G A F)** — 5 nốt được khoanh tròn bằng mực đỏ → Khoa: _"Tờ nhạc cũ. Có ai khoanh 5 nốt này... để làm gì?"_
  - ⚠️ _Chưa biết ký hiệu nốt nhạc — item "Tờ nhạc" nhưng không dùng được cho piano cho đến khi có "Bảng ký hiệu"._

**Triggers:**

- Không có trigger đặc biệt trong phòng này.

---

#### Vestibule

**Cửa:**

- Cửa chính: KHÓA từ bên ngoài (không thoát)
- Cửa ra salon: MỞ
- Cửa ra phòng ăn: MỞ
- Cửa ra phòng tiếp khách nhỏ: MỞ

**Items trong phòng:**

- `[EXAMINE]` Gương phủ vải đỏ → Khoa: _"Vải đỏ che gương? Bà chủ nhà kỳ lạ thật."_ → **không chạm thêm** — nếu player ấn tương tác lần 2: _"Thôi mình không mó vào."_
- `[EXAMINE]` Tranh ảnh gia đình Đỗ trên tường → Khoa: _"Gia đình lớn. Vợ chồng, hai con nhỏ. Trông... hạnh phúc."_
- `[INTERACT]` Tủ đứng cũ (2 ngăn) → ngăn dưới: quần áo cũ → ngăn trên: **KEY_01 (Chìa khoá nhà phụ)** + mảnh giấy nhỏ: _"Khoá bếp — phòng khi có khách không mời."_

**Camera animation:**

- Khi Khoa tương tác gương: camera zoom nhẹ vào vải đỏ 1 giây, sau đó trả về.

---

#### Phòng tiếp khách nhỏ

**Cửa:**

- Cửa ra vestibule: MỞ (dead-end — không nối sang phòng khác)

**Items trong phòng:**

- `[EXAMINE]` Tranh chân dung ông Đỗ Văn Minh (uy quyền, áo dài khăn đóng) → Khoa: _"Ông chủ nhà. Trông uy nghi lắm."_
- `[EXAMINE]` Tủ rượu cũ — trong ngăn dưới có chai rượu còn nguyên + `[EXAMINE]` tờ giấy: _"Mời khách quý — Đỗ Văn Minh, 1958."_
- `[EXAMINE]` Lò sưởi nhỏ góc phòng → Khoa: _"Lò sưởi Pháp. Đà Lạt lạnh — có lý."_

> **Mục đích:** Lore room. Tăng thời gian khám phá 3–5 phút. Không có item quan trọng.

---

#### Salon / Phòng khách

**Cửa:**

- Cửa ra vestibule: MỞ
- Cửa ra thư phòng: **KHÓA** → mở bằng piano puzzle

**Items trong phòng:**

- `[EXAMINE]` Piano grand cũ → Khoa: _"Đàn piano Pháp. Vẫn còn nguyên vẹn... ai đó chăm sóc kỹ lắm."_
  - `[INTERACT]` Piano — nếu chưa có Bảng ký hiệu: Khoa thử gõ vài phím → _"Mình không đọc được nhạc. Cần biết ký hiệu nốt mới gõ đúng được."_
  - `[INTERACT]` Piano — nếu có Bảng ký hiệu + Tờ nhạc: mở PianoInteractable UI → **Puzzle 5 nốt.**
- `[EXAMINE]` Sofa cũ → Khoa: _"Vải nhung đỏ. Còn nguyên. Đắt tiền lắm."_
- `[EXAMINE]` Cabinet bên cạnh lò sưởi → ngăn trên: khoá → ngăn dưới: MỞ → **Pin AA × 2** (add battery 30%)

**Triggers:**

- Sau khi gõ đúng 5 nốt → **camera cutscene ngắn:** camera zoom ra nhìn cửa thư phòng — tiếng khớp cửa mở → Khoa: _"Ký lạ... tiếng mở khoá? Chỉ có vậy thôi?"_ → cửa thư phòng unlock.

---

#### Thư phòng tầng trệt (mở bằng piano puzzle)

**Cửa:**

- Cửa từ salon: MỞ (sau puzzle)
- Cửa nối lại hành lang phía sau: MỞ từ bên trong (tạo shortcut sau khi vào)

**Items trong phòng:**

- `[EXAMINE]` Bàn làm việc + sách cũ → Khoa: _"Thư viện riêng. Ông chủ nhà đọc nhiều."_
- `[EXAMINE]` Cuốn nhật ký ngắn (không phải nhật ký chính — chỉ vài dòng): _"Ngày 15 tháng 3, 1964. Vật phẩm thứ nhất được đặt đúng chỗ. Tôi hi vọng điều này đủ."_ → Khoa: _"Vật phẩm nào? Đặt ở đâu?"_
- `[PICKUP]` Trên bệ sách: **Hộp âm nhạc đồng** — tương tác → _"Hộp nhạc đồng. Có lẽ bên trong rỗng — không mở được. Khoá bị hàn chì."_ → Audio trigger: **ký ức bà Lan #1** phát ngay khi cầm.
  - _Bà Lan (ký ức): "Ngày 3 tháng 9, 1963. Con Linh nói thấy mặt người trong giếng..."_
- `[EXAMINE]` Tranh vẽ mặt tiền biệt thự trên tường → Khoa: _"Biệt thự khi mới xây. Đẹp thật."_

**Triggers:**

- Sau khi Khoa nhặt hộp nhạc và bước ra cửa thư phòng vào hành lang → **trigger zone hành lang** kích hoạt.

---

#### Hành lang phía sau + Ghost Spawn

**Trigger zone** (đặt ngay sau cửa thư phòng):

- **Camera animation:** Camera pan chậm sang trái → thấy bóng Ma Vú Dài cuối hành lang nhìn về phía Khoa → bóng mờ đi.
- Khoa: _"Cái gì vừa—"_
- **Ghost spawn:** Ma Vú Dài bắt đầu patrol từ cuối hành lang → di chuyển về phía salon.
- **Tutorial hint:** Text nhỏ góc màn hình: `[C] Cúi xuống — [Tủ] Ẩn nấp`

**Tutorial hide sequence:**

- Tủ ẩn nấp ở góc hành lang — `[INTERACT]` → Khoa chui vào
- Ghost patrol qua trước tủ, dừng 2 giây, tiếp tục
- Sau ~30 giây ghost đi vào salon
- Khoa tự bước ra → Khoa: _"Ổn rồi... ổn rồi. Phải ra khỏi đây."_

---

#### Nhà phụ — Bếp (mở bằng KEY_01)

**Cửa:**

- Cửa từ sân sau: KHÓA — dùng KEY_01
- Cửa kho: KHÓA — dùng KEY_02
- Cửa phụ ra sân sau (lối khác): MỞ (một chiều — thoát ra sân sau được)

**Items trong phòng:**

- `[EXAMINE]` Bếp củi lớn → Khoa: _"Bếp Đông Dương chuẩn. Nấu cho cả gia đình lớn. Vẫn còn tro."_
- `[PICKUP]` **Bảng ký hiệu nốt nhạc** — treo tường bên cạnh cửa sổ → _"Bảng ký hiệu nốt nhạc — Do Rê Mi Fa Sol La Si. Vậy là giờ đọc được tờ nhạc rồi."_
- `[EXAMINE]` Móc tường bên bếp → **KEY_02 (Chìa khoá kho)** treo trên móc → Khoa: _"Chìa khoá cái kho kia?"_
- `[EXAMINE]` Đồ dùng nấu ăn cũ, nồi đất, bình nước → atmosphere

---

#### Kho (mở bằng KEY_02)

**Items trong phòng:**

- `[PICKUP]` **Pin AA × 4** (add battery 40%) — trong hộp gỗ nhỏ
- `[EXAMINE]` Lương thực cũ — gạo hoá đất, nước mắm cạn khô
- `[EXAMINE]` Nông cụ cũ (cuốc, xẻng)
- `[EXAMINE]` Góc kho: vết xước trên sàn gỗ — như thứ gì đó bị kéo đi → Khoa: _"Ai kéo cái gì nặng qua đây..."_

> **Mục đích:** Cung cấp pin dự phòng. Lore nhỏ. 2–3 phút khám phá.

---

#### Sân sau + Giếng (Death Sequence)

**Flow đến đây:**
Sau khi hide xong, ghost patrol về salon. Khoa cần ra sân sau qua hành lang phía sau.

**Cửa:**

- Hành lang → sân sau: MỞ (lối đi ngoài trời)

**Triggers:**

- Bước vào sân sau lần đầu: trigger **"thấy giếng lần đầu"** (foreshadow) — đèn pin chiếu xuống giếng, không thấy gì → Khoa: _"Giếng đá cổ. Sâu quá không thấy đáy."_
- Bước vào sân sau lần cuối (sau khi lấy hộp nhạc + đã hide): **DEATH SEQUENCE trigger zone.**

**Death sequence animation:**

1. Fade nhẹ — tiếng gió dừng
2. Khoa nhìn xuống giếng: _"Cái gì vậy... ánh sáng trong giếng?"_
3. Camera FP cúi dần xuống miệng giếng
4. 0.5s im lặng — mặt nước đen nhìn lên
5. Gương mặt méo mó xuất hiện trong mặt nước — mắt trắng đục nhìn thẳng camera
6. Bàn tay từ giếng vọt lên — camera giật mạnh xuống trong 0.3 giây
7. Màn hình đen. Tiếng nước ùa vào.
8. _Minh Khoa (tiếng cuối, như từ dưới nước): "...Ai đó... giúp..."_
9. Text: **ĐỖ MINH KHOA · 1979 – 2000**

---

#### Tóm tắt luồng Ch.1

```
Phòng ăn [Tờ nhạc]
  → Vestibule [KEY_01]
  → Phòng tiếp khách nhỏ [lore]
  → Salon [Pin AA]
  → Hành lang sau → Sân sau [foreshadow giếng]
  → Nhà phụ/Bếp [KEY_01 mở] → [Bảng ký hiệu] [KEY_02]
  → Kho [KEY_02 mở] → [Pin AA ×4]
  → Quay về Salon → Piano 5 nốt → Thư phòng mở
  → Thư phòng [Hộp nhạc] [Audio log #1]
  → Hành lang → GHOST SPAWN → Tutorial ẩn
  → Sân sau → DEATH
```

**Ước tính thời gian:** Khám phá đầy đủ ~25 phút. Speedrun ~12 phút.

---

### 10.4 Chapter 2 — Flow Chi Tiết

**Nhân vật:** Bích Ngọc | **Năm:** 1970 | **Khu vực:** Tầng 1 chính, một phần tầng trệt | **Target:** ~25–30 phút

**Nguồn sáng:** Đèn dầu — **cơ chế gió:** ngọn lửa dao động = ma gần. Lửa tắt = 10 giây trước khi Ma Vú Dài tấn công.

---

#### Vestibule (Điểm xuất phát)

**Cửa:**

- Cửa chính: vào được (Ngọc đẩy vào từ bên ngoài)
- Cửa ra salon: MỞ
- Cửa ra thư phòng: MỞ
- Cửa cổng cầu thang: **KHÓA** → KEY_03

**Items trong phòng:**

- `[EXAMINE]` Gương phủ vải đỏ → Ngọc: _"Bà dặn đừng nhìn vào mặt nước. Cái này... để nguyên."_
- `[EXAMINE]` Tranh ảnh gia đình (đã có từ Ch.1, cũ hơn → lighting khác) → Ngọc: _"Nhà này từng có gia đình sống..."_

---

#### Thư phòng tầng trệt

**Items trong phòng:**

- `[PICKUP]` Ngăn kéo bàn → **KEY_03 (Chìa khoá cổng cầu thang)**
- `[READ]` **Nhật ký bà Lan — Tập 1** (trên bàn) — 3 trang:
  - _"Ngày 3/9/1963: Con Linh thấy mặt người trong giếng..."_
  - _"Ngày 10/9/1963: Chìa khoá cầu thang tôi giữ trong thư phòng — không cho con lên tầng một mình."_
  - _"Ngày 15/9/1963: Bài nhạc đó... tôi ghi lên tường phòng con Linh để nhớ. Nhưng phấn hết rồi. Gió không bao giờ vào được phòng con Linh — góc khuất của hành lang. Nơi đó yên tĩnh nhất."_
  - → Ngọc: _"Gió không đổi hướng. Phòng bé Linh. Tường phía tây."_
  - → Hint kép: KEY_03 (cầu thang) và clue puzzle phòng bé Linh

---

#### Hành lang + Cầu thang (mở bằng KEY_03)

**Trigger khi lên tầng 1:**

- Camera pan chậm dọc hành lang tầng 1 (tối, nhiều tranh ảnh)
- Tiếng ọp ẹp của sàn gỗ
- **Audio ký ức tự động #1** phát khi Ngọc bước vào hành lang:
  - _Đỗ Linh (ký ức, giọng con nít): "Má ơi... con thấy nó lại rồi. Trong cái gương ở phòng tắm..."_

---

#### Phòng ông Đỗ (tầng 1)

**Cửa:** MỞ

**Items trong phòng:**

- `[PICKUP]` Bàn phấn trang điểm (có vẻ lạ trong phòng đàn ông — lore: ông Đỗ dùng để viết thư pháp) → **KEY_04 (Chìa khoá phòng bà Lan T1)**
- `[EXAMINE]` Bản đồ điền thổ treo tường → Ngọc: _"Đất đai của ông này rộng lắm."_
- `[EXAMINE]` Giá sách → một cuốn sách kỹ thuật về phong thuỷ (ông Đỗ tìm hiểu vì lý do...) → hint về 3 vật phẩm phong ấn nhưng không rõ ràng

---

#### Phòng bà Lan — Tầng 1 (mở bằng KEY_04)

**Cửa:** KHÓA → KEY_04

**Items trong phòng:**

- `[EXAMINE]` Gương ngủ phủ vải trắng trên tường → Ngọc: _"Bà ấy phủ gương cả trong phòng ngủ nữa."_
- `[READ]` **Nhật ký bà Lan — Tập 2** (trên tủ đầu giường):
  - _"Ngày 20/11/1963: Tôi không ngủ được. Nó gõ vào gương ban đêm. Tôi nghe tiếng nó trong mặt nước."_
  - _"Ngày 5/12/1963: Hai nốt cuối tôi chưa ghi được. Cần tìm thêm phấn. Nhưng đi đâu tìm phấn bây giờ?"_
  - → Ngọc: _"Bà ấy biết bài nhạc nhưng không ghi xong được."_
- `[PICKUP]` **Audio Log #2** — máy ghi âm nhỏ (thực ra là vật phẩm examine, không phải physical tape) → **Bà Lan (ký ức):** _"Ngày 3 tháng 9, 1963. Con Linh nói thấy mặt người trong giếng. Tôi bịt giếng lại bằng vải đỏ. Sáng hôm sau tấm vải biến mất."_

---

#### Phòng tắm

**Cửa:** MỞ — nhưng nên đi qua để đến phòng bé Linh (layout: phòng tắm nằm giữa hành lang và phòng bé Linh)

**Trigger nguy hiểm:**

- Bồn tắm có mặt nước tĩnh → **Ma Da trigger zone** tích cực
- Nếu camera nhìn vào bồn > 1 giây: mặt nước gợn → 2 giây: màn hình đổ xanh → 3 giây: chết
- **Cảnh báo:** Ngọc tự nhủ khi bước vào: _"Bà dặn đừng nhìn vào mặt nước. Đừng nhìn vào."_
- Có thể đi qua phòng tắm bằng cách nhìn tường (không nhìn bồn) → safe

---

#### Phòng bé Linh

**Cửa:** MỞ

**Items trong phòng:**

- `[EXAMINE]` Đồ chơi cũ (búp bê, khối gỗ) → **Audio ký ức #3:** _Đỗ Linh (ký ức): "Cái người trong giếng... nó nói nó ở đây lâu lắm rồi. Nó muốn mình xuống chơi cùng."_
- `[OBSERVE]` Ngọn lửa đèn dầu: **đứng yên hoàn toàn** (các phòng khác ngọn lửa dao động nhẹ) → Ngọc: _"Gió không vào được đây. Đúng rồi — góc khuất của galerie."_
- `[EXAMINE]` Tường phía tây → **Nét phấn bà Lan:**
  - Camera zoom vào tường, ánh đèn dầu hắt lên
  - 5 nốt nhạc được viết bằng phấn: D - E - G - A - F
  - Phần sau mờ dần — phấn chạy hết giữa chừng, nét cuối cùng không hoàn chỉnh
  - Ngọc: _"Bà ấy ghi ở đây. Năm nốt... nhưng còn thiếu. Chắc hai nốt nữa bà không kịp viết."_
  - → **Ghi vào sổ của Ngọc** (auto-note trong inventory: "Tường phòng Linh: D-E-G-A-F — 5/? nốt")

**Puzzle gõ tường:**

- Gõ tường từ trên xuống — ô thứ 3 có tiếng rỗng → Ngọc: _"Rỗng bên trong."_
- Tương tác tiếp → Ngọc tháo tranh treo → **cửa nhỏ ẩn** → bên trong: **Tấm gương bạc** (vỡ một góc)
- Nhặt gương → **tất cả gương trong nhà đập vỡ đồng loạt** — tiếng kính vỡ từ nhiều hướng
- **Ma Vú Dài thức dậy** — tiếng khóc trẻ con vang lên từ xa

**Escape sequence:**

- Cần chạy xuống cầu thang — nhưng mảnh gương vỡ rải rải sàn hành lang → bước lên phát tiếng → Ma nghe được
- Có thể đi chậm (nhón gót) hoặc chạy (tạo tiếng, Ma Chase ngay)
- Phòng tắm: Ma Da vẫn active — không nhìn vào bồn khi chạy qua

---

#### Sân sau — Death Sequence Ch.2

**Trigger:** Ngọc cầm gương bạc → ra sân sau → DEATH SEQUENCE

**Animation:**

1. Ngọc đứng giữa sân, trăng chiếu xuống
2. Hạ gương → nhìn vào mặt gương (camera FP pan xuống gương)
3. Trong gương: phản chiếu không phải mặt đất — gương mặt Ma Da nhìn lên
4. Ngọc: _"Trời ơi..."_
5. Gương kéo Ngọc vào — camera giật nhanh vào mặt gương → đen
6. Tiếng thủy tinh rơi. Im lặng.
7. Text: **NGUYỄN THỊ BÍCH NGỌC · 1951 – 1970**

---

#### Tóm tắt luồng Ch.2

```
Vestibule [gương vải đỏ — không chạm]
  → Thư phòng [KEY_03] [Nhật ký bà Lan T1 — hint cầu thang + phòng Linh]
  → Cầu thang [KEY_03 mở] → Tầng 1
  → Hành lang T1 [Audio ký ức #1 auto]
  → Phòng ông Đỗ [KEY_04]
  → Phòng bà Lan [KEY_04 mở] → [Nhật ký T2] [Audio log #2]
  → Phòng tắm [danger: Ma Da bồn tắm — không nhìn]
  → Phòng bé Linh [Audio ký ức #3] [Nét phấn 5 nốt]
    → Puzzle gõ tường → Cửa ẩn → [Gương bạc]
  → Gương vỡ đồng loạt → MA VÚ DÀI THỨC DẬY
  → Chạy xuống cầu thang (tránh mảnh gương)
  → Sân sau → DEATH
```

**Ước tính thời gian:** Khám phá đầy đủ ~28 phút. Speedrun ~15 phút.

---

### 10.5 Chapter 3 — Flow Chi Tiết

**Nhân vật:** Tuấn Hùng | **Năm:** 1990 | **Khu vực:** Tầng trệt + Tầng 1 + Tầng 2 + Tháp canh | **Target:** ~30 phút

**Nguồn sáng:** Đèn pin mạnh — **có thể tắt chủ động để ẩn náu** (nhưng không thấy đường).

**Ghost timing:** Ma Vú Dài bắt đầu patrol sau khi trời tối (~10 phút đầu an toàn, sau tháp canh thì full active).

---

#### Máy Ghi Âm Reel-to-Reel — Lore & Cơ Chế

**Thiết bị:** Máy Telefunken M10 reel-to-reel (1960s) — ông Đỗ mua từ Sài Gòn. Đặt trong thư phòng tầng trệt từ 1960.

**Cơ chế hoạt động:**

1. Máy chạy bằng hệ thống intercom điện trung tâm (cài từ 1950s)
2. Dây điện chạy qua tường — 3 công tắc relay phân bố ở 3 điểm trong nhà
3. Tất cả 3 relay phải ON → máy mới có nguồn
4. Băng phải được nạp vào trục máy trước khi phát
5. Nhấn PLAY → băng chạy → nghe qua loa của máy

**Băng reel:** Lưu trong phòng bà Lan (tầng 2) — bà Lan giấu băng ghi âm của ông Đỗ đó vì sợ con nghe.

---

#### Thư phòng tầng trệt (Bắt đầu khám phá)

**Cửa:** MỞ (Hùng vào từ cửa chính)

**Items:**

- `[EXAMINE]` **Máy ghi âm reel-to-reel** — không có băng, không có nguồn → Hùng: _"Máy Telefunken cũ. Không có băng. Dù sao cũng chưa có điện."_
- `[INTERACT]` **Switch #1** (relay nhỏ gắn tường cạnh máy) → flip ON → Hùng: _"Một. Còn cần tìm hai công tắc nữa đâu đó."_
- `[READ]` **Nhật ký cuối Đỗ Văn Minh** — trong cuốn sách khoá da đen (khoá tự rỉ mở được):
  - _"Tôi mang nó về từ cái giếng ngoài rừng. Ba vật phẩm tôi làm ra để phong ấn: hộp nhạc, gương bạc, muối đen. Riêng lẻ chúng vô dụng."_
  - _"Bài nhạc có bảy nốt. Năm nốt đầu vợ tôi đã biết. Hai nốt cuối — Si và Đô thăng — tôi ghi ở đây để ai đó sau này hoàn thành."_
  - _"Cửa hầm nằm sau tủ sách trong thư phòng. Chìa khoá bên trong hộp nhạc. Hộp nhạc mở khi đủ bảy nốt."_
  - → Hùng: _"Bảy nốt. Năm nốt ở đâu đó trong nhà này. Hai nốt còn lại là Si và Đô thăng."_
  - → **Auto-note:** "Nốt 6: B (Si), Nốt 7: C# (Đô thăng) — từ nhật ký ông Đỗ"

---

#### Tầng 1 — Hành lang & Switch #2

**Switch #2** (relay gắn tường — sau tủ ảnh gia đình):

- `[EXAMINE]` Tủ ảnh gia đình → Hùng: _"Ảnh gia đình Đỗ. Vợ chồng, con cái... trước năm 1965."_
- `[INTERACT]` Dịch tủ ra (interact) → lộ relay → flip ON → Hùng: _"Hai."_
- `[PICKUP]` **Mảnh bản đồ #0** (từ ngăn kéo nhỏ dưới tủ ảnh): mảnh giấy cũ — thấy một phần sơ đồ tầng 1

---

#### Tầng 2 — Phòng ngủ Đỗ Minh

**Cửa:** MỞ

**Items:**

- `[EXAMINE]` **Đồ chơi cũ** (con tàu gỗ) → **Audio ký ức #4:** _Đỗ Minh (ký ức): "Ba ơi, ba đừng xuống tầng hầm nữa. Con nghe tiếng ba nói chuyện với ai đó ở dưới đó."_
- `[PICKUP]` **Mảnh bản đồ #1** — dưới đệm giường (Đỗ Minh giấu vì tò mò)
- `[INTERACT]` **KEY_05 (Chìa khoá phòng bà Lan T2)** — trong **hộp đồ chơi gỗ** trên kệ

---

#### Tầng 2 — Phòng bà Lan (mở bằng KEY_05)

> **Horror design:** Đây là phòng Ma Vú Dài đang patrol. Player vào phòng của con ma.

**Cửa:** KHÓA → KEY_05

**Items:**

- `[EXAMINE]` Đầu giường → thấy vết cào trên tường → Hùng: _"Móng tay... hay là gì đó cào lên đây."_
- `[PICKUP]` **Băng reel-to-reel** — trong ngăn kéo tủ đầu giường (bọc vải đỏ) → Hùng: _"Băng ghi âm. Chữ viết tay trên nhãn: 'Đừng phát — Lan'. Bà ấy giấu băng này."_
- `[READ]` **Audio ký ức #5** tự phát khi nhặt băng:
  - _Bà Lan (ký ức): "Tôi giấu băng này vì không muốn con nghe giọng ông ấy nói những thứ đó. Nhưng nếu ai đó cần..."_
- `[PICKUP]` **Mảnh bản đồ #2** — sau bức tranh bà Lan treo tường
- `[EXAMINE]` Gương phủ vải trong phòng → Audio ký ức tự phát:
  - _Đỗ Linh (ký ức): "(giọng con nít) Cái người trong giếng... nó nói nó đói. Nó nói nó muốn mình xuống chơi cùng."_

**Ghost event:** Nếu Hùng ở trong phòng > 90 giây → Ma Vú Dài đi qua cửa (nhìn vào) → nghe thấy nếu Hùng không cúi / tắt đèn → Chase.

---

#### Tầng 2 — Phòng may

**Cửa:** MỞ — lối tắt ra tháp canh

**Items:**

- `[EXAMINE]` Khung thêu bỏ dở → Hùng: _"Tẩy não. Ai đó đang thêu thì... dừng lại."_
- `[EXAMINE]` Cửa ra tháp canh: **KHÓA** → KEY_06 (tìm trong hành lang)

---

#### Hành lang tầng 2 — KEY_06

- `[INTERACT]` Khung tranh gia đình lớn → dịch ra → **KEY_06 (Chìa khoá tháp canh)** trên đinh tường

---

#### Tháp canh (mở bằng KEY_06)

> Trời đã tối hoàn toàn khi Hùng lên đến đây. Ma Vú Dài bắt đầu patrol tích cực.

**Items:**

- `[PICKUP]` **Mảnh bản đồ #3** — trên bàn quan sát góc tháp
- `[INTERACT]` **Switch #3** (relay gắn cửa sổ tháp canh) → flip ON → Hùng: _"Ba. Đủ rồi."_
- `[OBSERVE]` **Nhìn qua cửa sổ xuống phòng bé Linh** (góc nhìn từ trên):
  - Camera FP tiến đến cửa sổ → nhìn xuống mái nhà phụ → qua cửa kính nhỏ phòng bé Linh thấy nét phấn trên tường
  - Hùng: _"Nét phấn... D-E-G-A-F. Năm nốt đầu. Cộng với Si và Đô thăng từ nhật ký ông Đỗ..."_
  - → **Trigger:** item "Tờ Nhạc Hoàn Chỉnh (7 nốt)" được thêm vào inventory
  - Hùng: _"Bảy nốt. D-E-G-A-F-B-C#. Cái này... quan trọng lắm. Phải ghi lại."_

---

#### Quay về Thư phòng — Phát Băng

**Ghép 3 mảnh bản đồ:**

- Combine 3 mảnh trong inventory → **Bản đồ tổng hợp** — highlight đỏ vị trí phía sau tủ sách trong thư phòng
- Hùng: _"Cửa hầm. Phía sau cái tủ sách đó."_

**Phát băng:**

- Đặt băng vào máy (`[INTERACT]` máy khi có băng trong inventory) → _"Đặt băng... Switch đủ rồi. Phát."_
- `[INTERACT]` nút PLAY → băng chạy
- **Camera animation:** zoom vào trục băng quay chậm → tiếng băng chạy
- **Đỗ Văn Minh (ký ức):** _"Tôi ghi lại cái này phòng khi không còn nhớ nữa. Cửa hầm nằm sau tủ sách trong thư phòng. Dùng chìa khoá trong hộp nhạc. Hộp nhạc mở bằng bảy nốt — vợ tôi đã biết năm nốt đầu. Đừng để ba vật chia lìa. Đừng bao giờ chia lìa."_
- Băng kết thúc → tiếng tĩnh điện

---

#### Sân trước — Death Sequence Ch.3

**Trigger:** Hùng cố thoát ra sân trước → DEATH SEQUENCE

**Animation:**

1. Hùng chạy ra sân trước. Dừng lại thở.
2. Tuấn Hùng: _"Thoát rồi... thoát rồi. Ổn. Mình ổn."_
3. Máy ghi âm trong tay tự bật lên (không ai nhấn)
4. Tiếng băng chạy... rồi giọng Hùng chính mình từ trong băng: _"— nó đứng sau mày rồi. Đừng quay lại. Đừng —"_
5. Tiếng băng bị cắt.
6. Hùng đứng im. Nhìn thẳng. Trong bóng tối trước mặt — bóng Ma Vú Dài đổ dài trên sàn. Bóng đến từ phía sau anh.
7. Tuấn Hùng: _"(giọng run) Không quay lại. Không quay lại. Không—"_
8. Tiếng vải kéo. Đen.
9. Text: **TRẦN TUẤN HÙNG · 1968 – 1990**

---

#### Tóm tắt luồng Ch.3

```
Thư phòng T0 [Switch #1] [Nhật ký ông Đỗ: nốt 6+7] [Máy ghi âm — cần băng]
  → Tầng 1: Hành lang [Switch #2] [Mảnh bản đồ #0]
  → Tầng 2: Phòng Đỗ Minh [KEY_05] [Audio ký ức #4] [Mảnh bản đồ #1]
  → Phòng bà Lan T2 [KEY_05 mở] [Băng reel] [Mảnh bản đồ #2] [Audio ký ức #5]
  → Hành lang T2 [KEY_06]
  → Phòng may → Tháp canh [KEY_06 mở] [Switch #3] [Mảnh bản đồ #3]
    → Nhìn qua cửa sổ → [Tờ Nhạc Hoàn Chỉnh 7 nốt auto-add]
  → Quay về Thư phòng T0:
    → Ghép 3 mảnh → [Bản đồ tổng hợp — highlight cửa hầm]
    → Đặt băng + PLAY → nghe ông Đỗ
  → Cố thoát → DEATH
```

**Ước tính thời gian:** Khám phá đầy đủ ~30 phút. Speedrun ~18 phút.

---

### 10.6 Chapter 4 — Flow Chi Tiết

**Nhân vật:** Lan Anh | **Năm:** 2020 | **Khu vực:** Toàn bộ + Tầng hầm | **Target:** ~30–35 phút

**Nguồn sáng:** Điện thoại — đèn pin app + **camera mode** để nhìn gương/mặt nước an toàn.

**Di sản từ các chapter trước (auto trong inventory khi bắt đầu):**

- Hộp âm nhạc đồng (không mở được — cần piano 7 nốt)
- Tấm gương bạc vỡ một góc (cần ghép mảnh)
- Lọ muối đen
- Tờ Nhạc Hoàn Chỉnh (7 nốt: D-E-G-A-F-B-C#)

---

#### Vestibule (Điểm xuất phát)

**Trigger auto:** Tiếng piano tự chơi từ xa — bài hát ru của bà Lan — khi Lan Anh vừa vào.

**Items:**

- `[EXAMINE]` Gương phủ vải đỏ → Lan Anh: _"Vải đỏ. Bà Lan phủ từ 1963. Tôi cần biết bên trong có gì — nhưng không dám nhìn thẳng."_
- `[INTERACT]` Gương — **Camera mode:** bật điện thoại lên, mở camera → nhìn gương qua màn hình điện thoại (safe) → thấy gương bình thường, không có bóng → `[INTERACT]` xé vải → **gương vestibule lộ ra** → không trigger Ma Da (an toàn)
  - Nếu nhìn thẳng không qua camera → Ma Da trigger bình thường

---

#### Salon — Piano 7 Nốt

**Cửa thư phòng:** KHÓA (cần piano 7 nốt như Ch.1 — nhưng lần này unlock key thay vì cửa)

**Puzzle piano 7 nốt:**

- Lan Anh: _"Bảy nốt. Chú Hùng — chú đã tìm ra."_
- Mở PianoInteractable UI → input 7 nốt theo thứ tự: D-E-G-A-F-B-C#
- **Camera cutscene:** Piano phát đủ bài — 7 nốt vang lên đầy đủ
- Hộp nhạc trong inventory **tự mở** — tiếng lên dây cót
- **Lấy từ hộp nhạc:** KEY_07 (Chìa khoá tầng hầm) + **Mảnh gương**
- Lan Anh: _"Chìa khoá. Và mảnh gương còn thiếu."_

---

#### Ghép Gương + Sân Sau

**Ghép gương:**

- Combine Tấm gương bạc vỡ + Mảnh gương → **Gương bạc hoàn chỉnh**
- Lan Anh: _"Đủ rồi. Gương nguyên vẹn."_

**Sân sau — Rải muối:**

- 8 điểm rải muối được visualize: ánh nến từ cửa sổ tầng trệt hắt ra nền sân, tạo pattern tròn sáng
- `[INTERACT]` từng điểm sáng → rải muối → hiệu ứng muối trắng trên sàn
- Đủ 8 điểm → **vòng tròn muối hoàn chỉnh quanh giếng** — Ma Vú Dài không thể vào vòng này

**Cảnh báo:** Nếu Ma Vú Dài đang patrol và Lan Anh đang rải muối → nguy hiểm. Ghost active từ đầu Ch.4.

---

#### Thư phòng — Tầng Hầm (KEY_07)

- Dịch tủ sách (bản đồ Ch.3 đã highlight vị trí) → lộ cửa gỗ nhỏ
- `[INTERACT]` cửa → dùng KEY_07 → mở
- Camera pan xuống bậc thang tối

---

#### Tầng Hầm — Phòng Thờ

**Ánh sáng:** Chỉ ánh nến trên bàn thờ. Không có nguồn sáng khác.

**Cấu trúc phòng:**

- Bàn thờ trung tâm với 3 vị trí trống hình chữ tam giác
- Mỗi vị trí có ký hiệu nhỏ khắc vào đá: hộp nhạc / gương / muối

**Đặt 3 vật:**

- `[INTERACT]` từng vị trí → đặt vật tương ứng
- Sau khi đặt đủ 3 → tiếng rung → cửa phòng thờ tự mở → **TRẬN CUỐI**

---

#### Trận Cuối

**Điều kiện:**

- Gương bạc hoàn chỉnh trong tay → có thể capture Ma Da
- Vòng muối ở sân sau → chặn Ma Vú Dài (nếu đã rải đủ 8 điểm)
- Hộp nhạc đặt lên bàn thờ → đang phát giai điệu phong ấn

**Cơ chế:**

| Tình huống                             | Hành động                                   | Kết quả                                                        |
| -------------------------------------- | ------------------------------------------- | -------------------------------------------------------------- |
| Ma Da xuất hiện trong giếng (mặt nước) | Soi gương bạc vào giếng                     | Ma Da bị nhốt trong gương — gương vỡ, thực thể bị phong ấn     |
| Ma Vú Dài tiến vào sân                 | Vòng muối đã rải → Ma Vú Dài không qua được | Ma đứng ngoài vòng — phong ấn dần dần khi hộp nhạc chơi đủ bài |
| Ma Vú Dài tiến vào sân                 | Vòng muối CHƯA rải đủ → Ma qua được         | Lan Anh phải rải nốt trong khi né tránh                        |

**Kết thúc trận cuối:** Camera cutscene — hộp nhạc chơi hết bài → ánh sáng trắng từ bàn thờ → 3 vật tan biến.

---

#### Ba Endings

_(xem Section 6.4)_

**Flag điều kiện:**

- Ending 1: `GameData.audioLogsHeard >= 8` (tất cả audio ký ức đã nghe qua 4 chapter)
- Ending 2: Đủ 3 vật + `audioLogsHeard < 8`
- Ending 3: Không đủ 3 vật HOẶC bị chết trong trận cuối

---

#### Tóm tắt luồng Ch.4

```
Vestibule [Camera mode → xé vải gương]
  → Salon: Piano 7 nốt → [Hộp nhạc mở: KEY_07 + Mảnh gương]
  → Combine gương → [Gương bạc hoàn chỉnh]
  → Sân sau: Rải 8 điểm muối (nguy hiểm — ghost active)
  → Thư phòng: Dịch tủ sách → [KEY_07 mở cửa hầm]
  → Tầng hầm: Đặt 3 vật lên bàn thờ
  → Trận cuối: Capture Ma Da (gương) + Phong ấn Ma Vú Dài (muối + hộp nhạc)
  → ENDING (1 / 2 / 3 tuỳ flag)
```

**Ước tính thời gian:** Khám phá đầy đủ ~30–35 phút. Speedrun ~20 phút.

---

### 10.7 Tổng Hợp Audio Logs — Điều Kiện Ending

Phải nghe đủ 8 audio log để unlock Ending 1.

| ID  | Nội dung                                                   | Tìm thấy tại                    | Chapter |
| --- | ---------------------------------------------------------- | ------------------------------- | ------- |
| #1  | Bà Lan: "Con Linh thấy mặt người trong giếng..."           | Thư phòng T0 — nhặt hộp nhạc    | Ch.1    |
| #2  | Bà Lan: "Tôi không ngủ được. Nó gõ vào gương..."           | Phòng bà Lan T1 — tủ đầu giường | Ch.2    |
| #3  | Đỗ Linh: "Cái người trong giếng... nó đói..."              | Phòng bé Linh — nhặt búp bê     | Ch.2    |
| #4  | Đỗ Minh: "Ba đừng xuống tầng hầm nữa..."                   | Phòng Đỗ Minh T2 — nhặt đồ chơi | Ch.3    |
| #5  | Bà Lan: "Tôi giấu băng vì không muốn con nghe..."          | Phòng bà Lan T2 — nhặt băng     | Ch.3    |
| #6  | Đỗ Linh (ký ức): "Nó nói nó muốn mình xuống chơi cùng"     | Phòng bà Lan T2 — examine gương | Ch.3    |
| #7  | Đỗ Văn Minh (băng reel): "Ba vật phẩm... đừng để chia lìa" | Thư phòng T0 — phát băng        | Ch.3    |
| #8  | Bà Lan (ký ức cuối): voice khi đặt gương lên bàn thờ       | Tầng hầm — bàn thờ              | Ch.4    |

> **Ghi chú:** Log #8 chỉ phát khi Lan Anh đặt gương lên bàn thờ — nếu bỏ qua (Ending 3 fail state) thì không nghe được.

---

_— HẾT · BIỆT THỰ BÓNG TỐI · GDD v3 —_
_Horror · Survival · Puzzle · Unity 3D · PS1 Aesthetic · Đà Lạt, Việt Nam_
