using UnityEngine;

// Tiện ích đổi model TRỰC QUAN dùng chung (không riêng cửa sổ) -- ẩn 1 GameObject cũ, hiện 1 GameObject
// mới thay vào đúng chỗ. Dùng cho case "cửa sổ phòng ăn bị kẹt" cảnh 3 (Jok yêu cầu 2026-07-30): kéo model
// cửa sổ THƯỜNG vào _oldVisual, kéo model "window.fbx" (Thuận làm, đã kẹt/chớn cửa) vào _newVisual, gọi
// Swap() từ Chapter1Scene3Manager.OnScene3Activated hoặc bất kỳ trigger nào khác.
//
// LƯU Ý (Thuận báo): FBX export từ Blender không tự liên kết texture trong Unity nếu texture không nằm
// cùng chỗ lúc import lần đầu -- Jok cần tự gán lại texture cho material của _newVisual trong Unity Editor
// TRƯỚC (Inspector, không qua script này). Script chỉ lo ẩn/hiện GameObject, không đụng gì tới material.
public class ModelSwap : MonoBehaviour
{
    [SerializeField] private GameObject _oldVisual;
    [SerializeField] private GameObject _newVisual;
    [Tooltip("Bật thì _newVisual bắt đầu TẮT sẵn (chờ gọi Swap()) -- tắt thì để nguyên trạng thái Inspector.")]
    [SerializeField] private bool _newVisualStartsHidden = true;

    private void Awake()
    {
        if (_newVisualStartsHidden && _newVisual != null) _newVisual.SetActive(false);
    }

    [ContextMenu("Swap (test tay)")]
    public void Swap()
    {
        if (_oldVisual != null) _oldVisual.SetActive(false);
        if (_newVisual != null) _newVisual.SetActive(true);
    }

    /// <summary>Đảo lại (phòng khi cần debug/khôi phục).</summary>
    public void RevertSwap()
    {
        if (_oldVisual != null) _oldVisual.SetActive(true);
        if (_newVisual != null) _newVisual.SetActive(false);
    }
}
