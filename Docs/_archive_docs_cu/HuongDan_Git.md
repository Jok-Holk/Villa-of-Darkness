HƯỚNG DẪN GIT — ĐỌC TRƯỚC KHI LÀM BẤT KỲ VIỆC NÀO

Dự án có dùng file model 3D (đuôi .glb, .fbx) được lưu theo kiểu đặc biệt gọi là Git LFS. Nếu không cài đúng, lúc tải project về Unity sẽ báo lỗi hàng loạt model không mở được (đã từng bị vậy thật rồi).

BƯỚC 1 — Cài đặt (chỉ làm 1 lần trên máy mình)

1. Cài Git: vào trang git-scm.com tải về cài, nếu máy chưa có.
2. Cài Git LFS: vào trang git-lfs.github.com tải về cài.
3. Mở CMD (hoặc Git Bash), gõ lệnh này rồi Enter, chỉ cần làm 1 lần:
   git lfs install

BƯỚC 2 — Sau khi tải project về (mỗi lần tải mới/cập nhật)

Dùng GitHub Desktop tải project về xong, MỞ THÊM CMD/Git Bash tại đúng thư mục project, gõ lệnh:
   git lfs pull

Lệnh này đảm bảo các file model 3D được tải đầy đủ thật, không bị thiếu. Làm bước này mỗi lần tải/cập nhật project, không hại gì nếu làm dư.

Cách kiểm tra nhanh có bị lỗi không: mở thử 1 file .glb bằng Notepad — nếu thấy toàn CHỮ (vài dòng bắt đầu bằng "version https://git-lfs...") thay vì lộn xộn ký tự lạ → chưa tải đủ, chạy lại lệnh git lfs pull ở trên.

BƯỚC 3 — Cách làm 1 việc được giao

1. Đảm bảo đang ở nhánh main, cập nhật mới nhất:
   git checkout main
   git pull
   git lfs pull

2. Tạo nhánh riêng cho việc của mình (đặt tên rõ ràng):
   git checkout -b edge/ten-viec/ten-minh
   Ví dụ: git checkout -b edge/chia-khoa/thuan

3. Làm việc trong SCENE RIÊNG của mình, không đụng file người khác.

4. Khi làm xong, lưu lại (chỉ lưu đúng file mình sửa):
   git add Assets/_Project/Scenes/ten-scene-cua-minh.unity
   git add Assets/_Project/Scenes/ten-scene-cua-minh.unity.meta
   git commit -m "làm xong: mô tả ngắn gọn việc đã làm"

5. Gửi nhánh của mình lên (không gửi thẳng vào main):
   git push origin edge/ten-viec/ten-minh

6. Báo nhóm trưởng để kiểm tra và gộp lại — đừng tự gộp vào main.

NHỮNG VIỆC TUYỆT ĐỐI KHÔNG LÀM

- Không dùng lệnh git push --force ở bất cứ đâu.
- Không commit/push thẳng vào nhánh main.
- Không xoá nhánh của người khác.
- Không dùng lệnh "git add -A" hay "git add ." — dễ lỡ tay thêm nhầm file người khác đang làm dở. Chỉ add đúng file mình sửa.
- Các file đuôi .meta cứ để Unity tự lo, thường đi kèm sẵn với file chính, không cần chỉnh gì.

NẾU GẶP LỖI LẠ

- Model bị lỗi mở không được → chạy lại "git lfs pull" trước khi báo ai.
- Bị báo xung đột (conflict) lúc gộp code → ĐỪNG tự chọn "Keep mine" hay "Keep theirs" nếu không chắc — chụp ảnh lỗi gửi nhóm trưởng xem trước.
