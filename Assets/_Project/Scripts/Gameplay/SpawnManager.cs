using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject SpawnAt(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        return Instantiate(prefab, position, Quaternion.identity);
    }

    public GameObject SpawnAt(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null) return null;
        return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
