using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiệu ứng khí quyển cho DeathUI (nhiễu TV/scanline/vignette/bóng ma/rung sắc/chớp trắng),
/// port lại từ DeathScreenUI.cs cũ (thuần code + Texture2D runtime, không phụ thuộc render pipeline
/// nên không cần shader/Volume riêng cho HDRP). Các layer con (GlitchNoise, ScanlineStrip, Vignette,
/// Apparition, WhiteFlash, HeadlineGhostR/C) do Tools/MainGame/Rebuild Death Screen UI tạo sẵn trong
/// scene; component này chỉ tìm và điều khiển chúng lúc Show()/Hide().
/// </summary>
[ExecuteAlways]
public class DeathScreenEffects : MonoBehaviour
{
    [Header("Noise (TV static)")]
    [SerializeField] private int noiseWidth = 128;
    [SerializeField] private int noiseHeight = 72;
    [SerializeField, Range(0f, 0.2f)] private float noiseDensity = 0.05f;
    [SerializeField, Range(0f, 1f)] private float noiseAlphaMin = 0.02f;
    [SerializeField, Range(0f, 1f)] private float noiseAlphaMax = 0.06f;
    [SerializeField, Range(0f, 1f)] private float noiseTearChance = 0.3f;
    [SerializeField, Range(0f, 0.3f)] private float noiseTearAmount = 0.06f;
    [SerializeField, Range(1f, 60f)] private float noiseTicksPerSecond = 20f;
    [SerializeField] private float scanlineSweepSeconds = 2.2f;
    [SerializeField] private Vector2 scanlineAlphaRange = new Vector2(0.10f, 0.22f);

    [Header("Vignette (KHÔNG đổi alpha, chỉ đổi màu - xem ghi chú bug cũ)")]
    [SerializeField] private int vignetteWidth = 192;
    [SerializeField] private int vignetteHeight = 108;
    [SerializeField, Range(0.3f, 1f)] private float vignetteCenterY = 0.82f;
    [SerializeField] private Color vignetteCenterColor = new Color32(0x10, 0x0F, 0x0D, 255);
    [SerializeField] private Color vignetteEdgeColor = new Color32(0x04, 0x04, 0x04, 255);

    [Header("Apparition (bóng ma)")]
    [SerializeField] private int apparitionSize = 160;
    [SerializeField, Range(0f, 1f)] private float apparitionCenterY = 0.42f;
    [SerializeField] private float apparitionFalloffPower = 1.6f;
    [SerializeField] private Vector2 apparitionIntervalRange = new Vector2(1.0f, 2.6f);
    [SerializeField] private float apparitionFadeDuration = 1.4f;
    [SerializeField, Range(0f, 1f)] private float apparitionFaintChance = 0.3f;
    [SerializeField] private float apparitionFaintAlpha = 0.08f;
    [SerializeField] private Vector2 apparitionStrongAlphaRange = new Vector2(0.25f, 0.55f);

    [Header("Chromatic ghost (rung sắc headline)")]
    [SerializeField] private Vector2 chromaticIntervalRange = new Vector2(1.5f, 4.5f);
    [SerializeField] private Vector2 chromaticHoldRange = new Vector2(0.06f, 0.14f);
    [SerializeField, Range(0f, 1f)] private float chromaticAlpha = 0.5f;

    [Header("White flash")]
    [SerializeField] private Vector2 flashIntervalRange = new Vector2(3f, 8f);
    [SerializeField, Range(0f, 1f)] private float flashChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float flashAlpha = 0.45f;
    [SerializeField] private float flashHoldSeconds = 0.04f;
    [SerializeField] private float flashFadeSeconds = 0.35f;

    [Header("Retry pulse")]
    [SerializeField] private Color retryPulseColor = new Color(0.42f, 0.10f, 0.09f);
    [SerializeField] private float retryPulseSpeed = 0.9f;

    private RawImage glitchImage;
    private Texture2D glitchTexture;
    private Color32[] glitchPixels;

    private Image scanlineImage;
    private RectTransform scanlineRect;
    private float scanlineCurrentAlpha;

