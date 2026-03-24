using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject SpawnAt(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        var obj = Instantiate(prefab, position, Quaternion.identity);
        return obj;
    }
}
