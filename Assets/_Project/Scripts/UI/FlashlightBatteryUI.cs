using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Đổi màu icon đèn pin trên HUD theo % pin còn lại (FlashlightController.BatteryLevel01),
/// dùng đúng 3 ngưỡng có sẵn trong FlashlightData (không tự bịa ngưỡng khác).
/// Gắn script này vào 1 GameObject trong Canvas HUD, rồi kéo 4 field trong Inspector.
/// </summary>
public class FlashlightBatteryUI : MonoBehaviour
{
    [Header("Refs - kéo vào Inspector")]
    [SerializeField] private FlashlightController _flashlight;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _tLabel;
    [SerializeField] private FlashlightData _data;

    [Header("4 màu theo mốc pin")]
    [SerializeField] private Color _colorFull = Color.white;      // > 50%
    [SerializeField] private Color _colorMedium = Color.yellow;   // 30% - 50%
    [SerializeField] private Color _colorLow = Color.red;         // 15% - 30%
    [SerializeField] private Color _colorCritical = Color.gray;   // < 15%

    [Header("Tốc độ nháy chữ T (lần/giây)")]
    [SerializeField] private float _blinkSpeedMedium = 1f;   // vàng: ~1 lần/giây
    [SerializeField] private float _blinkSpeedLow = 2f;      // đỏ: ~2 lần/giây

    // Trạng thái hiện tại, dùng để biết khi nào cần bật/tắt InvokeRepeating
    private enum BatteryState { Full, Medium, Low, Critical }
    private BatteryState _currentState = (BatteryState)(-1); // ép khác mọi giá trị để lần đầu luôn update

    private void OnEnable()
    {
        // Đảm bảo chữ T tắt sẵn lúc mới bật, tránh nhấp nháy lỗi 1 frame đầu
        if (_tLabel != null) _tLabel.enabled = false;
    }

    private void Update()
    {
        if (_flashlight == null || _icon == null || _data == null) return;

        float battery = _flashlight.BatteryLevel01; // 0.0 -> 1.0

        // Xác định trạng thái hiện tại dựa đúng 3 ngưỡng có sẵn trong asset
        BatteryState newState;
        if (battery > _data.flickerMediumThresh)
        {
            newState = BatteryState.Full;
        }
        else if (battery > _data.flickerLowThresh)
        {
            newState = BatteryState.Medium;
        }
        else if (battery > _data.flickerCriticalThresh)
        {
            newState = BatteryState.Low;
        }
        else
        {
            newState = BatteryState.Critical;
        }

        // Chỉ xử lý lại khi trạng thái THAY ĐỔI (đỡ tốn hiệu năng, đỡ spam InvokeRepeating)
        if (newState != _currentState)
        {
            _currentState = newState;
            ApplyState(newState);
        }
    }

    private void ApplyState(BatteryState state)
    {
        // Luôn hủy nháy cũ trước khi set trạng thái mới, tránh chồng nhiều InvokeRepeating
        CancelInvoke(nameof(ToggleTLabel));

        switch (state)
        {
            case BatteryState.Full:
                _icon.color = _colorFull;
                _tLabel.enabled = false;
                break;

            case BatteryState.Medium:
                _icon.color = _colorMedium;
                _tLabel.enabled = true;
                // nháy vừa: bật/tắt đều đặn theo _blinkSpeedMedium lần/giây
                InvokeRepeating(nameof(ToggleTLabel), 0f, 1f / (_blinkSpeedMedium * 2f));
                break;

            case BatteryState.Low:
                _icon.color = _colorLow;
                _tLabel.enabled = true;
                // nháy nhanh hơn: theo _blinkSpeedLow lần/giây
                InvokeRepeating(nameof(ToggleTLabel), 0f, 1f / (_blinkSpeedLow * 2f));
                break;

            case BatteryState.Critical:
                _icon.color = _colorCritical;
                _tLabel.enabled = true; // hiện, đứng yên, KHÔNG nháy
                break;
        }
    }

    // Hàm này chỉ đơn giản bật/tắt hiển thị chữ T -> tạo hiệu ứng nháy
    private void ToggleTLabel()
    {
        if (_tLabel != null) _tLabel.enabled = !_tLabel.enabled;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ToggleTLabel));
    }
}