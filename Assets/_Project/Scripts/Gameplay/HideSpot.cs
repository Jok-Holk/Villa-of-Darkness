using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _playerIsHiding = false;
    public void Interact() { _playerIsHiding = !_playerIsHiding; }
}
