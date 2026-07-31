using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Cutscene "đi bộ ra giếng" -- cutscene cuối trong 3 cái còn thiếu của Cảnh 3. Sau khi thoát ra sân sau,
// Player bị ép đi bộ theo đúng đường tới gần giếng (không tự do chạy nữa -- tạo cảm giác không thể tránh
// khỏi số phận), tắt nhạc nền căng thẳng đúng lúc này (Jok: "Ra tới hành lang sau thì sẽ tắt nhạc nền dự
// tính thế"), rồi TRẢ LẠI quyền điều khiển bình thường ngay khi tới nơi -- KHÔNG tự trigger jumpscare ở
// đây. WellDeathSequence.cs (đã có sẵn, gắn trên Well GameObject) tự lo phần "nhìn giếng quá lâu thì chết"
// qua GazeTrigger như cũ -- script này chỉ lo đúng đoạn đi bộ dẫn tới đó, không viết đè logic jumpscare.
public class WalkToWellCutscene : MonoBehaviour
{
    [Tooltip("Các điểm đi qua theo đúng thứ tự (VD men theo lối mòn ra giếng) -- tối thiểu 1 điểm.")]
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _walkSpeed = 1.3f;
    [Tooltip("Tắt hẳn nhạc nền căng thẳng (AudioManager.StopBGM) ngay khi bắt đầu đi bộ ra giếng.")]
    [SerializeField] private bool _stopBgmOnStart = true;

    public UnityEvent OnArrivedAtWell;

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
        if (_waypoints == null || _waypoints.Length == 0) return;
        // THÊM (Jok hỏi "có check hiện đang cảnh 3 không"): cutscene này CHỈ thuộc cảnh 3 -- tránh trigger
        // sai ngữ cảnh nếu Player debug-nhảy cảnh khác (CheckpointDebugTool) mà lỡ đi ngang đúng vùng này.
        if (!Chapter1Scene3Manager.IsActive) return;
        _hasPlayed = true;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        var player = PlayerController.Instance;
        if (player == null) yield break;

        if (_stopBgmOnStart) AudioManager.Instance?.StopBGM();

        player.SetInputEnabled(false);

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // giống pattern HideSpot -- Lerp trực tiếp transform, tránh CC đánh nhau vật lý

        foreach (var wp in _waypoints)
        {
            if (wp == null) continue;
            yield return WalkTo(player.transform, wp.position, wp.rotation);
        }

        if (cc != null) cc.enabled = true;

        player.SetInputEnabled(true);
        OnArrivedAtWell?.Invoke();
    }

    private IEnumerator WalkTo(Transform playerT, Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 fromPos = playerT.position;
        Quaternion fromRot = playerT.rotation;
        float distance = Vector3.Distance(fromPos, targetPos);
        float duration = Mathf.Max(0.1f, distance / _walkSpeed);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            playerT.SetPositionAndRotation(
                Vector3.Lerp(fromPos, targetPos, k),
                Quaternion.Slerp(fromRot, targetRot, k));
            yield return null;
        }
        playerT.SetPositionAndRotation(targetPos, targetRot);
    }
}
