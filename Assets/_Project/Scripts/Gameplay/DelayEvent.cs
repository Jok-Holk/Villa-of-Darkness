using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DelayEvent : MonoBehaviour
{
    [SerializeField] private float _delaySeconds = 1f;
    public UnityEvent OnDelayComplete;

    private Coroutine _delayCoroutine;

    public void StartDelay()
    {
        CancelDelay();
        Debug.Log($"<color=cyan>[DelayEvent]</color> Bắt đầu đợi {_delaySeconds} giây...");
        _delayCoroutine = StartCoroutine(DelayRoutine());
    }

    public void CancelDelay()
    {
        if (_delayCoroutine != null)
        {
            Debug.Log("<color=yellow>[DelayEvent]</color> Đã hủy đếm ngược cũ.");
            StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
        }
    }

    private IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(_delaySeconds);
        
        Debug.Log("<color=green>[DelayEvent]</color> Đã hết thời gian chờ! Đang kích hoạt Event...");
        
        // Kích hoạt các hàm đã đăng ký trong UnityEvent
        OnDelayComplete?.Invoke();
        
        _delayCoroutine = null;
    }
}