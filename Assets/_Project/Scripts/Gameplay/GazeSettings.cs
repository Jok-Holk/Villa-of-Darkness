using UnityEngine;

[CreateAssetMenu(fileName = "NewGazeSettings", menuName = "VillaOfDarkness/Gaze Settings")]
public class GazeSettings : ScriptableObject
{
    [Header("Timers")]
    public float gazeThreshold = 3f;    // Thời gian nhìn để kích hoạt cái chết
    public float warningThreshold = 1f; // Thời gian nhìn để bắt đầu cảnh báo

    [Header("Raycast")]
    public float maxDistance = 10f;     // Khoảng cách tối đa mà Player có thể nhìn thấy vật thể
}