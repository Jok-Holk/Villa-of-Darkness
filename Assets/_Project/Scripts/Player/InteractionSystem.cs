using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float _interactRange = 2.5f;
    [SerializeField] private LayerMask _interactLayer;

    public static bool IsInputBlocked = false;

    private void Update()
    {
        if (IsInputBlocked) return;
        if (HideSpot.AnyPlayerHiding) return;
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        Transform origin = Camera.main != null ? Camera.main.transform : transform;
        Ray ray = new Ray(origin.position, origin.forward);
        Debug.DrawRay(origin.position, origin.forward * _interactRange, Color.red, 0.5f);

        if (!Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactLayer)) return;

        Debug.Log($"[Interact] Hit: {hit.collider.gameObject.name}");

        IInteractable target = FindEnabledInteractable(hit.collider.gameObject);

        if (target == null && hit.collider.transform.parent != null)
            target = FindEnabledInteractable(hit.collider.transform.parent.gameObject);

        if (target == null)
            Debug.Log($"[Interact] Không tìm thấy IInteractable enabled trên {hit.collider.gameObject.name}");

        target?.Interact();
    }

    private IInteractable FindEnabledInteractable(GameObject go)
    {
        foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb.enabled && mb is IInteractable interactable)
                return interactable;
        }
        return null;
    }
}