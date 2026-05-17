using UnityEngine;

[CreateAssetMenu(fileName = "NewGhostData", menuName = "VillaOfDarkness/Ghost Data")]
public class GhostData : ScriptableObject {
    public string ghostName;
    public GameObject prefab;
    public float moveSpeed;
    public float detectionRadius;
}