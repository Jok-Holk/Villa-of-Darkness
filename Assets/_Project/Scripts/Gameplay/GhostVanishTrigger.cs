using UnityEngine;
using UnityEngine.Events;

// "Patrol ghost bình thường cho tới khi xong tới chỗ hành lang sau thì disappear ghost thôi" (Jok
// 2026-07-31) -- kết thúc pha ma đuổi hiện diện của Cảnh 3: Player chạy thoát tới đúng vùng hành lang sau,
// ma biến mất luôn (SetActive(false)), không đuổi tiếp ra sân sau/giếng. Đơn giản, không cần fade/hiệu ứng
// gì thêm -- đúng pattern DiaryReactionCutsceneTrigger đã dùng cho ma của nó (SetActive(false) trực tiếp).
public class GhostVanishTrigger : MonoBehaviour
{
    [SerializeField] private GhostAI _ghost;

    public UnityEvent OnGhostVanished;

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
        if (!Chapter1Scene3Manager.IsActive) return;
        if (_ghost == null) return;
        _hasTriggered = true;

        _ghost.gameObject.SetActive(false);
        OnGhostVanished?.Invoke();
    }
}
