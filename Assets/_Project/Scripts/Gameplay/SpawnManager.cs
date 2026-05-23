using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Default Spawn Settings")]
    public GameObject defaultPrefab;
    public Transform defaultSpawnPoint;

    // BỔ SUNG: Biến để ghi nhớ trạng thái đã spawn hay chưa
    private bool _hasSpawned = false;

    // UnityEvent dùng hàm này
    public void SpawnAt()
    {
        // BỔ SUNG: Nếu đã spawn rồi thì thoát luôn, không chạy các lệnh bên dưới
        if (_hasSpawned) return;

        if (defaultPrefab == null || defaultSpawnPoint == null)
        {
            Debug.LogWarning("SpawnManager: Missing prefab or spawn point!");
            return;
        }

        Instantiate(defaultPrefab, defaultSpawnPoint.position, defaultSpawnPoint.rotation);

        // BỔ SUNG: Đánh dấu là đã spawn lần đầu thành công
        _hasSpawned = true;
    }

    // TESTS dùng hàm này (Vector3)
    public GameObject SpawnAt(GameObject prefab, Vector3 position)
    {
        // BỔ SUNG: Kiểm tra tương tự cho hàm test
        if (_hasSpawned || prefab == null) return null;

        _hasSpawned = true;
        return Instantiate(prefab, position, Quaternion.identity);
    }

    // Version Transform → đổi tên để không bị overload conflict
    public GameObject SpawnAtTransform(GameObject prefab, Transform spawnPoint)
    {
        // BỔ SUNG: Kiểm tra tương tự cho hàm transform
        if (_hasSpawned || prefab == null || spawnPoint == null) return null;

        _hasSpawned = true;
        return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}