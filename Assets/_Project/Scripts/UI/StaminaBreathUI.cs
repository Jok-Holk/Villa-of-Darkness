using UnityEngine;
using UnityEngine.UI;

public class StaminaBreathUI : StaminaBarUI
{
    [SerializeField] private RectTransform barRoot;   // Pivot X = 0.5, Y = 0.5
    [SerializeField] private Image barFill;
    [SerializeField] private float minBreathSpeed = 1f;
    [SerializeField] private float maxBreathSpeed = 6f;
    [SerializeField] private float breathScaleAmount = 0.03f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.white;
    [SerializeField] private Color midColor = new Color(1f, 0.85f, 0.1f); // vàng
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float midThreshold = 0.5f; // ở mức stamina nào thì đạt màu vàng

    protected override void UpdateVisual(float smoothed, float raw)
    {
        float speed = Mathf.Lerp(maxBreathSpeed, minBreathSpeed, raw);
        float pulse = 1f + Mathf.Sin(Time.time * speed) * breathScaleAmount;

        barRoot.localScale = new Vector3(smoothed * pulse, pulse, 1f);

        // Gradient 3 mốc: full -> mid -> low
        Color targetColor;
        if (raw >= midThreshold)
        {
            // Từ 1.0 xuống midThreshold: trắng -> vàng
            float t = Mathf.InverseLerp(1f, midThreshold, raw);
            targetColor = Color.Lerp(fullColor, midColor, t);
        }
        else
        {
            // Từ midThreshold xuống 0: vàng -> đỏ
            float t = Mathf.InverseLerp(midThreshold, 0f, raw);
            targetColor = Color.Lerp(midColor, lowColor, t);
        }

        barFill.color = targetColor;

        // Khi chạm 0: nhấp nháy đỏ đậm để báo hết hơi rõ rệt
        if (raw <= 0f)
        {
            float blink = Mathf.PingPong(Time.time * 5f, 1f);
            barFill.color = Color.Lerp(new Color(0.6f, 0f, 0f), lowColor, blink);
        }
    }
}