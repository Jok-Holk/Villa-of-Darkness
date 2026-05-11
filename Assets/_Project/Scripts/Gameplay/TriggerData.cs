using UnityEngine;

[CreateAssetMenu(fileName = "NewTriggerData", menuName = "VillaOfDarkness/Trigger Data")]
public class TriggerData : ScriptableObject
{
    [Header("Settings")]
    public float delaySeconds = 3f;
    public string debugMessage = "Trigger Activated";
    
    [Header("Visuals")]
    public Color zoneGizmoColor = Color.green; // Dùng để vẽ màu trong Editor cho dễ phân biệt
}