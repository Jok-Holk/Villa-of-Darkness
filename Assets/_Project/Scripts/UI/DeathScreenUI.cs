using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
#pragma warning disable CS0414

public class DeathScreenUI : MonoBehaviour
{
    // ── Panel & Glitch ─────────────────────────────────────────────────────────
    [Header("Panel & Glitch")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private RawImage   glitchNoise;
    [SerializeField] private Image      scanlineStrip;

    // ── Newspaper Text ─────────────────────────────────────────────────────────
    [Header("Newspaper Text")]
    [SerializeField] private TMP_Text mastheadText;
    [SerializeField] private TMP_Text headlineText;
     [SerializeField] private TMP_Text quoteText;

    [Header("Nội dung cố định")]
    [SerializeField] private string masthead   = "SAIGON THỜI BÁO  ·  14/3/2000";
    [SerializeField] private string headline   = "PHÓNG VIÊN MẤT TÍCH TẠI BIỆT THỰ";
    
    // ── Typewriter ─────────────────────────────────────────────────────────────
    [Header("Typewriter")]
    [Tooltip("Giây giữa mỗi ký tự")]
    [SerializeField] private float charDelay  = 0.045f;
    [Tooltip("Giây dừng giữa các dòng")]
    [SerializeField] private float lineDelay  = 0.35f;
    [Tooltip("Xác suất phát tiếng gõ mỗi ký tự")]
    [SerializeField] [Range(0f,1f)] private float typeChance = 0.85f;

    // ── Âm thanh ───────────────────────────────────────────────────────────────
    [Header("Âm thanh")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   typeClip;    // tiếng gõ từng phím
    [SerializeField] private AudioClip   returnClip;  // tiếng carriage return cuối dòng

    // ── Buttons ────────────────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button     retryButton;
    [SerializeField] private Button     menuButton;
    [SerializeField] private GameObject buttonRow;    // ẩn cho đến khi in xong

    // ── Gốc giữ nguyên ────────────────────────────────────────────────────────
    [SerializeField] private bool   _isVisible     = false;
    [SerializeField] private string _characterName;
    public UnityEvent OnRetry = new UnityEvent();

    private Coroutine _glitch;
    private Coroutine _typewriter;
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

        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (menuButton  != null) menuButton.onClick.AddListener(GoMenu);
    }

    private void OnDestroy() { if (_noiseTex) Destroy(_noiseTex); }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void Show(string name, string years)
    {
        _isVisible     = true;
        _characterName = name;

        // Xoá sạch text trước
        SetText(mastheadText, "");
        SetText(headlineText, "");
         SetText(quoteText,    "");

        // Ẩn nút cho đến khi in xong
        if (buttonRow != null) buttonRow.SetActive(false);

        if (deathScreenPanel != null) deathScreenPanel.SetActive(true);

        // Glitch + typewriter chạy song song
        if (_glitch     != null) StopCoroutine(_glitch);
        if (_typewriter != null) StopCoroutine(_typewriter);
        _glitch     = StartCoroutine(GlitchRoutine());
        _typewriter = StartCoroutine(PrintNewspaper(name, years));
    }

    public void Hide()
    {
        _isVisible = false;

        if (_glitch     != null) { StopCoroutine(_glitch);     _glitch     = null; }
        if (_typewriter != null) { StopCoroutine(_typewriter); _typewriter = null; }

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

    // ── Typewriter ─────────────────────────────────────────────────────────────

    private IEnumerator PrintNewspaper(string name, string years)
    {
        // Dòng 1: Masthead
        yield return TypeLine(mastheadText, masthead);
        yield return new WaitForSecondsRealtime(lineDelay);

        // Dòng 2: Headline
        yield return TypeLine(headlineText, headline);
        yield return new WaitForSecondsRealtime(lineDelay);

         yield return new WaitForSecondsRealtime(lineDelay);

        // Dòng 4: Quote tên + năm
        yield return TypeLine(quoteText, $"\"{name}, {years}...\"");
        yield return new WaitForSecondsRealtime(lineDelay * 0.5f);

        // Hiện nút sau khi in xong
        if (buttonRow != null) buttonRow.SetActive(true);
    }

    /// In từng ký tự vào TMP_Text
    private IEnumerator TypeLine(TMP_Text tmp, string content)
    {
        if (tmp == null) yield break;
        tmp.text = "";

        foreach (char c in content)
        {
            tmp.text += c;
            PlayTypeSound(c);
            yield return new WaitForSecondsRealtime(charDelay * Random.Range(0.7f, 1.4f));
        }

        PlayReturnSound();
    }

    private void PlayTypeSound(char c)
    {
        if (typeClip == null || audioSource == null) return;
        if (char.IsWhiteSpace(c)) return;
        if (Random.value > typeChance) return;

        audioSource.pitch = Random.Range(0.9f, 1.15f);
        audioSource.PlayOneShot(typeClip, Random.Range(0.4f, 0.7f));
    }

    private void PlayReturnSound()
    {
        if (returnClip == null || audioSource == null) return;
        audioSource.PlayOneShot(returnClip, 0.6f);
    }

    // ── Glitch loop vĩnh viễn, unscaled time ──────────────────────────────────

    private IEnumerator GlitchRoutine()
    {
        float interval = 1f / 20f;
        float scanY    = 1f;

        while (true)
        {
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

            if (glitchNoise != null)
            {
                SetAlpha(glitchNoise, Random.Range(0.3f, 0.65f));
                glitchNoise.uvRect = new Rect(
                    Random.value < 0.3f ? Random.Range(-0.06f, 0.06f) : 0f,
                    0, 1, 1);
            }

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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static void SetText(TMP_Text tmp, string t) { if (tmp != null) tmp.text = t; }

    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}