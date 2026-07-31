using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Cutscene "ma lộ diện lần đầu, phát giọng, rồi bắt đầu đuổi" -- 1 trong 2 đoạn ma xuất hiện Jok mô tả
// 2026-07-31 (đoạn còn lại là DiaryReactionCutsceneTrigger.cs -- lướt qua rồi biến mất, KHÔNG liên quan).
//
// SỬA 2026-07-31 (Jok chỉnh lại toàn bộ thiết kế ban đầu):
// - KHÔNG teleport ma tới vị trí "kịch tính" nào cả -- ma cứ patrol bình thường, không có lý do gì để dịch
//   chuyển nó. Đã xoá field _ghostRevealPosition + gọi GhostAI.WarpTo() (method đó cũng đã xoá luôn).
// - KHÔNG khoá input Player, KHÔNG ép camera quay -- Player vẫn đang "rush" (tự do chạy), không phải đứng
//   xem cutscene bị động.
// - Giọng ma là AudioClip THÔ (KHÔNG phải DialogueAsset/DialogueUI) -- phát qua GhostAI.PlayDistortedVoice()
//   (tự hạ pitch cho biến dạng/kinh dị), không có phụ đề, không pause game.
public class GhostChaseIntroCutscene : MonoBehaviour
{
    [SerializeField] private GhostAI _ghost;

    [Tooltip("Giọng ma lúc lộ diện -- AudioClip thô (KHÔNG phải DialogueAsset), phát qua GhostAI.PlayDistortedVoice() (tự hạ pitch). Để trống thì bỏ qua bước này, vào Chase ngay.")]
    [SerializeField] private AudioClip _revealVoiceClip;
    [Tooltip("Khoảng chờ sau khi phát giọng rồi mới ForceEnterChase() -- KHÔNG chờ hết clip mới xong, để ma bắt đầu đuổi khi giọng vẫn đang vang, cảm giác dồn dập hơn.")]
    [SerializeField] private float _delayBeforeChase = 0.3f;

    public UnityEvent OnChaseStarted;

    private bool _hasPlayed = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayed) return;
        if (!other.CompareTag("Player")) return;
        // Cutscene này CHỈ thuộc cảnh 3 -- tránh trigger sai ngữ cảnh nếu Player debug-nhảy cảnh khác.
        if (!Chapter1Scene3Manager.IsActive) return;
        if (_ghost == null) return;
        _hasPlayed = true;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        if (_revealVoiceClip != null) _ghost.PlayDistortedVoice(_revealVoiceClip);

        yield return new WaitForSeconds(_delayBeforeChase);

        _ghost.ForceEnterChase();
        OnChaseStarted?.Invoke();
    }
}