    private RawImage vignetteImage;
    private Texture2D vignetteTexture;

    private RawImage apparitionImage;
    private Texture2D apparitionTexture;

    private Image whiteFlashImage;

    private TMP_Text titleSource;
    private TMP_Text ghostR;
    private TMP_Text ghostC;

    private Image retryImage;
    private Color retryBaseColor;
    private bool retryBaseColorCaptured;

    private Coroutine glitchCo;
    private Coroutine apparitionCo;
    private Coroutine chromaticCo;
    private Coroutine flashCo;
    private Coroutine retryPulseCo;

    private void Awake()
    {
        ResolveLayers();
        RebuildVignetteTexture();
    }

    private void OnValidate()
    {
        // Cho phép Jok kéo slider Vignette trong Inspector và thấy cập nhật ngay ở Edit Mode.
        if (this == null || !gameObject.scene.IsValid())
            return;

        ResolveLayers();
        RebuildVignetteTexture();
    }

    private void OnDisable()
    {
        StopAllEffectRoutines();
    }

    private void ResolveLayers()
    {
        titleSource = GetChildText("Title");

        retryImage = GetChildImage("RetryButton");
        if (retryImage != null && !retryBaseColorCaptured)
        {
            retryBaseColor = retryImage.color;
            retryBaseColorCaptured = true;
        }

        glitchImage = GetChildRawImage("GlitchNoise");
        vignetteImage = GetChildRawImage("Vignette");
        apparitionImage = GetChildRawImage("Apparition");
        whiteFlashImage = GetChildImage("WhiteFlash");
        ghostR = GetChildText("HeadlineGhostR");
        ghostC = GetChildText("HeadlineGhostC");

        scanlineRect = transform.Find("ScanlineStrip") as RectTransform;
        scanlineImage = scanlineRect != null ? scanlineRect.GetComponent<Image>() : null;
    }

    /// <summary>Bật DeathUI + chạy toàn bộ hiệu ứng khí quyển. Gọi từ GameController khi hiện màn chết.</summary>
    public void Show()
    {
        ResolveLayers();
        RebuildVignetteTexture();
        SetAllEffectAlphaZero();

        if (!Application.isPlaying)
            return;

        StopAllEffectRoutines();
        glitchCo = StartCoroutine(GlitchRoutine());
        apparitionCo = StartCoroutine(ApparitionRoutine());
        chromaticCo = StartCoroutine(ChromaticRoutine());
        flashCo = StartCoroutine(FlashRoutine());
        if (retryImage != null)
            retryPulseCo = StartCoroutine(RetryPulseRoutine());
    }

    /// <summary>Dừng toàn bộ hiệu ứng và reset alpha. Gọi trước khi tắt DeathUI (không reload scene).</summary>
    public void Hide()
    {
        StopAllEffectRoutines();
        SetAllEffectAlphaZero();

        if (glitchImage != null)
            glitchImage.uvRect = new Rect(0f, 0f, 1f, 1f);

        if (retryImage != null && retryBaseColorCaptured)
            retryImage.color = retryBaseColor;
    }

    private void StopAllEffectRoutines()
    {
        StopRoutine(ref glitchCo);
        StopRoutine(ref apparitionCo);
        StopRoutine(ref chromaticCo);
        StopRoutine(ref flashCo);
        StopRoutine(ref retryPulseCo);
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = null;
    }

    private void SetAllEffectAlphaZero()
    {
        SetGraphicAlpha(glitchImage, 0f);
        SetGraphicAlpha(scanlineImage, 0f);
        SetGraphicAlpha(apparitionImage, 0f);
        SetGraphicAlpha(whiteFlashImage, 0f);
        SetGraphicAlpha(ghostR, 0f);
        SetGraphicAlpha(ghostC, 0f);
        scanlineCurrentAlpha = 0f;
    }

    // ---------------------------------------------------------------- Glitch/Noise + Scanline

