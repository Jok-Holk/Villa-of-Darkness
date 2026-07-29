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
    [Tooltip("Số cảnh checkpoint này đại diện -- VD 2 = phòng ăn. Phải LỚN HƠN stage của checkpoint trước đó theo đúng thứ tự chơi thật (1 = cổng, do IntroManager tự lưu).")]
    [SerializeField] private int _stage = 2;

    [Tooltip("Chỉ lưu 1 lần đầu tiên đi qua — tắt nếu muốn checkpoint cập nhật lại mỗi lần Player đi ngang qua.")]
    [SerializeField] private bool _oneTimeOnly = true;

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_oneTimeOnly && _triggered) return;

        CheckpointManager.Save(_stage, other.transform.position, other.transform.rotation);
        _triggered = true;
    }
}
