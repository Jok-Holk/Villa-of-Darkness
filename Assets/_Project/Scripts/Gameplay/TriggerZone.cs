using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private string _targetTag = "Player";
    public UnityEvent OnTriggered;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag)) OnTriggered?.Invoke();
    }
}