    private IEnumerator GlitchRoutine()
    {
        float noiseTimer = 0f;
        float sweepElapsed = 0f;

        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            noiseTimer -= dt;
            sweepElapsed += dt;

            float sweepDuration = Mathf.Max(0.1f, scanlineSweepSeconds);
            float t = (sweepElapsed % sweepDuration) / sweepDuration;
            if (t < dt / sweepDuration)
                scanlineCurrentAlpha = Random.Range(scanlineAlphaRange.x, scanlineAlphaRange.y);
            UpdateScanlinePosition(t);

            if (noiseTimer <= 0f)
            {
                RegenerateNoiseTexture();
                ApplyRandomTear();
                noiseTimer = (1f / Mathf.Max(1f, noiseTicksPerSecond)) * Random.Range(0.7f, 1.3f);
            }

            yield return null;
        }
    }

    private void RegenerateNoiseTexture()
    {
        if (glitchTexture == null || glitchTexture.width != noiseWidth || glitchTexture.height != noiseHeight)
        {
            glitchTexture = new Texture2D(noiseWidth, noiseHeight, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            glitchPixels = new Color32[noiseWidth * noiseHeight];
            if (glitchImage != null)
                glitchImage.texture = glitchTexture;
        }

        for (int i = 0; i < glitchPixels.Length; i++)
        {
            if (Random.value < noiseDensity)
            {
                byte v = (byte)Random.Range(90, 210);
                glitchPixels[i] = new Color32(v, v, v, 255);
            }
            else
            {
                glitchPixels[i] = new Color32(0, 0, 0, 0);
            }
        }

        glitchTexture.SetPixels32(glitchPixels);
        glitchTexture.Apply(false);

        SetGraphicAlpha(glitchImage, Random.Range(noiseAlphaMin, noiseAlphaMax));
    }

    private void ApplyRandomTear()
    {
        if (glitchImage == null)
            return;

        if (Random.value < noiseTearChance)
        {
            float tear = Random.Range(-noiseTearAmount, noiseTearAmount);
            glitchImage.uvRect = new Rect(tear, 0f, 1f, 1f);
        }
        else
        {
            glitchImage.uvRect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    private void UpdateScanlinePosition(float t)
    {
        if (scanlineRect == null)
            return;

        float y = Mathf.Lerp(1f, 0f, t);
        scanlineRect.anchorMin = new Vector2(0f, y);
        scanlineRect.anchorMax = new Vector2(1f, y);
        SetGraphicAlpha(scanlineImage, scanlineCurrentAlpha);
    }

    // ---------------------------------------------------------------- Vignette

    private void RebuildVignetteTexture()
    {
        if (vignetteImage == null)
            return;

        if (vignetteTexture == null || vignetteTexture.width != vignetteWidth || vignetteTexture.height != vignetteHeight)
        {
            vignetteTexture = new Texture2D(vignetteWidth, vignetteHeight, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        var pixels = new Color32[vignetteWidth * vignetteHeight];
        float axisY = Mathf.Max(0.05f, Mathf.Max(vignetteCenterY, 1f - vignetteCenterY));

        for (int y = 0; y < vignetteHeight; y++)
        {
            float ny = (y / (float)(vignetteHeight - 1) - vignetteCenterY) / axisY;
            for (int x = 0; x < vignetteWidth; x++)
            {
                float nx = (x / (float)(vignetteWidth - 1) - 0.5f) / 0.5f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(dist));
                Color c = Color.Lerp(vignetteCenterColor, vignetteEdgeColor, k);
                c.a = 1f; // KHÔNG được đổi thành gradient alpha - xem ghi chú bug cũ ở đầu file.
                pixels[y * vignetteWidth + x] = c;
            }
        }

        vignetteTexture.SetPixels32(pixels);
        vignetteTexture.Apply(false);

        vignetteImage.texture = vignetteTexture;
        var col = vignetteImage.color;
        col.a = 1f;
        vignetteImage.color = col;
    }

    // ---------------------------------------------------------------- Apparition (ghost)

    private IEnumerator ApparitionRoutine()
    {
        BuildApparitionTexture();

        while (true)
        {
            yield return WaitRealtime(Random.Range(apparitionIntervalRange.x, apparitionIntervalRange.y));

            float target = Random.value < apparitionFaintChance
                ? apparitionFaintAlpha
                : Random.Range(apparitionStrongAlphaRange.x, apparitionStrongAlphaRange.y);

            yield return FadeGraphicAlpha(apparitionImage, target, apparitionFadeDuration);
            yield return WaitRealtime(Random.Range(0.4f, 1.0f));
            yield return FadeGraphicAlpha(apparitionImage, 0f, apparitionFadeDuration);
        }
    }

    private void BuildApparitionTexture()
    {
        if (apparitionImage == null)
            return;

        int size = Mathf.Max(8, apparitionSize);
        if (apparitionTexture == null || apparitionTexture.width != size)
        {
            apparitionTexture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        var pixels = new Color32[size * size];
        var center = new Vector2(0.5f, apparitionCenterY);

        for (int y = 0; y < size; y++)
        {
            float ny = y / (float)(size - 1);
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)(size - 1);
                float dist = Vector2.Distance(new Vector2(nx, ny), center) * 2f;
                float falloff = Mathf.Clamp01(1f - Mathf.Pow(Mathf.Clamp01(dist), apparitionFalloffPower));
                pixels[y * size + x] = new Color32(210, 210, 220, (byte)(falloff * 255));
            }
        }

        apparitionTexture.SetPixels32(pixels);
        apparitionTexture.Apply(false);
        apparitionImage.texture = apparitionTexture;
    }

    // ---------------------------------------------------------------- Chromatic ghost headline

    private IEnumerator ChromaticRoutine()
    {
        while (true)
        {
            yield return WaitRealtime(Random.Range(chromaticIntervalRange.x, chromaticIntervalRange.y));

            SyncGhostText();
            SetGraphicAlpha(ghostR, chromaticAlpha);
            SetGraphicAlpha(ghostC, chromaticAlpha);

            yield return WaitRealtime(Random.Range(chromaticHoldRange.x, chromaticHoldRange.y));

            SetGraphicAlpha(ghostR, 0f);
            SetGraphicAlpha(ghostC, 0f);
        }
    }

    private void SyncGhostText()
    {
        if (titleSource == null)
            return;

        if (ghostR != null) ghostR.text = titleSource.text;
        if (ghostC != null) ghostC.text = titleSource.text;
    }

    // ---------------------------------------------------------------- White flash

    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            yield return WaitRealtime(Random.Range(flashIntervalRange.x, flashIntervalRange.y));

            if (Random.value > flashChance)
                continue;

            SetGraphicAlpha(whiteFlashImage, flashAlpha);
            yield return WaitRealtime(flashHoldSeconds);
            yield return FadeGraphicAlpha(whiteFlashImage, 0f, flashFadeSeconds);
        }
    }

    // ---------------------------------------------------------------- Retry button pulse

    private IEnumerator RetryPulseRoutine()
    {
        while (true)
        {
            float k = (Mathf.Sin(Time.unscaledTime * retryPulseSpeed) + 1f) * 0.5f;
            retryImage.color = Color.Lerp(retryBaseColor, retryPulseColor, k);
            yield return null;
        }
    }

    // ---------------------------------------------------------------- Helpers

    private RawImage GetChildRawImage(string childName)
    {
        var t = transform.Find(childName);
        return t != null ? t.GetComponent<RawImage>() : null;
    }

    private Image GetChildImage(string childName)
    {
        var t = transform.Find(childName);
        return t != null ? t.GetComponent<Image>() : null;
    }

    private TMP_Text GetChildText(string childName)
    {
        var t = transform.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    private static void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
            return;

        var c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private IEnumerator FadeGraphicAlpha(Graphic graphic, float target, float duration)
    {
        if (graphic == null)
            yield break;

        float start = graphic.color.a;
        if (duration <= 0f)
        {
            SetGraphicAlpha(graphic, target);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetGraphicAlpha(graphic, Mathf.Lerp(start, target, Mathf.Clamp01(t / duration)));
            yield return null;
        }

        SetGraphicAlpha(graphic, target);
    }

    private static WaitForSecondsRealtime WaitRealtime(float seconds)
    {
        return new WaitForSecondsRealtime(Mathf.Max(0.001f, seconds));
    }
}
