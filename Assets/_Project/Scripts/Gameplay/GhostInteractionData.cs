using UnityEngine;

[CreateAssetMenu(fileName = "MaDaConfig", menuName = "VillaOfDarkness/Ghost Interaction")]
public class GhostInteractionData : ScriptableObject
{
    [Range(1f, 10f)]
    public float gazeThreshold = 3f;      // Thời gian nhìn để chết
    public float warningThreshold = 1f;   // Thời gian bắt đầu cảnh báo
    public float maxDistance = 10f;       // Khoảng cách tối đa Raycast nhìn thấy
}