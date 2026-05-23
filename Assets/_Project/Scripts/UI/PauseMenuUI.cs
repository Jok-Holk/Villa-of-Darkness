using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private GameObject _pauseMenuPanel;
    public UnityEvent OnPause = new UnityEvent();
    public UnityEvent OnResume = new UnityEvent();

    public void Pause()  
    { 
        if (_isPaused) return;
        _isPaused = true;  
        Time.timeScale = 0f; 
        _pauseMenuPanel.SetActive(true);
        // ✅ Hiện và mở khóa chuột
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
        OnPause.Invoke();
    }
    public void Resume() 
    { 
        if (!_isPaused) return;
        _isPaused = false; 
        Time.timeScale = 1f; 
        _pauseMenuPanel.SetActive(false);
         // ✅ Ẩn và khóa chuột lại như lúc chơi
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
        OnResume.Invoke();
    }
    public void Toggle()
    {
        if (_isPaused) Resume();
        else Pause();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }
    public void GoToMainMenu()
{
    _isPaused = false;
    Time.timeScale = 1f;
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    SceneManager.LoadScene("MainMenu"); // đổi tên đúng scene của bạn
}
}
