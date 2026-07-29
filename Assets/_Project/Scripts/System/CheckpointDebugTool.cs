using UnityEngine;

/// <summary>
/// Đặt vào scene (tên GameObject "CheckpointManager" cho dễ tìm trong Hierarchy) -- công cụ debug để
/// nhảy thẳng tới 1 checkpoint cụ thể lúc bấm Play, khỏi phải chơi lại từ đầu mỗi lần muốn test khu vực
/// xa (VD phòng ăn). CHỈ hoạt động trong Unity Editor (tự vô hiệu hoàn toàn lúc build ra game thật, không
/// ảnh hưởng người chơi thật dù quên trả field về mặc định).
///
/// Force Stage: -1 = không đụng gì, giữ nguyên tiến độ PlayerPrefs thật đang có (kể cả từ lần Play test
/// trước) -- dùng khi muốn test tiếp đúng chỗ đang dừng. 0 = XOÁ checkpoint, đảm bảo intro chạy lại từ
/// đầu -- dùng khi muốn test lại intro, KHÔNG được giả định "0 = mặc định = không ảnh hưởng gì" vì
/// PlayerPrefs có thể còn lưu stage cũ từ lần test trước. >0 = ép checkpoint về đúng stage đó lúc Awake
/// (trước khi IntroManager kiểm tra) -- ghi thẳng vào PlayerPrefs, Stop Play xong bấm Play lại vẫn giữ
/// nguyên, không cần set lại mỗi lần.
/// </summary>
public class CheckpointDebugTool : MonoBehaviour
{
    [System.Serializable]
    public class DebugStageEntry
    {
        [Tooltip("Chỉ để nhìn cho dễ trong Inspector, không dùng trong code")]
        public string label;
        public int stage;
        public Transform spawnPoint;
    }

    [Tooltip("-1 = KHÔNG đụng gì, giữ nguyên tiến độ PlayerPrefs thật đang có (kể cả từ lần Play test trước). " +
             "0 = XOÁ checkpoint, đảm bảo intro chạy lại từ đầu (CheckpointManager có thể đang còn lưu stage cũ từ lần test trước, để 0 mà không xoá thì KHÔNG chắc chạy intro). " +
             ">0 = ép checkpoint về đúng stage đó lúc Awake (trước khi IntroManager kiểm tra).")]
    [SerializeField] private int forceStage = -1;

    [Tooltip("Danh sách cảnh -- kéo Transform điểm spawn tương ứng vào từng dòng. Thêm dòng mới khi có cảnh 3 trở đi.")]
    [SerializeField] private DebugStageEntry[] debugStageSpawnPoints = new DebugStageEntry[]
    {
        new DebugStageEntry { label = "1 - Cổng vào (sau intro)", stage = 1 },
        new DebugStageEntry { label = "2 - Phòng ăn",             stage = 2 },
    };

    // BUG THẬT 2026-07-27 (Jok phát hiện): forceStage là giá trị LƯU TRONG FILE SCENE, còn Awake() chạy lại
    // MỖI LẦN scene reload -- kể cả lúc Retry sau khi chết trong lúc chơi thật, không chỉ lúc bấm Play lần
    // đầu. Nếu Inspector đang để 0 (dư lại từ lần test intro trước), mỗi lần chết -> reload -> Awake() chạy
    // lại -> CheckpointManager.Clear() xoá luôn checkpoint 2 vừa đạt được, quay về đầu game oan uổng.
    // Fix: chỉ áp dụng Force Stage đúng 1 lần cho MỖI PHIÊN PLAY thật (static sống qua các lần reload scene
    // trong cùng 1 lần bấm Play, nhưng tự reset khi Stop/Play lại nhờ Domain Reload mặc định của Unity Editor).
    private static bool _appliedThisPlaySession = false;

    private void Awake()
    {
#if !UNITY_EDITOR
        // Chỉ hoạt động trong Unity Editor -- lỡ quên trả Force Stage về -1 trước khi build thì game thật
        // (người chơi thật) KHÔNG bị ảnh hưởng gì cả, script này coi như không tồn tại ngoài Editor.
        return;
#else
        if (_appliedThisPlaySession) return; // đã áp dụng 1 lần đầu phiên Play này rồi -- các lần scene reload sau (Retry chết) KHÔNG đụng lại checkpoint thật nữa
        _appliedThisPlaySession = true;

        if (forceStage < 0) return; // -1 = không đụng gì, giữ nguyên tiến độ thật

        if (forceStage == 0)
        {
            // XOÁ hẳn checkpoint (không chỉ "không làm gì") -- đảm bảo IntroManager thấy HasCheckpoint=false
            // thật sự, chạy lại intro từ đầu, kể cả khi PlayerPrefs còn đang lưu stage cũ từ lần Play test trước.
            CheckpointManager.Clear();
            Debug.Log("[CheckpointDebugTool] Force Stage = 0 -- đã xoá checkpoint, intro sẽ chạy lại từ đầu.");
            return;
        }

        foreach (var entry in debugStageSpawnPoints)
        {
            if (entry.stage != forceStage) continue;
            if (entry.spawnPoint == null)
            {
                Debug.LogWarning($"[CheckpointDebugTool] Stage {forceStage} (\"{entry.label}\") chưa gán Spawn Point -- bỏ qua, không ép được.");
                return;
            }

            CheckpointManager.ForceSave(forceStage, entry.spawnPoint.position, entry.spawnPoint.rotation);
            Debug.Log($"[CheckpointDebugTool] Đã ép checkpoint về stage {forceStage} (\"{entry.label}\") tại {entry.spawnPoint.position}.");
            return;
        }

        Debug.LogWarning($"[CheckpointDebugTool] forceStage={forceStage} nhưng không có entry nào trong danh sách khớp stage này.");
#endif
    }
}
