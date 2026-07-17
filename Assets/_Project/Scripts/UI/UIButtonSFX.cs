using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Gắn script này vào từng Button hoặc Slider trong UI.
/// Tự phát 1 trong 2 âm thanh click, luân phiên A-B-A-B cho MỌI nút/slider trong game
/// (vì dùng biến static dùng chung).
/// </summary>
public class UIButtonSFX : MonoBehaviour, IPointerUpHandler
{
    [Header("Kéo 2 file âm thanh click vào đây")]
    [SerializeField] private AudioClip _clipA;
    [SerializeField] private AudioClip _clipB;

    // static => TẤT CẢ instance của script này (trên mọi Button/Slider) dùng chung 1 biến này
    // => đảm bảo luân phiên toàn cục A-B-A-B, không phải riêng từng nút tự đếm
    private static bool _useA = true;

    private Button _button;
    private Slider _slider;

    private void Start()
    {
        // Thử lấy Button trước
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(PlayClick);
        }

        // Thử lấy Slider
        _slider = GetComponent<Slider>();
        if (_slider != null)
        {
            // KHÔNG dùng onValueChanged vì sẽ bắn liên tục mỗi frame lúc đang kéo (dí tiếng khó chịu)
            // Dùng IPointerUpHandler (OnPointerUp bên dưới) => chỉ phát âm thanh khi người chơi THẢ tay ra
        }

        if (_button == null && _slider == null)
        {
            Debug.LogWarning($"[UIButtonSFX] Object '{name}' không có Button hoặc Slider để gắn SFX!", this);
        }
    }

    // Được gọi tự động bởi EventSystem khi người chơi thả chuột/tay ra khỏi Slider (kéo xong)
    public void OnPointerUp(PointerEventData eventData)
    {
        // Chỉ xử lý nếu object này có Slider (tránh Button gọi trùng 2 lần)
        if (_slider != null)
        {
            PlayClick();
        }
    }

    private void PlayClick()
    {
        AudioClip clipToPlay = _useA ? _clipA : _clipB;
        AudioManager.Instance?.PlaySFX(clipToPlay);

        _useA = !_useA; // đảo trạng thái cho lần bấm/kéo TIẾP THEO (của bất kỳ nút nào)
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(PlayClick);
        }
    }
}