# BIỆT THỰ ĐỖ GIA — BUILD SPECIFICATION v1
## Villa of Darkness · Tài liệu kiến trúc nội bộ

---

## ⚠️ CHANGELOG — DIFF SO VỚI BẢN GỐC


### Tầng Trệt
| | Bản gốc (Jok paste) | GDD v3 (canonical) |
|--|---------------------|-------------------|
| Bếp | **Bên trong** nhà, NW corner, 4×4.5m | **Nhà phụ riêng biệt** ngoài sân sau |
| Nhà kho | **Bên trong** nhà, NW corner, 4×3.5m | **Nhà phụ kho** ngoài sân sau (vẫn locked KEY_01) |
| Sảnh chính | Foyer 5×5m ở giữa mặt Nam | **Vestibule** — hành lang ngang hẹp ngay sau cửa chính |
| WC | Không có | **Có** — góc ĐÔNG phía Nam |
| Véranda | Chỉ là hiên ngoài (1.2m) | **Phòng riêng** bên trong, phía ĐÔNG-BẮC |
| Entry Ch.1 | Cửa sổ bếp (bếp trong nhà) | Cửa sổ Véranda hé mở (mặt ĐÔNG, từ hiên vườn phải) |
| Bảng ký hiệu nốt | Bếp trong nhà | Nhà phụ kho (ngoài, cần KEY_01) |

### Tầng 1
| | Bản gốc | GDD v3 |
|--|---------|--------|
| Phòng bà Lan | 1 phòng (4×4.5m) | **2 phòng riêng biệt**: Chambre de Madame I + Boudoir (thêm phòng thay đồ) |
| Phòng sinh hoạt | Salon de famille 5×5m | **Không còn** — diện tích phân bổ cho phòng ông Đỗ rộng hơn |
| Phòng ngủ phụ | Chambre d'appoint | Giữ, đổi tên **Chambre vide** (phòng trống — ghost dừng ở đây) |

### Tầng 2
| | Bản gốc | GDD v3 |
|--|---------|--------|
| Phòng đọc sách | Có | **Đổi thành Phòng Trà** (Salon de thé) |
| Phòng tắm | Không có T2 | **Thêm phòng tắm T2** (nhỏ, góc ĐÔNG) |
| Tên phòng bà Lan T2 | Phòng vợ | **Chambre de retraite** (phòng điên loạn) |

---

