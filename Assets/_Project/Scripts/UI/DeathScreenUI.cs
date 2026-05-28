using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
#pragma warning disable CS0414

public class DeathScreenUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private TMP_Text   nameText;
    [SerializeField] private TMP_Text   yearText;
    [SerializeField] private RawImage   glitchNoise;
    [SerializeField] private Image      scanlineStrip;

    [SerializeField] private bool   _isVisible     = false;
    [SerializeField] private string _characterName;

    // Giữ lại cho ai dùng ngoài, nhưng Retry() không còn phụ thuộc vào nó nữa
    public UnityEvent OnRetry = new UnityEvent();

    private Coroutine _glitch;
    private Texture2D _noiseTex;

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

    public void Show(string name, string years)
    {
        _isVisible     = true;
        _characterName = name;

        if (nameText != null) nameText.text = name;
        if (yearText != null) yearText.text  = years;

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

        // Gọi thẳng GameManager – không phụ thuộc Inspector hay UnityEvent
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerRespawn();
        else
            Debug.LogWarning("[DeathScreenUI] Không tìm thấy GameManager.Instance!");

        OnRetry.Invoke(); // vẫn giữ để tương thích nếu có listener khác
    }

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
                float tear = Random.value < 0.3f ? Random.Range(-0.06f, 0.06f) : 0f;
                glitchNoise.uvRect = new Rect(tear, 0, 1, 1);
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

    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}