using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private string _targetTag = "Player";
    [SerializeField] private bool _triggerOnce = true;

    public UnityEvent OnTriggered;

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && _triggerOnce) return;

        if (other.CompareTag(_targetTag))
        {
            OnTriggered?.Invoke();
            _hasTriggered = true;
        }
    }
}
