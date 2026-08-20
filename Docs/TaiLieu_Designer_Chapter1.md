# TÀI LIỆU DESIGNER — CHAPTER 1: NGUYỄN MINH KHOA

> Nguồn thật: `COT_TRUYEN_Chapter1_MinhKhoa.md`. File này trích phần bố trí tương tác/item/khoá cửa + toàn bộ nội dung nhật ký/sổ ghi nợ để designer dựng scene, không kèm phần lời thoại thu âm (xem `KichBan_LongTieng_Chapter1.md`).

---

## 1. LORE OÁN KHÍ (vì sao đúng cái giếng này bị ám)

Ông Đỗ Văn Minh là kiểu người "ai cho tiền thì làm" — thời Pháp thuộc thì làm ăn với kiến trúc sư/khách Pháp (chính villa này xây năm **1940** cũng vậy — KHÔNG dùng 1945, vì đó là năm Nhật đảo chính Pháp (3/1945) rồi Việt Nam giành độc lập (9/1945), quá loạn lạc cho 1 công trình villa yên bình), Pháp rút đi (1954-1956, sau Hiệp định Geneva, Đà Lạt thuộc về Việt Nam Cộng Hoà chứ không còn thuộc Pháp) thì ông chuyển sang làm ăn với người Mỹ (cố vấn viện trợ) — không trung thành với ai, chỉ trung thành với tiền.

Trong số các việc làm ăn với phía Mỹ có việc thuê phu đào mở rộng giếng đá sau vườn năm 1963. Một phu tên Tư chết đuối trong lúc đào, ông Minh chi tiền bịt miệng thay vì báo cáo hay lo hậu sự đàng hoàng — cái chết oan đó không được giải oan, sinh ra oán khí bám lấy chính cái giếng, đánh thức thứ đã ngủ yên dưới đó (Ma Da). Đây là NGUỒN GỐC vì sao đúng cái giếng này bị ám, không phải ngẫu nhiên.

Tài liệu (nhật ký, sổ ghi nợ) không nói thẳng ra chuyện đổi phe Pháp→Mỹ — chỉ gợi ý qua 2 chi tiết rời rạc: bảng tên cổng ("1940, kiến trúc sư Pháp phối hợp ông Đỗ Văn Minh") + 1 câu nhật ký Tháng Ba ("ngày trước làm ăn với Pháp, giờ là Mỹ"). Người chơi tinh ý tự ghép, không có ai giải thích trực tiếp.

**Ông Đỗ Văn Minh và Bà Lan phản ứng HOÀN TOÀN KHÁC NHAU trước cùng 1 sự thật — không được lẫn lộn:**

- **Ông Minh:** không tự nhận mình sai, chỉ CẢM THẤY gia đình bị ám/bị nguyền rủa (dread mơ hồ, không rõ nguyên nhân) — tự biện minh, né tránh, không bao giờ nối được nhân-quả thật. Xuyên suốt nhật ký, đỉnh điểm ở đoạn Tháng Mười ("gia đình này bị ám" — ông vẫn không biết TẠI SAO).
- **Bà Lan:** NGƯỢC LẠI, bà BIẾT rõ chuyện chồng làm hại người khác (vụ phu chết ở giếng) — nhưng đã chọn im lặng, mặc kệ, vì cuộc sống gia đình đang ấm no sung túc. Sự ăn năn của bà (thể hiện qua ghi âm Đoạn 4, xem kịch bản lồng tiếng) là ăn năn vì đã CHỌN LÀM NGƠ, không phải vì "không biết gì".

---

## 2. CHÌA KHOÁ & CỬA

| Cửa / lối đi | Vị trí | Cách mở | Chìa khoá lấy ở đâu |
|---|---|---|---|
| Cửa sổ Phòng Ăn | Ngoài cổng → Phòng Ăn | Không khoá, hé sẵn từ đầu | — (điểm vào duy nhất, Phần I) |
| Cửa Bếp ↔ Phòng Ăn | Phòng Ăn → Bếp | **Không khoá** — cửa hầu bàn nội bộ, gia nhân ra vào hằng ngày, không có lý do khoá | — (quyết định thiết kế cố định) |
| Cửa Kho | Bếp → Kho (nhà phụ) | Khoá vật lý — cần **KEY_01** | Ngăn kéo tủ cabinet ở Salon, bẩy ra bằng đế đèn nến đồng (Phần V) |
| Cửa Thư Phòng | Hành lang → Thư Phòng | Khoá cơ khí ẩn, KHÔNG dùng chìa khoá vật lý — mở bằng giải đúng puzzle piano | Giải đúng 5 nốt Mi→Đô→Fa→Rê→Sol trên piano Salon (Phần VIII) |
| Tủ áo (HideSpot) | Hành lang | Không khoá — chỗ ẩn nấp, không phải cửa cần mở | — |

