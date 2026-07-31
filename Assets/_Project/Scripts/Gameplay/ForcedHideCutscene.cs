using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Cutscene "bị dí phải trốn ngay" -- Player KHÔNG tự chọn trốn (khác HideSpot.Interact() bình thường bấm
// E), bị ép trốn thẳng ngay khi chạy vào đúng trigger này (đặt trên đường chạy trốn ma đang đuổi thật). Tái
// dùng NGUYÊN HideSpot.EnterRoutine() có sẵn (camera slide + mở/hé cửa) qua ForceEnter(), không viết logic
// trốn riêng -- tránh lệch hành vi so với lúc Player tự tương tác.
//
// SỬA 2026-07-31 (Jok chỉnh: "ghost không bị đóng băng lúc Player trốn -- kiểu không tìm thấy, dừng trước
// tủ, rồi quay lại patrol"): ĐÃ XOÁ hẳn _freezeGhostDuringHide/_chasingGhost -- không cần can thiệp gì vào
// ghost ở đây nữa, để nguyên CanDetectPlayer()/CanHearPlayer()/EnterInvestigate()/EnterPatrol() có sẵn trong
// GhostAI tự lo (mất dấu Player đang trốn -> tự đi tới last known position đứng 1 lúc -> tự quay lại patrol).
//
// THÊM (Jok: "người chơi thì rush, chỗ trốn xong mới pause lại dialogue voice bình thường"): sau khi
// HideSpot xác nhận Player ĐÃ trốn xong (IsPlayerHiding=true, khác lúc ghost trò chuyện chỉ phát AudioClip
// thô không pause), phát 1 DialogueAsset THẬT qua DialogueUI (có phụ đề + pause bình thường) -- đây là thoại
// của PLAYER (MK-HIDE-xx), không phải giọng ma.
public class ForcedHideCutscene : MonoBehaviour
{
    [SerializeField] private HideSpot _hideSpot;

    [Tooltip("Thoại Player SAU KHI đã trốn xong (MK-HIDE-xx) -- DialogueAsset thật, có phụ đề + pause bình thường qua DialogueUI. Để trống nếu chưa có, cutscene vẫn chạy xong bình thường.")]
    [SerializeField] private DialogueAsset _hideDialogue;

    public UnityEvent OnForcedHideTriggered;

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
        if (_hideSpot == null) return;
        _hasPlayed = true;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        _hideSpot.ForceEnter();

        // Chờ HideSpot THẬT SỰ xong (camera slide + mở/hé cửa hoàn tất, IsPlayerHiding=true) rồi mới phát
        // thoại -- không phát chồng lên lúc đang slide vào.
        while (!_hideSpot.IsPlayerHiding)
            yield return null;

        OnForcedHideTriggered?.Invoke();

        if (_hideDialogue != null && DialogueUI.Instance != null)
        {
            DialogueUI.Instance.StartDialogue(_hideDialogue);
            while (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
                yield return null;
        }
    }
}
