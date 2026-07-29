using UnityEngine;

// Đặt lên 1 Collider (isTrigger=true) gần cửa mà ghost cần đi qua trong lúc tuần tra/đuổi bắt -- GhostAI
// phát hiện qua OnTriggerEnter, tự dừng lại + mở khoá + mở cửa rồi đi tiếp, thay vì đi xuyên qua cửa đóng.
// Kích thước/vị trí Collider: đặt ngay trước cửa, đủ rộng để ghost chắc chắn chạm vào trước khi tới sát cửa.
[RequireComponent(typeof(Collider))]
public class GhostDoorway : MonoBehaviour
{
    [Tooltip("Cửa cần mở khi ghost đi qua đây.")]
    [SerializeField] private DoorController _door;

    [Tooltip("Thời gian ghost đứng chờ (giây) trong lúc \"mở cửa\" trước khi đi tiếp -- canh khớp animation mở cửa thật.")]
    [SerializeField] private float _openDelay = 0.6f;

    public DoorController Door => _door;
    public float OpenDelay => _openDelay;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
