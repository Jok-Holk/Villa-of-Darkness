# UI Style Guide — Biệt Thự Bóng Tối
> Dành cho: Nguyễn Trường Vũ  
> Mảng: UI thuần 2D — Canvas, screen transitions, text, icons  
> Dùng ChatGPT/AI để hỗ trợ layout + animation code. Giữ sáng tạo cá nhân trong visual style.

---

## 1. TRIẾT LÝ UI

**Ít = nhiều.** Player không nên thấy HUD khi đang trong gameplay.

| Nguyên tắc | Áp dụng |
|---|---|
| Minimal HUD | Không hiện gì khi gameplay — chỉ interaction prompt |
| Paper/journal aesthetic | Inventory, notes = trông như giấy cũ, không phải popup hiện đại |
| Typewriter effect | Text xuất hiện từng chữ, không flash ngay |
| Không pop màu | Chỉ cream, sepia, đen — không neon, không red/green sáng |

---

## 2. MÀU SẮC + FONT

```
Background:     #0D0B0A  (gần đen ấm)
Text chính:     #F0E6D3  (cream ngà)
Text phụ:       #8A7A6A  (nâu xám mờ)
Accent:         #C4A35A  (vàng đồng — dùng tiết kiệm)
Horror/warning: #6B1A1A  (đỏ tối)
Paper bg:       #E8D5B0  (giấy vàng cũ)
```

**Fonts — tải Google Fonts, kiểm tra Vietnamese diacritics:**

| Font | Dùng cho |
|---|---|
| Playfair Display | Title lớn, chapter name |
| Special Elite | Body text, notes, journal |
| VT323 | Số, timer, horror effect |
| Be Vietnam Pro | Fallback — đảm bảo đủ dấu tiếng Việt |

---

## 3. CÁC MÀN HÌNH CẦN LÀM

### MAIN MENU
- Background: ảnh tĩnh mặt tiền biệt thự tối + sương mù nhẹ
- Title: **BIỆT THỰ BÓNG TỐI** — Playfair Display, tracking rộng, cream
- Menu items: text thuần, không button box. Underline khi hover.
- Animation: typewriter reveal từng item, cursor `_` nhấp nháy

### PAUSE MENU
- Không full overlay — vignette tối + blur nhẹ background
- Panel nhỏ giữa màn, border kiểu khung giấy
- Items: TIẾP TỤC / CÀI ĐẶT / VỀ MENU CHÍNH

### DEATH SCREEN *(script DeathScreenUI.cs đã có — chỉ làm visual)*
Concept: **newspaper headline style**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
     SAIGON THỜI BÁO · 14/3/2000
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  PHÓNG VIÊN MẤT TÍCH TẠI BIỆT THỰ
  "Nguyễn Minh Khoa, 28 tuổi..."
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
     [ THỬ LẠI ]      [ MENU ]
```
Animation: fade in như báo đang in. Text từng dòng. Tiếng máy đánh chữ.

### INVENTORY UI *(InventoryUI.cs đã có — chỉ làm visual)*
- Background: texture giấy cũ
- Grid 3×3, item icon = black ink sketch style trên nền cream
- Item được chọn = highlight vàng đồng nhẹ
- Tabs: VẬT PHẨM · BẢN GHI · BẢN ĐỒ
- Mô tả item: handwritten font góc phải

### INTERACTION PROMPT
```
          [ E ]  Nhặt Nến
```
- Bottom center. Font VT323. Size nhỏ.
- Fade in 0.2s khi nhìn vào object. Fade out ngay khi rời.

### CHAPTER TRANSITION *(ChapterTransition.cs đã có)*
```
Màn đen → typewriter:
"Chương 1 — Căn Nhà Của Ký Ức"
Subtitle mờ: "Biệt Thự Gia Đình Đặng · 1965–2000"
→ Fade vào scene
```

### SETTINGS MENU
- Master / BGM / SFX slider
- Độ sáng slider
- Ngôn ngữ VI / EN toggle

---

## 4. ASSETS CẦN TẠO

| Asset | Cách làm |
|---|---|
| Item icons (candle, key, musicbox...) | Vẽ tay / AI image gen — black ink on cream |
| Newspaper header texture | Canva hoặc AI image gen |
| Font files | Download Google Fonts → import Unity |

---

## 5. GỢI Ý DÙNG ChatGPT

```
"Unity UI Canvas: Death screen newspaper style.
Dark background, serif font title, body text fades in line by line
with typewriter coroutine C#. Color: #0D0B0A bg, #F0E6D3 text.
Vietnamese font support required."
```

---

*Fictional Station Studio · Villa of Darkness*
