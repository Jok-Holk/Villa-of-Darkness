using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lưu vị trí/hướng nhìn + "stage" (cảnh) của checkpoint gần nhất qua PlayerPrefs (prefix "VoD_") --
/// sống qua cả Stop/Play lại trong Editor lẫn tắt/mở lại game thật, không chỉ trong 1 session như trước.
/// Stage là số nguyên tăng dần theo tiến độ (0 = chưa có checkpoint = chạy intro từ đầu, 1 = cổng vào sau
/// intro, 2 = phòng ăn, các cảnh sau tự đánh số tiếp) -- dùng để CheckpointDebugTool.cs liệt kê danh sách
/// cho Jok ép nhảy thẳng tới 1 cảnh cụ thể lúc test, khỏi phải chơi lại từ đầu.
///
/// Liên kết với inventory: GameData.collectedItems đã tự sống qua reload sẵn rồi (nhưng KHÔNG qua
/// PlayerPrefs, chỉ static trong session), nên checkpoint chỉ cần lo phần vị trí + tuỳ chọn snapshot lại
/// danh sách item tại thời điểm lưu (dùng nếu muốn "chết mất đồ nhặt sau checkpoint" -- mặc định KHÔNG
/// revert, giữ nguyên tiến trình nhặt đồ).
///
/// Cách dùng: đặt component Checkpoint.cs (trigger, có field "stage") vào world tại các điểm mốc.
/// GameManager.PlayerRespawn() tự gọi Restore() sau khi scene load lại nếu có checkpoint. IntroManager.cs
/// tự Save(stage: 1, ...) ngay khi xong cinematic lần đầu.
/// </summary>
public static class CheckpointManager
{
    private const string PrefStage = "VoD_CheckpointStage";
    private const string PrefPosX  = "VoD_CheckpointPosX";
    private const string PrefPosY  = "VoD_CheckpointPosY";
    private const string PrefPosZ  = "VoD_CheckpointPosZ";
    private const string PrefRotY  = "VoD_CheckpointRotY"; // chỉ cần xoay quanh trục Y (nhìn ngang) -- đủ dùng để spawn Player, không cần Quaternion đầy đủ

    public static bool HasCheckpoint => CurrentStage > 0;
    public static int  CurrentStage  => PlayerPrefs.GetInt(PrefStage, 0);
    public static Vector3    Position => new Vector3(PlayerPrefs.GetFloat(PrefPosX, 0f), PlayerPrefs.GetFloat(PrefPosY, 0f), PlayerPrefs.GetFloat(PrefPosZ, 0f));
    public static Quaternion Rotation => Quaternion.Euler(0f, PlayerPrefs.GetFloat(PrefRotY, 0f), 0f);

    private static List<string> _snapshotItems = new List<string>();

    // THÊM 2026-07-27: Lưu/khôi phục trạng thái cửa (khoá/mở khoá) + puzzle đã giải (piano...) theo đúng
    // checkpoint -- trước đây CHỈ có vị trí Player + inventory được nhớ, cửa/puzzle luôn reset về đúng giá
    // trị mặc định lúc thiết kế scene mỗi lần Retry, kể cả tiến độ đã đạt được TRƯỚC checkpoint (VD đã mở
    // khoá 1 cửa từ sớm, chết ở xa hơn, Retry xong cửa đó lại khoá lại dù vẫn còn chìa trong túi).
    //
    // Cách hoạt động: mỗi object có trạng thái cần nhớ (DoorController, PianoInteractable...) tự đăng ký 1
    // cặp getter/setter với 1 ID ổn định (đường dẫn hierarchy) ngay lúc Awake() -- CHỈ Awake(), không phải
    // Start(), vì Restore() được gọi ngay khi SceneManager.sceneLoaded bắn ra (sau Awake() nhưng TRƯỚC
    // Start() của các object khác trong scene mới) nên đăng ký ở Start() sẽ trễ mất 1 nhịp, Restore() gọi
    // vào lúc chưa kịp đăng ký gì cả. ForceSave() đọc getter của TẤT CẢ object đã đăng ký tại thời điểm lưu,
    // Restore() ghi lại đúng giá trị đó qua setter -- object không có mặt lúc lưu (chưa xuất hiện trong
    // game) thì đơn giản là không có gì để khôi phục, giữ nguyên mặc định.
    //
    // CHỈ sống trong RAM (giống GameData.collectedItems) -- KHÔNG qua PlayerPrefs như vị trí/stage, vì mỗi
    // flag cần 1 kiểu lưu trữ có cấu trúc (key-value) khác hẳn vài số float đơn giản. Nghĩa là chỉ đúng
    // trong CÙNG 1 lần mở game (Retry/chết nhiều lần) -- tắt hẳn Unity/game xong mở lại thì trạng thái cửa
    // reset về mặc định scene (vị trí Player vẫn nhớ được nhờ PlayerPrefs như cũ, không đổi).
    private static readonly Dictionary<string, System.Func<bool>>   _flagGetters = new Dictionary<string, System.Func<bool>>();
    private static readonly Dictionary<string, System.Action<bool>> _flagSetters = new Dictionary<string, System.Action<bool>>();
    private static readonly Dictionary<string, bool> _flagsAtCheckpoint = new Dictionary<string, bool>();