## MỤC LỤC
1. [Tổng Quan & Bối Cảnh Lịch Sử](#1)
2. [Thông Số Kỹ Thuật Xây Dựng](#2)
3. [Khuôn Viên Tổng Thể](#3)
4. [Hàng Rào & Cổng Chính](#4)
5. [Vườn Trước & Lối Vào](#5)
6. [Tầng Trệt (Chapter 1 & 4)](#6)
7. [Tầng 1 (Chapter 2 & 4)](#7)
8. [Tầng 2 + Tháp Canh (Chapter 3 & 4)](#8)
9. [Tầng Hầm (Chapter 4 Only)](#9)
10. [Sân Sau & Giếng](#10)
11. [Hành Lang Vườn & Lối Đi Ngoại Thất](#11)
12. [Hệ Thống Cầu Thang](#12)
13. [Logic Gameplay Gắn Với Kiến Trúc](#13)
14. [Bảng Chìa Khoá & Khoá (KEY/LOCK Master)](#14)
15. [Ghost Patrol Routes Per Chapter](#15)
16. [Cross-Chapter Item Inheritance](#16)
17. [Decay & Visual State Per Zone](#17)

---

<a name="1"></a>
## 1. TỔNG QUAN & BỐI CẢNH LỊCH SỬ

| Thông số | Giá trị |
|----------|---------|
| Tên | Biệt Thự Đỗ Gia (Đỗ Gia Villa) |
| Năm xây dựng (lore) | 1945 |
| Bỏ hoang từ | 1965 (sau cái chết của bà Lan) |
| Chủ sở hữu | Đỗ Văn Minh — địa chủ giàu có người Việt |
| Kiến trúc sư (lore) | Kiến trúc sư Pháp phối hợp Đỗ Văn Minh |
| Vị trí | Giữa rừng thông Đà Lạt, sườn đồi nhẹ |
| Phong cách | Indochine Style — pha trộn Pháp + Á Đông, thích ứng khí hậu Đà Lạt |

**Gia đình Đỗ:**
- **Đỗ Văn Minh** — chủ nhà, nhân vật bí ẩn trung tâm
- **Bà Lan** — vợ, dần phát điên sau cái chết của con gái. Chết 1965.
- **Đỗ Linh** — con gái nhỏ, chết bí ẩn. Là nguồn gốc Ma Vú Dài.
- **Đỗ Minh** — con trai 12 tuổi, sống sót / biến mất

**Nhân vật chơi:**
| Chapter | Nhân vật | Vai trò |
|---------|----------|---------|
| Ch.1 | Minh Khoa | Thanh niên mạo hiểm, đột nhập đêm |
| Ch.2 | Bích Ngọc | Bạn của Khoa, tìm kiếm anh |
| Ch.3 | Tuấn Hùng | Thám tử/nhà nghiên cứu |
| Ch.4 | Lan Anh | Hậu duệ gia đình Đỗ, kết thúc lời nguyền |

**Nguyên tắc kiến trúc tuân thủ:**
- Đối xứng trái-phải (cân bằng âm-dương theo phong cách cổ điển Pháp)
- Tất cả tầng có cùng diện tích footprint — tường chịu lực thẳng hàng theo chiều dọc
- Không quá 3 tầng nổi (đúng quy định biệt thự Pháp tại Đà Lạt)
- Mái ngói dốc — thích hợp khí hậu mưa nhiều Đà Lạt
- Tường đôi đá chẻ với xỉ than cách nhiệt
- Mỗi phòng chính có lò sưởi + cửa sổ thông ra hoa viên

---

<a name="2"></a>
## 2. THÔNG SỐ KỸ THUẬT XÂY DỰNG

### 2.1 Footprint Chính (Khối Nhà)

| Thông số | Giá trị | Ghi chú |
|----------|---------|---------|
| Chiều rộng nhà (mặt tiền) | 16.0m | Hướng Nam — mặt nhìn ra cổng |
| Chiều sâu nhà | 12.0m | Không tính hiên/hành lang ngoài |
| Chiều rộng kể hiên hai bên | 18.4m | +1.2m mỗi bên hiên hành lang |
| Chiều sâu kể hiên trước/sau | 14.4m | +1.2m hiên trước, +1.2m hiên sau |
| Diện tích sàn mỗi tầng (trong tường) | ~192 m² | 16m × 12m |
| Diện tích sàn kể hiên | ~265 m² | 18.4m × 14.4m |

### 2.2 Chiều Cao Tầng

| Tầng | Chiều cao trần (lọt lòng) | Chiều cao sàn-sàn | Ghi chú |
|------|--------------------------|------------------|---------|
| Tầng hầm | 2.4m | 2.7m | Nửa chìm, nửa nổi 0.8m trên mặt đất |
| Tầng trệt | 3.6m | 4.0m | Tầng đại sảnh — trần cao nhất |
| Tầng 1 | 3.2m | 3.6m | Tầng sinh hoạt chính gia đình |
| Tầng 2 | 3.0m | 3.4m | Phòng ngủ con + phòng vợ |
| Tháp canh | 2.8m | 3.2m | Phòng nhỏ trên nóc góc Đông |

Tổng chiều cao từ mặt đất đến nóc mái chính: **~14.5m**
Tổng chiều cao tháp canh: **~17.0m** (điểm cao nhất toàn công trình)

### 2.3 Tường

| Vị trí | Độ dày | Cấu trúc | Ghi chú |
|--------|--------|----------|---------|
| Tường ngoài (chịu lực) | 40cm | Đá chẻ 2 lớp × 12cm + khoảng cách 16cm đổ xỉ than | Cách nhiệt, cách âm, giữ ấm |
| Tường trong (chịu lực) | 30cm | Đá chẻ 1 lớp, trát vữa cát-vôi 2 mặt | Phân chia không gian chính |
| Tường ngăn (không chịu lực) | 15cm | Gạch nung, trát vữa | Phân chia phòng phụ |
| Tường tầng hầm | 50cm | Đá chẻ 2 lớp + xỉ than dày hơn | Chống ẩm, chịu áp đất |

### 2.4 Sàn

| Vị trí | Cấu trúc | Độ dày | Bề mặt |
|--------|----------|--------|--------|
| Sàn tầng hầm | Bê tông đá hộc trên nền đất đầm | 20cm | Đá mài thô |
| Sàn tầng trệt | Dầm gỗ lim + ván sàn gỗ trên tường hầm | 25cm | Gạch bông hoa văn Đông Dương |
| Sàn tầng 1 | Dầm gỗ lim + ván gỗ | 25cm | Gỗ lim đánh bóng — kẽo kẹt khi đi |
| Sàn tầng 2 | Dầm gỗ lim + ván gỗ | 25cm | Gỗ lim đánh bóng |
| Sàn tháp canh | Dầm gỗ + ván gỗ | 20cm | Gỗ thông mộc |

### 2.5 Trần

| Vị trí | Cấu trúc | Ghi chú |
|--------|----------|---------|
| Trần tầng hầm | Dầm gỗ lim lộ thiên + ván sàn tầng trệt phía trên | Thô, u ám |
| Trần tầng trệt | Trát vữa trên lưới gỗ, đắp phào chỉ thạch cao | Phào chỉ trang trí hoa văn |
| Trần tầng 1 | Trát vữa + phào chỉ đơn giản hơn | Ít trang trí hơn tầng trệt |
| Trần tầng 2 | Gỗ mái lộ thiên (vì kèo gỗ) | Phòng áp mái, thấy được cấu trúc kèo |

### 2.6 Mái

| Thông số | Giá trị |
|----------|---------|
| Kiểu mái | Mái ngói dốc kiểu chồng diêm (hai lớp mái chính) |
| Độ dốc | 45° (mái chính), 35° (mái hiên phụ) |
| Vật liệu | Ngói đất nung xám (ardoise giả — phổ biến Đà Lạt) |
| Mái nhô | 80cm ra khỏi tường ngoài — che mưa tạt |
| Ống khói lò sưởi | 4 ống — 2 mặt Đông, 2 mặt Tây (đối xứng) |
| Tháp canh | Mái chóp bát giác, ngói tương tự, đỉnh có chóp sắt |

### 2.7 Cửa

| Loại | Kích thước (R × C) | Vật liệu | Ghi chú |
|------|--------------------|----------|---------|
| Cửa chính (đôi) | 1.8m × 2.8m | Gỗ lim, bản lề sắt rèn tay | Khóa sắt cổ, chốt then trong |
| Cửa phòng trong | 0.9m × 2.4m | Gỗ lim, tay nắm đồng | Một cánh |
| Cửa phòng đôi (thư phòng) | 1.4m × 2.4m | Gỗ lim, kính mờ ô nhỏ | Hai cánh Pháp |
| Cửa sổ tầng trệt | 1.0m × 1.8m | Gỗ hai lớp: lá sách ngoài + kính trong | Bậu cửa đá cao 80cm từ sàn |
| Cửa sổ tầng 1-2 | 1.0m × 1.6m | Tương tự, lá sách Đà Lạt | Bậu cửa đá cao 80cm từ sàn |
| Cửa sổ bếp (hé mở) | 0.8m × 1.2m | Gỗ, chốt yếu — gameplay entry Ch.1 | Bản lề cũ mục, có thể đẩy mở |
| Cửa tầng hầm (ẩn) | 0.9m × 2.0m | Gỗ sơn giống tường, sau tủ sách | Ch.4 only — ẩn |

---

<a name="3"></a>
## 3. KHUÔN VIÊN TỔNG THỂ

### Kích thước khu đất

| Thông số | Giá trị |
|----------|---------|
| Tổng diện tích khu đất | ~1,500 m² (30m rộng × 50m sâu) |
| Hướng cổng chính | Nam |
| Hướng sân sau + giếng | Bắc |
| Độ dốc địa hình | Nhẹ — cao dần từ Nam (cổng) → Bắc (sân sau) ~1.5m chênh lệch |

### Bố cục khuôn viên (từ Nam → Bắc)

```
[NAM — CỔNG SẮT]
     |
     ▼ (5m) — Lối đi đá + vườn trước
     |
[HÀNH LANG VƯỜN TRÁI] ←── NHÀ CHÍNH 16×12m ──→ [HÀNH LANG VƯỜN PHẢI]
     |                                                    |
     |          (mặt tiền hướng Nam)                      |
     |                                                    |
     ▼ (phía sau nhà — 8m)                               |
     |                                                    |
[SÂN SAU — ĐÁ LÁT + GIẾNG ĐÁ]                         |
     |                                                    |
[RỪNG THÔNG — RANH GIỚI KHU ĐẤT]
[BẮC]
```

---

<a name="4"></a>
## 4. HÀNG RÀO & CỔNG CHÍNH

### 4.1 Hàng Rào

| Thông số | Giá trị |
|----------|---------|
| Chiều cao | 1.8m |
| Chân tường rào | Đá chẻ xám, cao 60cm, dày 30cm |
| Phần trên | Song sắt rèn tay, hoa văn xoắn đơn giản kiểu Pháp |
| Trụ rào | Đá chẻ 40cm × 40cm, cách nhau 3m, đỉnh trụ có mũ đá hình chóp |
| Tổng chu vi | ~150m (bao quanh toàn khu đất) |
| Tình trạng (2000) | Rêu bám, sắt gỉ nâu đỏ, một số chỗ cong vẹo, dây leo bám |

### 4.2 Cổng Chính

| Thông số | Giá trị |
|----------|---------|
| Vị trí | Giữa mặt Nam, đối xứng với cửa chính nhà |
| Chiều rộng mở | 3.0m (2 cánh × 1.5m) |
| Chiều cao cổng | 2.5m (cánh cổng), trụ cổng 3.0m |
| Vật liệu cánh | Sắt rèn uốn, hoa văn hoa lá Á Đông cách điệu |
| Trụ cổng | Đá chẻ xám, 50cm × 50cm, bảng tên "ĐỖ GIA" sắt đúc gắn trên trụ trái |
| Khóa | Xích sắt quấn + ổ khóa cũ (gameplay: mở được từ trong) |
| Tình trạng | Cổng khép hờ — đẩy được, kẽo kẹt nặng |

---

<a name="5"></a>
## 5. VƯỜN TRƯỚC & LỐI VÀO

### 5.1 Lối Đi Chính (Cổng → Cửa Chính)

| Thông số | Giá trị |
|----------|---------|
| Chiều dài | 12m (từ cổng đến bậc thềm hiên trước) |
| Chiều rộng | 1.5m |
| Vật liệu | Đá phiến lát phẳng, khe cỏ mọc xen |
| Đặc điểm | Thẳng, đối xứng, hai bên trồng cây bụi thấp (đã hoang dại) |

### 5.2 Vườn Trước

| Khu vực | Mô tả |
|---------|-------|
| Hai bên lối đi | Bãi cỏ ~4m mỗi bên, cây bụi hoa cẩm tú cầu (héo úa), bồn hoa đá tròn |
| Góc Tây-Nam | Cây thông cổ thụ 1 cây, gốc to 60cm, tán rợp bóng |
| Góc Đông-Nam | Bụi cây dã quỳ hoang dại, băng ghế đá cũ |
| Tình trạng | Cỏ mọc cao ngang đầu gối, lá rụng dày, rêu bám đá |

### 5.3 Bậc Thềm Hiên Trước

| Thông số | Giá trị |
|----------|---------|
| Số bậc | 5 bậc (tổng cao 75cm — do nhà nâng trên hầm nửa chìm) |
| Chiều rộng bậc | 4.0m (chính giữa mặt tiền) |
| Chiều sâu bậc | 30cm |
| Chiều cao bậc | 15cm |
| Vật liệu | Đá granite xám, mài nhẵn |
| Lan can thềm | Đá đúc baluster kiểu Pháp, cao 80cm, hai bên bậc |

---

<a name="6"></a>
## 6. TẦNG TRỆT / GROUND FLOOR
*(Chapter 1 gameplay chính · Chapter 4 reuse)*

- Cao độ sàn: **+0.75m** so với mặt đất (trên tầng hầm nửa chìm)
- Chiều cao trần: **3.6m**
- Diện tích trong tường: 16m × 12m = **192 m²**
- **Bếp + Kho: NGOÀI nhà** (nhà phụ riêng biệt — xem §6.4)

### 6.1 Bố Cục Phòng (Nhìn Từ Trên — BẮC lên trên)

```
                    BẮC (sân sau)
    ┌──────────────────────────────────────┐
    │            GALERIE SÂN SAU          │
    ├──────────────────┬───────────────────┤
    │                  │  THƯ PHÒNG       │
    │  SALON           │  Cabinet de       │
    │  (Phòng Khách)   │  travail          │
    │  Piano + lò sưởi ├───────────────────┤
    │  ~5.7m × ~6.5m   │  VÉRANDA         │
    │                  │  (Phòng Sân)     │
    ├──────────────────┴──────────┬────────┤
    │      HÀNH LANG NGANG        │  CẦU  │
    │      (couloir central)      │ THANG │
    ├──────────────┬──────────────┤        │
    │  PHÒNG ĂN   │  V-HÀNH LANG │  WC   │
    │  Salle à     │  (2.3m wide) │       │
    │  manger      │              │       │
    ├──────────────┴──────────────┴────────┤
    │         VESTIBULE (Tiền Sảnh)        │
    │   Tranh gia đình · Gương vải đỏ ⚠    │
    │              ↓ CỬA CHÍNH            │
    └──────────────────────────────────────┘
                    NAM (mặt tiền)
```

**Chú thích bố cục:**
- **TÂY (trái):** Salon (full depth, từ galerie sân sau → hành lang)
- **ĐÔNG (phải):** Thư Phòng (BẮC) → Véranda (giữa) → WC + Cầu Thang (NAM)
- **V-Hành Lang:** Hành lang dọc hẹp (2.3m) nối Vestibule → Hành lang ngang, chia ĐÔNG-TÂY
- Không còn bếp/kho bên trong — diện tích phân bổ cho các phòng rộng hơn

### 6.2 Chi Tiết Từng Phòng

#### VESTIBULE *(Tiền Sảnh / Hall d'entrée)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 16m rộng × ~3m sâu (full-width strip) |
| Vị trí | Ngay sau cửa chính, NAM — toàn bộ chiều rộng nhà |
| Sàn | Gạch bông hoa văn caro đen-trắng kiểu Đông Dương |
| Trần | 3.6m, phào chỉ thạch cao, rosette trung tâm (đèn chùm đã rớt) |
| Cửa | Cửa đôi gỗ lim ra hiên NAM, lối thông V-hành lang (BẮC), lối thông salon (TÂY), thư phòng (ĐÔNG) |
| Gameplay | **Điểm bắt đầu Ch.4.** Tranh gia đình Đỗ treo tường BẮC — bị rách. **Gương phủ vải đỏ** trên tường — ⚠ ĐỪNG MỞ (cảnh báo sớm). |

#### SALON DE RÉCEPTION *(Phòng Khách)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5.7m (TÂY) × ~6.5m (sâu từ hành lang → galerie) |
| Vị trí | TÂY toàn bộ, từ hành lang ngang đến galerie sân sau |
| Sàn | Gạch bông hoa văn |
| Lò sưởi | Tường TÂY, đá granite, mặt lò 1.2m × 1.0m |
| Piano | Góc TÂY-BẮC — piano đứng gỗ đen, phím ngà vàng ố |
| Cửa sổ | 2 mặt TÂY (nhìn ra hiên + hành lang vườn trái), 1 mặt BẮC (galerie sân sau) |
| Nội thất gameplay | **Tủ cabinet gỗ** — ngăn kéo bị kẹt, cần nến để撬 |
| Gameplay | **Ch.1 piano puzzle** — D-E-G-A-F → mở thư phòng. Ch.4: 7 nốt D-E-G-A-F-B-C# |

#### CABINET DE TRAVAIL *(Thư Phòng)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~8m (ĐÔNG) × ~3.3m sâu |
| Vị trí | ĐÔNG-BẮC (góc sau-phải, tiếp giáp galerie sân sau) |
| Sàn | Gỗ lim |
| Lò sưởi | Tường ĐÔNG, nhỏ hơn salon |
| Tủ sách | Tường BẮC — gỗ lim cao 2.4m, kín tường. **SAU TỦ SÁCH = CỬA HẦM (Ch.4)** |
| Bàn làm việc | Bàn gỗ lớn giữa phòng, ngăn kéo → **hộp nhạc đồng** (Ch.1) |
| Cửa sổ | 2 mặt ĐÔNG (nhìn ra hiên vườn phải) |
| Cửa | Cửa đôi Pháp (kính mờ) thông Vestibule |
| Gameplay | Ch.1: hộp nhạc. Ch.2–4: nhật ký bà Lan, máy ghi âm, công tắc tần số #1. |

#### VÉRANDA *(Phòng Sân)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~8m (ĐÔNG) × ~3.4m sâu |
| Vị trí | ĐÔNG-giữa, giữa thư phòng (BẮC) và WC (NAM) |
| Sàn | Gạch bông, thoáng hơn các phòng |
| Cửa sổ | 1 mặt ĐÔNG (**HÉ MỞ — LỐI VÀO CH.1**), 1 mặt BẮC |
| Cửa | Thông thư phòng (BẮC), thông hành lang ngang (NAM-TÂY) |
| Gameplay | **Entry point Ch.1** — Khoa leo qua cửa sổ ĐÔNG từ hiên vườn phải. Phòng này nhìn ra giếng (mặt BẮC). |

#### HÀNH LANG NGANG *(Couloir central)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 16m (ĐÔNG-TÂY) × ~2.5m |
| Vị trí | Ngang giữa nhà, chia tầng trệt NAM/BẮC |
| Sàn | Gạch bông |
| Đặc điểm | **Tối nhất tầng trệt** — không cửa sổ trực tiếp |
| Gameplay | Ma Vú Dài patrol dọc đây. Âm thanh bước chân rõ nhất. |

#### SALLE À MANGER *(Phòng Ăn)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5.7m (TÂY) × ~4.5m sâu |
| Vị trí | TÂY-NAM (trái vestibule, phía sau V-hành lang) |
| Sàn | Gạch bông |
| Nội thất | Bàn ăn gỗ 8 ghế, tủ chén bát dọc tường BẮC |
| Cửa sổ | 1 mặt TÂY, 1 mặt NAM (nhìn ra hiên trước) |
| Gameplay | **Ngăn kéo tủ chén** → tờ nhạc cũ (5 nốt D-E-G-A-F khoanh tròn). Clue đầu tiên Ch.1. |

#### WC *(Cabinet de toilette)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~2.5m × ~3m |
| Vị trí | ĐÔNG-NAM, cạnh cầu thang, giữa Vestibule và hành lang |
| Sàn | Đá mài trắng |
| Gameplay | Không có puzzle. Âm thanh bước chân ma vọng vào đây — jump scare ambient. |

#### KHU CẦU THANG CHÍNH
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 3.0m × 3.5m |
| Vị trí | ĐÔNG-BẮC phần hạ (giữa WC và hành lang, phía ĐÔNG) |
| Kiểu | Chữ U — 2 vế thẳng + 1 chiếu nghỉ, gỗ lim, song sắt rèn |
| Nối | Tầng trệt ↔ Tầng 1 ↔ Tầng 2 ↔ Tháp canh |

### 6.3 Cửa Ra Sân Sau (Tầng Trệt)
- Vị trí: Tường BẮC, cuối V-hành lang dọc / cạnh hành lang ngang
- Kích thước: 1.0m × 2.4m, cửa đơn gỗ lim
- Tình trạng: **Khóa then trong**, chìa treo trên đinh gần cửa
- Gameplay Ch.1: Lối duy nhất ra sân sau → giếng → **death sequence**

### 6.4 NHÀ PHỤ (Ngoại Thất — Riêng Biệt)
*(Tách biệt hoàn toàn khỏi nhà chính, kết nối qua sân sau + galerie BẮC)*

```
[SÂN SAU]
          ←── Galerie BẮC ──→
                               ┌──────────────────┐
                               │   NHÀ PHỤ BẾP   │
                               │   Cuisine        │
                               │   ~5m × ~4m      │
                               ├──────────────────┤
                               │  NHÀ PHỤ KHO    │
                               │  Dépôt (LOCKED)  │
                               │  ~5m × ~4m       │
                               │  🔑 cần KEY_01   │
                               └──────────────────┘
```

#### NHÀ PHỤ BẾP *(Cuisine — dépendance)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5m × ~4m |
| Vị trí | Góc ĐÔNG-BẮC khuôn viên, sân sau, tách biệt nhà chính |
| Sàn | Đá lát thô |
| Nội thất | Bếp than đá, kệ gỗ mục, chậu rửa đá |
| Lối vào | Từ sân sau; hoặc từ galerie BẮC nhà chính (không có cửa nối thẳng) |
| Tình trạng | **Hoang phế nhất** — mái thủng một góc, rác tích tụ |
| Gameplay | Không có puzzle. Atmospheric. Tiếng gió qua mái thủng. |

#### NHÀ PHỤ KHO *(Dépôt — LOCKED KEY_01)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5m × ~4m |
| Vị trí | Liền kề nhà phụ bếp (phía NAM hơn) |
| Sàn | Đá thô |
| Đặc điểm | **TỐI HOÀN TOÀN** — không cửa sổ. Kệ gỗ, thùng gỗ, dụng cụ cũ. |
| Cửa | **KHÓA, cần KEY_01** |
| Gameplay | **Bảng ký hiệu nốt nhạc** treo tường → giải mã 5 nốt → đánh piano. Đây là "bất ngờ" của Ch.1 — player phải stealth qua sân sau rồi mới vào được. |

---

<a name="7"></a>
## 7. TẦNG 1 / FIRST FLOOR
*(Chapter 2 gameplay chính · Chapter 4 reuse)*

- Cao độ sàn: **+4.75m** so với mặt đất
- Chiều cao trần: **3.2m**
- Diện tích: 16m × 12m = **192 m²** (y chang tầng trệt)

### 7.1 Bố Cục Phòng (GDD v3 — BẮC lên trên)

```
                    BẮC (sân sau)
    ┌──────────────────────────────────────┐
    │  PHÒNG TẮM │   CẦU    │ CHAMBRE    │
    │  Salle de  │  THANG   │ VIDE       │
    │  bains     │          │ (p.trống)  │
    │  (Ma Da!)  │          │            │
    ├─────────────┤          ├────────────┤
    │  CHAMBRE   │          │ BOUDOIR    │
    │  MADAME I  │          │ DE MADAME  │
    │  (B.Lan I) │          │ (B.Lan II) │
    │  (nhật ký) │          │            │
    ├─────────────┴──────────┴────────────┤
    │            HÀNH LANG DÀI           │
    │   (tối nhất game · gỗ kẽo kẹt)    │
    ├─────────────┬───────────┬───────────┤
    │  CHAMBRE   │  BAN      │ CHAMBRE   │
    │  MONSIEUR  │  CÔNG     │ LA FILLE  │
    │  (ô.Đỗ)   │  (balcon) │ (bé Linh) │
    │  KEY_03 ↓  │  mặt NAM  │ gương bạc │
    └─────────────┴───────────┴───────────┘
                    NAM (mặt tiền)
```

**Thay đổi so với bản gốc:**
- ✅ Thêm **Boudoir de Madame** (B.Lan II) — phòng thay đồ/ngồi riêng của bà Lan (cạnh Chambre Madame I)
- ✅ Đổi "Phòng Sinh Hoạt" → **Ban Công** + **Chambre de Monsieur** rộng hơn
- ✅ "Chambre d'appoint" đổi thành **Chambre Vide** (phòng trống — ghost dừng ở đây)

### 7.2 Chi Tiết Từng Phòng

#### HÀNH LANG DÀI *(Couloir long)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 16.0m × ~1.5m |
| Đặc biệt | **Tối nhất game.** Tranh ảnh gia đình Đỗ treo dày đặc cả hai tường |
| Sàn | Gỗ lim — **kẽo kẹt to khi đi** (âm thanh cảnh báo ghost) |
| Cửa sổ | **KHÔNG CÓ** — chỉ sáng từ phòng hé cửa |
| Gameplay | Audio log #2 (tranh hành lang). Tiếng bước chân không rõ nguồn. Tủ khoá hành lang (cần KEY_03). |

#### CHAMBRE DE MONSIEUR *(Phòng ông Đỗ)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5.5m × ~5m |
| Vị trí | TÂY-NAM |
| Lò sưởi | Tường TÂY |
| Nội thất | Giường đôi gỗ khung sắt, tủ quần áo, bàn làm việc, gương (vải đỏ che) |
| Gameplay | Ch.2: **KEY_03** (chìa tủ hành lang) — dưới bàn làm việc. |

#### BAN CÔNG *(Balcon — mặt Nam)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 1.2m sâu × ~3m rộng |
| Vị trí | Chính giữa mặt NAM tầng 1 |
| Lan can | Sắt rèn hoa văn, gỉ sét |
| Gameplay | Nhìn ra vườn trước. Atmospheric — nghe tiếng gió + thấy cổng sắt. |

#### CHAMBRE DE LA FILLE *(Phòng Bé Linh)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~3m × ~5m |
| Vị trí | ĐÔNG-NAM |
| Đặc biệt | **KHÔNG CÓ GIÓ** — kín gió nhất (đèn dầu không dao động) |
| Nội thất | Giường nhỏ, búp bê cũ, bức vẽ trẻ con |
| Tường TÂY | Tranh che **cửa ẩn → gương bạc** |
| Gameplay | Ch.2: gõ tường → tiếng rỗng ô thứ 3 → tháo tranh → cửa nhỏ ẩn → **gương bạc (KEY_04)**. Nét phấn 5 nốt nhạc. Audio log #4. |

#### CHAMBRE DE MADAME I *(Phòng Ngủ Bà Lan I)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~4.5m |
| Vị trí | TÂY-BẮC |
| Lò sưởi | Tường TÂY |
| Nội thất | Giường đơn, bàn viết nhỏ (**nhật ký bà Lan**), gương (vải đỏ che) |
| Gameplay | Ch.2: nhật ký → *"gương nằm sau nơi gió không vào được"*. Audio log #3. |

#### BOUDOIR DE MADAME *(Phòng Thay Đồ Bà Lan — B.Lan II T1)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~3.5m |
| Vị trí | ĐÔNG-BẮC, liền kề Chambre Madame I |
| Đặc điểm | Phòng thay đồ riêng — chuẩn biệt thự Đông Dương (phụ nữ có boudoir riêng). Tủ quần áo lớn, bàn phấn, ghế bành nhỏ. |
| Nội thất | Quần áo mốc treo trong tủ, hộp nữ trang rỗng, gương bàn phấn (vỡ một góc) |
| Gameplay | Ch.2: không có puzzle chính. Atmospheric. Gợi ý về lối sống tách biệt của vợ chồng Đỗ. |

#### SALLE DE BAINS *(Phòng Tắm — Ma Da!)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~3m × ~3.5m |
| Vị trí | TÂY-BẮC (cạnh cầu thang) |
| Sàn | Đá mài trắng + gạch ốp tường trắng |
| Nội thất | **Bồn tắm sứ chân sư tử (Pháp cổ điển)**, chậu rửa sứ, gương tường (nứt) |
| Gameplay | **Ma Da ẩn trong bồn tắm.** Đi qua KHÔNG NHÌN VÀO bồn (hold Ctrl = cúi đầu). Nhìn → trigger Ma Da chase. |
| Cửa sổ | 1 nhỏ mặt BẮC (kính mờ) |

#### CHAMBRE VIDE *(Phòng Trống)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~3.5m |
| Vị trí | ĐÔNG-BẮC (cạnh cầu thang, phía trên) |
| Đặc điểm | **Trống rỗng hoàn toàn** — không có đồ đạc. Sàn gỗ có dấu vết kéo đồ (đồ cũ bị mang đi). |
| Gameplay | **Ghost dừng ở đây trong patrol** — đứng giữa phòng ~5s rồi tiếp tục. Âm thanh: tiếng thở nặng. Ch.3: mảnh bản đồ #2 (dưới sàn gỗ lỏng). |

---

<a name="8"></a>
## 8. TẦNG 2 / SECOND FLOOR + THÁP CANH
*(Chapter 3 gameplay chính · Chapter 4 reuse)*

- Cao độ sàn: **+8.35m** so với mặt đất
- Chiều cao trần: **3.0m** (vát xuống 2.2m sát tường ngoài)
- Diện tích: 16m × 12m = **192 m²**

### 8.1 Bố Cục Phòng (GDD v3 — BẮC lên trên)

```
                    BẮC (sân sau)
    ┌─────────────┬──────────┬───────────┐
    │   KHO       │  → THÁP │ P.CON    │
    │  (Débarras) │   CANH  │ TRAI     │
    │             │ ↑ lối   │ (Đỗ Minh)│
    ├─────────────┤   lên   ├───────────┤
    │  PHÒNG TRÀ  │         │ PHÒNG TẮM│
    │  Salon de   │         │ T2       │
    │  thé        │         │ (nhỏ)    │
    ├─────────────┴──────────┴───────────┤
    │           HÀNH LANG               │
    ├─────────────┬──────────┬───────────┤
    │  CHAMBRE   │  BAN     │  PHÒNG   │
    │  RETRAITE  │  CÔNG    │  CHƠI    │
    │  (B.Lan II)│  mặt NAM │  Salle   │
    │  (điên)    │          │  de jeux │
    └─────────────┴──────────┴───────────┘
                    NAM (mặt tiền)
```

**Thay đổi so với bản gốc:**
- ✅ "Phòng đọc sách" → **Salon de thé** (phòng trà — đúng hơn với biệt thự Đông Dương)
- ✅ Thêm **Phòng Tắm T2** (salle de bains nhỏ, góc ĐÔNG-BẮC)
- ✅ "Phòng trống/kho" → **Débarras** (kho lộn xộn, có mảnh bản đồ)
- ✅ "Phòng con gái T2" → bỏ (Linh chỉ có 1 phòng ở T1); **Chambre de retraite** là của bà Lan T2

### 8.2 Chi Tiết Từng Phòng

#### CHAMBRE DE RETRAITE *(Phòng Bà Lan II — Điên Loạn)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5m × ~5m |
| Vị trí | TÂY-NAM |
| Đặc biệt | **Tường phủ đầy chữ viết tay** — mật mã, tên con, ký hiệu nhạc lặp đi lặp lại. Khác hẳn các phòng khác — gây choáng visual. |
| Nội thất | Bàn trang điểm (audio log #6), gương vỡ, quần áo rách tung toé, lọ muối rải trên sàn |
| Gameplay | Ch.3: đọc chữ tường → manh mối tầng hầm. Audio log #6. **Lọ muối** (pickup Ch.3 → dùng Ch.4). |

#### BAN CÔNG T2 *(Balcon 2ème étage)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 1.2m sâu × ~3m rộng |
| Vị trí | Chính giữa mặt NAM tầng 2 (cùng trục ban công T1) |
| Gameplay | Nhìn xuống vườn trước. Mảnh bản đồ #3 nằm trên lan can (đã ướt, nhàu nát). |

#### SALLE DE JEUX *(Phòng Chơi)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~5m × ~5m |
| Vị trí | ĐÔNG-NAM |
| Đặc biệt | Đồ chơi cũ rải rác — búp bê, xe gỗ, bảng đen con nít. |
| Gameplay | Bức vẽ Đỗ Linh *"Nó nói nó đói"* → audio log #7. |

#### SALON DE THÉ *(Phòng Trà)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~4m |
| Vị trí | TÂY-BẮC |
| Đặc điểm | Bộ bàn trà gỗ teak, ghế mây, tủ chè cụ Pháp. Cửa sổ nhìn ra vườn sau. |
| Gameplay | Không có puzzle chính. Atmospheric — mùi trà khô còn lưu lại. Audio ambient nhẹ. |

#### CHAMBRE DU FILS *(Phòng Con Trai — Đỗ Minh)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~3.5m |
| Vị trí | ĐÔNG-BẮC (cạnh tháp canh) |
| Nội thất | Giường nhỏ, đồ chơi, **con tàu gỗ** |
| Gameplay | Ch.3: audio log #5 — ký ức Đỗ Minh: *"Ba đừng xuống hầm"* |

#### DÉBARRAS *(Kho Lộn Xộn)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~4m × ~3.5m |
| Vị trí | TÂY-BẮC (cạnh tháp canh, phía TÂY hơn) |
| Đặc điểm | Thùng cũ, đồ linh tinh, không có cửa sổ. Tối. |
| Gameplay | Ch.3: **mảnh bản đồ #3** (trong thùng gỗ dưới đáy). |

#### SALLE DE BAINS T2 *(Phòng Tắm Tầng 2 — nhỏ)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | ~2.5m × ~3m |
| Vị trí | ĐÔNG-BẮC, cạnh phòng con trai |
| Đặc điểm | Chỉ có chậu rửa + bồn tắm nhỏ. Không bồn sứ chân sư tử như T1. |
| Gameplay | Không có sự kiện riêng. Gương tường duy nhất ở đây chưa bị vỡ (Ch.2 chưa lên đây). |

#### THÁP CANH *(Tour de guet — SAFE ZONE)*
| Thông số | Giá trị |
|----------|---------|
| Vị trí | Góc ĐÔNG-BẮC, nhô lên trên mái chính |
| Kích thước trong | 3.0m × 3.0m (bát giác ngoài) |
| Lối lên | Cầu thang gỗ xoắn hẹp từ khu cầu thang tầng 2 |
| Cửa sổ | 4 mặt — view toàn cảnh (cổng, vườn, sân sau, rừng) |
| Mái | Chóp bát giác, ngói xám, đỉnh chóp sắt |
| **GAMEPLAY QUAN TRỌNG** | **SAFE ZONE — Ghost dừng chân cầu thang tháp, khóc ~10s, BỎ ĐI. KHÔNG leo lên.** Tháp gần nguyên vẹn — có lực bảo vệ. |
| Gameplay | Ch.3: mảnh bản đồ (ghép 3 mảnh ở đây) + **công tắc tần số #3** + **tờ giấy 7 nốt**. |

---

<a name="9"></a>
## 9. TẦNG HẦM / BASEMENT
*(Chapter 4 Only)*

- Cao độ sàn: **-1.95m** so với mặt đất (nửa chìm dưới đất)
- Chiều cao trần: **2.4m**
- Diện tích: 16m × 12m = **192 m²** (y chang các tầng trên)

### 9.1 Bố Cục

```
                    BẮC
    ┌──────────────────────────────────┐
    │                                  │
    │         KHU LƯU TRỮ             │
    │        (rộng, tối, kệ gỗ)       │
    │         10.0m × 6.0m            │
    │                                  │
    ├──────────────────┬───────────────┤
    │                  │               │
    │   HÀNH LANG HẦM  │  PHÒNG THỜ   │
    │   3.0m × 6.0m   │  (BÀN THỜ    │
    │                  │  3 VẬT PHẨM) │
    │                  │  6.0m × 6.0m │
    │                  │               │
    │  ↑ CẦU THANG    │               │
    │  (từ thư phòng) │               │
    └──────────────────┴───────────────┘
                    NAM
```

### 9.2 Chi Tiết

#### PHÒNG THỜ *(Salle du culte — Final Room Ch.4)*
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 6.0m × 6.0m |
| Đặc điểm | **Tối nhất game** — chỉ ánh nến bàn thờ |
| Bàn thờ | Đá granite đen, 1.5m × 0.8m × 0.9m, **3 hõm lõm** khớp hình: hộp nhạc (trái), gương bạc (giữa), lọ muối (phải) |
| Sàn | Đá mài đen |
| Tường | Đá chẻ thô, không trát |
| Gameplay | **Final room Ch.4**: đặt 3 vật → trận cuối. 2 ghost cùng lúc. |

#### LỐI XUỐNG HẦM
- Từ thư phòng tầng trệt → sau tủ sách → **cửa ẩn** (Ch.4 mở khi đã giải Ch.3 bản đồ)
- Cầu thang: rộng 0.8m, đá chẻ, 20 bậc, tối hoàn toàn
- Chỉ mở được khi **đã có KEY_10** (chìa khoá tầng hầm)

---

<a name="10"></a>
## 10. SÂN SAU & GIẾNG

### 10.1 Sân Sau

| Thông số | Giá trị |
|----------|---------|
| Kích thước | 16m (rộng, bằng nhà) × 8m (sâu) |
| Mặt sân | Đá phiến lát (5m gần nhà) + đất cỏ (3m phía rừng) |
| Cây | 2 cây thông cổ thụ góc Tây-Bắc và Đông-Bắc |
| Rào sau | Hàng rào đá chẻ thấp 1.2m + cổng phụ nhỏ 1.0m ra rừng |

### 10.2 Giếng Đá *(Puits de pierre)*

| Thông số | Giá trị |
|----------|---------|
| Vị trí | Chính giữa sân sau, cách tường Bắc nhà 4m |
| Đường kính ngoài | 1.2m |
| Đường kính trong | 0.8m |
| Chiều cao thành | 0.7m trên mặt đất |
| Vật liệu | Đá chẻ xám, xây vòng tròn, vữa cát-vôi |
| Sâu | ~8m (lore — nước đen bất thường) |
| Đặc biệt | **Vải đỏ cũ buộc quanh thành** (bà Lan cố bịt giếng). Ánh sáng xanh phát ra ban đêm. |
| Gameplay | Ch.1: **death sequence** — Khoa cúi nhìn → bị kéo xuống. Ch.4: rải muối 8 điểm quanh giếng. |

### 10.3 Vị Trí 8 Điểm Muối (Ch.4)

```
        N
    8       2
  7    GIẾNG   3
    6       4
        5
        S
```

Mỗi điểm cách giếng **2.5m**, tạo vòng tròn đường kính 5m.

---

<a name="11"></a>
## 11. HÀNH LANG VƯỜN & LỐI ĐI NGOẠI THẤT

### 11.1 Hiên Bao Quanh *(Véranda/Galerie)*

| Mặt | Rộng | Đặc điểm |
|-----|------|----------|
| Hiên trước (Nam) | 1.2m sâu × 16m | Cột đá tròn kiểu Doric mỗi 2.5m, lan can baluster đá. Bậc thềm 5 bậc chính giữa |
| Hiên sau (Bắc) | 1.2m sâu × 16m | Cột tương tự, cửa ra sân sau |
| Hiên trái (Tây) | 1.2m sâu × 12m | Nối vườn trái, cửa sổ bếp nhìn ra |
| Hiên phải (Đông) | 1.2m sâu × 12m | Nối vườn phải, cửa sổ thư phòng nhìn ra |

> **Gameplay — Galerie Mechanics:**
> - Ban đêm (sau khi ghost xuất hiện): đi galerie → **sanity drain ~30%/phút**
> - Ch.1: Ma Vú Dài KHÔNG patrol galerie → galerie là **lối tắt chiến thuật** ra sân sau / nhà kho
> - Tiếng bước chân player trên sàn galerie **không trigger ghost hearing** (sàn đá — khác gỗ)

### 11.2 Hành Lang Vườn

#### HÀNH LANG VƯỜN TRÁI (Tây)
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 2.0m rộng × 20m dài |
| Kiểu | Pergola gỗ, cột gỗ mỗi 2.5m, dây leo bám (wisteria hoang) |
| Mặt đường | Đá phiến, khe rêu |
| Cửa vào | Đầu Nam: cổng nhỏ sắt, **KHÓA** |

#### HÀNH LANG VƯỜN PHẢI (Đông)
| Thông số | Giá trị |
|----------|---------|
| Kích thước | 2.0m rộng × 20m dài |
| Kiểu | Tương tự bên trái |
| Cửa vào | Đầu Nam: cổng nhỏ sắt, **HÉ MỞ — dẫn player vào Ch.1** |

### 11.3 Logic Gameplay Hành Lang Vườn (Ch.1 Entry)

```
1. Cổng sắt chính → đẩy vào → vào vườn trước
2. Đi thẳng lối đá → cửa chính → KHÓA (then cài trong)
3. Nhìn trái → hành lang vườn trái → cổng nhỏ KHÓA ✗
4. Nhìn phải → hành lang vườn phải → cổng nhỏ HÉ MỞ ✓
5. Đi dọc hành lang vườn phải → hiên ĐÔNG
6. Thấy cửa sổ VÉRANDA (mặt ĐÔNG) hé mở — bản lề cũ mục ✓
7. Chui qua cửa sổ → VÀO VÉRANDA → GAMEPLAY BẮT ĐẦU
```

> **Lưu ý (GDD v3):** Entry point là **cửa sổ Véranda** (mặt ĐÔNG), KHÔNG phải cửa sổ bếp. Bếp là nhà phụ ngoài sân sau — Khoa chưa biết đến đó lúc đầu game.

---

<a name="12"></a>
## 12. HỆ THỐNG CẦU THANG

### 12.1 Cầu Thang Chính (Interior)

| Thông số | Giá trị |
|----------|---------|
| Vị trí | Đông-Bắc nhà, cùng vị trí mỗi tầng |
| Kiểu | Chữ U — 2 vế thẳng + 1 chiếu nghỉ |
| Chiều rộng thân | 1.1m |
| Bậc/tầng | 22 bậc (chiều cao bậc ~18cm) |
| Vật liệu | Gỗ lim, tay vịn tròn gỗ, song sắt rèn hoa văn |
| Nối | Tầng trệt → Tầng 1 → Tầng 2 → Tháp canh (đoạn cuối hẹp hơn) |

### 12.2 Cầu Thang Hầm (Hidden)

| Thông số | Giá trị |
|----------|---------|
| Vị trí | Đông-Nam (sau tủ sách thư phòng) |
| Kiểu | Xoắn ốc đá chẻ, hẹp |
| Chiều rộng | 0.8m |
| Bậc | 20 bậc |
| Tình trạng | Ẩm, rêu, **tối hoàn toàn** |
| Mở | Ch.4 only — cần KEY_10 |

### 12.3 Bậc Thềm Hiên (Exterior)
- Hiên trước: **5 bậc đá** (75cm tổng)
- Hiên sau: **3 bậc đá** (45cm — sân sau cao hơn vườn trước)

---

<a name="13"></a>
## 13. LUỒNG GAMEPLAY CHI TIẾT PER CHAPTER

### Chapter 1 — Minh Khoa (Tầng Trệt)

> **Bối cảnh:** Đêm khuya. Ghost patrol từ phút 0 — không cần trigger.

```
[START]
Cổng sắt khép hờ → đẩy vào, vào vườn trước
↓
Cửa chính KHÓA → hành lang vườn phải hé mở → hiên ĐÔNG
→ cửa sổ Véranda (mặt ĐÔNG, bản lề mục) → chui vào
↓
[ENTER] Véranda (tutorial: nghe âm thanh bước chân ghost từ hành lang)

[CLUE 1] Phòng ăn → Ngăn kéo tủ chén → TỜ NHẠC (5 nốt D E G A F)
  → Player chưa biết đánh ở đâu

[CLUE 2] Sảnh → Gương phủ vải đỏ → ⚠ WARNING (đừng mở!)
  → Atmospheric hint, không phải puzzle

[PUZZLE CHAIN — Tủ Cabinet Bị Kẹt]
  a. Salon: EXAMINE tủ cabinet → "ngăn kéo bị kẹt, cần vật nhỏ để撬"
  b. Lò sưởi: EXAMINE → NẾN + ĐẾ ĐỒNG → [PICKUP]
  c. INTERACT nến lên ngăn kéo → KEY_01 (chìa khoá nhà kho)

[STEALTH SEGMENT]
  Galerie sân sau (lối tắt — ghost không patrol đây)
  → Hành lang ngang → cửa nhà kho → [USE KEY_01]
  → Kho → BẢNG KÝ HIỆU NỐT NHẠC (giải mã: D=Đô, E=Mi...)

[PUZZLE PIANO]
  Salon → Piano → Gõ D - E - G - A - F
  → Thư phòng MỞ (cửa đôi Pháp tự bật)

[COLLECT]
  Thư phòng → bàn làm việc → HỘP NHẠC ĐỒNG (di vật → Ch.4)

[TUTORIAL ẨN NÁU]
  Ghost tìm được Khoa → confrontation
  → Tủ áo nhà kho OR tủ sảnh → HideSpot 15s
  → Ghost bỏ đi sau 15s

[DEATH SEQUENCE]
  Khoa ra sân sau → giếng → cúi xuống nhìn → BỊ KÉO XUỐNG → CHẾT
  → End Ch.1 (death là kết thúc đúng của chapter)
```

**Ghost Ch.1 — Ma Vú Dài:**
- Patrol từ phút 0, route ~60s/vòng: hành lang ngang → salon (8s) → phòng sân → galerie sân sau (ngang qua) → quay lại
- Sau khi piano giải: **+10% speed**, hearingRadius tăng 8m → 10m
- Không đi vào galerie theo lối dài — chỉ cắt qua

---

### Chapter 2 — Bích Ngọc (Tầng 1)

```
[START]
Cửa chính KHÔNG khóa (ghost đã phá khoá) → vào sảnh
↓
Lên cầu thang chính → Tầng 1

[CLUE CHAIN]
  Phòng ngủ bà Lan I (Tây-Bắc) → nhật ký:
  "Gương nằm sau nơi gió không vào được"
  → Audio log #3

[PUZZLE GIÓ]
  Di chuyển với đèn dầu qua từng phòng T1
  → Quan sát ngọn lửa dao động
  → Phòng bé Linh (Đông-Nam): ngọn lửa KHÔNG dao động
  → [EXAMINE] tường Tây → "tiếng rỗng ở ô thứ 3"
  → Tháo tranh → CỬA NHỎ ẨN → GƯƠNG BẠC (KEY_04)

[PHÒNG TẮM — MA DA]
  Đi qua hành lang → vào phòng tắm
  → KHÔNG NHÌN VÀO BỒN TẮM (mechanic: hold Ctrl cúi đầu)
  → Nếu nhìn: Ma Da xuất hiện, chase sequence

[GƯƠNG VỠ]
  Trigger event: tất cả gương tầng 1 vỡ đồng loạt
  → Ghost phát hiện → chase
  → Chạy xuống cầu thang (mảnh gương rơi = obstacle)
  → Ra ngoài → thoát… nhưng gương bạc còn thiếu → "chưa đủ"

[DEATH / CLIFFHANGER]
  Sân sau → bóng tối → CHẾT / bất tỉnh
  → End Ch.2
```

---

### Chapter 3 — Tuấn Hùng (Tầng 2 + Toàn nhà)

```
[START] Thư phòng tầng trệt → máy ghi âm → audio log #8
  → Biết cần 3 công tắc tần số + 3 mảnh bản đồ

[3 CÔNG TẮC TẦN SỐ]
  #1 → Thư phòng tầng trệt (dưới bàn)
  #2 → Hành lang tầng 1 (trong tủ khoá — cần KEY_03)
  #3 → Tháp canh (Safe Zone)

[3 MẢNH BẢN ĐỒ]
  #1 → Nhà kho tầng trệt (trong thùng gỗ)
  #2 → Phòng ngủ phụ tầng 1 (dưới gối)
  #3 → Phòng chơi tầng 2 (sau bức tranh)

[GHÉP BẢN ĐỒ]
  Tháp canh (Safe Zone) → ghép → xác định:
  "Cửa hầm nằm sau tủ sách thư phòng tầng trệt"
  + TỜ GIẤY 7 NỐT (Đỗ Văn Minh ghi tay)

[MU ỐI MUỐI — Ch3 mechanic]
  Rải muối lọ (pickup phòng bà Lan II) lên:
  → Ngưỡng cửa từng phòng → ghost bị chặn không qua
  → Chiến thuật: rải muối để tạo safe corridor

[SAFE WINDOW]
  10 phút đầu Ch.3: không có ghost
  Sau 10 phút: cả 2 ghost patrol (Ma Vú Dài + bóng Ma Ông Đỗ)

[DEATH SEQUENCE]
  Chạy ra sân trước → Ma Vú Dài đuổi từ phía sau → CHẾT
```

---

### Chapter 4 — Lan Anh (Toàn bộ + Tầng hầm)

```
[START] Sảnh chính → tranh gia đình rách → manh mối cuối

[PIANO 7 NỐT]
  Salon → Piano → D - E - G - A - F - B - C#
  → Hộp nhạc đồng (từ Ch.1) MỞ → bên trong: KEY_10

[GHÉP GƯƠNG BẠC]
  Tầng 1, phòng bé Linh → vị trí cũ cửa ẩn
  → Đặt gương bạc (từ Ch.2) vào khung cũ → gương nguyên vẹn

[RẢI MUỐI 8 ĐIỂM]
  Sân sau → 8 điểm quanh giếng (vị trí theo bản đồ Ch.3)
  → Giếng ngừng phát sáng xanh → an toàn tạm thời

[MỞ TẦNG HẦM]
  Thư phòng → sau tủ sách → [USE KEY_10] → cầu thang đá hầm

[FINAL ROOM]
  Phòng thờ → Bàn thờ 3 hõm:
  - Đặt hộp nhạc đồng (trái)
  - Đặt gương bạc (giữa)
  - Đặt lọ muối (phải)
  → Trận cuối: 2 ghost cùng lúc (patrolSpeed 2.0f, chaseSpeed 5.0f)
  → Nếu sống sót 60s → ending sequence

[3 ENDINGS]
  Ending 1 (True): audioLogsHeard ≥ 8 → hiểu toàn bộ lore → bà Lan được giải thoát
  Ending 2 (Normal): audioLogsHeard 4-7 → thoát khỏi nhà nhưng lời nguyền còn
  Ending 3 (Bad): audioLogsHeard < 4 → chết trong hầm
```

---

<a name="14"></a>
## 14. BẢNG CHÌA KHOÁ & KHOÁ (KEY/LOCK MASTER)

| ID | Tên | Lấy ở đâu | Mở cái gì | Chapter |
|----|-----|-----------|-----------|---------|
| KEY_01 | Chìa khoá nhà kho | Ngăn kéo tủ cabinet salon (cần nến để撬) | LOCK_01: cửa nhà kho tầng trệt | Ch.1 |
| KEY_02 | Piano 5 nốt D-E-G-A-F | Giải piano salon | LOCK_02: cửa đôi thư phòng tầng trệt | Ch.1 |
| KEY_03 | Chìa khoá tủ hành lang | Dưới bàn làm việc phòng ngủ ông Đỗ T1 | LOCK_03: tủ khoá hành lang T1 (chứa công tắc #2) | Ch.2 |
| KEY_04 | Gương bạc | Cửa ẩn sau tranh phòng bé Linh T1 | — (vật phẩm di sản, dùng Ch.4) | Ch.2 |
| KEY_05 | Công tắc tần số #1 | Thư phòng tầng trệt (dưới bàn) | — (cần 3 công tắc để unlock máy ghi âm) | Ch.3 |
| KEY_06 | Công tắc tần số #2 | Tủ khoá hành lang T1 (cần KEY_03) | — | Ch.3 |
| KEY_07 | Công tắc tần số #3 | Tháp canh | — (kết hợp 3 → unlock audio log #8 + tờ giấy 7 nốt) | Ch.3 |
| KEY_08 | Tờ giấy 7 nốt | Tháp canh (Hùng ghép bản đồ) | — (input piano Ch.4) | Ch.3 → Ch.4 |
| KEY_09 | Piano 7 nốt D-E-G-A-F-B-C# | Salon Ch.4 | LOCK_09: hộp nhạc đồng → KEY_10 | Ch.4 |
| KEY_10 | Chìa khoá tầng hầm | Trong hộp nhạc đồng (sau KEY_09) | LOCK_10: cửa ẩn sau tủ sách thư phòng | Ch.4 |

**Di vật cross-chapter:**
- Hộp nhạc đồng: Ch.1 lấy → Ch.4 dùng
- Gương bạc: Ch.2 lấy → Ch.4 dùng
- Lọ muối: Ch.3 lấy (phòng bà Lan II) → Ch.4 dùng
- Tờ giấy 7 nốt: Ch.3 lấy → Ch.4 dùng

---

<a name="15"></a>
## 15. GHOST PATROL ROUTES PER CHAPTER

### Chapter 1 — Ma Vú Dài (Tầng Trệt Only)

```
PATROL TỪ PHÚT 0 — không trigger spawn.

Route (~60s/vòng):
Hành lang ngang (đi Tây → Đông, 8s)
→ Salon de réception (dừng 5s, quay đầu)
→ Hành lang ngang (đi Đông → Tây, 8s)
→ Sảnh chính (nhìn quanh 3s)
→ Phòng sân / Véranda (nếu cửa mở)
→ Galerie sân sau (cắt ngang nhanh)
→ Quay vào hành lang
→ Lặp

Sau khi piano giải:
  patrolSpeed: 1.5f → 1.7f
  hearingRadius: 8m → 10m
  Route không đổi nhưng đi nhanh hơn
```

### Chapter 2 — Ma Da + Ma Vú Dài (Tầng 1 + T0)

```
Ma Vú Dài:
  Sau sự kiện gương vỡ → patrol cả T0 + T1
  Speed: 1.5f → 2.0f
  Ưu tiên hành lang dài T1

Ma Da:
  Trigger-based: chỉ xuất hiện khi player NHÌN VÀO bồn tắm
  Sau trigger: chase T1, tắt sau 30s nếu player thoát
```

### Chapter 3 — Ma Vú Dài + Bóng Ma Ông Đỗ

```
10 phút đầu: KHÔNG CÓ GHOST (safe window để setup)

Sau 10 phút:
  Ma Vú Dài: patrol toàn nhà (T0, T1, T2)
    Dừng dưới chân cầu thang tháp, khóc ~10s → bỏ đi
    KHÔNG leo lên tháp canh

  Bóng Ma Ông Đỗ:
    Chỉ patrol T2 + hầm corridor
    patrolSpeed: 1.5f, chaseSpeed: 4.0f
    Không đuổi quá 1 tầng (dừng ở chiếu nghỉ cầu thang)

Muối ngưỡng cửa: ghost bị chặn (không qua)
```

### Chapter 4 — Cả 2 Ghost Cùng Lúc (Full House)

```
Ma Vú Dài + Ma Ông Đỗ:
  patrolSpeed: 2.0f (cả 2)
  chaseSpeed: 5.0f (cả 2)
  hearingRadius: 10m
  sightAngle: 90° → gaze timer: 3s → 1.5s
  Patrol toàn bộ nhà trừ tháp canh

Tầng hầm:
  Cả 2 xuất hiện sau khi player đặt vật phẩm thứ 3 lên bàn thờ
  60s survival → ending sequence
```

---

<a name="16"></a>
## 16. CROSS-CHAPTER ITEM INHERITANCE

| Vật phẩm | Lấy | Chapter dùng | Vị trí lưu (GameData) |
|----------|-----|--------------|----------------------|
| Hộp nhạc đồng | Thư phòng T0, sau piano Ch.1 | Ch.4 (đặt bàn thờ) | `VoD_Items`: "music_box" |
| Gương bạc | Cửa ẩn phòng Linh T1, Ch.2 | Ch.4 (đặt bàn thờ) | `VoD_Items`: "silver_mirror" |
| Lọ muối | Phòng bà Lan II T2, Ch.3 | Ch.4 (đặt bàn thờ + rải giếng) | `VoD_Items`: "salt_jar" |
| Tờ giấy 7 nốt | Tháp canh, Ch.3 | Ch.4 (đọc để biết chuỗi piano) | `VoD_Items`: "note_paper_7" |

**Điều kiện Ending:**
| Ending | Điều kiện | Kết quả |
|--------|-----------|---------|
| Ending 1 — True End | `audioLogsHeard >= 8` | Bà Lan được giải thoát, lời nguyền kết thúc |
| Ending 2 — Normal | `audioLogsHeard >= 4` | Thoát khỏi nhà, lời nguyền còn |
| Ending 3 — Bad | `audioLogsHeard < 4` | Chết trong hầm, không hiểu lore |

---

<a name="17"></a>
## 17. DECAY & VISUAL STATE PER ZONE

*(Hướng dẫn cho artist — nhà bỏ hoang từ 1965, năm game ~2000, ~35 năm decay)*

| Khu vực | Mức decay | Chi tiết |
|---------|-----------|----------|
| Tầng trệt | Trung bình | Bụi dày, mạng nhện góc trần, sơn tường bong từng mảng lớn. Gạch bông còn hoa văn nhưng vỡ 2-3 viên. Đồ nội thất vẫn nhận ra được, chỉ mốc và cũ. |
| Tầng 1 | Nặng | Gỗ sàn ẩm, một vài tấm võng (nhưng player vẫn đi được). Tường thấm nước thành vệt nâu vàng dài. Vải rèm mục rách. |
| Tầng 2 | Nặng nhất | Kèo mái thấy bầu trời qua chỗ ngói vỡ góc Đông-Tây. Cây dây leo chui vào từ cửa sổ. Một số đồ vật đổ hẳn. |
| Tháp canh | Gần nguyên vẹn | Có lực bảo vệ siêu nhiên — bụi ít, không có dấu hiệu decay rõ. Cửa sổ kính còn nguyên. Lạ một cách đáng sợ. |
| Hành lang T1 | Nặng | Tối hoàn toàn, tranh trên tường một số bị rớt hoặc méo mó. Mùi mốc nặng nhất nhà. |
| Tầng hầm | Hoang sơ | Ẩm ướt, rêu mọc trên tường đá, nền đá đọng vũng nước nhỏ. Không có mùi mốc — mùi đất lạnh. Không có rác, không có bụi — như thể được "giữ sạch" một cách siêu nhiên. |
| Sân sau | Hoang dại | Cỏ dại mọc qua khe đá lát. Rêu bám thành giếng dày. Nhánh thông rụng đầy. |
| Nhà kho | Hoang phế nhất | Mùi ẩm, kệ gỗ mục một nửa, thùng gỗ gãy đáy. Tối hoàn toàn — không cửa sổ. |

**Màu sắc tham khảo (PBR):**
| Material | Base Color | Roughness | Ghi chú |
|----------|------------|-----------|---------|
| Tường trát vôi (decay) | `#C8B898` | 0.85 | Vàng ngả nâu, bong tróc |
| Gạch bông tầng trệt | `#D4C8A8` + hoa văn | 0.75 | Pattern caro đen-trắng ố vàng |
| Gỗ lim sàn T1 | `#5C3D1E` | 0.70 | Tối, ẩm, vài chỗ sáng hơn do mòn |
| Đá chẻ hầm | `#4A4A4A` | 0.90 | Xám đậm, rêu xanh tối bám |
| Ngói xám | `#787878` | 0.80 | Một số viên nứt/vỡ, rêu vàng nâu |

---

## PHỤ LỤC A: CHECKLIST DỰNG LAYOUT

### Khối cần dựng theo thứ tự:
- [ ] Khu đất 30m × 50m (nền đất + rừng thông viền)
- [ ] Hàng rào + cổng chính (sắt rèn + trụ đá)
- [ ] Vườn trước + lối đi đá (12m × 1.5m)
- [ ] Hành lang vườn trái (cổng khóa) + phải (cổng hé)
- [ ] Hiên bao quanh 4 mặt (1.2m) + cột Doric
- [ ] Bậc thềm trước 5 bậc + lan can baluster
- [ ] **Tầng hầm** (footprint 16×12, nửa chìm)
- [ ] Tầng trệt: 7 phòng + hành lang + cầu thang
- [ ] Tầng 1: 6 phòng + hành lang + cầu thang (cùng footprint)
- [ ] Tầng 2: 6 phòng + hành lang + cầu thang (cùng footprint)
- [ ] Tháp canh góc Đông-Bắc (3×3m bát giác)
- [ ] Mái ngói dốc 45° + ống khói 4 cái (đối xứng Đông-Tây)
- [ ] Sân sau (16×8m) + giếng đá (R=0.6m, thành cao 0.7m)
- [ ] Cầu thang hầm (ẩn, sau tủ sách thư phòng)

### Lưu ý đối xứng:
- Mặt tiền Nam: đối xứng trái-phải qua trục cửa chính
- Cửa sổ mặt Tây = số lượng tương đương mặt Đông
- Ống khói: 2 Đông, 2 Tây
- Ban công tầng 1 và tầng 2: cùng vị trí mặt Nam chính giữa

### Các điểm gameplay phải dựng đúng vị trí:
- [ ] Piano: góc TÂY-BẮC salon tầng trệt
- [ ] Lò sưởi: tường TÂY salon + tường ĐÔNG thư phòng + tường TÂY các phòng ngủ
- [ ] Tủ sách: tường BẮC thư phòng (che cửa hầm)
- [ ] Bồn tắm sứ chân sư tử: phòng tắm T1
- [ ] Giếng: chính giữa sân sau, cách tường BẮC nhà 4m
- [ ] **Cửa sổ Véranda hé mở: mặt ĐÔNG** (entry point Ch.1 — bản lề mục)
- [ ] **Nhà phụ bếp + kho: NGOÀI sân sau**, góc ĐÔNG-BẮC khuôn viên
- [ ] **Cửa kho nhà phụ: khóa KEY_01**
- [ ] Bảng ký hiệu nốt: treo tường trong kho nhà phụ
- [ ] Tủ cabinet có ngăn kéo kẹt: salon, tường BẮC
- [ ] 8 điểm muối quanh giếng: vòng tròn R=2.5m
- [ ] Ban công: T1 + T2 cùng trục, mặt NAM chính giữa

---

*Build Spec v1 · Fictional Station Studio · Villa of Darkness*
*Fictional Station Studio · Villa of Darkness*