Chapter 1 chỉ có ĐÚNG 1 khoá vật lý thật (Kho, KEY_01). Bếp cố tình không khoá — không có lý do hợp lý để khoá 1 cửa nội bộ nhà bếp; nếu sau này cần thêm độ khó/exploration cho Bếp thì làm ở Phase sau.

---

## 3. THỨ TỰ TRẢI NGHIỆM (10 bước)

1. Vào nhà qua cửa sổ Phòng Ăn — nhặt mảnh giấy ①(E). *(Phần II)*
2. Vestibule — mảnh ②(C). *(Phần III)*
3. Tiếp Khách — mảnh ③(F). *(Phần IV)*
4. Salon (lần 1) — mảnh ④(D) + đế đèn nến → bẩy ngăn kéo → KEY_01. *(Phần V)*
5. Bếp (không khoá) → Kho (mở bằng KEY_01) — bảng ký hiệu nốt nhạc + Sổ ghi nợ (manh mối oán khí) + mảnh ⑤(G) → đủ 5 mảnh. *(Phần VI)*
6. Sân Sau — lướt qua giếng, chưa tương tác được, chỉ foreshadow. *(Phần VII)*
7. Salon (lần 2) — giải piano đúng thứ tự Mi→Đô→Fa→Rê→Sol → cửa Thư Phòng tự mở. *(Phần VIII)*
8. Thư Phòng — đọc nhật ký (4 đoạn) → tiếng cọt kẹt "...Ai đó?" (scare nhỏ, KHÔNG phải Ma Vú Dài thật) → hộp nhạc phát 5 đoạn ghi âm Bà Lan → nhặt hộp nhạc. *(Phần IX)*
9. Rời Thư Phòng ra Hành Lang — CHẠM MẶT THẬT Ma Vú Dài → trốn tủ áo (cinematic slide vào/ra + xoay góc nhìn qua khe cửa) → thoát. *(Phần X)*
10. Sân Sau — giếng phát sáng → death sequence. *(Phần XI)*

**Ghi nhớ:** Piano LUÔN đứng trước Nhật ký (bước 7 mở khoá phòng chứa nhật ký ở bước 8). Ghi âm Bà Lan nằm CHUNG cảnh với Nhật ký. Gặp Ma Vú Dài thật xảy ra NGAY SAU KHI rời Thư Phòng (bước 9), không phải ngay sau khi gấp nhật ký lại (khoảnh khắc "...Ai đó?" ở bước 8 chỉ là tiếng động nhỏ chưa xác định).

---

## 4. PUZZLE PIANO

