# Sound Catalog — Chapter 1 + Main Menu (cho Suno Pro)

> Scope: chỉ những gì Chapter 1 + Main Menu thực sự cần cho báo cáo giai đoạn 1. Rút gọn từ GDD v8 mục 1.8 (full 4-chapter), bỏ bớt phần Ch.2–4.
> Suno hợp nhất cho **Soundtrack** (nhạc có giai điệu/ambient drone) và các **SFX dạng texture/drone** (gió, hum, tiếng thì thầm). SFX cơ học ngắn (bước chân, cửa, phím đàn) Suno KHÔNG hợp — nên dùng thư viện SFX free thật (freesound.org) thay vì generate, tôi có ghi chú riêng ở mỗi mục.

---

## MỤC 1 — SOUNDTRACK (nhạc, có giai điệu/mood, dùng Suno instrumental mode)

| Track | Dùng ở đâu | Mood / mô tả để paste vào Suno | Ghi chú |
|---|---|---|---|
| `Theme_Main` | Main Menu (nền cho 2 scene flythrough) + loading | "Slow dark ambient drone, sparse solo piano notes echoing, melancholic, haunted Indochine mansion at dusk, no percussion, no vocals, cinematic horror trailer texture, 60-90 BPM feel but rubato" | Piano nên gợi 7 nốt bài phong ấn nhưng KHÔNG rõ ràng — nghe quen mà không nhận ra. Cần bản loop 1-2 phút, seamless. |
| `Amb_CH1_Day` | Khám phá bình thường trong nhà | "Dark ambient drone, low sustained strings, subtle wind texture, unsettling but calm, no melody, no beat, minimal, dread underlying stillness" | Loop dài (3-5 phút), volume nhỏ, sẽ tăng khi gần piano. |
| `Amb_CH1_Ghost` | Ma Vú Dài trong tầm 15m | "High tension string tremolo, rising dissonant strings, suspense horror sting building, no resolution, no beat" | Fade in 3s / fade out 5s theo code — cần bản có thể loop mượt ở đoạn giữa. |
| `Mus_Piano_Solve` | Piano puzzle giải xong | "Simple solo piano melody, 5-7 notes, melancholic and eerie, slightly off-key children's lullaby feel, no other instruments, plays once, no loop" | Non-loop, ngắn (~10-15s). |
| `Mus_WellJumpscare` *(mới thêm — theo rework Ma Da)* | Nhìn giếng quá 3s → jumpscare | "Sudden sharp orchestral hit, horror stinger, one-shot jump scare sound, dissonant brass and strings cluster, short 1-2 seconds" | Cần bản 1-2s để đồng bộ đúng frame jumpscare. |

---

## MỤC 2 — SOUND EFFECT (SFX, ngắn/chức năng — ưu tiên freesound.org, Suno chỉ dùng được cho nhóm có dấu ✦)

### Nhóm 1 — Bước chân & di chuyển
- `Footstep_Wood_Normal` (3-5 biến thể) — bước chân sàn gỗ, pitch random ±3%.
- `Footstep_Wood_Creak` (2-3 biến thể) — ván ọp ẹp, trigger ngẫu nhiên ~15%/bước.
- `Breath_Scared` (2-3 biến thể) ✦ — thở hổn hển, Suno có thể generate dạng "human breathing under stress, horror foley" nhưng foley thật (freesound) sẽ tự nhiên hơn.
- `Heartbeat` ✦ — "slow heartbeat sound effect, tense, looping, low frequency thump" — Suno OK cho cái này.

### Nhóm 2 — Tương tác & UI
- `Interact_Door_Open` (2-3 biến thể) — cửa gỗ cũ mở, lò xo + kẽo kẹt.
- `Interact_Door_Locked` — lắc nắm cửa, không mở được.
- `Interact_Item_Pickup` — giấy/kim loại tuỳ vật phẩm.
- `Interact_Item_Examine` — lật giấy/chạm bề mặt.
- `Interact_Piano_Key` (5-7 nốt cần cho sequence) — **xem file riêng piano note samples** (Salamander/darosh repo đã gửi ở plan), KHÔNG dùng Suno cho nốt đàn thật.
- `Interact_Piano_Wrong` — piano lạc điệu ngắn.
- `UI_Inventory_Open` — nhẹ, không gây chú ý.

### Nhóm 3 — Môi trường/Ambient
- `Amb_Wind_Outside` ✦ — "howling wind outside old house, exterior ambience, no melody" — Suno OK.
- `Amb_Creak_Floor` (3-5 biến thể) — sàn gỗ tự kêu ngẫu nhiên, không do player.
- `Amb_Water_Drip` (2-3 biến thể) — nước nhỏ giọt bếp.
- `Amb_Fire_Crackle` — lò sưởi salon.
- `Amb_Well_Hum` ✦ — "low ominous hum from a stone well, subterranean drone, unsettling" — Suno OK, gần giống ambient drone.

### Nhóm 4 — Ma & kinh dị
- `Ghost_MVD_Footstep` (3-4 biến thể) — bước chân Ma Vú Dài, nhịp không đều.
- `Ghost_MVD_Breathing` ✦ — "raspy inhuman breathing, spatial horror, slow" — Suno OK thử.
- `Ghost_MVD_Appear` ✦ — "sudden horror jumpscare stinger, reverb tail, orchestral hit" — trùng hướng với `Mus_WellJumpscare`, có thể dùng chung 1 bản khác biến thể nhỏ.
- `Ghost_MDA_Whisper` (2-4 biến thể) ✦ — "creepy child whisper, binaural, very quiet, unsettling" — Suno thử được nhưng whisper giọng người thật nghe rợn hơn nếu record tay.
- `Scare_Object_Fall` — vật rơi ngẫu nhiên trong nhà.

---

## Ưu tiên generate trước (nếu không kịp làm hết)

1. `Theme_Main` (menu cần ngay cho 2 scene flythrough)
2. `Amb_CH1_Day` + `Amb_CH1_Ghost` (chạy suốt lúc chơi)
3. `Mus_Piano_Solve` + `Mus_WellJumpscare`
4. Còn lại làm sau nếu còn giờ — không có cũng không chặn playthrough (AudioManager có thể chạy im lặng tạm ở các slot chưa có clip).
