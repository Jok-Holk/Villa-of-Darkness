using UnityEngine;

/// <summary>
/// Đặt vào world tại vị trí muốn làm checkpoint — cần Collider (IsTrigger = true)
/// trên chính object này, layer/tag không bắt buộc đặc biệt. Player đi ngang qua
/// sẽ tự lưu vị trí + hướng nhìn vào CheckpointManager.
///
/// Đây là component hạ tầng — CHƯA đặt sẵn trong scene, chờ setup gameplay thật
/// (Jok tự chọn vị trí đặt checkpoint theo thiết kế level).
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Chỉ lưu 1 lần đầu tiên đi qua — tắt nếu muốn checkpoint cập nhật lại mỗi lần Player đi ngang qua.")]
    [SerializeField] private bool _oneTimeOnly = true;

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_oneTimeOnly && _triggered) return;

        CheckpointManager.Save(other.transform.position, other.transform.rotation);
        _triggered = true;
    }
}
