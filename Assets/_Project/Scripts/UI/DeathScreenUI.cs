using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
#pragma warning disable CS0414

/// <summary>
/// HIERARCHY:
/// DeathScreen                      ← script gắn ở đây (luôn active)
///  └─ DeathScreenPanel             ← SetActive = false lúc đầu
///      ├─ Background   Image       #0D0D0D stretch full
///      ├─ GlitchNoise  RawImage    stretch full, color white alpha 0
///      ├─ ScanlineStrip Image      height 4, color white alpha 0
///      └─ PaperPanel   Image       #111111, width ~520, center
///          ├─ Masthead  TMP_Text   monospace, size 12, #999999, center
///          ├─ RuleTop   Image      height 2, #555555, stretch ngang
///          ├─ Headline  TMP_Text   bold, size 26, #E8E0D0
///          ├─ RuleMid   Image      height 1, #444444
///          ├─ SubText   TMP_Text   size 13, #888888
///          ├─ Quote     TMP_Text   italic, size 15, #BBBBBB
///          ├─ RuleBot   Image      height 2, #555555
///          └─ ButtonRow HorizontalLayoutGroup spacing 80
///              ├─ RetryButton  "[ THỬ LẠI ]"
///              └─ MenuButton   "[ MENU ]"
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    // ── Panel & glitch (giữ từ gốc) ───────────────────────────────────────────
    [Header("Panel & Glitch")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private RawImage   glitchNoise;
    [SerializeField] private Image      scanlineStrip;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip   deathMusic;

    // ── Newspaper text (thêm mới) ──────────────────────────────────────────────
    [Header("Newspaper Text")]
    [SerializeField] private TMP_Text mastheadText;  // "SAIGON THỜI BÁO · 14/3/2000"
    [SerializeField] private TMP_Text headlineText;  // dòng tiêu đề lớn
    [SerializeField] private TMP_Text subText;       // dòng phụ nhỏ
    [SerializeField] private TMP_Text quoteText;     // tên + năm dạng quote

    [Header("Nội dung cố định")]
    [SerializeField] private string masthead    = "ĐÀ LẠT THỜI BÁO  ·  14/3/2000";
    [SerializeField] private string headline    = "SINH VIÊN MẤT TÍCH TẠI BIỆT THỰ ĐỖ GIA";
    [SerializeField] private string subContent  = "Nguồn tin từ Công an Lâm Đồng xác nhận\ntrường hợp mất tích bí ẩn vào đêm 13/3.";

    // ── Buttons (thêm mới) ────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    // ── Bầu không khí ma ám (thêm mới — vignette, bóng ma, rung sắc, chớp trắng) ──
    [Header("Atmosphere")]
    [SerializeField] private RawImage vignetteImage;
    [SerializeField] private RawImage apparitionImage;
    [SerializeField] private Image    whiteFlash;
    [SerializeField] private TMP_Text headlineGhostR;
    [SerializeField] private TMP_Text headlineGhostC;

    // Jok tự chỉnh trực tiếp trong Inspector — kéo số này rồi bấm lại "VoD/Temp/Preview DeathScreen" để thấy ngay,
    // không cần nhờ sửa code + recompile mỗi lần nữa. 0.5 = tâm sáng ở giữa màn hình thật, 1 = sát mép trên.
    [SerializeField] [Range(0.3f, 1f)] private float vignetteCenterY = 0.82f;

    // ── Gốc giữ nguyên ────────────────────────────────────────────────────────
    [SerializeField] private bool   _isVisible     = false;
    [SerializeField] private string _characterName;
    public UnityEvent OnRetry = new UnityEvent();

    private Coroutine _glitch;
    private Coroutine _apparition;
    private Coroutine _chroma;
    private Coroutine _flash;
    private Coroutine _pulse;
    private Texture2D _noiseTex;
    private Texture2D _vignetteTex;
    private Texture2D _apparitionTex;
    private Color      _retryBaseColor;

    // ══════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        _noiseTex            = new Texture2D(128, 72, TextureFormat.RGBA32, false);
        _noiseTex.filterMode = FilterMode.Point;
        _noiseTex.wrapMode   = TextureWrapMode.Repeat;
        if (glitchNoise != null) glitchNoise.texture = _noiseTex;

        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
        SetAlpha(glitchNoise,   0f);
        SetAlpha(scanlineStrip, 0f);

        // Gắn button listener bằng code – không phụ thuộc Inspector sau reload
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (menuButton  != null) menuButton.onClick.AddListener(GoMenu);

        // Text cố định gán 1 lần
        if (mastheadText != null) mastheadText.text = masthead;
        if (headlineText != null) headlineText.text = headline;
        if (subText      != null) subText.text      = subContent;

        // Bầu không khí: vignette + bóng ma sinh texture 1 lần lúc Awake (giống _noiseTex)
        // 192x108 khớp tỉ lệ 16:9 thật của màn hình (1920x1080) — xem comment trong hàm generate.
        _vignetteTex = GenerateVignetteTexture(192, 108, vignetteCenterY);
        if (vignetteImage != null)
        {
            vignetteImage.texture = _vignetteTex;
            vignetteImage.color   = Color.white;
        }

        _apparitionTex = GenerateApparitionTexture(96, 138); // khớp tỉ lệ 500x720 của RectTransform Apparition
        if (apparitionImage != null)
        {
            apparitionImage.texture = _apparitionTex;
            apparitionImage.color   = Color.white;
            SetAlpha(apparitionImage, 0f);
        }

        SetAlpha(whiteFlash,     0f);
        SetAlpha(headlineGhostR, 0f);
        SetAlpha(headlineGhostC, 0f);

        if (retryButton != null && retryButton.targetGraphic is Image retryImg)
            _retryBaseColor = retryImg.color;
    }

    private void OnDestroy()
    {
        if (_noiseTex)      Destroy(_noiseTex);
        if (_vignetteTex)   Destroy(_vignetteTex);
        if (_apparitionTex) Destroy(_apparitionTex);
    }

    // Cho phép Jok kéo "Vignette Center Y" trong Inspector và thấy cập nhật ngay cả khi đang ở Edit Mode
    // (lúc bật Preview DeathScreen) — không cần recompile hay bấm VoD tool nữa.
    private void OnValidate()
    {
        if (vignetteImage == null) return;
        if (_vignetteTex != null) DestroyImmediate(_vignetteTex, true);
        _vignetteTex = GenerateVignetteTexture(192, 108, vignetteCenterY);
        vignetteImage.texture = _vignetteTex;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void Show(string name, string years)
    {
        _isVisible     = true;
        _characterName = name;

        // Quote dùng tên + năm động
        if (quoteText != null) quoteText.text = $"\"{name}, {years}...\"";

        if (deathScreenPanel != null) deathScreenPanel.SetActive(true);
        if (vignetteImage    != null) vignetteImage.gameObject.SetActive(true);
        if (apparitionImage  != null) apparitionImage.gameObject.SetActive(true);
        if (whiteFlash       != null) whiteFlash.gameObject.SetActive(true);

        if (_glitch != null) StopCoroutine(_glitch);
        _glitch = StartCoroutine(GlitchRoutine());

        if (_apparition != null) StopCoroutine(_apparition);
        _apparition = StartCoroutine(ApparitionRoutine());

        if (_chroma != null) StopCoroutine(_chroma);
        _chroma = StartCoroutine(ChromaticRoutine());

        if (_flash != null) StopCoroutine(_flash);
        _flash = StartCoroutine(FlashRoutine());

        if (_pulse != null) StopCoroutine(_pulse);
        _pulse = StartCoroutine(RetryPulseRoutine());

        // Nhạc chạy độc lập Time.timeScale (AudioSource không bị pause ảnh hưởng) — phát 1 lần, không loop,
        // để tự hết rồi im lặng (đúng ý "hết phim"), không cần chờ Retry/Menu mới tắt.
        if (musicSource != null && deathMusic != null)
        {
            musicSource.clip = deathMusic;
            musicSource.loop = false;
            // Bài có ~2-3s khoảng lặng mở đầu (TV static tắt dần) — cảm giác delay, skip thẳng qua.
            musicSource.time = Mathf.Min(2f, Mathf.Max(0f, deathMusic.length - 0.1f));
            musicSource.Play();
        }
    }

    public void Hide()
    {
        _isVisible = false;

        if (_glitch     != null) { StopCoroutine(_glitch);     _glitch     = null; }
        if (_apparition != null) { StopCoroutine(_apparition); _apparition = null; }
        if (_chroma     != null) { StopCoroutine(_chroma);     _chroma     = null; }
        if (_flash      != null) { StopCoroutine(_flash);      _flash      = null; }
        if (_pulse      != null) { StopCoroutine(_pulse);      _pulse      = null; }

        SetAlpha(glitchNoise,     0f);
        SetAlpha(scanlineStrip,   0f);
        SetAlpha(apparitionImage, 0f);
        SetAlpha(headlineGhostR,  0f);
        SetAlpha(headlineGhostC,  0f);
        SetAlpha(whiteFlash,      0f);
        if (glitchNoise != null) glitchNoise.uvRect = new Rect(0, 0, 1, 1);
        if (retryButton != null && retryButton.targetGraphic is Image retryImg)
            retryImg.color = _retryBaseColor;

        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
        if (vignetteImage    != null) vignetteImage.gameObject.SetActive(false);
        if (apparitionImage  != null) apparitionImage.gameObject.SetActive(false);
        if (whiteFlash       != null) whiteFlash.gameObject.SetActive(false);

        if (musicSource != null && musicSource.isPlaying) musicSource.Stop();
    }

    public void Retry()
    {
        Debug.Log("[DeathScreenUI] Sự kiện: Người chơi nhấn THỬ LẠI");
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerRespawn();
        else
            Debug.LogWarning("[DeathScreenUI] Không tìm thấy GameManager.Instance!");
        OnRetry.Invoke();
    }

    public void GoMenu()
    {
        Debug.Log("[DeathScreenUI] Sự kiện: Người chơi nhấn MENU");
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            Debug.LogWarning("[DeathScreenUI] Không tìm thấy GameManager.Instance!");
    }

    // ── Glitch loop vĩnh viễn, unscaled time ──────────────────────────────────

    private IEnumerator GlitchRoutine()
    {
        float interval = 1f / 20f;
        float scanY    = 1f;

        while (true)
        {
            // Noise texture — mật độ hạt giảm mạnh (0.45→0.10) để ra hạt nhiễu mỏng, không phải caro dày đặc
            // (code gốc chưa từng được thấy chạy thật vì glitchNoise field bị None từ trước tới giờ).
            Color32[] px = new Color32[128 * 72];
            for (int i = 0; i < px.Length; i++)
            {
                if (Random.value < 0.05f)
                {
                    // Xám trắng-đen thuần (R=G=B, không ngả xanh lá) — nghiêng về xám hơn là trắng chói
                    byte b = (byte)Random.Range(90, 210);
                    px[i]  = new Color32(b, b, b, 255);
                }
                else px[i] = new Color32(0, 0, 0, 0);
            }
            _noiseTex.SetPixels32(px);
            _noiseTex.Apply();

            // Glitch noise layer — alpha giảm tiếp (0.06-0.16 → 0.02-0.06), mật độ hạt giảm tiếp (0.10 → 0.05):
            // vẫn thấy caro to do texture noise chỉ 128x72 + FilterMode.Point (mỗi hạt phóng to thành khối vuông
            // to khi kéo giãn lên 1920x1080) — đây là bản chất kiểu "static nhiễu TV" nên giữ Point, chỉ giảm alpha/mật độ.
            if (glitchNoise != null)
            {
                SetAlpha(glitchNoise, Random.Range(0.02f, 0.06f));
                float tear = Random.value < 0.3f ? Random.Range(-0.06f, 0.06f) : 0f;
                glitchNoise.uvRect = new Rect(tear, 0, 1, 1);
            }

            // Scanline trượt dọc
            if (scanlineStrip != null)
            {
                scanY -= Time.unscaledDeltaTime * Random.Range(0.4f, 0.9f);
                if (scanY < -0.1f) scanY = 1.1f;
                var   rt = scanlineStrip.rectTransform;
                float h  = ((RectTransform)rt.parent).rect.height;
                rt.anchoredPosition = new Vector2(0, (scanY - 0.5f) * h);
                SetAlpha(scanlineStrip, Random.Range(0.10f, 0.22f));
            }

            yield return new WaitForSecondsRealtime(interval * Random.Range(0.7f, 1.3f));
        }
    }

    // ── Bóng ma ẩn hiện ngẫu nhiên (unscaled time — chạy được cả khi game pause) ──
    private IEnumerator ApparitionRoutine()
    {
        if (apparitionImage == null) yield break;
        float timer = 0f;
        while (true)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                timer = Random.Range(1.0f, 2.6f);
                float target = Random.value < 0.3f ? 0.08f : Random.Range(0.25f, 0.55f);
                yield return FadeGraphic(apparitionImage, target, 1.4f);
            }
            yield return null;
        }
    }

    private IEnumerator FadeGraphic(Graphic g, float target, float duration)
    {
        float start = g.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(g, Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetAlpha(g, target);
    }

    // ── Rung sắc tiêu đề (chromatic aberration) — chớp nhoáng ngẫu nhiên ──────────
    private IEnumerator ChromaticRoutine()
    {
        if (headlineGhostR == null || headlineGhostC == null) yield break;
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(1.5f, 4.5f));
            SetAlpha(headlineGhostR, 0.5f);
            SetAlpha(headlineGhostC, 0.5f);
            yield return new WaitForSecondsRealtime(Random.Range(0.06f, 0.14f));
            SetAlpha(headlineGhostR, 0f);
            SetAlpha(headlineGhostC, 0f);
        }
    }

    // ── Chớp trắng ngẫu nhiên (hậu jumpscare) ─────────────────────────────────────
    private IEnumerator FlashRoutine()
    {
        if (whiteFlash == null) yield break;
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(3f, 8f));
            if (Random.value < 0.5f) continue;

            SetAlpha(whiteFlash, 0.45f);
            yield return new WaitForSecondsRealtime(0.04f);
            yield return FadeGraphic(whiteFlash, 0f, 0.35f);
        }
    }

    // ── Nút Thử Lại đổi màu dần từ vàng ấm sang đỏ theo chu kỳ ───────────────────
    private IEnumerator RetryPulseRoutine()
    {
        if (retryButton == null || !(retryButton.targetGraphic is Image img)) yield break;
        Color pulseColor = new Color(0.42f, 0.10f, 0.09f, _retryBaseColor.a);
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * 0.9f) + 1f) * 0.5f;
            img.color = Color.Lerp(_retryBaseColor, pulseColor, t);
            yield return null;
        }
    }

    // width/height PHẢI khớp tỉ lệ thật của RectTransform sẽ hiển thị texture này —
    // nếu dùng texture vuông kéo giãn lên khung 1920x1080 (16:9), vòng tối bị bóp méo dồn hết
    // ra rìa ngoài cùng, giữa màn hình gần như trong suốt hoàn toàn (bug từng gặp — "chả thấy gì").
    private static Texture2D GenerateVignetteTexture(int width, int height, float centerYRatio = 0.82f)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        // Tâm sáng — centerYRatio 0.5 = giữa màn hình thật, càng gần 1 càng lên cao. Chỉnh qua Inspector
        // (field "Vignette Center Y" trên DeathScreen), không hardcode nữa.
        Vector2 center = new Vector2(width / 2f, height * centerYRatio);
        var     px     = new Color32[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                // Khoảng cách chuẩn hoá riêng theo trục X/Y (elip khớp khung hình) thay vì khoảng cách tròn thô.
                float nx = (x - center.x) / center.x;
                float ny = (y - center.y) / center.y;
                float d  = Mathf.Sqrt(nx * nx + ny * ny);
                float t  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.25f) / 0.75f));
                // Alpha ĐẶC HOÀN TOÀN (255) — vignette giờ là nền chính, không được để lộ cảnh 3D
                // dù chỉ vài % (255 sàn 245 trước đó vẫn lộ vật sáng phía sau). Gradient giờ chỉ đổi
                // MÀU (ấm hơn ở giữa, tối hơn ở rìa) chứ không đổi alpha, tránh lặp lại bug "thấy xuyên cảnh".
                byte a = 255;
                // Màu ấm gần đen (#0A0A09 mockup) ở giữa, tối dần về đen thuần ở rìa — không ngả xanh dương.
                byte baseR = (byte)Mathf.Lerp(16f, 4f, t);
                byte baseG = (byte)Mathf.Lerp(15f, 4f, t);
                byte baseB = (byte)Mathf.Lerp(13f, 4f, t);
                px[y * width + x] = new Color32(baseR, baseG, baseB, a);
            }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateApparitionTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(width / 2f, height * 0.42f);
        var     px     = new Color32[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float nx = (x - center.x) / (width * 0.42f);
                float ny = (y - center.y) / (height * 0.42f);
                float d  = Mathf.Sqrt(nx * nx + ny * ny);
                float a  = Mathf.Pow(1f - Mathf.Clamp01(d), 1.6f);
                px[y * width + x] = new Color32(190, 198, 205, (byte)(a * 200));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}