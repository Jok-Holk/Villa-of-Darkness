using UnityEngine;

[CreateAssetMenu(fileName = "NewTriggerSettings", menuName = "VillaOfDarkness/Trigger Settings")]
public class TriggerSettings : ScriptableObject
{
    public string targetTag = "Player";
    public bool triggerOnce = true;
    public Color debugColor = new Color(0, 1, 0, 0.3f); // Màu để vẽ Zone trong Editor
}