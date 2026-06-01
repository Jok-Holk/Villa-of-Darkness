using UnityEngine;

public class GhostProximitySanity : MonoBehaviour
{
    [SerializeField] private float _drainPerSecond = 0.08f;  // 8%/giây
    [SerializeField] private SanitySystem _sanitySystem;

    private bool _playerInRange = false;

    private void Update()
    {
        if (!_playerInRange) return;
        if (_sanitySystem == null) return;

        _sanitySystem.DecreaseSanity(_drainPerSecond * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
    }
}