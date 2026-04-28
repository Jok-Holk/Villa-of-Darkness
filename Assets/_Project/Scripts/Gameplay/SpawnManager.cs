using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Default Spawn Settings")]
    public GameObject defaultPrefab;
    public Transform defaultSpawnPoint;

    // UnityEvent dùng hàm này
    public void SpawnAt()
    {
        if (defaultPrefab == null || defaultSpawnPoint == null)
        {
            Debug.LogWarning("SpawnManager: Missing prefab or spawn point!");
            return;
        }

        Instantiate(defaultPrefab, defaultSpawnPoint.position, defaultSpawnPoint.rotation);
    }

    // TESTS dùng hàm này (Vector3)
    public GameObject SpawnAt(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        return Instantiate(prefab, position, Quaternion.identity);
    }

    // Version Transform → đổi tên để không bị overload conflict
    public GameObject SpawnAtTransform(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null) return null;
        return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}