using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lưu vị trí/hướng nhìn Player tại checkpoint gần nhất — sống qua reload scene
/// (static, giống GameData). Liên kết với inventory: GameData.collectedItems đã
/// tự sống qua reload sẵn rồi, nên checkpoint chỉ cần lo phần vị trí + tuỳ chọn
/// snapshot lại danh sách item tại thời điểm lưu (dùng nếu muốn "chết mất đồ nhặt
/// sau checkpoint" — mặc định KHÔNG revert, giữ nguyên tiến trình nhặt đồ).
///
/// Cách dùng: đặt component Checkpoint.cs (trigger) vào world tại các điểm mốc.
/// GameManager.PlayerRespawn() tự gọi Restore() sau khi scene load lại nếu có checkpoint.
/// </summary>
public static class CheckpointManager
{
    public static bool HasCheckpoint { get; private set; }
    public static Vector3    Position { get; private set; }
    public static Quaternion Rotation { get; private set; }

    private static List<string> _snapshotItems = new List<string>();

    public static void Save(Vector3 position, Quaternion rotation)
    {
        HasCheckpoint = true;
        Position      = position;
        Rotation      = rotation;
        _snapshotItems = new List<string>(GameData.collectedItems);

        Debug.Log($"[Checkpoint] Đã lưu tại {position}.");
    }

    /// <summary>Gọi sau khi scene reload xong (Retry). revertInventory=true nếu muốn
    /// mất các item nhặt SAU checkpoint — mặc định false, giữ nguyên toàn bộ tiến trình.</summary>
    public static void Restore(Transform player, bool revertInventory = false)
    {
        if (!HasCheckpoint || player == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // tắt tạm — CharacterController chặn set position trực tiếp
        player.position = Position;
        player.rotation = Rotation;
        if (cc != null) cc.enabled = true;

        if (revertInventory)
        {
            GameData.collectedItems.Clear();
            GameData.collectedItems.AddRange(_snapshotItems);
        }

        Debug.Log($"[Checkpoint] Đã khôi phục Player về {Position}" + (revertInventory ? " (đã revert inventory về lúc lưu)." : "."));
    }

    public static void Clear()
    {
        HasCheckpoint = false;
        _snapshotItems.Clear();
    }
}
