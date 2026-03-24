using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _isOpen = false;
    public void Interact() { _isOpen = !_isOpen; }
}
