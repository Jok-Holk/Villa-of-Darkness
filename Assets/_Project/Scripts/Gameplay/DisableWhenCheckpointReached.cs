using UnityEngine;

// Tự tắt Collider (không còn raycast tương tác được nữa) ngay lúc Start() nếu checkpoint đã tiến xa hơn
// 1 mốc nhất định -- dùng cho vật tương tác chỉ có ý nghĩa ở giai đoạn TRƯỚC đó (VD cửa chính/đá chặn
// ngoài trời -- khi Retry hoặc debug Force Stage thẳng vào trong nhà thì không cần tương tác được nữa).
//
// CHỈ xử lý case "vừa load scene đã ở stage cao hơn" (Retry, debug-jump). Case "vừa chuyển stage SỐNG
// trong lúc đang chơi" (VD WindowEntryTrigger vừa hoàn thành) do chính script gây ra chuyển cảnh tự tắt
// trực tiếp lúc đó -- component này không tự nghe được sự kiện, chỉ check 1 lần lúc Start().
public class DisableWhenCheckpointReached : MonoBehaviour
{
    [Tooltip("Tắt tương tác ngay khi CheckpointManager.CurrentStage >= số này")]
    [SerializeField] private int disableAtStage = 2;

    private void Start()
    {
        if (CheckpointManager.CurrentStage < disableAtStage) return;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
