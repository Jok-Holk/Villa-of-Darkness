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

    // ── Newspaper text (thêm mới) ──────────────────────────────────────────────
    [Header("Newspaper Text")]
    [SerializeField] private TMP_Text mastheadText;  // "SAIGON THỜI BÁO · 14/3/2000"
    [SerializeField] private TMP_Text headlineText;  // dòng tiêu đề lớn
    [SerializeField] private TMP_Text subText;       // dòng phụ nhỏ
    [SerializeField] private TMP_Text quoteText;     // tên + năm dạng quote

    [Header("Nội dung cố định")]
    [SerializeField] private string masthead    = "SAIGON THỜI BÁO  ·  14/3/2000";
    [SerializeField] private string headline    = "PHÓNG VIÊN MẤT TÍCH TẠI BIỆT THỰ";
    [SerializeField] private string subContent  = "Nguyễn Minh Khoa, 28 tuổi...";

    // ── Buttons (thêm mới) ────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    // ── Gốc giữ nguyên ────────────────────────────────────────────────────────
    [SerializeField] private bool   _isVisible     = false;
    [SerializeField] private string _characterName;
    public UnityEvent OnRetry = new UnityEvent();

    private Coroutine _glitch;
    private Texture2D _noiseTex;

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
    }

    private void OnDestroy() { if (_noiseTex) Destroy(_noiseTex); }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void Show(string name, string years)
    {
        _isVisible     = true;
        _characterName = name;

        // Quote dùng tên + năm động
        if (quoteText != null) quoteText.text = $"\"{name}, {years}...\"";

        if (deathScreenPanel != null) deathScreenPanel.SetActive(true);

        if (_glitch != null) StopCoroutine(_glitch);
        _glitch = StartCoroutine(GlitchRoutine());
    }

    public void Hide()
    {
        _isVisible = false;

        if (_glitch != null) { StopCoroutine(_glitch); _glitch = null; }
        SetAlpha(glitchNoise,   0f);
        SetAlpha(scanlineStrip, 0f);
        if (glitchNoise      != null) glitchNoise.uvRect = new Rect(0, 0, 1, 1);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
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
            // Noise texture
            Color32[] px = new Color32[128 * 72];
            for (int i = 0; i < px.Length; i++)
            {
                if (Random.value < 0.45f)
                {
                    byte b = (byte)Random.Range(140, 255);
                    px[i]  = new Color32(b, (byte)Mathf.Min(b + 30, 255), b, 255);
                }
                else px[i] = new Color32(0, 0, 0, 0);
            }
            _noiseTex.SetPixels32(px);
            _noiseTex.Apply();

            // Glitch noise layer
            if (glitchNoise != null)
            {
                SetAlpha(glitchNoise, Random.Range(0.3f, 0.65f));
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
                SetAlpha(scanlineStrip, Random.Range(0.25f, 0.5f));
            }

            yield return new WaitForSecondsRealtime(interval * Random.Range(0.7f, 1.3f));
        }
    }

    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}