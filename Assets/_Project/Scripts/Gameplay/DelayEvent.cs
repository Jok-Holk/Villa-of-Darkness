using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DelayEvent : MonoBehaviour
{
    [SerializeField] private float _delaySeconds = 1f;
    public UnityEvent OnDelayComplete;
    public void StartDelay() { StartCoroutine(Delay()); }
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(_delaySeconds);
        OnDelayComplete?.Invoke();
    }
}