5 nốt: **Mi(E) → Đô(C) → Fa(F) → Rê(D) → Sol(G)** — lấy THẬT từ mở đầu khúc bình ca Gregorian "Dies Irae" ("Dies irae, dies illa"), điệu Dorian, không thăng giáng. Verify: [mfiles.co.uk](https://www.mfiles.co.uk/scores/gregorian-dies-irae.htm), [Wikipedia — Dies irae](https://en.wikipedia.org/wiki/Dies_irae).

Model `Prop_Piano_FullKeys.fbx` xếp phím thật theo vị trí vật lý Đô①-Rê②-Mi③-Fa④-Sol⑤-La⑥-Si⑦. Chơi đúng thứ tự trên bắt buộc di chuyển ZIGZAG: Mi(③)→Đô(①) lùi 2 phím, Đô(①)→Fa(④) tiến 3 phím, Fa(④)→Rê(②) lùi 2 phím, Rê(②)→Sol(⑤) tiến 3 phím — không phải quét 1 chiều rồi bấm Space liên tục.

Sai: tiếng phím lạc điệu, reset chuỗi phím. 3 lần sai liên tiếp → nhịp bước chân Ma Vú Dài dồn dập hơn 5 giây.

**Việc cần build (Thuận):** đổi `_correctSequence` trong `PianoInteractable.cs`/Inspector từ `["D","E","G","A","F"]` (cũ) sang `["E","C","F","D","G"]` (mới); `_playableKeys` phải chứa ĐỦ 7 phím thật theo đúng vị trí vật lý, không phải bản rút gọn 5 phím liền kề.

---

## 5. NĂM MẢNH GIẤY

| # | Nốt | Khu vực | Vị trí đặt |
|---|---|---|---|
| ① | E (Mi) | Phòng Ăn | Trong ngăn kéo bàn ăn |
| ② | C (Đô) | Vestibule | Rơi trên sàn (vị trí cụ thể để Jok tự đặt) |
| ③ | F (Fa) | Phòng Tiếp Khách | Rơi trên sàn gần lối vào (vị trí cụ thể để Jok tự đặt) |
| ④ | D (Rê) | Salon | Rơi trên sàn cạnh sofa (vị trí cụ thể để Jok tự đặt) |
| ⑤ | G (Sol) | Kho | Cuối vệt kéo lê bí ẩn trên sàn kho (vị trí cụ thể để Jok tự đặt) |

Mỗi mảnh: giấy ố vàng, xé rách 1 cạnh, có 1 vòng khoanh đỏ quanh số thứ tự + 1 nốt nhạc viết tay bên cạnh.

Bảng ký hiệu nốt nhạc (tìm trong Kho): khắc tay Đô–Rê–Mi–Fa–Sol–La–Si tương ứng C-D-E-F-G-A-B, giúp người chơi giải mã 5 chữ cái trên mảnh giấy thành nốt.

---

## 6. NHẬT KÝ ĐẦY ĐỦ — Đỗ Văn Minh, 1964

*(Tìm thấy: Thư Phòng, nằm mở trên bàn làm việc. Hiển thị dạng UI/panel đọc được, KHÔNG đọc thành tiếng — chỉ 3 câu phản ứng ngắn sau khi đọc mới có voice, xem kịch bản lồng tiếng.)*

**Ngày 8 tháng Ba, 1964**

Sáng nay trời Đà Lạt trở lạnh bất thường quá, sương giăng kín cả khu vườn, mãi tới gần trưa mới chịu tan hẳn. Tôi ngồi viết mấy dòng này bên chiếc bàn gỗ gõ đỏ đặt đóng riêng từ Sài Gòn mang lên, mắt cứ nhìn xuống khoảng sân sau, chỗ thằng Minh đang chật vật tập cho em nó đi cái xe đạp ba bánh màu đỏ tôi mua tặng nó dịp Tết. Con Linh mới mười một tháng thôi mà đã lẫm chẫm được vài bước không cần vịn rồi, nhanh nhẹn hơn hẳn anh nó hồi bằng tuổi ấy — thằng Minh hồi đó thì rụt rè lắm, cứ bíu chặt lấy váy mẹ nó, không chịu buông ra.

Công việc dạo này bận hơn thường lệ. Bản vẽ mở rộng cho dinh thự trên đường Yết Kiêu đã tới giai đoạn hoàn thiện, tôi phải đi lại Sài Gòn hai lần trong tháng để trình duyệt, mỗi lần đi về mất trọn ba ngày đường. Người đứng tên bảo trợ công trình lần này là một ông cố vấn người Mỹ, cố vấn gì đó thuộc phái đoàn viện trợ, tôi cũng không rành lắm chức vụ ông ta — chỉ biết ông ta trả công hậu hĩ hơn hẳn mức thường thấy, hậu đến mức đôi lúc tôi cũng phải tự hỏi liệu số tiền đó có thật sự chỉ để trả cho vài bản vẽ của tôi thôi hay không, nhưng thôi, tôi gạt ngay ý nghĩ đó sang một bên, nhà cửa đang cần tiền, hỏi nhiều làm chi cho mệt người. Ngẫm cũng lạ thật, ngày trước tôi làm ăn với người Pháp không hà, giờ người Pháp đi hết rồi thì lại tới phiên người Mỹ ngồi vào cái chỗ đó — mà thôi, tiền ai trả thì cũng là tiền, tôi đâu có thời gian đâu mà ngồi chọn phe chọn phái. Nhà tôi ở lại một mình quán xuyến cả gia trang, cả người làm lẫn hai đứa nhỏ — tôi vẫn áy náy vì để nàng vất vả một mình nhiều đến vậy, nhưng nàng chưa bao giờ than lấy một câu, chỉ cười bảo "ông cứ lo việc lớn của ông, việc nhà để tôi."

Thằng Minh dạo này hay để ý theo tôi ra công trình mỗi cuối tuần, cứ hỏi đủ thứ chuyện xây cất, làm tôi vừa buồn cười vừa hãnh diện trong bụng — chắc sau này nó nối nghiệp cha thật. Con Linh thì khác hẳn anh nó, quấn mẹ hơn quấn cha, chiều nào cũng đòi mẹ nó bế ra hiên sau ngồi chơi tới tối mới chịu vào. Nhà tôi cũng chiều con lắm, tối nào cũng ngồi đó ru con ngủ trước khi bế vào giường — chuyện thường tình thôi, gia đình nào chẳng vậy, tôi cũng chẳng để tâm.

Chỉ có một điều nho nhỏ cứ khiến tôi hơi lấn cấn: cái điệu ru đó nghe lạ tai lắm, không giống bất kỳ câu hò hay dân ca nào tôi từng nghe qua cả. Hỏi thì nhà tôi bảo học được từ một gánh hát rong hồi nhỏ ở quê, nghe một lần rồi nhớ mãi không quên. Chuyện nhỏ thôi mà, chắc gì tôi phải để ý làm chi cho mệt óc. Thôi, ngày mai phải dậy sớm, còn đi giám sát tiến độ đổ móng công trình mới nữa.

**Ngày 22 tháng Sáu, 1964**

Chuyện xảy ra chiều nay khiến tôi không thể nào không ghi lại ngay được, dù kim đồng hồ đã chỉ quá mười giờ đêm rồi, và tay tôi, thú thật, vẫn còn hơi run khi cầm cây bút này đây.

Mọi chuyện bắt đầu hết sức bình thường thôi. Nhà tôi vào bếp từ khoảng bốn giờ rưỡi chiều để lo bữa tối, cứ đinh ninh là hai đứa nhỏ đang chơi đùa ngoài hiên, có thằng Minh trông chừng — năm nay nó mười hai tuổi rồi, cũng đủ lớn để tôi với nhà tôi yên tâm giao em gái cho nó mỗi khi bận việc. Vậy mà chỉ vỏn vẹn mười lăm phút thôi, thằng Minh mải mê với cuốn truyện tranh mới mua ở Sài Gòn, không hề hay biết em nó đã lặng lẽ tuột khỏi tầm mắt tự lúc nào.

Cả nhà nháo nhào đi tìm ngay lúc phát hiện ra. Tôi chạy khắp khu vườn gọi tên con, tim đập thình thịch trong lồng ngực như tiếng trống trận vậy.

Người làm vườn già là người tìm thấy nó trước tiên, ở cái chỗ tôi ít ngờ tới nhất: mép giếng đá cổ tận cuối khu vườn sau, chỗ tôi vẫn dặn đi dặn lại tất cả người trong nhà phải hết sức cẩn trọng, vì rêu phong bám dày, đá trơn trượt quanh năm. Con Linh đứng đó, hai bàn chân trần giẫm ngay trên phiến đá ướt sát mép giếng, chỉ hụt chân thêm một bước nữa thôi là rơi thẳng xuống làn nước tối đen phía dưới rồi. Cái điều khiến người làm vườn dựng tóc gáy kể lại với tôi sau đó không phải là chuyện con bé đứng gần hiểm nguy đến vậy đâu, mà là VẺ MẶT của nó cơ — hoàn toàn không sợ hãi gì cả, cứ đứng yên như tượng, một bàn tay nhỏ xíu chỉ thẳng xuống mặt nước, môi mấp máy lặp đi lặp lại đúng hai chữ: "chú ơi, chú ơi." Mà có ai ở đó đâu.

Nhà tôi bế xốc con lên ngay lúc chạy tới, ôm chặt vào lòng chạy thẳng vào nhà, người run lên bần bật suốt cả quãng đường luôn. Tối đó, qua lớp vách gỗ mỏng, tôi nằm nghe nàng ru con gần một tiếng đồng hồ liền — lâu hơn hẳn mọi khi, giọng nàng đôi lúc khàn đi vì hát liên tục không nghỉ, nghe cứ như thể chính nàng đang cố tự trấn an mình nhiều hơn là ru con ngủ vậy.

Tôi không dám hỏi thêm gì tối đó, chỉ lặng lẽ ngồi cạnh, nắm lấy tay nàng thôi. Nhưng đêm nay tôi không tài nào chợp mắt được — cứ nằm trằn trọc mãi, cứ tưởng chừng như nghe thấy văng vẳng ngoài vườn tiếng nước động khẽ khàng dưới đáy giếng, mà đêm nay trời lặng gió mà, có cơn nào đủ mạnh để khua động mặt nước ấy đâu.

**Ngày 14 tháng Chín, 1964**

Tôi thật sự không biết phải bắt đầu từ đâu để viết cho phải đạo nữa. Đã ba ngày trôi qua rồi, kể từ cái buổi sáng định mệnh ấy, mà tôi vẫn chưa thể nào chấp nhận nổi những gì đang diễn ra trước mắt mình.

Nhà tôi không còn ở đây nữa.

Không một lời từ biệt. Không một mảnh giấy nào để lại giải thích. Không một dấu chân nào hằn trên lối đi rải sỏi ngoài vườn, dù sương đêm hôm đó đọng ướt khắp nơi — tôi đã cùng người làm kiểm tra kỹ lưỡng từng tấc đất quanh nhà rồi, khắp cổng chính, khắp hàng rào sắt bao quanh khu đất, không một chỗ nào có dấu hiệu bị phá cả. Nàng biến mất đi, y như thể chưa từng tồn tại trên cõi đời này vậy, chỉ để lại đúng một vật duy nhất đặt ngay ngắn trên bàn làm việc của tôi vào sáng hôm sau: cái hộp nhạc bằng đồng nàng vẫn nâng niu giữ gìn từ hồi con gái — khoá hộp bị hàn kín lại bằng chì nguội, chính tay nàng, tôi nhận ra ngay nét hàn quen thuộc từ những lần nàng tự sửa đồ trang sức của mình.

Tôi đã báo cho cảnh sát tỉnh rồi, cho cả họ hàng bên ngoại nàng ở Sài Gòn nữa, mà không ai biết gì cả, không ai nghe được tin tức gì hết. Tôi cứ tự trách mình mãi — nếu tối đó tôi không đi Sài Gòn công tác, nếu tôi ở nhà, chắc mọi chuyện đã khác rồi.

Sáng hôm kia có một người đàn bà lạ mặt, tôi không biết bà ta là ai, cứ đứng trước cổng nhà rất lâu, ăn mặc lam lũ, nói là vợ của một trong số mấy người phu khuân vác hồi trước làm công trình mở rộng cái giếng đá sau vườn năm ngoái đó. Bà ta hỏi tôi về khoản tiền công còn thiếu, rồi hỏi tôi tại sao chồng bà ta đi làm buổi cuối ở khu đất này rồi không bao giờ về nhà nữa. Tôi cũng chẳng biết trả lời sao, chỉ bảo người làm đưa cho bà ta ít tiền rồi tiễn khéo ra khỏi cổng thôi — dạo này đầu óc tôi cũng không còn sức đâu mà lo chuyện cũ nữa, chỉ mong tìm được tin tức nhà tôi trước đã. Giờ ngồi nghĩ lại, tôi cũng không nhớ rõ nữa, cái vụ đó rốt cuộc ai đứng ra giải quyết, hồ sơ công trình năm ấy ai lo liệu, tôi cũng chịu.

Thằng Minh khóc ròng rã suốt hai đêm liền không dứt, mười hai tuổi đầu mà đã phải gồng mình lên làm chỗ dựa cho em gái rồi. Con Linh thì ngược hẳn lại — im lặng đến mức đáng sợ, không khóc lấy một giọt nước mắt nào, cứ ngồi lặng thinh bên khung cửa sổ nhìn ra phía vườn sau hàng giờ liền. Tôi thì hoàn toàn bất lực, chẳng biết phải hỏi han con bé ra sao cho phải, sợ càng hỏi càng khiến nó thêm hoảng loạn.

Đêm hôm kia, lúc đang trằn trọc không sao chợp mắt được, tôi nghe thấy nó — rất khẽ khàng, rất xa xăm: đúng cái điệu ru đó, vọng lên từ hướng khu vườn sau. Mà đâu còn ai ở đó để cất tiếng hát cho ai nghe nữa đâu. Tôi cứ ngồi im vậy rất lâu trong bóng tối, không dám bật đèn, cũng không dám ra xem.

**Ngày 30 tháng Mười, 1964** *(nét chữ run rẩy hơn hẳn những đoạn trước, dừng đột ngột giữa câu)*

Tôi biết mình đã trì hoãn quá lâu rồi. Suốt một tháng ròng nay, tôi cứ tự nhủ với chính mình hoài rằng đây chỉ là mấy cơn hoảng loạn vô cớ của một người đàn ông vừa mất vợ thôi.

Nhưng đêm qua, không ngủ được, tôi lấy cây vĩ cầm cũ ra, dò lại từng nốt trong trí nhớ, ghi hẳn ra khuông nhạc cho chắc, kẻo mình lại tưởng tượng. Rồi tôi lục lại tủ sách, tìm đúng cuốn tổng phổ Requiem tôi mang về từ Paris năm nào, cuốn tôi mua sau buổi lễ cầu hồn ở nhà thờ hôm ấy — lâu lắm rồi tôi không giở tới nó. Tôi đặt hai bản nhạc cạnh nhau dưới ánh đèn dầu, so từng nốt một.

Khớp. Cả năm nốt Đô-Rê-Mi-Fa-Sol nhà tôi vẫn ru con, đều nằm trong đúng đoạn mở đầu bản **Dies Irae** — "Ngày Phán Xét" — khúc thánh ca cổ hàng trăm năm người ta xướng lên để tiễn đưa người chết. Tôi ngồi sững trước hai cuốn sổ rất lâu, không dám tin vào chính đôi mắt mình.

Có một chuyện tôi chưa từng dám ghi ra, kể từ cái ngày dọn về đây. Người làm cũ trong vùng, họ vẫn hay rỉ tai nhau một câu chuyện xưa về cây đàn dương cầm đó — rằng nó từng của một gia đình khác, trước cả gia đình tôi kia, mà hễ ai gõ trọn vẹn được khúc nhạc giấu trong đó thì cây đàn nó sẽ tự tấu lên hết cả bài, khỏi cần đụng tay vào phím nữa. Tôi vốn không tin mấy chuyện nhảm nhí đó đâu — cho tới cái đêm tôi thử gõ lại năm nốt mình vừa ghép được, ngay trên chính cây đàn ấy.

Tôi cũng không dám chắc là mình đã gõ đủ chưa nữa. Trí nhớ tôi chỉ giữ được có năm nốt thôi, mà cứ mường tượng hoài là còn thiếu — bài nhạc nghe cụt lủn làm sao ấy, như một câu nói bỏ lửng nửa chừng. Nhưng mà ngay lúc tôi vừa gõ xong nốt cuối cùng, đèn trong nhà nó đồng loạt chao một cái, đúng một cái thôi, rồi thôi. Tôi hỏi lại thì không ai trong nhà đụng vào cái công tắc đêm đó cả.

Tôi không thể để ai trong nhà này chơi lại điệu nhạc ấy một lần nào nữa. Tối nay, sau khi hai con đã ngủ, tôi xé tờ nhạc nhà tôi từng tự tay chép ra thành năm mảnh, giấu mỗi mảnh một nơi trong nhà — không cho phép bản thân giữ chúng cùng một chỗ, sợ một ngày yếu lòng mà ghép lại. Nếu có ai đọc được những dòng này — xin đừng đi tìm đủ năm mảnh giấy ấy. Đừng ngồi vào cây đàn đó.

Tôi nghe thấy tiếng bước chân ngoài hành *(cắt ngang giữa câu — hết nhật ký)*

---

## 7. SỔ GHI NỢ NHÂN CÔNG (tìm thấy trong Kho)

*(Cuốn sổ bìa da mốc, chữ viết tay ghi chép chi tiêu công trình, xen tiếng Pháp lẫn tiếng Việt. Đây là item hé lộ nguồn gốc oán khí, khách quan, không nhân vật nào tự thuật lại bằng lời — viết theo lối bút toán mơ hồ/quan liêu, KHÔNG viết thẳng "bịt miệng" hay "che giấu" vì không ai tự thú bằng văn bản chính thức của mình.)*

> "12/4/1963 — Chi 200 đồng, mướn 6 phu công nhật theo yêu cầu ông Harrison (phái đoàn viện trợ Mỹ), việc mở rộng giếng đá sau vườn."
>
> "30/4/1963 — Ngừng việc giếng đá. Chi thêm 50 đồng, khoản 'phụ cấp' gia đình phu tên Tư. Không ghi lý do."
>
> "3/5/1963 — Trừ lương 3 phu còn lại, lý do ghi sổ: tự ý bỏ việc."

Không có lời giải thích thêm — không nhắc lại tên "Tư" ở đâu khác, cũng không nói vì sao 3 người còn lại nghỉ việc cùng lúc. Người chơi tự suy ra chuyện mờ ám từ khoảng trống này.

*Lưu ý quan trọng: KHÔNG đặt claim "để Chapter 2 tìm lại" cho item này — Chapter 2 (Bích Ngọc) diễn ra năm 1970, TRƯỚC Chapter 1 (Khoa, năm 2000), nên không thể nhặt được đồ Khoa làm rơi 30 năm sau. Item này chỉ thuộc phạm vi Chapter 1, không cần nói rõ số phận sau đó.*

---

## 8. BẢNG TƯƠNG TÁC ĐẦY ĐỦ (31 vật)

| # | Khu vực | Vật thể | Loại tương tác | Kết quả / item | Suy nghĩ / Lời thoại |
|---|---------|---------|-----------------|-----------------|------------------------|
| 1 | Ngoài cổng | Cửa sổ phòng ăn | INTERACT | Vào nhà, bắt đầu gameplay | Lời thoại (cutscene intro) |
| 2 | Phòng Ăn | Bát đĩa vỡ | EXAMINE | Lore | Suy nghĩ |
| 3 | Phòng Ăn | Tranh phong cảnh Đà Lạt | EXAMINE | Lore | Suy nghĩ |
| 4 | Phòng Ăn | Mảnh giấy số ① | EXAMINE + PICKUP | Item: mảnh 1/5 — nốt E (Mi) | Suy nghĩ |
| 5 | Vestibule | Gương phủ vải đỏ | EXAMINE | Lore + cảnh báo | Suy nghĩ |
| 6 | Vestibule | Vải đỏ | INTERACT (cấm) | CHẾT NGAY nếu nhấc | Không cần lời thoại |
| 7 | Vestibule | Tủ giày | EXAMINE | Lore | Suy nghĩ |
| 8 | Vestibule | Mảnh giấy số ② | EXAMINE + PICKUP | Item: mảnh 2/5 — nốt C (Đô) | Suy nghĩ |
| 9 | Phòng Tiếp Khách | Chân dung ông chủ nhà | EXAMINE | Lore | Suy nghĩ |
| 10 | Phòng Tiếp Khách | Lò sưởi | EXAMINE | Lore | Suy nghĩ |
| 11 | Phòng Tiếp Khách | Mảnh giấy số ③ | EXAMINE + PICKUP | Item: mảnh 3/5 — nốt F (Fa) | Suy nghĩ |
| 12 | Salon | Piano (chưa đủ điều kiện) | INTERACT | Thoại từ chối | Suy nghĩ |
| 13 | Salon | Sofa nhung đỏ | EXAMINE | Lore | Suy nghĩ |
| 14 | Salon | Mảnh giấy số ④ | EXAMINE + PICKUP | Item: mảnh 4/5 — nốt D (Rê) | Suy nghĩ |
| 15 | Salon | Tranh ảnh gia đình | EXAMINE | Lore | Suy nghĩ |
| 16 | Salon | Tủ cabinet (ngăn kéo kẹt) | EXAMINE | Gợi ý cần đòn bẩy | Suy nghĩ |
| 17 | Salon | Nến + đế đồng | EXAMINE + PICKUP | Item: Đế đồng (đòn bẩy) | Suy nghĩ |
| 18 | Salon | Ngăn kéo tủ cabinet | USE đế đồng | KEY_01 (chìa khoá kho) | Suy nghĩ |
| 19 | Kho | Cửa kho (LOCK_01) | USE KEY_01 | Mở kho | Suy nghĩ |
| 20 | Kho | Bảng ký hiệu nốt nhạc | EXAMINE + PICKUP | Item: giải mã C-D-E-F-G = Đô-Rê-Mi-Fa-Sol | Suy nghĩ |
| 21 | Kho | Dụng cụ cũ | EXAMINE | Lore | Suy nghĩ |
| 22 | Kho | Sổ ghi nợ nhân công | EXAMINE + PICKUP | Item: di vật thứ 2 — hé lộ nguồn gốc oán khí | Suy nghĩ |
| 23 | Kho | Vết xước bí ẩn trên sàn | EXAMINE | Lore, câu hỏi bỏ ngỏ | Suy nghĩ |
| 24 | Kho | Mảnh giấy số ⑤ | EXAMINE + PICKUP | Item: mảnh 5/5 — nốt G (Sol), đủ bộ E-C-F-D-G | Suy nghĩ |
| 25 | Sân Sau | Giếng đá | KHÔNG tương tác | Foreshadow | Suy nghĩ |
| 26 | Salon | Piano (đủ điều kiện) | INTERACT | Puzzle zigzag Mi→Đô→Fa→Rê→Sol → mở khoá Thư Phòng | Suy nghĩ (giật mình ngắn, KHÔNG phải lời thoại) |
| 27 | Thư Phòng | Tủ sách | EXAMINE | Lore (Ch.4 mới tương tác được) | Suy nghĩ |
| 28 | Thư Phòng | Nhật ký dang dở | EXAMINE | 4 đoạn nhật ký Đỗ Văn Minh 1964 | Suy nghĩ khi đọc + lời thoại ngắn sau khi đọc |
| 29 | Thư Phòng | Hộp âm nhạc đồng | INTERACT (phát 5 đoạn băng) + PICKUP | Di vật Chapter 1 | Suy nghĩ (Khoa) — 5 đoạn băng là lời thoại thật, giọng Bà Lan |
| 30 | Hành Lang | Tủ áo | INTERACT (HideSpot) | Trốn Ma Vú Dài — cinematic | Lời thoại (cutscene trốn) |
| 31 | Sân Sau | Giếng đá (lần 2) | AUTO-TRIGGER | DEATH SEQUENCE | Lời thoại (cutscene chết) |

---

## 9. GHI CHÚ BUILD TỔNG HỢP

- **Piano:** đổi `_correctSequence` trong `PianoInteractable.cs` từ `["D","E","G","A","F"]` → `["E","C","F","D","G"]`; `_playableKeys` phải dùng đủ 7 phím thật theo đúng vị trí vật lý. (Thuận)
- **Nhật ký:** hiển thị dạng UI/panel đọc được, không đọc thành tiếng — chỉ 3 câu phản ứng ngắn sau khi đọc mới có voice.
- **Di vật cuối chương:** Hộp nhạc đồng (item pre-existing, số phận sau đó do GDD gốc quy định — xem lưu ý bên dưới) + Sổ ghi nợ nhân công (item mới của Chapter 1, KHÔNG claim số phận sau đó).
- **Lưu ý timeline (2026-07-15):** Chapter 2 (Bích Ngọc) diễn ra năm 1970 — TRƯỚC Chapter 1 (Khoa, năm 2000) — theo `GDD_BietThuBongToi_v3.md` ("2000, 1970, 1990, 2020"). Bảng hand-off vật phẩm sẵn có trong GDD gốc (hộp nhạc truyền Khoa→Ngọc→Hùng→Lan Anh) có vẻ cũng vướng lỗi này, nhưng đó là vấn đề CÓ SẴN từ trước, ngoài phạm vi sửa Chapter 1 lần này — chỉ đảm bảo sổ ghi nợ (item MỚI) không lặp lại lỗi tương tự.
- **Ghost nav:** Ma Vú Dài patrol qua Salon mỗi ~60 giây từ hướng hành lang trong lúc Khoa còn ở Phần V-VIII; KHÔNG tuần tra tới khu Bếp/Kho (Phần VI) — khoảng thở duy nhất của chương.
