using UnityEngine;

// Trigger 1 lần phát "suy nghĩ" (DialogueAsset hasVoice=false -- chữ nổi màn hình, KHÔNG thu giọng, đúng quy
// ước story bible "SUY NGHĨ khác LỜI THOẠI") khi Player đi qua đúng chỗ -- dùng cho gợi ý định hướng đơn
// giản (VD "cửa sổ này không ra được nữa rồi, phải tìm đường khác"). KHÔNG khoá input, KHÔNG phải cutscene.
[RequireComponent(typeof(Collider))]
public class ThoughtTrigger : MonoBehaviour
{
    [SerializeField] private DialogueAsset _thought;
    [Tooltip("Chỉ chạy khi ĐANG ở cảnh 3 -- tắt nếu muốn dùng ở cảnh khác.")]
    [SerializeField] private bool _onlyDuringScene3 = true;

    private bool _hasTriggered = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (_onlyDuringScene3 && !Chapter1Scene3Manager.IsActive) return;
        if (_thought == null || DialogueUI.Instance == null) return;
        _hasTriggered = true;
        DialogueUI.Instance.StartDialogue(_thought);
    }
}
