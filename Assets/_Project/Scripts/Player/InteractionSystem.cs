using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float _interactRange = 2.5f;
    [SerializeField] private LayerMask _interactLayer;

    public static bool IsInputBlocked = false;

    private IInteractable _currentTarget;

    private void Update()
    {
        if (IsInputBlocked || HideSpot.AnyPlayerHiding)
        {
            SetCurrentTarget(null);
            return;
        }

        SetCurrentTarget(RaycastForInteractable());

        if (_currentTarget != null && Input.GetKeyDown(KeyCode.E))
            _currentTarget.Interact();
    }

    private void SetCurrentTarget(IInteractable target)
    {
        if (target == _currentTarget) return;
        _currentTarget = target;
        if (target != null)
        {
            string label = (target as IInteractableLabel)?.InteractLabel;
            InteractPromptUI.Instance?.Show(label);
        }
        else InteractPromptUI.Instance?.Hide();
    }

    private IInteractable RaycastForInteractable()
    {
        Transform origin = Camera.main != null ? Camera.main.transform : transform;
        Ray ray = new Ray(origin.position, origin.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactLayer)) return null;

        IInteractable target = FindEnabledInteractable(hit.collider.gameObject);
        if (target == null && hit.collider.transform.parent != null)
            target = FindEnabledInteractable(hit.collider.transform.parent.gameObject);

        return target;
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