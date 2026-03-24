using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float _interactRange = 2.5f;
    [SerializeField] private LayerMask _interactLayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.Interact();
        }
    }
}