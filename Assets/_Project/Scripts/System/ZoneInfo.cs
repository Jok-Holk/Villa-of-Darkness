using UnityEngine;

/// <summary>
/// Marker component trên root GameObject của mỗi additive zone scene.
/// Ghi chú zone nào do ai phụ trách.
/// </summary>
public class ZoneInfo : MonoBehaviour
{
    [Header("Zone Info")]
    [TextArea(1, 3)]
    public string description;
    public string[] assignedTo;

    public void Setup(string desc, string[] assigned)
    {
        description = desc;
        assignedTo  = assigned;
    }
}
