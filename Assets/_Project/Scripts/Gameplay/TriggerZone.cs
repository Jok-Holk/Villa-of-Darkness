using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    // Kéo file ScriptableObject đã tạo vào đây
    [SerializeField] private TriggerSettings _settings; 

    public UnityEvent OnTriggered;
    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu chưa gán Settings thì thoát để tránh lỗi
        if (_settings == null) return;

        if (_hasTriggered && _settings.triggerOnce) return;

        if (other.CompareTag(_settings.targetTag))
        {
            OnTriggered?.Invoke();
            _hasTriggered = true;
            Debug.Log($"[Trigger] {gameObject.name} activated by {_settings.targetTag}");
        }
    }

    // Phần bổ sung: Vẽ vùng Trigger trong Editor để dễ quan sát khi làm Level Design
    private void OnDrawGizmos()
    {
        if (_settings != null)
        {
            Gizmos.color = _settings.debugColor;
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.DrawCube(transform.position + col.center, col.size);
            }
        }
    }
}