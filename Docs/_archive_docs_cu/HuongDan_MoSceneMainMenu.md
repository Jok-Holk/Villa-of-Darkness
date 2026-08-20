TẠI SAO PHẢI MỞ THÊM SCENE MAINMENU KHI TEST — ĐỌC TRƯỚC KHI LÀM VIỆC

Game có 1 số hệ thống dùng chung (âm lượng, đồ hoạ, chuyển màn hình, hiệu ứng tối màn hình...) — mấy thứ này CHỈ được tạo ra khi mở scene MainMenu lên. Nếu bạn tạo 1 scene trống mới rồi bấm Play luôn, mấy hệ thống này KHÔNG có, dẫn tới:

- Không báo lỗi đỏ gì cả trong Console.
- Nhưng âm thanh không phát, hoặc 1 số thứ không hoạt động, dù bạn làm đúng hết các bước.
- Bạn sẽ tưởng mình làm sai — thật ra chỉ vì thiếu bước mở thêm MainMenu.

CÁCH LÀM ĐÚNG (làm mỗi lần test)

1. Mở scene MỚI của bạn làm scene chính (File > Open Scene).
2. Mở thêm scene MainMenu cùng lúc: File > Open Scene (Additive) > chọn Assets/_Project/Scenes/MainMenu.unity.
3. Bấm Play — giờ mọi thứ hoạt động bình thường.
4. Khi lưu lại, CHỈ lưu scene của bạn (Ctrl+S lúc scene của bạn đang được chọn). TUYỆT ĐỐI không lưu lại scene MainMenu.

QUY TẮC LÀM UI (nếu việc của bạn có màn hình/giao diện)

- Canvas chọn kiểu "Screen Space - Overlay", không chọn "Screen Space - Camera" — chọn sai kiểu này UI sẽ bị mờ/nhoè theo cảnh 3D phía sau.
- Trước khi chỉnh kích thước/vị trí, kiểm tra "Scale" của object phải là (1, 1, 1) — nếu là số khác (ví dụ 2.9) thì UI sẽ bị to/nhỏ sai dù số bạn chỉnh trông có vẻ đúng.
- Chữ tiếng Việt có dấu (ẫ, ố, ữ...) KHÔNG hiện đúng với font "JustMeAgainDownHere SDF" — nếu là chữ tiếng Việt có dấu, dùng font khác hoặc viết tiếng Anh tạm.
- Muốn phát âm thanh trong code của mình → gọi qua AudioManager.Instance.PlaySFX(...), đừng tự viết cách phát âm thanh riêng.

KIỂM TRA NHANH TRƯỚC KHI BÁO XONG (nếu việc có UI)

- Đã mở thêm MainMenu lúc test chưa (không phải scene trống)?
- Scale của UI đã là 1 chưa?
- Canvas đã chọn đúng kiểu Overlay chưa?
- Chữ tiếng Việt có dấu có bị lỗi không?
