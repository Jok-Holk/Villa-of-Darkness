using UnityEngine;

public class SanityZone : MonoBehaviour
{
    public enum ZoneType { Safe, Danger }
    [SerializeField] private ZoneType _zoneType = ZoneType.Safe;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var sanity = FindObjectOfType<SanitySystem>();
        if (sanity == null) return;

        sanity.SetSafeZone(_zoneType == ZoneType.Safe);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var sanity = FindObjectOfType<SanitySystem>();
        if (sanity == null) return;

        // Khi ra ngoài → luôn về trạng thái nguy hiểm
        sanity.SetSafeZone(false);
    }
}