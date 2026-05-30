using UnityEngine;

public class SanityZoneVisual : MonoBehaviour
{
    [SerializeField] private Color _color = new Color(1f, 0f, 0f, 0.15f);

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = _color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);

        Gizmos.color = new Color(_color.r, _color.g, _color.b, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}