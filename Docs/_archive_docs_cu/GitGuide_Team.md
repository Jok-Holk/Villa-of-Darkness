# Hướng dẫn Git cho team — đọc trước khi đụng vào bất kỳ ticket nào

> Dự án dùng **Git LFS** cho file model (`.glb`, `.fbx`). Nếu không làm đúng bước dưới, model sẽ import lỗi hàng loạt trong Unity (đã từng xảy ra thật — 550+ model lỗi "Failed to import" vì file chỉ là pointer text, chưa tải nội dung thật).

## 1. Cài đặt bắt buộc TRƯỚC KHI clone (làm 1 lần duy nhất trên máy)

1. Cài **Git** (không chỉ GitHub Desktop) — tải tại git-scm.com nếu chưa có, để dùng được lệnh dòng lệnh khi cần.
2. Cài **Git LFS**: tải tại git-lfs.github.com, sau đó mở terminal (Git Bash/CMD) gõ:
   ```
   git lfs install
   ```
   Chỉ cần làm 1 lần cho mỗi máy.

## 2. Vì sao GitHub Desktop có nguy cơ

GitHub Desktop TỰ ĐỘNG xử lý LFS khi clone lần đầu, nhưng khi **pull nhánh mới** hoặc **checkout qua lại giữa nhiều nhánh**, có trường hợp nó không tải đủ nội dung LFS thật — để lại file `.glb`/`.fbx` chỉ là **con trỏ text** (vài trăm byte) thay vì file model thật (vài MB). Unity thấy file bất thường này sẽ báo lỗi import hàng loạt.

**Cách chắc chắn nhất — chạy tay 1 lệnh sau MỖI LẦN clone/pull/checkout nhánh:**
```
git lfs pull
```
Lệnh này ép tải lại TOÀN BỘ nội dung LFS thật khớp với các con trỏ đang có trên máy — ngay cả khi GitHub Desktop đã pull xong, chạy thêm lệnh này không hại gì, chỉ để chắc chắn.

**Cách kiểm tra nhanh có bị lỗi pointer không:** mở 1 file `.glb` bất kỳ bằng Notepad — nếu thấy toàn chữ (vài dòng bắt đầu bằng `version https://git-lfs...`) thay vì lung tung ký tự nhị phân → CHƯA tải LFS thật, chạy `git lfs pull` ngay.

## 3. Quy trình làm 1 ticket (branch mới theo cấu trúc `edge/...`)

```bash
# 1. Đảm bảo đang ở main, đã cập nhật mới nhất
git checkout main
git pull
git lfs pull

# 2. Tạo nhánh riêng cho ticket của bạn
git checkout -b edge/<ten-ticket>/<username>
# Ví dụ: git checkout -b edge/door-garden/tuananh

# 3. Làm việc trong SCENE RIÊNG của bạn (không đụng file người khác)
# ... làm ticket, test theo Tasks_Chapter1.md ...

# 4. Commit — CHỈ add đúng file bạn thực sự sửa, không add lung tung
git add Assets/_Project/Scenes/<scene-cua-ban>.unity
git add Assets/_Project/Scenes/<scene-cua-ban>.unity.meta
git commit -m "feat(edge): mô tả ngắn gọn ticket đã làm"

# 5. Push nhánh của bạn (KHÔNG push vào main)
git push origin edge/<ten-ticket>/<username>

# 6. Báo Jok review — Jok sẽ merge, không tự merge vào main
```

## 4. Việc TUYỆT ĐỐI KHÔNG làm

- **Không** `git push --force` bất kỳ đâu.
- **Không** commit/push thẳng vào `main`.
- **Không** xoá nhánh của người khác.
- **Không** dùng `git add -A` hay `git add .` bừa — dễ vô tình add file rác/file người khác đang dở dang. Add đích danh file bạn sửa.
- **Không** cần lo về các file `.meta` — Unity tự tạo, luôn commit kèm file `.meta` đi cùng file chính (nếu tạo file mới mà thiếu `.meta`, Unity sẽ tự sinh lại khi mở, nhưng tốt nhất vẫn commit đủ cặp).

## 5. Nếu gặp lỗi lạ khi Pull/Checkout

- Model bị lỗi import hàng loạt → chạy `git lfs pull` trước khi báo Jok.
- Conflict khi merge → ĐỪNG tự ý chọn "Keep mine"/"Keep theirs" nếu không chắc — chụp ảnh lỗi, báo Jok.
- Nhánh `main` hiện tại đã đi trước `origin/main` một khoảng dài (lịch sử dồn từ nhiều đợt merge trước) — nếu thấy lệch nhiều, đó là bình thường, không phải bạn làm sai gì.
