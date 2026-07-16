using UnityEngine;
using UnityEngine.UI;

public class StaminaBreathUI : StaminaBarUI
{
    [SerializeField] private RectTransform barRoot;   // object có Pivot X = 0.5
    [SerializeField] private Image barFill;
    [SerializeField] private float minBreathSpeed = 1f;
    [SerializeField] private float maxBreathSpeed = 6f;
    [SerializeField] private float breathScaleAmount = 0.03f; // giảm nhẹ vì đã có scale co 2 đầu

    protected override void UpdateVisual(float smoothed, float raw)
    {
        // Nhịp thở nhẹ cộng thêm vào scale chính
        float speed = Mathf.Lerp(maxBreathSpeed, minBreathSpeed, raw);
        float pulse = 1f + Mathf.Sin(Time.time * speed) * breathScaleAmount;

        // Scale X theo stamina -> co từ 2 đầu vào giữa
        // Scale Y giữ nguyên (hoặc pulse nhẹ theo nhịp thở)
        barRoot.localScale = new Vector3(smoothed * pulse, pulse, 1f);

        // Màu chuyển liên tục trắng -> đỏ theo stamina (không chỉ khi gần cạn)
        barFill.color = Color.Lerp(Color.red, Color.white, smoothed);

        // Khi chạm 0: nhấp nháy đỏ mạnh để báo hết hơi
        if (raw <= 0f)
        {
            float blink = Mathf.PingPong(Time.time * 5f, 1f);
            barFill.color = Color.Lerp(new Color(0.6f, 0f, 0f), Color.red, blink);
        }
    }
}