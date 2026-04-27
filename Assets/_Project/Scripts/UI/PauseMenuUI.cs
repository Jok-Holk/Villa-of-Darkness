using UnityEngine;
using UnityEngine.Events;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private bool _isPaused = false;
    public UnityEvent OnPause = new UnityEvent();
    public UnityEvent OnResume = new UnityEvent();

    public void Pause()  
    { 
        if (_isPaused) return;
        _isPaused = true;  
        Time.timeScale = 0f; 
        OnPause.Invoke();
    }
    public void Resume() 
    { 
        if (!_isPaused) return;
        _isPaused = false; 
        Time.timeScale = 1f; 
        OnResume.Invoke();
    }
    public void Toggle()
    {
        if (_isPaused) Resume();
        else Pause();
    }
}
