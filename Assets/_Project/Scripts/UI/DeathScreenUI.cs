using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
#pragma warning disable CS0414

public class DeathScreenUI : MonoBehaviour
{
    // ── THÊM: refs UI ─────────────────────────────────────────────────────────
    [Header("Refs")]
    [SerializeField] private GameObject deathScreenPanel;  // kéo DeathScreen_Panel vào
    [SerializeField] private TMP_Text   nameText;
    [SerializeField] private TMP_Text   yearText;
    [SerializeField] private RawImage   glitchNoise;       // Raw Image con, không cần Texture
    [SerializeField] private Image      scanlineStrip;     // Image con dải ngang ~40px

    // ── GỐC: giữ nguyên ───────────────────────────────────────────────────────
    [SerializeField] private bool   _isVisible     = false;
    [SerializeField] private string _characterName;
    public UnityEvent OnRetry = new UnityEvent();

    // ── THÊM: runtime ─────────────────────────────────────────────────────────
    private Coroutine _glitch;
    private Texture2D _noiseTex;

    // ══════════════════════════════════════════════════════════════════════════
    // THÊM: Awake – tạo noise texture + ẩn panel
    private void Awake()
    {
        _noiseTex            = new Texture2D(128, 72, TextureFormat.RGBA32, false);
        _noiseTex.filterMode = FilterMode.Point;
        _noiseTex.wrapMode   = TextureWrapMode.Repeat;
        if (glitchNoise != null) glitchNoise.texture = _noiseTex;

        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
        SetAlpha(glitchNoise,   0f);
        SetAlpha(scanlineStrip, 0f);
    }

    private void OnDestroy() { if (_noiseTex) Destroy(_noiseTex); }

    // ══════════════════════════════════════════════════════════════════════════
    // GỐC Show – thêm: gán text, hiện panel, chạy glitch
    public void Show(string name, string years)
    {
        _isVisible     = true;
        _characterName = name;

        // THÊM ▼
        if (nameText != null) nameText.text = name;
        if (yearText != null) yearText.text  = years;
        if (deathScreenPanel != null) deathScreenPanel.SetActive(true);

        if (_glitch != null) StopCoroutine(_glitch);
        _glitch = StartCoroutine(GlitchRoutine());
    }

    // GỐC Hide – thêm: ẩn panel + dừng glitch
    public void Hide()
    {
        _isVisible = false;

        // THÊM ▼
        if (_glitch != null) { StopCoroutine(_glitch); _glitch = null; }
        SetAlpha(glitchNoise,   0f);
        SetAlpha(scanlineStrip, 0f);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
    }

    // GỐC Retry – thêm: Debug.Log trước khi invoke
    public void Retry()
    {
        Debug.Log("[DeathScreenUI] Sự kiện: Người chơi nhấn THỬ LẠI"); // THÊM
        OnRetry.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // THÊM: Glitch coroutine
    private IEnumerator GlitchRoutine()
    {
        float elapsed = 0f, duration = 2.2f, interval = 1f / 20f;
        float scanY = 1f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // -- noise texture --
            float density = Mathf.Lerp(0.55f, 0.05f, t);
            Color32[] px  = new Color32[128 * 72];
            for (int i = 0; i < px.Length; i++)
            {
                if (Random.value < density)
                {
                    byte b = (byte)Random.Range(160, 255);
                    px[i]  = new Color32(b, (byte)Mathf.Min(b + 30, 255), b, 255);
                }
                else px[i] = new Color32(0, 0, 0, 0);
            }
            _noiseTex.SetPixels32(px);
            _noiseTex.Apply();

            // -- glitch noise layer --
            if (glitchNoise != null)
            {
                SetAlpha(glitchNoise, Mathf.Lerp(0.65f, 0f, t * t));
                float tear = Random.value < 0.3f ? Random.Range(-0.06f, 0.06f) : 0f;
                glitchNoise.uvRect = new Rect(tear, 0, 1, 1);
            }

            // -- scanline strip --
            if (scanlineStrip != null)
            {
                scanY -= Time.deltaTime * Random.Range(0.4f, 0.9f);
                if (scanY < -0.1f) scanY = 1.1f;
                var rt = scanlineStrip.rectTransform;
                float h = ((RectTransform)rt.parent).rect.height;
                rt.anchoredPosition = new Vector2(0, (scanY - 0.5f) * h);
                SetAlpha(scanlineStrip, Mathf.Lerp(0.45f, 0f, t * t));
            }

            float wait = interval * Random.Range(0.7f, 1.3f);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }

        SetAlpha(glitchNoise,   0f);
        SetAlpha(scanlineStrip, 0f);
        if (glitchNoise != null) glitchNoise.uvRect = new Rect(0, 0, 1, 1);
        _glitch = null;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // THÊM: helper
    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}