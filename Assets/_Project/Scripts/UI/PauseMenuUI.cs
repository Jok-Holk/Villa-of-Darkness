using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private bool _isPaused = false;
    public void Pause()  { _isPaused = true;  Time.timeScale = 0f; }
    public void Resume() { _isPaused = false; Time.timeScale = 1f; }
}
