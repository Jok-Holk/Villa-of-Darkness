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
        _delayCoroutine = StartCoroutine(DelayRoutine());
    }

    public void CancelDelay()
    {
        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
        }
    }

    private IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(_delaySeconds);
        OnDelayComplete?.Invoke();
        _delayCoroutine = null;
    }
}
