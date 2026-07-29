using UnityEngine;

// Component nhỏ dùng chung -- wire vào bất kỳ UnityEvent nào (OnPickedUp, OnSequenceComplete...) để lưu
// checkpoint đúng lúc đó, KHÔNG cần Player đi ngang qua 1 trigger collider như Checkpoint.cs. Dùng cho các
// mốc tiến độ xảy ra qua tương tác/cutscene (VD nhặt xong hộp âm nhạc -> chuẩn bị sang cảnh 3) thay vì vị
// trí cố định trong world.
//
// SỬA 2026-07-27 (Jok chỉnh): KHÔNG dùng vị trí Player hiện tại lúc lưu -- giả dụ đúng lúc đó đang bị ma
// (VD "ma vú dài") đuổi sát, lưu ngay chỗ nguy hiểm đó thì Retry xong vẫn thua ngay lập tức, vô lý. Phải
// dùng 1 SPAWN POINT CỐ ĐỊNH đặt sẵn trong scene (an toàn, Jok tự chọn vị trí) -- giống hệt cách
// CheckpointDebugTool.DebugStageEntry.spawnPoint đang làm cho các checkpoint debug.
public class SaveCheckpointOnEvent : MonoBehaviour
{
    [Tooltip("Số cảnh checkpoint này đại diện -- phải LỚN HƠN mọi checkpoint trước đó theo đúng thứ tự chơi thật.")]
    [SerializeField] private int _stage = 3;

    [Tooltip("Vị trí spawn CỐ ĐỊNH, AN TOÀN cho checkpoint này -- KHÔNG dùng vị trí Player lúc lưu (có thể đang bị ma đuổi sát ngay lúc đó). Tự đặt 1 Transform trong scene ở chỗ hợp lý rồi kéo vào đây.")]
    [SerializeField] private Transform _spawnPoint;

    /// <summary>Gọi từ UnityEvent (VD PickupItem.OnPickedUp) -- lưu vị trí/hướng nhìn của _spawnPoint, KHÔNG
    /// phải vị trí Player hiện tại.</summary>
    public void Save()
    {
        if (_spawnPoint == null)
        {
            Debug.LogWarning($"[SaveCheckpointOnEvent] '{gameObject.name}' chưa gán Spawn Point -- không lưu được checkpoint stage {_stage}. Vào Inspector kéo 1 Transform đặt sẵn ở vị trí an toàn vào field này.");
            return;
        }

        CheckpointManager.Save(_stage, _spawnPoint.position, _spawnPoint.rotation);
    }
}