    /// <summary>Gọi từ Awake() của object có trạng thái cần nhớ theo checkpoint (cửa khoá, puzzle đã giải...).
    /// id nên ổn định qua các lần scene reload -- dùng CheckpointManager.GetHierarchyPath(transform) là đủ
    /// cho hầu hết trường hợp (object đặt cố định trong scene, không đổi cha lúc chạy).</summary>
    public static void RegisterFlag(string id, System.Func<bool> getter, System.Action<bool> setter)
    {
        _flagGetters[id] = getter;
        _flagSetters[id] = setter;
    }

    /// <summary>Đường dẫn hierarchy đầy đủ (Cha/Con/ChauChat) -- dùng làm ID ổn định cho RegisterFlag() khi
    /// object không có ID thủ công riêng.</summary>
    public static string GetHierarchyPath(Transform t)
    {
        if (t == null) return string.Empty;
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = $"{t.name}/{path}";
        }
        return path;
    }

    /// <summary>Dùng lúc gameplay thật (IntroManager, Checkpoint.cs trigger) -- CHỈ ghi đè nếu stage mới
    /// >= stage đang lưu, tránh 1 checkpoint cũ vô tình ghi đè lên checkpoint mới hơn nếu gọi sai thứ tự.
    /// Dùng ForceSave() thay vào đó nếu cần ép ghi đè bất kể thứ tự (VD CheckpointDebugTool).</summary>
    public static void Save(int stage, Vector3 position, Quaternion rotation)
    {
        if (stage < CurrentStage) return;
        ForceSave(stage, position, rotation);
    }

    /// <summary>Ghi đè checkpoint bất kể stage hiện tại đang ở đâu -- dùng cho debug tool (ép nhảy lùi về
    /// 1 cảnh trước đó để test) hoặc các trường hợp cố ý muốn ghi đè không cần so sánh thứ tự.</summary>
    public static void ForceSave(int stage, Vector3 position, Quaternion rotation)
    {
        PlayerPrefs.SetInt(PrefStage, stage);
        PlayerPrefs.SetFloat(PrefPosX, position.x);
        PlayerPrefs.SetFloat(PrefPosY, position.y);
        PlayerPrefs.SetFloat(PrefPosZ, position.z);
        PlayerPrefs.SetFloat(PrefRotY, rotation.eulerAngles.y);
        PlayerPrefs.Save();

        _snapshotItems = new List<string>(GameData.collectedItems);

        // Chụp lại TOÀN BỘ trạng thái cửa/puzzle đã đăng ký tại đúng thời điểm này -- object nào đã unlock/
        // solve TRƯỚC lúc lưu sẽ giữ nguyên true, object chưa đạt tới vẫn giữ false.
        _flagsAtCheckpoint.Clear();
        foreach (var kvp in _flagGetters)
            _flagsAtCheckpoint[kvp.Key] = kvp.Value();

        Debug.Log($"[Checkpoint] Đã lưu stage {stage} tại {position} ({_flagsAtCheckpoint.Count} trạng thái cửa/puzzle).");
    }

    /// <summary>Gọi sau khi scene reload xong (Retry) hoặc lúc IntroManager phát hiện đã có checkpoint.
    /// revertInventory=true nếu muốn mất các item nhặt SAU checkpoint -- mặc định false, giữ nguyên toàn
    /// bộ tiến trình.</summary>
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

        // Ghi lại đúng trạng thái cửa/puzzle đã chụp lúc lưu -- object nào KHÔNG có trong snapshot (chưa
        // từng đăng ký lúc lưu, VD nằm sau checkpoint) mặc định về false (chưa mở khoá/chưa giải).
        foreach (var kvp in _flagSetters)
        {
            bool value = _flagsAtCheckpoint.TryGetValue(kvp.Key, out bool v) && v;
            kvp.Value(value);
        }

        Debug.Log($"[Checkpoint] Đã khôi phục Player về stage {CurrentStage} ({Position}), {_flagSetters.Count} trạng thái cửa/puzzle" + (revertInventory ? ", đã revert inventory về lúc lưu." : "."));
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(PrefStage);
        PlayerPrefs.DeleteKey(PrefPosX);
        PlayerPrefs.DeleteKey(PrefPosY);
        PlayerPrefs.DeleteKey(PrefPosZ);
        PlayerPrefs.DeleteKey(PrefRotY);
        _snapshotItems.Clear();
    }
}
