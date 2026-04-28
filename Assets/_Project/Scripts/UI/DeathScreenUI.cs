using UnityEngine;
using UnityEngine.Events;
#pragma warning disable CS0414

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private bool _isVisible = false;
    [SerializeField] private string _characterName;
    public UnityEvent OnRetry = new UnityEvent();

    public void Show(string name, string years) 
    { 
        _isVisible = true; 
        _characterName = name;
    }
    public void Hide() { _isVisible = false; }
    public void Retry() { OnRetry.Invoke(); }
}
