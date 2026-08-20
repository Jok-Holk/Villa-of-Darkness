# Plan — Hoàn thiện Chapter 1

> 2026-07-06 · PM: Jok. File này = chiến lược tổng. Task chi tiết → `Tasks_Chapter1.md`. Kiến trúc kỹ thuật (SettingsManager/Audio/Fog...) → `Architecture_MainMenuFoundation.md`.

## Bối cảnh

- Giảng viên bắt buộc cả nhóm làm game thật (không chỉ báo cáo), scope cắt còn Chapter 1.
- Map cũ **phế bỏ hoàn toàn** — dựng lại từ số 0 trên khuôn kiến trúc mới.
- Team lịch sử làm kém — chuyển sang **task cực nhỏ, độc lập, kiểm chứng được**, không giao "1 khu vực lớn" cho 1 người nữa.
- **Không tổ chức theo Phase nữa.** Bỏ hẳn naming `phase2.x/...`.

## Mô hình làm việc

1. **Jok dựng khuôn kiến trúc trước** (tường ngoài + mặt bằng từng tầng, kể cả tầng trống) — không giao được, xem `Tasks_Chapter1.md` mục 0.
2. **Mỗi người tạo 1 scene Unity MỚI, RIÊNG** cho phần việc của mình — không ai chung file với ai, không ai đụng khuôn kiến trúc gốc.
3. **Test độc lập bằng Additive Scene**: mở scene của mình làm scene chính, sau đó `File → Open Scene (Additive) → Assets/_Project/Scenes/MainMenu.unity` để có sẵn toàn bộ hệ thống nền (SettingsManager, AudioManager, GameManager, FogManager, ScreenFader) mà KHÔNG cần hiểu cách chúng hoạt động bên trong. Chi tiết lý do + cách làm → `Architecture_MainMenuFoundation.md`.
4. **Task cực nhỏ** (xem `Tasks_Chapter1.md`) — mỗi ticket làm xong trong 1 buổi, có bước test rõ ràng, có câu hỏi xác nhận đã đọc kỹ hướng dẫn.
5. **Không theo ngày/tuần** — xong ticket nào báo ngay ticket đó.
6. **Jok ghép cuối cùng**: gom tất cả scene lẻ vào khuôn kiến trúc thật, bake NavMesh/Occlusion/Lighting, xử lý xung đột nếu có.

## Git — cấu trúc nhánh mới (không theo Phase)

```
edge/<hạng-mục>/<username>
```
Ví dụ: `edge/door-garden/tuananh`, `edge/inventory-itemdata/thuan`, `edge/audiolog-01/tan`.

- Mỗi ticket nhỏ trong `Tasks_Chapter1.md` = 1 nhánh riêng, commit nhỏ, PR/báo Jok review khi xong đúng 1 ticket.
- Không tạo nhánh lớn ôm nhiều ticket — vỡ trận là mất công review, khó tách lỗi ai gây ra.

## Nguyên tắc phân bổ ticket

- Ticket không gắn cứng cho 1 người — ai làm xong ticket của mình trước thì nhận ticket tiếp theo trong hàng đợi (`Tasks_Chapter1.md` liệt kê theo thứ tự ưu tiên, không theo tên người).
- Ai bị kẹt >1 buổi ở 1 ticket → trả lại hàng đợi, Jok giao người khác, không ngồi im.
- Việc phi-dev (báo cáo, slide, use case, video demo, test checklist, build đa máy...) vẫn giữ nguyên như trước, chạy song song, không đổi.

## Cách tiếp cận khi làm ticket — mindset, không chỉ làm-theo-bước

Vấn đề gốc không phải thiếu hướng dẫn — là thói quen làm việc. 5 quy tắc sau áp dụng cho MỌI ticket, không riêng gì:

1. **Test sau MỖI bước nhỏ, không dồn lại test 1 lần cuối.** Kéo xong 1 field → Play thử ngay xem có tác dụng không, rồi mới làm bước tiếp theo. Nếu làm 5 bước rồi mới test, lỗi ở bước nào sẽ không biết.
2. **Đừng đoán — nhìn vào Inspector/Console để xác nhận.** "Chắc là được rồi" không phải bằng chứng. Field còn ghi "None"/"Missing" = chưa xong, dù bạn nghĩ đã kéo. Console không có dòng đỏ KHÔNG có nghĩa là chạy đúng — phải thấy đúng KẾT QUẢ (âm thanh phát, UI hiện, vật thể di chuyển...), không chỉ "không lỗi".
3. **Lỗi im lặng là loại lỗi phổ biến nhất trong dự án này** — code dùng `?.` (an toàn) nên thiếu field/thiếu object KHÔNG báo lỗi đỏ, chỉ đơn giản không chạy gì cả. Nếu làm đúng hướng dẫn mà "không có gì xảy ra" → nghi ngờ ĐẦU TIÊN là 1 field nào đó còn trống, không phải code sai.
4. **Bắt chước đúng 1 ví dụ đã chạy được, đừng tự sáng tạo cách mới.** Nếu ticket na ná ticket khác đã có người làm xong, mở scene của họ ra xem chính xác họ setup thế nào rồi làm y hệt cho phần của mình.
5. **Test lại từ đầu như người chơi lần đầu trước khi báo xong** — không phải chỉ test đúng cái mình vừa sửa. Thoát Play, vào lại, làm lại luồng từ đầu.

Nếu làm đúng 5 điều trên mà vẫn không ra — báo Jok NGAY kèm mô tả cụ thể đã thử gì, đừng ngồi đoán mò nhiều giờ.

## Định nghĩa "xong Chapter 1"

Chơi được đầu-cuối: đi từ ngoài vào nhà → khám phá → piano puzzle → ít nhất 1 Audio Log → ghost đe doạ + núp trốn + gương → chết/retry → pause/về menu. KHÔNG yêu cầu 0 bug tuyệt đối hay asset đa dạng như game thương mại.
