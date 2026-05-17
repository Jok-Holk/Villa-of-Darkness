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
        // Dùng Camera.main để raycast đúng hướng nhìn kể cả khi nhìn lên/xuống
        Transform origin = Camera.main != null ? Camera.main.transform : transform;
        Ray ray = new Ray(origin.position, origin.forward);

        Debug.DrawRay(origin.position, origin.forward * _interactRange, Color.red, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactLayer))
        {
            Debug.Log($"[Interact] Hit: {hit.collider.gameObject.name}");
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.Interact();
        }
    }
}